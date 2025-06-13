using System.Threading.Tasks;

namespace DotNetSdkSampleConsoleApp
{
    internal interface ICommand
    {
        Task Execute();
    }
}
