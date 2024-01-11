using System;
using System.Threading.Tasks;

using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.GeometryBackend;

using CommandLine;
using ShapeDiver.SDK.Container;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Base functionality for commands (authentication, etc)
    /// </summary>
  
    class BaseCommand
    {
        [Option('k', "key_id", HelpText = "ShapeDiver access key id (browser based authentication will be used if not specified)")]
        public string KeyId { get; set; }

        [Option('s', "key_secret", HelpText = "ShapeDiver access key secret")]
        public string KeySecret { get; set; }

        /// <summary>
        /// Get an unauthenticated instance of the SDK.
        /// </summary>
        /// <returns></returns>
        protected IShapeDiverSDK GetSDK()
        {
            return new ShapeDiverSDK();
        }

        /// <summary>
        /// Get an authenticated instance of the SDK. 
        /// </summary>
        /// <returns></returns>
        protected async Task<IShapeDiverSDK> GetAuthenticatedSDK()
        {
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
