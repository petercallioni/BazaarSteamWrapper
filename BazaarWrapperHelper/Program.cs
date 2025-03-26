using System.Management;
using System.Runtime.InteropServices;

namespace BazaarWraperHelper
{
    /// <summary>
    /// This program is a helper for the BazaarWrapper project. It gets the command line of a process by its ID.
    /// It needs to be run as an administrator to access the command line of other processes.
    /// </summary>
    class Program
    {
        static string helperFileName = "BazaarWrapperHelper.txt";

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: BazaarWrapperHelper.exe <ProcessID>");
                return 1;
            }

            if (!int.TryParse(args[0], out int processId))
            {
                Console.Error.WriteLine("Invalid Process ID.");
                return 1;
            }

            try
            {
                // Call your existing method to get the command line.
                string cmdLine = GetCommandLine(processId) ?? "";
                string tempFile = Path.Combine(Path.GetTempPath(), helperFileName);
                File.WriteAllText(tempFile, cmdLine);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return 1;
            }
            return 0;
        }

        public static string? GetCommandLine(int processId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string query = $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject searchObject in searcher.Get())
                    {
                        object obj = searchObject;
                        return searchObject["CommandLine"]?.ToString();
                    }
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return null;
        }
    }
}
