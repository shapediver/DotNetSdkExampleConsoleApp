using System;
using System.Threading.Tasks;

using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.PlatformBackend.DTO;

using CommandLine;
using ShapeDiver.SDK.Authentication;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Log out the currently logged in user (clear persisted refresh token).
    /// </summary>
    [Verb("logout", isDefault: false, HelpText = "Log out the currently logged in user (clear persisted refresh token).")]

    class LogoutCommand : BaseCommand, ICommand
    {
        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // get authenticated SDK
                var sdk = GetSDK();
                var cleared = sdk.AuthenticationClient.ClearPersistedRefreshToken();
                if (cleared)
                    Console.WriteLine($"{Environment.NewLine}Refresh token cleared.");
                else
                    Console.WriteLine($"{Environment.NewLine}No refresh token found.");
           
            });
          
        }
    }
}
