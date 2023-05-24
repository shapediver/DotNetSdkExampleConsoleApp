# DotNetSdkSampleConsoleApp
Sample console application using the [ShapeDiver .NET SDK](https://www.nuget.org/packages?q=shapediver).

# How to get support

  * [ShapeDiver APIs and SDKs help pages](https://help.shapediver.com/doc/apis-and-sdks)
  * [ShapeDiver Forum](https://forum.shapediver.com)

# About the SDK

The [ShapeDiver .NET SDK](https://www.nuget.org/packages?q=shapediver) bundles the following functionality into a coherent package. 

  * Authentication client
    * Authenticate as a user of the [ShapeDiver Platform](https://help.shapediver.com/doc/platform-backend)
    * Supports authentication via [Platform API access keys](https://help.shapediver.com/doc/platform-api-access-keys)
    * Supports authentication via the ShapeDiver Platform
  * Platform Backend client
    * SDK for the [Platform Backend API](https://help.shapediver.com/doc/platform-backend#PlatformBackend-PlatformBackendAPI)
    * Makes use of the Authentication client and transparently handles Json Web Token refresh
  * Geometry Backend client
    * SDK for the [Geometry Backend API](https://help.shapediver.com/doc/geometry-backend)
    * Can be used with the Platform Backend client for generating Json Web Tokens
    * Can be used with the token generator of dedicated ShapeDiver Geometry Backend systems
    
# Usage

Build the console application using Visual Studio. Run the executable without options to get basic help: 

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  access-key-demo    Demo using Platform API access keys

  text-io            Demo calling a ShapeDiver model with text input and output

  help               Display more information on a specific command.

  version            Display version information.

```

How to get help for individual commands, examples: 

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help text-io-demo
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -t, --backend_ticket

  -u, --model_view_url

  -i, --input_file

  -o, --output_file

  --help                  Display this help screen.

  --version               Display version information.
```

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help access-key-demo
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -i, --key_id

  -s, --key_secret

  --help              Display this help screen.

  --version           Display version information.
```
