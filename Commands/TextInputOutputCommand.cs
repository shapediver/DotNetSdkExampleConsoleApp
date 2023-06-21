using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.GeometryBackend;
using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;

using CommandLine;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Demo command calling a ShapeDiver model that has text inputs and outputs. 
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
    [Verb("text-io-demo", isDefault: false, HelpText = "Demo calling a ShapeDiver model with text input and output.")]

    class TextInputOutputCommand : BaseCommand, ICommand
    {
        [Option('t', "backend_ticket", HelpText = "Provide backend_ticket AND model_view_url, OR an identifier")]
        public string BackendTicket { get; set; }

        [Option('u', "model_view_url", HelpText = "Provide backend_ticket AND model_view_url, OR an identifier")]
        public string ModelViewUrl { get; set; }

        [Option('m', "model", HelpText = "Identifier for the model (slug, url or id). Provide and identifier, OR backend_ticket AND model_view_url. When using an identifier, also specify key_id and key_secret or use browser based authentication.")]
        public string IdOrSlug { get; set; }

        [Option('i', "input_file")]
        public string InputFileName { get; set; }

        [Option('o', "output_file")]
        public string OutputFileName { get; set; }

        public async Task Execute()
        {
            try
            {
                // validate input
                if (String.IsNullOrEmpty(IdOrSlug) && (String.IsNullOrEmpty(BackendTicket) || String.IsNullOrEmpty(ModelViewUrl)))
                {
                    Console.Write("Enter slug or id (press Enter to specify backend ticket and model view URL instead): ");
                    BackendTicket = ReadLine();
                }
                if (String.IsNullOrEmpty(IdOrSlug))
                {
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
                }
                if (String.IsNullOrEmpty(IdOrSlug) && (String.IsNullOrEmpty(BackendTicket) || String.IsNullOrEmpty(ModelViewUrl)))
                {
                    throw new ArgumentException($"Either a model identifier, or backend ticket AND model view URL must be specified");
                }

                if (String.IsNullOrEmpty(InputFileName))
                {
                    Console.Write("Input file name: ");
                    InputFileName = Console.ReadLine();
                }
              
                if (!File.Exists(InputFileName))
                    throw new ArgumentException($"File {InputFileName} can not be read");
                var inputFileData = File.ReadAllText(InputFileName);

                // in case the identifier is a url, guess the slug from it
                if (!String.IsNullOrEmpty(IdOrSlug) && IdOrSlug.StartsWith("https://"))
                    IdOrSlug = IdOrSlug.Split('/').Last();

                // get SDK, authenticated to the platform in case we need to use the platform API
                var sdk = String.IsNullOrEmpty(IdOrSlug) ? new ShapeDiverSDK() : await GetAuthenticatedSDK();

                // Create a session based context using the given backend ticket and model view URL. 
                // Note: In case the model requires token authorization, please extend this call and pass a token creator. 
                Console.Write("Creating session ... ");
                var stopWatch = Stopwatch.StartNew();
                var context = String.IsNullOrEmpty(IdOrSlug) ?
                    await sdk.GeometryBackendClient.GetSessionContext(BackendTicket, ModelViewUrl, new List<GDTO.TokenScopeEnum>() { GDTO.TokenScopeEnum.GroupView, GDTO.TokenScopeEnum.GroupExport }) :
                    await sdk.GeometryBackendClient.GetSessionContext(IdOrSlug, sdk.PlatformClient, new List<PDTO.ModelTokenScopeEnum>() { PDTO.ModelTokenScopeEnum.GroupView, PDTO.ModelTokenScopeEnum.GroupExport });
                Console.WriteLine($"done ({stopWatch.ElapsedMilliseconds}ms)");
            
                // Identify text input parameters
                // - textParameter is used for rather short input strings 
                // - textFileParameter is used for longer input strings
                // The Grasshopper model uses data from either one of them.

                var textParameter = context.ModelData.Parameters.Values.Where(p => p.Type == GDTO.ParameterTypeEnum.String).FirstOrDefault();
                if (textParameter == null)
                    throw new Exception("Model does not expose a parameter of type 'String'");

                var textFileParameter = context.ModelData.Parameters.Values.Where(p => p.Type == GDTO.ParameterTypeEnum.File).FirstOrDefault();
                if (textParameter == null)
                    throw new Exception("Model does not expose a parameter of type 'File'");

                // Identify export to compute
                var textExport = context.ModelData.Exports.Values.Where(e => e.Type == GDTO.ExportTypeEnum.Download).FirstOrDefault();
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
                    Console.Write("Uploading input data ... ");
                    stopWatch.Restart();
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(inputFileData)))
                    {
                        var uploadResult = await context.GeometryBackendClient.UploadFile(context, textFileParameter.Id, stream, "text/plain");
                        // set the value of the File parameter to the id of the uploaded file
                        paramDict.Add(uploadResult.ParameterId, uploadResult.FileId);
                    }
                    Console.WriteLine($"done ({stopWatch.ElapsedMilliseconds}ms)");
                }

                // Run export
                Console.Write("Computing ... ");
                stopWatch.Restart();
                var exportResult = await context.GeometryBackendClient.ComputeExport(context, textExport.Id, paramDict);
                Console.WriteLine($"done ({stopWatch.ElapsedMilliseconds}ms)");

                if (exportResult.HasFailed)
                {
                    throw new Exception(exportResult.Message);
                }

                var asset = context.GeometryBackendClient.GetAllExportAssets(context, exportResult).FirstOrDefault();
                if (asset == null)
                    throw new Exception("Expected to find an export asset");

                // save export result to file
                var fileName = String.IsNullOrEmpty(OutputFileName) ? asset.Filename : OutputFileName;
                if (String.IsNullOrEmpty(fileName))
                    throw new Exception("Expected file name to save results to");

                Console.Write($"Downloading and saving results to {fileName} ...");
                stopWatch.Restart();
                using (var fileStream = File.Create(fileName))
                {
                    (await asset.GetStream()).CopyTo(fileStream);
                }
                Console.WriteLine($"done ({stopWatch.ElapsedMilliseconds}ms)");

                // close session
                Console.Write($"Closing session ...");
                stopWatch.Restart();
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

    }
}
