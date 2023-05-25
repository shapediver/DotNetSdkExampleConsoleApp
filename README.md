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
    * Supports authentication via the ShapeDiver Platform (OAuth 2.0 authorization code flow)
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

# Commands


## Command [`text-io-demo`](Commands/TextInputOutputCommand.cs)

This command shows how to use the SDK for ShapeDiver models that support the input and output of text. 
The input and output strings can be large, up to the limit defined by your [ShapeDiver subscription](https://www.shapediver.com/pricing). 
Please upload the corresponding [Grasshopper model](Grasshopper/TextInputOutputCommand.ghx) using your ShapeDiver account for testing. 
You will need a _backend ticket_ and the _Model view URL_ of your model, both available on the _Developers_ tab when viewing your model on the [ShapeDiver Platform](https://help.shapediver.com/doc/online-platform).

### Usage

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

### Example (interactive input of options)
```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe text-io-demo
Enter backend ticket: 176129440dedba57391786b24ec2374e176c976a8939c5d9a4771270753653a447a1603dcac3e1e1f4b5a86571b686b3c162e032ca6c6f767926f2ecf7cbb27f8824e20d96f3d82d8c3514a61a96c4f0d95c59c3c2803ad8e531f51979123a6d660d97284d2f5ea54f13fe94fac2d47240cc208e20b23ee3-f94c0d435e8c61736cc3832e93273cae
Enter model view URL: https://sdr7euc1.eu-central-1.shapediver.com
Input file name: TextInputOutput_200k.txt
Creating session ... done (739ms)
Uploading input data ... done (506ms)
Computing ... done (764ms)
Downloading and saving results to export.txt ...done (179ms)
Closing session ...done (77ms)
```

### Example (command line input of options)
```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe text-io-demo ^
  --backend_ticket 176129440dedba57391786b24ec2374e176c976a8939c5d9a4771270753653a447a1603dcac3e1e1f4b5a86571b686b3c162e032ca6c6f767926f2ecf7cbb27f8824e20d96f3d82d8c3514a61a96c4f0d95c59c3c2803ad8e531f51979123a6d660d97284d2f5ea54f13fe94fac2d47240cc208e20b23ee3-f94c0d435e8c61736cc3832e93273cae ^
  --model_view_url https://sdr7euc1.eu-central-1.shapediver.com ^
  --input_file TextInputOutput_200k.txt
Creating session ... done (739ms)
Uploading input data ... done (506ms)
Computing ... done (764ms)
Downloading and saving results to export.txt ...done (179ms)
Closing session ...done (77ms)
```

## Command [`access-key-demo`](Commands/DemoCommand.cs)

This command shows how to authenticate using [Platform API access keys](https://help.shapediver.com/doc/platform-api-access-keys) 
and make basic calls to the [Platform Backend API](https://help.shapediver.com/doc/platform-backend#PlatformBackend-PlatformBackendAPI) 
and the [Geometry Backend API](https://help.shapediver.com/doc/geometry-backend). 

### Usage

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help access-key-demo
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -i, --key_id

  -s, --key_secret

  --help              Display this help screen.

  --version           Display version information.
```

### Example (interactive input of options)

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe access-key-demo
Enter ShapeDiver access key id (or username/email): SUPPRESSED
Enter ShapeDiver access key secret (or password): SUPPRESSED

IsAuthenticated: True

User Id: SUPPRESSED
Username: SUPPRESSED
FirstName: SUPPRESSED
LastName: SUPPRESSED
Email: SUPPRESSED
Credits used this month: 61

Usage of exports and embedded sessions in the past 5 days:
Exports on 20230520: 0
Credits for embedded sessions on 20230520: 0
Exports on 20230521: 0
Credits for embedded sessions on 20230521: 0
Exports on 20230522: 0
Credits for embedded sessions on 20230522: 0
Exports on 20230523: 0
Credits for embedded sessions on 20230523: 0
Exports on 20230524: 9
Credits for embedded sessions on 20230524: 0

Latest published models:
        Title: twistedtower-sdeuc1, Slug: twistedtower-sdeuc1
        Title: TextInputOutput, Slug: textinputoutput-sdeuc1
        Title: TextInputOutput, Slug: textinputoutput
        Title: TextInputOutput, Slug: textinputoutput
        Title: TextInputOutput, Slug: textinputoutput
        Title: email-export-test-1, Slug: email-export-test-1-2
     
Latest published model which allows backend access:
        Title: TextInputOutput, Slug: textinputoutput

Parameters and outputs of latest published model which allows backend access:
Parameters:
        Id: 11d2e52d-11b4-4955-9803-4d3c301fbf23, Name: Text file URL, Type: String
        Id: ed45c461-1ac8-4665-a7c0-50e724311a61, Name: Text file, Type: File
        Id: 0adf1b1c-c6f3-42df-ac2c-f636155b9c83, Name: Text (up to 10k chars), Type: String
        Id: 2ee2872a-e6ce-437c-b923-20f01b2f8024, Name: Extension, Type: StringList
        Id: aff3ba11-427d-48ca-b4e0-387e0c230cf5, Name: End of Line, Type: StringList
        Id: bf60cf5e-038d-44d5-9677-94115a30d814, Name: Filename, Type: String
        Id: 629e9b25-d179-4823-b88c-cefddadd5eae, Name: Encoding, Type: StringList
Outputs:
        Id: 6313b6247eb91dfd1583d6be25d189d9, Name: Tag, Uid: e06e85d9-1aa7-4e62-b1ab-375425797f06
        Id: 9e41ab6cf4536ccef8ef99478f70ec05, Name: Text output, Uid: bb341a14-4bd2-4b4c-94cc-6519c1581a63
Binary glTF files available:

```

