﻿using System;
using System.Threading.Tasks;

using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.PlatformBackend.DTO;

using CommandLine;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Demo command showing basic usage of the SDK. 
    /// Makes use of 
    /// <see cref="https://help.shapediver.com/doc/platform-api-access-keys">Platform API access keys</see> 
    /// for authentication, or browser based authentication. 
    /// </summary>
    [Verb("demo", isDefault: false, HelpText = "Demo which prints some information about your account.")]

    class DemoCommand : BaseCommand, ICommand
    {
        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // get authenticated SDK
                var sdk = await GetAuthenticatedSDK();
                Console.WriteLine($"{Environment.NewLine}IsAuthenticated: {sdk.AuthenticationClient.IsAuthenticated}");

                // get user information
                var user = (await sdk.PlatformClient.UserApi.Get<UserDto>(sdk.AuthenticationClient.GetUserId(), UserGetEmbeddableFields.Used_Credits)).Data;

                Console.WriteLine();
                Console.WriteLine($"User Id: {user.Id}");
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"FirstName: {user.FirstName}");
                Console.WriteLine($"LastName: {user.LastName}");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"Credits used this month: {user.UsedCredits.UsedCreditsCurrentMonth}");

                // get detailed information about usage in the past days
                int numDays = 5;
                Console.WriteLine();
                Console.WriteLine($"Usage of exports and embedded sessions in the past {numDays} days:");
                long unixTimeNow = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                long unixTimeTenDaysAgo = unixTimeNow - (numDays + 1) * 86400;
                var analyticsQuery = sdk.PlatformClient.UserAnalyticsApi.CreateQueryBody();
                analyticsQuery.AddFilter(ex => ex.Property(d => d.TimestampType).EqualTo(AnalyticsTimestampTypeEnum.Day));
                analyticsQuery.AddFilter(ex => ex.Property(d => d.TimestampDate).GreaterOrEqualTo(unixTimeTenDaysAgo));
                analyticsQuery.AddFilter(ex => ex.Property(d => d.UserId).EqualTo(user.Id));
                var analyticsResult = await sdk.PlatformClient.UserAnalyticsApi.Query(analyticsQuery);
                foreach (var dailyStats in analyticsResult.Data.Result)
                {
                    Console.WriteLine($"Exports on {dailyStats.Timestamp}: {dailyStats.Data.Export.Sum}");
                    Console.WriteLine($"Credits for embedded sessions on {dailyStats.Timestamp}: {dailyStats.Data.Embedded.BillableCount}");
                }
                if (analyticsResult.Data.Result.Count == 0)
                {
                    Console.WriteLine("No aggregated analytics found.");
                }

                // get latest 10 published models
                var query = sdk.PlatformClient.ModelApi.CreateQueryBody(10);
                query.AddSorter(SorterType.Created_At, SortOrder.Desc);
                query.AddFilter(ex => ex.Property(m => m.Status).EqualTo(ModelStatusEnum.Done));
                query.AddFilter(ex => ex.Property(m => m.UserId).EqualTo(user.Id));
                var result = await sdk.PlatformClient.ModelApi.Query(query);
                var models = result.Data.Result;

                Console.WriteLine();
                if (models.Count == 0)
                {
                    Console.WriteLine("No published models found.");
                }
                else
                {
                    Console.WriteLine("Latest published models:");
                    foreach (var model in models)
                    {
                        Console.WriteLine($"\tTitle: {model.Title}, Slug: {model.Slug}");
                    }
                }

                // get latest model which allows backend access
                query = sdk.PlatformClient.ModelApi.CreateQueryBody(1);
                query.AddSorter(SorterType.Created_At, SortOrder.Desc);
                query.AddFilter(ex => ex.Property(m => m.Status).EqualTo(ModelStatusEnum.Done));
                query.AddFilter(ex => ex.Property(m => m.UserId).EqualTo(user.Id));
                query.AddFilter(ex => ex.Property(m => m.BackendAccess).EqualTo(true));
                query.AddFilter(ex => ex.Property(m => m.DeletedAt).IsNull());
                result = await sdk.PlatformClient.ModelApi.Query(query);
                models = result.Data.Result;

                Console.WriteLine();
                if (models.Count == 0)
                {
                    Console.WriteLine("No published models found which allow backend access.");
                }
                else
                {
                    Console.WriteLine("Latest published model which allows backend access:");
                    foreach (var model in models)
                    {
                        Console.WriteLine($"\tTitle: {model.Title}, Slug: {model.Slug}");
                    }

                    // get parameters of latest model
                    var context = await sdk.GeometryBackendClient.GetSessionContext(models[0].Id, sdk.PlatformClient);
                    Console.WriteLine();
                    Console.WriteLine("Parameters and outputs of latest published model which allows backend access:");
                    Console.WriteLine("Parameters:");
                    foreach (var param in context.ModelData.Parameters)
                    {
                        Console.WriteLine($"\tId: {param.Key}, Name: {param.Value.Name}, Type: {param.Value.Type}");
                    }
                    Console.WriteLine("Outputs:");
                    foreach (var output in context.ModelData.Outputs)
                    {
                        Console.WriteLine($"\tId: {output.Key}, Name: {output.Value.Name}, Uid: {output.Value.Uid}");
                    }
                    Console.WriteLine("Binary glTF files available:");
                    foreach (var asset in sdk.GeometryBackendClient.GetAllOutputAssetsForFormat(context, "glb"))
                    {
                        Console.WriteLine($"\tOutput Name: {context.ModelData.Outputs[asset.OutputId].Name}, Format: {asset.Format}, Size: {asset.Size}");
                    }
                }
            });
          
        }
    }
}
