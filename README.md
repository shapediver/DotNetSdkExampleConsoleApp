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

  demo                    Demo which prints some information about your account.

  get-model               Get information about a ShapeDiver model.

  list-models             List ShapeDiver models, sorted by descending date of creation.

  text-io-demo            Demo calling a ShapeDiver model with text input and output.

  upload-model            Upload a Grasshopper model to ShapeDiver.

  upload-model-verbose    Upload a Grasshopper model to ShapeDiver (verbose output).

  help                    Display more information on a specific command.

  version                 Display version information.
```

# Commands


## Command [`demo`](Commands/DemoCommand.cs)

This command shows how to authenticate using [Platform API access keys](https://help.shapediver.com/doc/platform-api-access-keys) or 
via the ShapeDiver Platform using your browser
and make basic calls to the [Platform Backend API](https://help.shapediver.com/doc/platform-backend#PlatformBackend-PlatformBackendAPI) 
and the [Geometry Backend API](https://help.shapediver.com/doc/geometry-backend). 

### Usage

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help demo
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -k, --key_id

  -s, --key_secret

  --help              Display this help screen.

  --version           Display version information.
```

### Example (interactive input of options)

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe demo

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


## Command [`get-model`](Commands/GetModelCommand.cs)

This command shows how to get information about a model from the 
[Platform Backend API](https://help.shapediver.com/doc/platform-backend#PlatformBackend-PlatformBackendAPI). 

### Usage

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help get-model
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -i, --identifier    Identifier of the model (slug, model id, or geometry backend model id)

  -e, --embed         Request all available embed fields

  -j, --json          Output JSON instead of text

  -k, --key_id        ShapeDiver access key id (browser based authentication will be used if not specified)

  -s, --key_secret    ShapeDiver access key secret

  --help              Display this help screen.

  --version           Display version information.
```

### Examples

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe get-model
Model identifier (slug, model id, geometry backend model id): https://www.shapediver.com/app/m/bookshelf-exportable
Get model information...

Model id (platform): 97aa9a17-c86f-408e-bb77-5bb10508a319
Title: Bookshelf, exportable
Slug: bookshelf-exportable
Description: A bookshelf design making use of a reciprocal connection detail, exportable as DWG, DXF, 3DM, STEP, STL.
Visibility: Public
VisibilityNominal: Public
Thumbnail:
Status: Done
Created at: 1667589854
Updated at: 1681404039
Embedding enabled: False
Private link sharing slug:
Backend access enabled: False
JWT required: False
```

```

C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe get-model -i bookshelf-exportable -e
Get model information...

Model id (platform): 97aa9a17-c86f-408e-bb77-5bb10508a319
Title: Bookshelf, exportable
Slug: bookshelf-exportable
Description: A bookshelf design making use of a reciprocal connection detail, exportable as DWG, DXF, 3DM, STEP, STL.
Visibility: Public
VisibilityNominal: Public
Thumbnail:
Status: Done
Created at: 1667589854
Updated at: 1681404039
Embedding enabled: False
Private link sharing slug:
Backend access enabled: False
JWT required: False

Get extended model information...

Geometry backend properties:
{
  "stat": "confirmed",
  "msg": null,
  "settings": {
    "compute": {
      "deny_script": null,
      "ftype": "ghx",
      "initial_warmup": false,
      "max_comp_time": 30000,
      "max_export_size": 536870912,
      "max_idle_minutes": 30,
      "max_model_size": 536870912,
      "max_output_size": 536870912,
      "max_texture_size": 16777216,
      "max_wait_time": 0,
      "num_loaded_max": 6,
      "num_loaded_min": 2,
      "num_preloaded_min": 0,
      "session_rate_limit": null,
      "trust": "full"
    },
    "ticket": {
      "pub": false,
      "accessdomains": [
        "www.shapediver.com",
        "shapediver.com"
      ],
      "backendaccess": false
    },
    "token": {
      "auth_groups": null,
      "require_iframe": false,
      "require_token": false
    }
  }
}

Geometry backend system:
        Alias: sdr7euc1
        Model view URL: https://sdr7euc1.eu-central-1.shapediver.com
        Description: Rhino 7, shared Geometry Backend

Bookmark: False

Decoration:
        https://sduse1-assets.shapediver.com/images/model/98ec3c3a-54a4-4655-a446-4f2ed4d87741.jpg
        https://sduse1-assets.shapediver.com/images/model/98ec3811-d14b-429f-acfe-6a4927b11da4.jpg
        https://sduse1-assets.shapediver.com/images/model/98ec3c51-157e-4658-b7ed-b2102625bfe7.jpg

User properties:
{
  "id": "f0a20eea-cccd-11eb-9aaa-0e98d64d1685",
  "username": "Alex Schiftner @ShapeDiver",
  "avatar_url": "https://sduse1-assets.shapediver.com/images/profile/Q1mD7ZPThA.jpeg",
  "slug": "alexatshapediver-com",
  "first_name": "Alexander",
  "last_name": "Schiftner",
  "visibility": "public",
  "visibility_nominal": "public",
  "policies_granted": null,
  "policies_denied": null,
  "permissions": null,
  "policies_denied_context": null
}
```




## Command [`list-models`](Commands/ListModelsCommand.cs)

This command shows how to list ShapeDiver models using the 
[Platform Backend API](https://help.shapediver.com/doc/platform-backend#PlatformBackend-PlatformBackendAPI). 

### Usage

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help list-models
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -u, --user          Filter models owned by you

  -l, --limit         Limit for the number of models to list, defaults to 10

  -o, --offset        Offset for continuing query

  -v, --visibility    Visibility of the model, one of "private", "organization", "shared", or "public"

  -q, --search        Search string, used for searching models by slug, title, and model view URL

  -k, --key_id        ShapeDiver access key id (browser based authentication will be used if not specified)

  -s, --key_secret    ShapeDiver access key secret

  --help              Display this help screen.

  --version           Display version information.
```

### Examples

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe list-models -v public -q shelf
Id;Slug;Title
984b57f9-1162-401d-aff7-f1795bbda329;youtube-shelf-outputs;YouTube Shelf Outputs
97f6b6db-c025-4da0-b0c2-52c96588e626;shelf-youtube-exports;YouTube Shelf Exports
97ea2903-d18f-4f26-9765-a671d3f36c6c;shelf-youtube;YouTube Shelf
97aa9a17-c86f-408e-bb77-5bb10508a319;bookshelf-exportable;Bookshelf, exportable
976e6854-a7a5-435b-b515-5bb3c677a6dd;geometry-backend-example;Bookshelf
9652f590-4d70-4140-8839-7860a6fe0128;base-shelf-xml-v041-1;BASE_SHELF_XML_V041
96529c61-b305-42ae-9787-fee7d5ab1ca6;base-shelf-xml-v040-3;BASE_SHELF_XML_V040
965296de-87fa-4b2d-9145-8d156915c90a;base-shelf-xml-v040;BASE_SHELF_XML_V040
9651280b-54fa-435c-906c-c28db8cfbd31;base-shelf-xml-v039;BASE_SHELF_XML_V039
965122d8-196e-4b0c-87bf-10ba6576420a;shelf-doors-v006;SHELF_DOORS_V038
Offset for continuing query: Hy2M+4WGkjt0y0ay31r+nw==
```


## Command [`text-io-demo`](Commands/TextInputOutputCommand.cs)

This command shows how to use the SDK for ShapeDiver models that support the input and output of text. 
The input and output strings can be large, up to the limit defined by your [ShapeDiver subscription](https://www.shapediver.com/pricing). 
Please upload the corresponding [Grasshopper model](Grasshopper/TextInputOutput.ghx) using your ShapeDiver account for testing. 
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

## Commands [`upload-model`](Commands/UploadCommand.cs) and [`upload-model-verbose`](Commands/UploadCommandVerbose.cs)

This command shows how to upload and publish a Grasshopper model to ShapeDiver.

### Usage

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe help upload-model
DotNetSdkSampleConsoleApp 1.0.0.0
Copyright ©  2023

  -f, --filename      Required. Path to Grasshopper model (.gh or .ghx)

  -t, --title         Title of the model on the ShapeDiver Platform

  -k, --key_id        ShapeDiver access key id (browser based authentication will be used if not specified)

  -s, --key_secret    ShapeDiver access key secret

  --help              Display this help screen.

  --version           Display version information.
```

### Examples

```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe upload-model -f ARRS_improved_materials_R6.ghx
Starting model upload and check, please wait...
Model upload and check completed.
```
(Model opens in browser)
```
C:\Users\...\DotNetSdkSampleConsoleApp\bin\Debug>DotNetSdkSampleConsoleApp.exe upload-model-verbose -f ARRS_improved_materials_R6.ghx
Create model...
Upload model...
Waiting for model check to start...
Waiting for model check to start...
Maximum allowed computation time: 30 seconds
Waiting for model check to finish...
Publishing confirmed model...
```
(Model opens in browser)
