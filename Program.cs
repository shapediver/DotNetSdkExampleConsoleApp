﻿using System;
using System.Threading.Tasks;

using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.PlatformBackend.DTO;
using ShapeDiver.SDK.GeometryBackend;

namespace DotNetSdkSampleConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.Write("Enter ShapeDiver access key id (or username/email): ");
                string key_id = Console.ReadLine();
                Console.Write("Enter ShapeDiver access key secret (or password): ");
                string key_secret = Console.ReadLine();

                // create instance of SDK, authenticate
                var sdk = new ShapeDiverSDK();
                await sdk.AuthenticationClient.Authenticate(key_id, key_secret);
                
                Console.WriteLine($"{Environment.NewLine}IsAuthenticated: {sdk.AuthenticationClient.IsAuthenticated}");

                // get user information
                var user = ( await sdk.PlatformClient.UserApi.Get<UserDto>(sdk.AuthenticationClient.GetUserId(), UserGetEmbeddableFields.Used_Credits) ).Data;

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
                long unixTimeTenDaysAgo = unixTimeNow - (numDays+1) * 86400;
                var analyticsQuery = sdk.PlatformClient.UserAnalyticsApi.CreateQueryBody();
                analyticsQuery.Filters.Add(UserAnalyticsQuery.Start.Property(d => d.TimestampType).EqualTo(AnalyticsTimestampTypeEnum.Day));
                analyticsQuery.Filters.Add(UserAnalyticsQuery.Start.Property(d => d.TimestampDate).GreaterOrEqualTo(unixTimeTenDaysAgo));
                analyticsQuery.Filters.Add(UserAnalyticsQuery.Start.Property(d => d.UserId).EqualTo(user.Id));
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
                query.Sorters.Add(new Sorter(SorterType.Created_At, SortOrder.Desc));
                query.Filters.Add(ModelQuery.Start.Property(m => m.Status).EqualTo(ModelStatusEnum.Done));
                query.Filters.Add(ModelQuery.Start.Property(m => m.UserId).EqualTo(user.Id));
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
                query.Sorters.Add(new Sorter(SorterType.Created_At, SortOrder.Desc));
                query.Filters.Add(ModelQuery.Start.Property(m => m.Status).EqualTo(ModelStatusEnum.Done));
                query.Filters.Add(ModelQuery.Start.Property(m => m.UserId).EqualTo(user.Id));
                query.Filters.Add(ModelQuery.Start.Property(m => m.BackendAccess).EqualTo(true));
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
                    Console.WriteLine("Parameters of latest published model which allows backend access:");
                    foreach (var param in context.ModelData.Parameters)
                    {
                        Console.WriteLine($"\tId: {param.Key}, Name: {param.Value.Name}, Type: {param.Value.Type}");
                    }
                }

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
    }
}
