using System.Diagnostics;

namespace BazaarWrapper
{
    public class GameMonitor
    {
        /// <summary>
        /// Monitors for the game process, returns it and also the launch options used to start it.
        /// </summary>
        /// <param name="startArguments"></param>
        /// <returns></returns>
        public GameProcess MonitorForGameProcess(IProcessLaunchArguments startArguments)
        {
            Process? gameProcess = null;

            // Polling loop: Check every second for the game process
            while (gameProcess == null)
            {
                // Get the game process
                Process[] processes = Process.GetProcessesByName(Constants.BAZAAR_EXE);
                if (processes.Length > 0)
                    gameProcess = processes[0];
                else
                    Thread.Sleep(1000);
            }

            // Call the helper to get the command line
            string longArgs = startArguments.GetLaunchArguments(gameProcess);

            return new GameProcess(gameProcess, ParseCommandLine(longArgs));
        }

        /// <summary>
        /// Parses a command line string into an executable path and arguments.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="executablePath"></param>
        /// <param name="arguments"></param>
        /// <exception cref="ArgumentException"></exception>
        private GameLaunchParameters ParseCommandLine(string command)
        {
            string executablePath;
            string arguments;

            // Handle cases where the path might be quoted
            int firstSpaceIndex = command.IndexOf(' ');
            if (firstSpaceIndex > 0 && command[0] == '"')
            {
                // Path is quoted; find the closing quote
                int closingQuoteIndex = command.IndexOf('"', 1);
                if (closingQuoteIndex > 0)
                {
                    executablePath = command.Substring(1, closingQuoteIndex - 1);
                    arguments = command.Substring(closingQuoteIndex + 1).TrimStart();
                }
                else
                {
                    throw new ArgumentException("Invalid command format: missing closing quote.");
                }
            }
            else if (firstSpaceIndex > 0)
            {
                // Path is not quoted
                executablePath = command.Substring(0, firstSpaceIndex);
                arguments = command.Substring(firstSpaceIndex + 1).TrimStart();
            }
            else
            {
                // No arguments; the entire command is just the executable path
                executablePath = command;
                arguments = string.Empty;
            }

            return new GameLaunchParameters(executablePath, arguments);
        }
    }
}
