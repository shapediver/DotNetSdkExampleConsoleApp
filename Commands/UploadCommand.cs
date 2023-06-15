using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;

using CommandLine;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Command for uploading a Grasshopper model to ShapeDiver. Uses the CreateAndUploadModel function from the SDK. 
    /// Compare with <see cref="UploadCommandVerbose"/>, which provides more feedback to the user. 
    /// </summary>
    [Verb("upload-model-simple", isDefault: false, HelpText = "Upload a Grasshopper model to ShapeDiver")]
    class UploadCommand : BaseCommand, ICommand
    {
        [Option('f', "filename", HelpText = "Path to Grasshopper model (.gh or .ghx)", Required = true)]
        public string Filename { get; set; }

        [Option('t', "title", HelpText = "Title of the model on the ShapeDiver Platform")]
        public string Title { get; set; }
        
        private List<PDTO.ModelTokenScopeEnum> Scopes = new List<PDTO.ModelTokenScopeEnum>() {
                PDTO.ModelTokenScopeEnum.GroupOwner,
                PDTO.ModelTokenScopeEnum.GroupExport,
                PDTO.ModelTokenScopeEnum.GroupView
        };

        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // check if file exists
                if (!File.Exists(Filename))
                {
                    throw new Exception("Could not read Grasshopper file");
                }
                var fi = new FileInfo(Filename);
                if (fi.Extension != ".gh" && fi.Extension != ".ghx")
                {
                    throw new Exception($"{Filename} is not a Grasshopper file");
                }

                // get authenticated SDK
                var sdk = await GetAuthenticatedSDK();

                // create model call
                var createDto = new PDTO.ModelCreateDto()
                {
                    FileType = fi.Extension == ".ghx" ? PDTO.ModelFileTypeEnum.GrasshopperXml : PDTO.ModelFileTypeEnum.GrasshopperBinary,
                    Title = Title,
                    Filename = fi.Name,
                };

                Console.WriteLine($"Starting model upload and check, please wait...");
                var context = await sdk.GeometryBackendClient.CreateAndUploadModel(sdk.PlatformClient, createDto, Filename, true);

                if (context.ModelData.Model.Status != GDTO.ModelStatusEnum.Confirmed)
                {
                    if (!String.IsNullOrEmpty(context.ModelData.Model.Msg))
                    {
                        throw new Exception($"Model checking failed: {context.ModelData.Model.Msg}");
                    }
                    else
                    {
                        throw new Exception($"Model checking failed");
                    }
                }

                // Open model on platform
                Console.WriteLine($"Model upload and check completed.");
                Process.Start($"https://shapediver.com/app/m/{context.PlatformModelId}");
            });

            Console.WriteLine($"{Environment.NewLine}Press Enter to close...");
            Console.ReadLine();
        }
    }
}
