using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;

using CommandLine;
using DotNetSdkSampleConsoleApp.Util;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Command for uploading a Grasshopper model to ShapeDiver. Uses individual SDK calls to show what's happening behind the scences. 
    /// Provides more feedback to the user. 
    /// Compare with <see cref="UploadCommand"/>. 
    /// </summary>
    [Verb("upload-model-verbose", isDefault: false, HelpText = "Upload a Grasshopper model to ShapeDiver (verbose output).")]
    class UploadCommandVerbose : BaseCommand, ICommand
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
                Console.WriteLine($"Create model...");
                var createResult = await sdk.PlatformClient.ModelApi.Create(createDto);

                // get a context for the model, upload the grasshopper definition
                var context = await sdk.GeometryBackendClient.GetContext(createResult.Data.Id, sdk.PlatformClient, Scopes);
                Console.WriteLine($"Upload model...");
                await sdk.GeometryBackendClient.UploadModel(context, Filename);

                // wait for model check
                var modelDto = await GeometryBackendUtils.WaitForModelCheck(sdk, context);

                if (modelDto.Model.Status == GDTO.ModelStatusEnum.Pending)
                {
                    Console.WriteLine($"Model checking requires manual intervention by ShapeDiver, please visit https://shapediver.com/app/library/pending");
                    Process.Start("https://shapediver.com/app/library/pending");
                    return;
                }

                if (modelDto.Model.Status != GDTO.ModelStatusEnum.Confirmed)
                {
                    if (!String.IsNullOrEmpty(modelDto.Model.Msg))
                    {
                        throw new Exception($"Model checking failed: {modelDto.Model.Msg}");
                    }
                    else
                    {
                        throw new Exception($"Model checking failed");
                    }
                }

                // Update model status on platform and publish it
                Console.WriteLine($"Publishing confirmed model...");
                await sdk.PlatformClient.ModelApi.Patch(context.PlatformModelId);
                await sdk.PlatformClient.ModelApi.Patch(context.PlatformModelId, new PDTO.ModelPatchDto() { Status = PDTO.ModelStatusEnum.Done });
                Process.Start($"https://shapediver.com/app/m/{createResult.Data.Slug}");
            });

        }
    }
}
