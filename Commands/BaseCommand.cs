using System;
using System.Threading.Tasks;

using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.PlatformBackend.DTO;
using ShapeDiver.SDK.GeometryBackend;

using CommandLine;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Base functionality for commands (authentication, etc)
    /// </summary>
  
    class BaseCommand
    {
        [Option('i', "key_id")]
        public string KeyId { get; set; }

        [Option('s', "key_secret")]
        public string KeySecret { get; set; }

        /// <summary>
        /// Get an authenticated instance of the SDK
        /// </summary>
        /// <returns></returns>
        protected async Task<IShapeDiverSDK> GetAuthenticatedSDK()
        {
            if (String.IsNullOrEmpty(KeyId))
            {
                Console.Write("Enter ShapeDiver access key id (Press Enter to authenticate via browser): ");
                KeyId = Console.ReadLine();
            }
            if (!String.IsNullOrEmpty(KeyId))
            {
                if (String.IsNullOrEmpty(KeySecret))
                {
                    Console.Write("Enter ShapeDiver access key secret (Press Enter to authenticate via browser): ");
                    KeySecret = Console.ReadLine();
                }
            }

            // create instance of SDK, authenticate
            var sdk = new ShapeDiverSDK();
            if (!String.IsNullOrEmpty(KeyId) && !String.IsNullOrEmpty(KeySecret))
            {
                await sdk.AuthenticationClient.Authenticate(KeyId, KeySecret);
            }
            else
            {
                await sdk.AuthenticationClient.AuthenticateViaPlatform();
            }

            return sdk;
        }

        protected async Task WrapExceptions(Func<Task> action)
        {
            try
            {
                await action();
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
        }

    }
}
