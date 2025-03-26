using System.Diagnostics;

namespace BazaarWrapper
{
    public interface IProcessLaunchArguments
    {
        public string GetLaunchArguments(Process process);
    }
}
