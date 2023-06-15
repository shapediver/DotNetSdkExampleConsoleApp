using System;
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

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Get information about a ShapeDiver model from the platform.
    /// </summary>
    [Verb("get-model", isDefault: false, HelpText = "Get information about a ShapeDiver model.")]
    class GetModelCommand : BaseCommand, ICommand
    {
        [Option('i', "identifier", HelpText = "Identifier of the model (slug, model id, or geometry backend model id)", Required = false)]
        public string Identifier { get; set; }

        [Option('e', "embed", HelpText = "Request all available embed fields")]
        public bool EmbedFields { get; set; }

        [Option('j', "json", HelpText = "Output JSON instead of text")]
        public bool JsonOutput { get; set; }

        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // get identifier
                if (String.IsNullOrEmpty(Identifier))
                {
                    Console.Write("Model identifier (slug, model id, geometry backend model id): ");
                    Identifier = Console.ReadLine();
                }
                if (String.IsNullOrEmpty(Identifier))
                {
                    throw new Exception($"Refusing to proceed without a model identifier");
                }

                // in case the identifier is a url, guess the slug from it
                if (Identifier.StartsWith("https://"))
                    Identifier = Identifier.Split('/').Last();

                // get authenticated SDK
                var sdk = await GetAuthenticatedSDK();

                // get model call
                if (!JsonOutput)
                    Console.WriteLine($"Get model information...");
                var model = (await sdk.PlatformClient.ModelApi.Get<PDTO.ModelDto>(Identifier)).Data;

                if (JsonOutput && !EmbedFields)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));
                    return;
                }

                if (!JsonOutput)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Model id (platform): {model.Id}");
                    Console.WriteLine($"Title: {model.Title}");
                    Console.WriteLine($"Slug: {model.Slug}");
                    Console.WriteLine($"Description: {model.Description}");
                    Console.WriteLine($"Visibility: {model.Visibility}");
                    Console.WriteLine($"VisibilityNominal: {model.VisibilityNominal}");
                    Console.WriteLine($"Thumbnail: {model.ThumbnailUrl}");

                    if (model.Permissions.Contains(PDTO.PermissionModelEnum.GetRestModelOwner))
                    {
                        Console.WriteLine($"Status: {model.Status}");
                        Console.WriteLine($"Created at: {model.CreatedAt}");
                        Console.WriteLine($"Updated at: {model.UpdatedAt}");
                        Console.WriteLine($"Embedding enabled: {model.UseGlobalAccessdomains}");
                        Console.WriteLine($"Private link sharing slug: {model.LinkSharingSlug}");
                        Console.WriteLine($"Backend access enabled: {model.BackendAccess}");
                        Console.WriteLine($"JWT required: {model.RequireToken}");
                    }
                }

                if (!EmbedFields || (model.Status != PDTO.ModelStatusEnum.Confirmed && model.Status != PDTO.ModelStatusEnum.Done))
                    return;

                // check which permissions we have, add embed fields
                if (!JsonOutput)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Get extended model information...");
                }
                var embedFields = PlatformBackendUtils.MapModelPermissionsToEmbedFields(model);
                model = (await sdk.PlatformClient.ModelApi.Get<PDTO.ModelDto>(Identifier, embedFields.ToArray())).Data;

                if (JsonOutput)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));
                    return;
                }

                if (model.Accessdomains?.Count() > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Per-model domains from which the model may be embedded:");
                    model.Accessdomains.ForEach(d => Console.WriteLine($"\t{d.Name}"));
                }
                if (model.GlobalAccessdomains?.Count() > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Per-account domains from which the model may be embedded:");
                    model.GlobalAccessdomains.ForEach(d => Console.WriteLine($"\t{d.Name}"));
                }
                if (model.BackendProperties != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Geometry backend properties:");
                    Console.WriteLine(JsonConvert.SerializeObject(model.BackendProperties, Formatting.Indented));
                }
                if (model.BackendSystem != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Geometry backend system:");
                    Console.WriteLine($"\tAlias: {model.BackendSystem.Alias}");
                    Console.WriteLine($"\tModel view URL: {model.BackendSystem.ModelViewUrl}");
                    Console.WriteLine($"\tDescription: {model.BackendSystem.Description}");
                }
                if (model.Bookmark != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Bookmark: {model.Bookmark.Bookmarked}");
                }
                if (model.Decoration != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Decoration:");
                    model.Decoration.ForEach(d => Console.WriteLine($"\t{d.Url}"));
                }
                if (model.Organization != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Organization properties:");
                    Console.WriteLine(JsonConvert.SerializeObject(model.Organization, Formatting.Indented));
                }
                if (model.User != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"User properties:");
                    Console.WriteLine(JsonConvert.SerializeObject(model.User, Formatting.Indented));
                }
                if (model.Tags?.Count() > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Tags:");
                    model.Tags.ForEach(d => Console.WriteLine($"\t{d.Name}"));
                }

            });

        }
    }
}
