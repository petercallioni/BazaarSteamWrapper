using System.ComponentModel;
using System.Diagnostics;

namespace BazaarWrapper
{
    public class MainController
    {
        string launcherExe;

        public MainController(string launcherExe)
        {
            this.launcherExe = launcherExe;
        }

        public void Start()
        {
            Process launcher = StartLauncher();

            // Waits until the game process is found
            GameProcess game = StartGameMonitor();

            //Kill the launcher and game processes
            launcher.Kill();
            game.Process.Kill();

            // Parse the command line to get the game executable and arguments
            GameLaunchParameters gameLaunchParameters = game.GameLaunchParameters;

            // Restart the game under our process; wait 1 second before restarting to ensure the game process is fully terminated
            Thread.Sleep(1000);
            RestartGame(gameLaunchParameters.ExePath, gameLaunchParameters.LaunchArgs);
        }

        /// <summary>
        /// Restarts the game process.
        /// </summary>
        /// <param name="gameExe"></param>
        /// <param name="gameArgs"></param>
        /// <exception cref="Exception"></exception>
        private void RestartGame(string gameExe, string gameArgs)
        {
            ProcessStartInfo gameStartInfo = new ProcessStartInfo
            {
                FileName = gameExe,
                Arguments = gameArgs,
                UseShellExecute = false,
            };

            Process? newGameProcess = null;

            newGameProcess = Process.Start(gameStartInfo);

            if (newGameProcess == null)
            {
                throw new Exception("Game failed to start.");
            }

            newGameProcess.WaitForExit();
        }

        /// <summary>
        /// Starts the game monitor process. It will kill the existing game process when found.
        /// </summary>
        /// <returns></returns>
        private GameProcess StartGameMonitor()
        {
            Task<GameProcess> getLaunchString = new Task<GameProcess>(() =>
            {
                GameMonitor GameMonitor = new GameMonitor();
                return GameMonitor.MonitorForGameProcess(new ProcessHelperExecutor());
            });

            getLaunchString.Start();

            getLaunchString.Wait();

            return getLaunchString.Result;
        }

        /// <summary>
        /// Starts the launcher process.
        /// </summary>
        /// <param name="asAdmin"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Process StartLauncher(bool asAdmin = false)
        {
            ProcessStartInfo launcherInfo = new ProcessStartInfo();
            launcherInfo.FileName = launcherExe;

            if (asAdmin)
            {
                launcherInfo.UseShellExecute = true;
                launcherInfo.Verb = "runas";
            }

            Process? launcherProcess = null;

            try
            {
                // Attempt to start the launcher
                launcherProcess = Process.Start(launcherInfo);
            }
            catch (Win32Exception)
            {
                if (asAdmin)
                {
                    // Already tried with admin privileges; rethrow the exception
                    throw;
                }

                // Try again with admin privileges
                launcherProcess = StartLauncher(true);
            }

            if (launcherProcess == null)
            {
                throw new Exception("Launcher failed to start.");
            }

            return launcherProcess;
        }
    }
}
