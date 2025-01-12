//using ShareLib;

//ProcessorCreator processorCreator = new ProcessorCreator();
//var ps = processorCreator.CreateProcess("dotnet", "C:\\Users\\danielshih.REDMOND\\source\\repos\\ProcessorLab\\net6.0\\ExampleWorker.dll");
//var task = Task.Run(async () => {
//    var message = await ps.StandardOutput.ReadLineAsync();
//    Console.WriteLine(message);
//});
//Console.WriteLine($"wait for 5s, and shutdown pid : {ps.Id}");
//processorCreator.CloseProcessByCTRL(ps.Id);
//while (true) {
//    var pid = Console.ReadLine();
//    processorCreator.CloseProcessByCTRL(int.Parse(pid));
//    Console.WriteLine($"Executed CloseProcessByCTRL: {pid}");
//}


using ShareLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;


class Program
{


    static void Main(string[] args)
    {
        ProcessorCreator processor = new ProcessorCreator();
        var process = processor.CreateProcess("dotnet", "..\\ExampleWorker.dll");

        Console.WriteLine($"Started task process with ID: {process.Id}");
        int isClose = 0;
        Task.Delay(8000).ContinueWith(async (task) =>
        {
            Interlocked.Exchange(ref isClose, 1);
            await SendingCloseSignal(processor, process);
        });

        int i = 0;

        if (File.Exists("./curVal.txt") && int.TryParse(File.ReadAllText("./curVal.txt"), out var val))
        {
            i = val + 1;
        }

        while (Interlocked.CompareExchange(ref isClose, 1, 1) == 0) {
            var input = JsonSerializer.Serialize(new ValueModel()
            {
                Value = i
            });
            Console.WriteLine($"Input: {i}");
            process.StandardInput.WriteLine(input);
            Console.WriteLine($"Loop Read: {process.StandardOutput.ReadLine()}");
            i++;
        }

        Console.WriteLine($"Loop Out Read: {process.StandardOutput.ReadToEnd()}");
    }

    private static async Task SendingCloseSignal(ProcessorCreator processor, Process process)
    {
        Console.WriteLine("Sending CTRL_C_EVENT...");
        processor.CloseProcessByBreak(process);

        await process.WaitForExitAsync();
        // Check if the process is still running
        if (!process.HasExited)
        {
            Console.WriteLine("Process is still running.");
        }
        else
        {
            Console.WriteLine("Process has exited.");
        }

        Console.WriteLine("process WaitForExitAsync");
    }
}