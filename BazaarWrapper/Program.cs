using System.Runtime.InteropServices;

namespace BazaarWrapper
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Usage: BazaarWrapper.exe <launcher exe>");
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("This application runs on Windows only.");
            }

            MainController mainController = new MainController(args[0]);
            mainController.Start();
        }
    }
}
