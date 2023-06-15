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
    /// List identifiers of ShapeDiver models from the platform.
    /// </summary>
    [Verb("list-models", isDefault: false, HelpText = "List ShapeDiver models, sorted by descending date of creation.")]
    class ListModelsCommand : BaseCommand, ICommand
    {
        

        [Option('u', "user", HelpText = "Filter models owned by you")]
        public bool FilterUser { get; set; }

        [Option('l', "limit", HelpText = "Limit for the number of models to list, defaults to 10")]
        public int? Limit { get; set; }

        [Option('o', "offset", HelpText = "Offset for continuing query")]
        public string Offset { get; set; }

        [Option('v', "visibility", HelpText = "Visibility of the model, one of \"private\", \"organization\", \"shared\", or \"public\"")]
        public string Visibility { get; set; }

        [Option('q', "search", HelpText = "Search string, used for searching models by slug, title, and model view URL")]
        public string SearchString { get; set; }

        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // get authenticated SDK
                var sdk = await GetAuthenticatedSDK();

                // prepare query
                var query = sdk.PlatformClient.ModelApi.CreateQueryBody(Limit == null ? 10 : Limit.Value, true /* attain limit */, Offset);
                query.AddSorter(SorterType.Created_At, SortOrder.Desc);
                query.AddFilter(ex => ex.Property(m => m.DeletedAt).IsNull());
                query.AddFilter(ex => ex.Property(m => m.Status).InArray(new List<PDTO.ModelStatusEnum>() { PDTO.ModelStatusEnum.Done, PDTO.ModelStatusEnum.Confirmed } ));
                if (FilterUser)
                    query.AddFilter(ex => ex.Property(m => m.UserId).EqualTo(sdk.AuthenticationClient.GetUserId()));
                if (!String.IsNullOrEmpty(Visibility))
                    query.AddFilter(ex => ex.Property(m => m.Visibility).EqualTo(Visibility));
                if (!String.IsNullOrEmpty(SearchString))
                    query.AddFilter(ex => ex.Property(m => m.Slug).Like(SearchString).Or().Property(m => m.Title).Like(SearchString));

                // query model call
                var result = (await sdk.PlatformClient.ModelApi.Query<PDTO.ModelPublicDto>(query)).Data;
                if (result.Result.Count == 0) {
                    Console.WriteLine("No matching models found.");
                    return;
                }

                Console.WriteLine($"Id;Slug;Title");
                foreach (var model in result.Result)
                {
                    Console.WriteLine($"{model.Id};{model.Slug};{model.Title}");
                }

                if (!String.IsNullOrEmpty(result.Pagination.NextOffset))
                {
                    Console.WriteLine($"Offset for continuing query: {result.Pagination.NextOffset}");
                }
                else
                {
                    Console.WriteLine($"No more matching models found.");
                }


            });

        }
    }
}
