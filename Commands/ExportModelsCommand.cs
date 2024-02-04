﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

using ShapeDiver.SDK.PlatformBackend;
using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;

using CommandLine;
using System.Linq;
using DotNetSdkSampleConsoleApp.Util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ShapeDiver.SDK.PlatformBackend.DTO;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Export models from the ShapeDiver platform.
    /// </summary>
    [Verb("export-models", isDefault: false, HelpText = "Export models from the ShapeDiver platform.")]
    class ExportModelsCommand : BaseCommand, ICommand
    {
        [Option('u', "userid", HelpText = "Filter models owned by the given user id, instead of your own")]
        public string UserId { get; set; }

        [Option('b', "backendsystem", HelpText = "Filter models by backend system")]
        public string BackendSystemFilter { get; set; }

        [Option('d', "download", HelpText = "Download models")]
        public bool Download { get; set; }

        [Option("deleted", HelpText = "Include deleted models")]
        public bool Deleted { get; set; }


        private List<ModelTokenScopeEnum> ContextScopes = new List<ModelTokenScopeEnum>() { 
            ModelTokenScopeEnum.GroupView,
            ModelTokenScopeEnum.FileDownload
        };

        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // get authenticated SDK
                var sdk = await GetAuthenticatedSDK();

                // prepare query
                var query = sdk.PlatformClient.ModelApi.CreateQueryBody(10, true /* attain limit */);

                query.AddSorter(SorterType.Created_At, SortOrder.Desc);

                if (!Deleted)
                    query.AddFilter(ex => ex.Property(m => m.DeletedAt).IsNull());

                query.AddFilter(ex => ex.Property(m => m.Status).InArray(new List<PDTO.ModelStatusEnum>() { PDTO.ModelStatusEnum.Done, PDTO.ModelStatusEnum.Confirmed } ));

                if (UserId != null)
                    query.AddFilter(ex => ex.Property(m => m.UserId).EqualTo(UserId));
                else
                    query.AddFilter(ex => ex.Property(m => m.UserId).EqualTo(sdk.AuthenticationClient.GetUserId()));

                query.EmbeddableFields = BackendSystemFilter != null ? new List<ModelQueryEmbeddableFields>() { ModelQueryEmbeddableFields.Backend_System } : null;
          
                // prepare CSV export
                var csvFilename = $"models-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.csv";
                Action<string> appendCsvLine = (string message) =>
                {
                    Console.WriteLine(message);
                    File.AppendAllText(csvFilename, message + Environment.NewLine);
                };
                appendCsvLine("created at;slug;title;status");

                // query models
                var models = (await sdk.PlatformClient.ModelApi.Query<PDTO.ModelDto>(query)).Data;
                if (models.Result.Count == 0) {
                    Console.WriteLine("No matching models found.");
                    return;
                }

                while (true)
                {
                    foreach (var model in models.Result)
                    {
                        if (BackendSystemFilter != null)
                        {
                            if (!model.BackendSystem.ModelViewUrl.Contains(BackendSystemFilter))
                                continue;
                        }

                        // write csv line
                        DateTime createdAt = (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddSeconds((double)model.CreatedAt);
                        appendCsvLine($"{createdAt};{model.Slug};{model.Title};{model.Status}");

                        // download model
                        if (Download) {
                            var context = await sdk.GeometryBackendClient.GetContext(model.Id, sdk.PlatformClient, ContextScopes);
                            var fileEnding = context.ModelData.Setting.Computation.FileType == GDTO.ModelFileTypeEnum.GrasshopperBinary ? "gh" : "ghx";
                            var filename = $"{createdAt.ToString("yyyyMMddHHmmss")}-{model.Slug}.{fileEnding}";
                            using (var stream = await sdk.GeometryBackendClient.DownloadModel(context))
                            {
                                using (var fileStream = File.Create(filename))
                                {
                                    stream.CopyTo(fileStream);
                                }
                            }
                            
                        }
                    }

                    if (String.IsNullOrEmpty(models.Pagination?.NextOffset))
                        break;

                    query.Offset = models.Pagination.NextOffset;
                    models = (await sdk.PlatformClient.ModelApi.Query<PDTO.ModelDto>(query)).Data;
                }


            });

        }
    }
}
