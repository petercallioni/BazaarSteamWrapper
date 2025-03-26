using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ElevatedCommandHelper
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: ElevatedHelper.exe <ProcessID>");
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
                string cmdLine = GetCommandLine(processId);
                string tempFile = Path.Combine(Path.GetTempPath(), "ElevatedHelperOutput.txt");
                File.WriteAllText(tempFile, cmdLine);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return 1;
            }
            return 0;
        }

        // Structure for PROCESS_BASIC_INFORMATION (partial)
        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        // Structure for UNICODE_STRING
        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        // P/Invoke declarations
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            ref PROCESS_BASIC_INFORMATION processInformation,
            uint processInformationLength,
            out uint returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            int processAccess,
            bool bInheritHandle,
            int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_READ = 0x0010;

        /// <summary>
        /// Retrieves the command line of the target process by reading its PEB.
        /// </summary>
        public static string GetCommandLine(int processId)
        {
            // Open the process with the required accesses.
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
            if (hProcess == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                // Get the process basic information.
                PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                uint returnLength = 0;
                int status = NtQueryInformationProcess(hProcess, 0 /* ProcessBasicInformation */, ref pbi, (uint)Marshal.SizeOf(pbi), out returnLength);
                if (status != 0)
                    return string.Empty;

                // pbi.PebBaseAddress points to the PEB.
                IntPtr pebAddress = pbi.PebBaseAddress;

                // The offset for the ProcessParameters pointer in the PEB:
                //   0x20 on 64-bit systems or 0x10 on 32-bit systems.
                int processParametersOffset = IntPtr.Size == 8 ? 0x20 : 0x10;
                byte[] procParamAddressBytes = new byte[IntPtr.Size];
                IntPtr bytesRead;

                if (!ReadProcessMemory(hProcess, pebAddress + processParametersOffset, procParamAddressBytes, procParamAddressBytes.Length, out bytesRead))
                    return string.Empty;

                IntPtr processParametersAddress = (IntPtr)(IntPtr.Size == 8
                    ? BitConverter.ToInt64(procParamAddressBytes, 0)
                    : BitConverter.ToInt32(procParamAddressBytes, 0));

                // In the RTL_USER_PROCESS_PARAMETERS structure, the CommandLine field is found at a specific offset.
                // Typical offsets:
                //   0x70 on 64-bit systems,
                //   0x40 on 32-bit systems.
                int commandLineOffset = IntPtr.Size == 8 ? 0x70 : 0x40;
                byte[] unicodeStringBuffer = new byte[Marshal.SizeOf(typeof(UNICODE_STRING))];

                if (!ReadProcessMemory(hProcess, processParametersAddress + commandLineOffset, unicodeStringBuffer, unicodeStringBuffer.Length, out bytesRead))
                    return string.Empty;

                // Convert the raw data into a UNICODE_STRING structure.
                GCHandle handle = GCHandle.Alloc(unicodeStringBuffer, GCHandleType.Pinned);
                UNICODE_STRING commandLineUnicode = (UNICODE_STRING)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(UNICODE_STRING));
                handle.Free();

                // Read the actual command line string from the target process memory.
                byte[] commandLineBytes = new byte[commandLineUnicode.Length];
                if (!ReadProcessMemory(hProcess, commandLineUnicode.Buffer, commandLineBytes, commandLineBytes.Length, out bytesRead))
                    return string.Empty;

                return Encoding.Unicode.GetString(commandLineBytes);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
    }
}
