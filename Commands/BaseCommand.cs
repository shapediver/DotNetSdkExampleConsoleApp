using CommandLine;
using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.GeometryBackend;
using ShapeDiver.SDK.PlatformBackend;
using System;
using System.Threading.Tasks;

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
        /// Client ID for generic applications
        /// </summary>
        const string CLIENT_ID = "827bcbdc-8a5c-481a-b09a-e498074d91ca";

        /// <summary>
        /// Get an unauthenticated instance of the SDK.
        /// </summary>
        /// <returns></returns>
        protected IShapeDiverSDK GetSDK()
        {
            return new ShapeDiverSDK(CLIENT_ID);
        }

        /// <summary>
        /// Get an authenticated instance of the SDK. 
        /// </summary>
        /// <returns></returns>
        protected async Task<IShapeDiverSDK> GetAuthenticatedSDK()
        {
            // create instance of SDK, authenticate
            var sdk = new ShapeDiverSDK(CLIENT_ID);
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
                Console.WriteLine($"{Environment.NewLine}Complete exception info: {e}");
            }
            catch (PlatformBackendError e)
            {
                Console.WriteLine($"{Environment.NewLine}PlatformBackendError: {e.Message}");
                Console.WriteLine($"{Environment.NewLine}Complete exception info: {e}");
            }
            catch (AuthenticationError e)
            {
                Console.WriteLine($"{Environment.NewLine}AuthenticationError: {e.Message}");
                Console.WriteLine($"{Environment.NewLine}Complete exception info: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Environment.NewLine}Error: {e.Message}");
                Console.WriteLine($"{Environment.NewLine}Complete exception info: {e}");
            }
        }

    }
}
