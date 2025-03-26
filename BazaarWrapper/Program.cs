using System.Diagnostics;

namespace BazaarWrapper
{
    class Program
    {
        public static void Main(string[] args)
        {
            Go(args);
        }

        public static async void Go(string[] args)
        {
            // Define your game executable path and long command-line parameters
            string launcherExe = @"C:\Program Files\Tempo Launcher - Beta\Tempo Launcher - Beta.exe";
            string gameExe = @"E:\Games\TheBazaar\bazaarwinprodlatest\TheBazaar.exe";

            ProcessStartInfo launcherInfo = new ProcessStartInfo
            {
                FileName = launcherExe,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process launcherProcess = null;

            try
            {
                launcherProcess = Process.Start(launcherInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Launcher failed to start: " + ex.Message);
                return;
            }

            Task<string> getLaunchString = new Task<string>(() =>
            {
                return MonitorForGameProcess(launcherProcess, gameExe);
            });

            getLaunchString.Start();

            getLaunchString.Wait();

            // 5. Relaunch the game with the captured parameters.
            ProcessStartInfo gameStartInfo = new ProcessStartInfo
            {
                FileName = gameExe,
                Arguments = getLaunchString.Result,
                UseShellExecute = false,
            };

            Process newGameProcess = null;

            try
            {
                newGameProcess = Process.Start(gameStartInfo);
                Console.WriteLine("Game relaunched successfully.");
                newGameProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error relaunching the game: " + ex.Message);
            }

            // Optionally block the main thread until a desired exit condition.
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();


        }
        static string MonitorForGameProcess(Process launcherProcess, string gameExe)
        {
            const string gameProcessName = "TheBazaar"; // without extension
            Process gameProcess = null;

            // Polling loop: Check every second
            while (gameProcess == null)
            {
                Process[] processes = Process.GetProcessesByName(gameProcessName);
                if (processes.Length > 0)
                    gameProcess = processes[0];
                else
                    Thread.Sleep(1000);
            }

            Console.WriteLine("Game process detected. Attempting to read command line...");

            try
            {
                if (launcherProcess != null && !launcherProcess.HasExited)
                {
                    launcherProcess.Kill();
                    launcherProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error killing the launcher process: " + ex.Message);
            }

            // 3. Retrieve the command-line arguments.
            //string longArgs = ProcessCommandLineReader.GetCommandLine(gameProcess.Id);
            string longArgs = RetrieveCommandLine(gameProcess.Id);
            Console.WriteLine("Intercepted parameters: " + longArgs);

            // 4. Kill the game and launcher processes.
            try
            {
                if (!gameProcess.HasExited)
                {
                    gameProcess.Kill();
                    gameProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error killing the game process: " + ex.Message);
            }

            return longArgs;
        }

        public static string RetrieveCommandLine(int targetProcessId)
        {
            string helperExePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ElevatedCommandHelper.exe");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = helperExePath,
                Arguments = targetProcessId.ToString(),
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            Process proc = Process.Start(psi);
            proc.WaitForExit();

            // After the helper completes, read the file.
            string tempFile = Path.Combine(Path.GetTempPath(), "ElevatedHelperOutput.txt");

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
