using ShareLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

class Program
{
    // P/Invoke declarations
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);

    // Delegate for handling control events
    private delegate bool ConsoleCtrlHandlerDelegate(CtrlType ctrlType);

    // Control signal types
    private enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    private static ConsoleCtrlHandlerDelegate _handler; // Keep a reference to prevent GC
    static TaskCompletionSource tcs = new TaskCompletionSource();
    static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine($"[{Process.GetCurrentProcess().Id}, {DateTime.Now:yyyy:MM:dd hh:mm:ss}] Ctrl+C received! Cleaning up!!...\r\n");
            e.Cancel = true; // Prevent default behavior if necessary
            tcs.SetResult();
            Console.WriteLine($"[{Process.GetCurrentProcess().Id}, {DateTime.Now:yyyy:MM:dd hh:mm:ss}] Ctrl+C received! end!!...\r\n");
        };

        int currVal = 0;
        while (!tcs.Task.IsCompleted) {
            var inputMessage  = Console.ReadLine();
            var model = JsonSerializer.Deserialize<ValueModel>(inputMessage);
            //mock logical
            Thread.Sleep(1000);
            currVal = model.Value;
            Console.WriteLine($"[{DateTime.Now:yyyy:MM:dd hh:mm:ss}] model.val:{model.Value}");
        }
        Console.WriteLine("wait for task Completed");
        await tcs.Task;
        await File.WriteAllLinesAsync("./curVal.txt", new string[] { currVal.ToString() });
        Console.WriteLine("Task Is TaskCompleted");
    }

    private static bool ConsoleCtrlHandler(CtrlType ctrlType)
    {
        switch (ctrlType)
        {
            case CtrlType.CTRL_C_EVENT:
                File.AppendAllTextAsync("./log.txt", $"[{Process.GetCurrentProcess().Id}] Ctrl+C received! Cleaning up!!...\r\n");
                // Perform cleanup or graceful shutdown
                tcs.SetResult();
                return true; // Signal handled

            case CtrlType.CTRL_CLOSE_EVENT:
                Console.WriteLine("Console is closing...");
                // Perform cleanup if necessary
                return true;

            case CtrlType.CTRL_LOGOFF_EVENT:
            case CtrlType.CTRL_SHUTDOWN_EVENT:
                Console.WriteLine("System is shutting down...");
                return true;

            default:
                return false; // Pass to default handler
        }
    }
}