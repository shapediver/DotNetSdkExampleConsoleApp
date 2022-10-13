using System;
using System.Threading.Tasks;

using ShapeDiver.SDK;
using ShapeDiver.SDK.Authentication;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.PlatformBackend.DTO;
using ShapeDiver.SDK.GeometryBackend;
using CommandLine;
using DotNetSdkSampleConsoleApp.Commands;

namespace DotNetSdkSampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<DemoCommand, object>(args)
                .WithParsed<ICommand>(t => t.Execute().Wait());
        }
    }
}
