using CommandLine;
using DotNetSdkSampleConsoleApp.Commands;
using System.Linq;
using System.Reflection;

namespace DotNetSdkSampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var types = Assembly.GetAssembly(typeof(Program)).GetTypes()
                .Where(t => (typeof(ICommand)).IsAssignableFrom(t) && !t.IsInterface)
                .ToArray();

            Parser.Default.ParseArguments(args, types)
                .WithParsed<ICommand>(t => t.Execute().Wait());

        }
    }
}
