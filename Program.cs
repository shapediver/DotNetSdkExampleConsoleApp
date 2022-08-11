using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                Console.Write("Enter ShapeDiver access key id: ");
                string key_id = Console.ReadLine();
                Console.Write("Enter ShapeDiver access key secret: ");
                string key_secret = Console.ReadLine();

                // create instance of SDK, authenticate
                var sdk = new ShapeDiverSDK();
                await sdk.AuthenticationClient.Authenticate(key_id, key_secret);
                
                Console.WriteLine($"IsAuthenticated: {sdk.AuthenticationClient.IsAuthenticated}");

                // get user information
                var user = ( await sdk.PlatformClient.UserApi.Get<UserDto>(sdk.AuthenticationClient.GetUserId()) ).Data;

                Console.WriteLine();
                Console.WriteLine($"User Id: {user.Id}");
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"FirstName: {user.FirstName}");
                Console.WriteLine($"LastName: {user.LastName}");
                Console.WriteLine($"Email: {user.Email}");

                // get latest 10 published models
                var query = sdk.PlatformClient.ModelApi.CreateQueryBody(10);
                query.Sorters.Add(new Sorter(SorterType.Created_At, SortOrder.Desc));
                query.Filters.Add(ModelQuery.Start.Property(m => m.Status).EqualTo(ModelStatusEnum.Done));
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
                Console.WriteLine($"GeometryBackendError: {e.Message}");
            }
            catch (PlatformBackendError e)
            {
                Console.WriteLine($"PlatformBackendError: {e.Message}");
            }
            catch (AuthenticationError e)
            {
                Console.WriteLine($"AuthenticationError: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("Press Enter to close...");
            Console.ReadLine();
        }
    }
}
