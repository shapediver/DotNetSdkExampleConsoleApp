using System;
using System.Threading.Tasks;

using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.GeometryBackend;

using CommandLine;
using System.Collections.Generic;
using ShapeDiver.SDK.GeometryBackend.DTO;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Demo command using a ShapeDiver model that has text inputs and outputs for batch processing. 
    /// The command reads input data from files in a given input directory, and writes output data to 
    /// corresponding files in an output directory. 
    /// Multiple computation requests are issued in parallel. 
    /// 
    /// How to use this: 
    /// (1) Upload TextInputOutput.ghx (see directory "Grasshopper")
    ///     https://www.shapediver.com/app/m/upload
    /// (2) Enable backend access for the model
    ///     https://help.shapediver.com/doc/enable-backend-access
    /// (3) Copy the backend ticket and Model view URL from the "Developers" tab
    /// (4) Use them when calling this command
    /// 
    /// The Grasshopper model "TextInputOutput.ghx" has a text input parameter for strings up to
    /// 10k characters, and a file input parameter for longer strings.
    /// </summary>
    [Verb("text-io-batch-demo", isDefault: false, HelpText = "Demo using a ShapeDiver model with text input and output for batch processing.")]

    class TextInputOutputBatchCommand : ICommand
    {
        [Option('t', "backend_ticket")]
        public string BackendTicket { get; set; }

        [Option('u', "model_view_url")]
        public string ModelViewUrl { get; set; }

        [Option('i', "input_dir", HelpText = "Path to the directory to read input data from")]
        public string InputDirectory { get; set; }

        [Option('o', "output_dir", HelpText = "Path to the directory to write output data to")]
        public string OutputDirectory { get; set; }

        List<ComputationTask> Pool = new List<ComputationTask>();

        public async Task Execute()
        {
            try
            {
                // validate input
                if (String.IsNullOrEmpty(BackendTicket))
                {
                    Console.Write("Enter backend ticket: ");
                    BackendTicket = ReadLine();
                }
                if (String.IsNullOrEmpty(ModelViewUrl))
                {
                    Console.Write("Enter model view URL: ");
                    ModelViewUrl = Console.ReadLine();
                }
                if (String.IsNullOrEmpty(InputDirectory))
                {
                    Console.Write("Path to input directory: ");
                    InputDirectory = Console.ReadLine();
                }
                if (String.IsNullOrEmpty(OutputDirectory))
                {
                    Console.Write("Path to output directory: ");
                    OutputDirectory = Console.ReadLine();
                }

                if (!Directory.Exists(InputDirectory))
                    throw new ArgumentException($"Directory {InputDirectory} can not be read");
                if (!Directory.Exists(OutputDirectory))
                    throw new ArgumentException($"Directory {OutputDirectory} can not be read");

                var inputFileNames = Directory.GetFiles(InputDirectory);
                
                // Create instance of SDK
                var sdk = new ShapeDiverSDK();

                // Create a session based context using the given backend ticket and model view URL. 
                // Note: In case the model requires token authorization, please extend this call and pass a token creator. 
                Console.Write("Creating session ... ");
                var stopWatch = Stopwatch.StartNew();
                var context = await sdk.GeometryBackendClient.GetSessionContext(BackendTicket, ModelViewUrl, new List<TokenScopeEnum>() { TokenScopeEnum.GroupView });
                Console.WriteLine($"done ({stopWatch.ElapsedMilliseconds}ms)");

                // show some statistics
                int numTotal = inputFileNames.Count();
                int numDone = 0;
                int numFailed = 0;
                long timeSpent = 0;

                // run computation(s)
                var taskStopWatch = Stopwatch.StartNew();
                var iter = inputFileNames.GetEnumerator();
                while (true)
                {
                    while (Pool.Count < 10)
                    {
                        if (!iter.MoveNext())
                            break;
                        var inputFileName = (string)iter.Current;
                        var fi = new FileInfo(inputFileName);
                        var outputFileName = Path.Combine(OutputDirectory, fi.Name);

                        Pool.Add(new ComputationTask(CreateComputationTask(context, inputFileName, outputFileName), fi.Name));
                    }

                    if (Pool.Count == 0)
                        break;

                    List<ComputationTask> newPool = new List<ComputationTask>();
                    foreach (var task in Pool)
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            numDone++;
                            timeSpent += task.Processingime;
                            Console.WriteLine($"Done/Failed/Total: {numDone} ({((double)numDone / numTotal).ToString("P1")}) / {numFailed} / {numTotal} | Avg time: {(timeSpent / numDone).ToString("d")}ms | Avg parallelism: {((float)timeSpent / taskStopWatch.ElapsedMilliseconds).ToString("F2")}");
                        } 
                        else if (task.Status == TaskStatus.Faulted)
                        {
                            Console.WriteLine($"{task.Name} - error");
                            numFailed++;
                            Console.WriteLine($"Done/Failed/Total: {numDone} ({((double)numDone / numTotal).ToString("P1")}) / {numFailed} / {numTotal} | Avg time: {(timeSpent / numDone).ToString("d")}ms | Avg parallelism: {((float)timeSpent / taskStopWatch.ElapsedMilliseconds).ToString("F2")}");
                        }
                        else
                        {
                            newPool.Add(task);
                        }
                    }

                    Pool = newPool;

                    await Task.Delay(10);
                }

                // close session
                Console.Write($"Closing session ...");
                await context.GeometryBackendClient.CloseSessionContext(context);
                Console.WriteLine($"done ({stopWatch.ElapsedMilliseconds}ms)");
            }
            catch (GeometryBackendError e)
            {
                Console.WriteLine($"{Environment.NewLine}GeometryBackendError: {e.Message}");
            }
            catch (PlatformBackendError e)
            {
                Console.WriteLine($"{Environment.NewLine}PlatformBackendError: {e.Message}");
            }
            catch (AuthenticationError e)
            {
                Console.WriteLine($"{Environment.NewLine}AuthenticationError: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Environment.NewLine}Error: {e.Message}");
            }

            Console.WriteLine($"{Environment.NewLine}Press Enter to close...");
            Console.ReadLine();
        }

        private async Task CreateComputationTask(IGeometryBackendContext context, string inputFileName, string outputFileName)
        {
            // Read input data
            var inputFileData = File.ReadAllText(inputFileName);

            // Identify text input parameters
            // - textParameter is used for rather short input strings 
            // - textFileParameter is used for longer input strings
            // The Grasshopper model uses data from either one of them.

            var textParameter = context.ModelData.Parameters.Values.Where(p => p.Type == ParameterTypeEnum.String).FirstOrDefault();
            if (textParameter == null)
                throw new Exception("Model does not expose a parameter of type 'String'");

            var textFileParameter = context.ModelData.Parameters.Values.Where(p => p.Type == ParameterTypeEnum.File).FirstOrDefault();
            if (textParameter == null)
                throw new Exception("Model does not expose a parameter of type 'File'");

            // Identify export to compute
            var textExport = context.ModelData.Exports.Values.Where(e => e.Type == ExportTypeEnum.Download).FirstOrDefault();
            if (textExport == null)
                throw new Exception("Model does not expose an export of type 'download'");

            // Prepare parameter data
            var paramDict = new Dictionary<string, string>();
            if (inputFileData.Length <= textParameter.Max)
            {
                // length is below maximum of text parameter, avoid uploading input data as file
                paramDict.Add(textParameter.Id, inputFileData);
            }
            else
            {
                // length exceeds maximum of text parameter, upload as file
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(inputFileData)))
                {
                    var uploadResult = await context.GeometryBackendClient.UploadFile(context, textFileParameter.Id, stream, "text/plain");
                    // set the value of the File parameter to the id of the uploaded file
                    paramDict.Add(uploadResult.ParameterId, uploadResult.FileId);
                }
            }

            // Run export
            var exportResult = await context.GeometryBackendClient.ComputeExport(context, textExport.Id, paramDict);
  
            if (exportResult.HasFailed)
            {
                throw new Exception(exportResult.Message);
            }

            var asset = context.GeometryBackendClient.GetAllExportAssets(context, exportResult).FirstOrDefault();
            if (asset == null)
                throw new Exception("Expected to find an export asset");

            // save export result to file
            var fileName = String.IsNullOrEmpty(outputFileName) ? asset.Filename : outputFileName;
            if (String.IsNullOrEmpty(fileName))
                throw new Exception("Expected file name to save results to");

            using (var fileStream = File.Create(fileName))
            {
                (await asset.GetStream()).CopyTo(fileStream);
            }
        }

        /// <summary>
        /// Version of ReadLine which can read more than 254 characters
        /// </summary>
        /// <returns></returns>
        private string ReadLine()
        {
            using (var inputStream = Console.OpenStandardInput(512))
            {
                var reader = Console.In;
                try
                {
                    Console.SetIn(new StreamReader(inputStream, Encoding.Default, false, 512));
                    return Console.ReadLine();
                } 
                finally
                {
                    Console.SetIn(reader);
                }
            }
        }

        private class ComputationTask
        {
            public Task Task { get; private set; }

            public string Name { get; private set; }

            /// <summary>
            /// Note: This is the computation time plus all the overhead of data upload/download etc
            /// </summary>
            public long Processingime { get; private set; }

            public TaskStatus Status => Task.Status;

            public ComputationTask(Task task, string name)
            {
                Task = task;
                Name = name;

                var stopWatch = Stopwatch.StartNew();
                Task.ContinueWith(t =>
                {
                    Processingime = stopWatch.ElapsedMilliseconds;
                    stopWatch.Stop();
                });
                Name = name;
            }
        }


    }
}
