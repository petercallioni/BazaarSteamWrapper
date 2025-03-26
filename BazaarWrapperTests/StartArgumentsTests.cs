using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BazaarWrapper.Tests
{
    [TestClass()]
    public class StartArgumentsTests
    {
        [TestMethod()]
        public void GetStartArgumentsTest()
        {
            // Sample string <path>TheBazaar.exe -screen-width <width> -screen-height <height> -logintoken=<string>

            const string gameProcessName = "TheBazaar";
            Process gameProcess = Process.GetProcessesByName(gameProcessName)[0];

            ProcessHelperExecutor startArguments = new ProcessHelperExecutor(true);
            string actualArguments = startArguments.GetLaunchArguments(gameProcess);

            Regex regex = new Regex(@".*TheBazaar.exe -screen-width \S+ -screen-height \S+ -logintoken=\S+ .*");
            Assert.IsTrue(regex.IsMatch(actualArguments));
        }
    }
}