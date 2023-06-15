using CommandLine;
using DotNetSdkSampleConsoleApp.Commands;

namespace DotNetSdkSampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parser.Default.ParseArguments<DemoCommand, object>(args)
            //    .WithParsed<ICommand>(t => t.Execute().Wait());
            Parser.Default.ParseArguments<DemoCommand, TextInputOutputCommand, UploadCommandVerbose, UploadCommand>(args)
                .WithParsed<DemoCommand>(t => t.Execute().Wait())
                .WithParsed<TextInputOutputCommand>(t => t.Execute().Wait())
                .WithParsed<UploadCommandVerbose>(t => t.Execute().Wait())
                .WithParsed<UploadCommand>(t => t.Execute().Wait());

        }
    }
}
