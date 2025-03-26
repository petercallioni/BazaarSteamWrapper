using System.Diagnostics;

namespace BazaarWrapper
{
    public class ProcessHelperExecutor : IProcessLaunchArguments
    {
        string helperExePath;
        string workingDirectory;

        public ProcessHelperExecutor(bool fromTest = false)
        {
            workingDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // If running from a test, the working directory is different.
            if (fromTest)
            {
                workingDirectory = workingDirectory.Replace("BazaarWrapperTests", "BazaarWrapper");
            }

            helperExePath = Path.Combine(workingDirectory, Constants.HELPER_EXE);
        }

        // This method will launch the helper process and wait for it to complete.
        public string GetLaunchArguments(Process process)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = helperExePath,
                Arguments = process.Id.ToString(),
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            Process? proc = Process.Start(psi);

            if (proc == null)
            {
                throw new Exception("Helper process failed to start.");
            }

            proc.WaitForExit();

            // After the helper completes, read the file.
            string tempFile = Path.Combine(Path.GetTempPath(), Constants.HELPER_TXT);

            if (File.Exists(tempFile))
            {
                string output = File.ReadAllText(tempFile);
                File.Delete(tempFile); // Clean up if desired.
                return output.Trim();
            }
            else
            {
                throw new Exception("Output file was not created.");
            }
        }
    }
}