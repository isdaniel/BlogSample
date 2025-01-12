using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ShareLib
{
    public class ProcessorCreator
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool dwProcessId);

        const uint CTRL_C_EVENT = 0;
        //const uint CTRL_BREAK_EVENT = 1;
        
        public Process CreateProcess(string fileName,string args) {

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = fileName,
                    Arguments = args,
                    CreateNoWindow = false, // Ensure the process is attached to a console window
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };
            process.Start();
            return process;
        }

        public void CloseProcessByBreak(Process process) {
            //FreeConsole();

            ////// Attach to the console of the target process
            //if (!AttachConsole((uint)process.Id))
            //{
            //    Console.WriteLine($"Failed to attach to process console. Error: {Marshal.GetLastWin32Error()}");
            //    return;
            //}

            SetConsoleCtrlHandler(null, true);

            //// Send the CTRL_C_EVENT to the process group
            //if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, (uint)process.Id))
            //{
            //    Console.WriteLine($"Failed to send CTRL_C_EVENT. Error: {Marshal.GetLastWin32Error()}");
            //}
            GenerateConsoleCtrlEvent(CTRL_C_EVENT, (uint)process.Id);
        }
    }

    public class ValueModel
    {
        public int Value { get; set; }
    }

}
