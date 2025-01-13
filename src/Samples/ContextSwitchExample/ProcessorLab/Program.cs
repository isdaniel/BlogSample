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
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;


class Program
{


    static void Main(string[] args)
    {
        ProcessorCreator processor = new ProcessorCreator();
        List<Task> tasks = new List<Task>();
        for (int v = 0; v < 2; v++)
        {
            int k = v;
            var task = Task.Run(() => {
                var process = processor.CreateProcess("dotnet", "./ExampleWorkerApp/ExampleWorker.dll");
                Console.WriteLine($"Started task process with ID: {process.Id}");
                //string pipeLineName = $"{Guid.NewGuid().ToString("N")}_{process.Id}";
                string pipeLineName = $"PipeName_{k}";
                int isClose = 0;
                Task.Delay(8000).ContinueWith(async (task) =>
                {
                    Interlocked.Exchange(ref isClose, 1);
                    await SendingCloseSignal(pipeLineName);
                });
                Console.WriteLine(pipeLineName);
                process.StandardInput.WriteLine(pipeLineName);

                int i = 0;

                if (File.Exists($"././{pipeLineName}.txt") && int.TryParse(File.ReadAllText($"./{pipeLineName}.txt"), out var val))
                {
                    i = val + 1;
                }

                while (Interlocked.CompareExchange(ref isClose, 1, 1) == 0)
                {
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
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
    }

    private static async Task SendingCloseSignal(string pipeName)
    {
        using (var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
        {
            Console.WriteLine("Waiting for ProcessorLab to connect...");
            server.WaitForConnection();
            Console.WriteLine("Sending context switch singal...");
            using (var reader = new StreamReader(server))
            using (var writer = new StreamWriter(server))
            {
                writer.AutoFlush = true;
                await writer.WriteLineAsync("context-switch");
                var res = await reader.ReadLineAsync();
                Console.WriteLine($"res from SendingCloseSignal: {res}");
            }

            Console.WriteLine("end context switch singal...");
        }

        //Console.WriteLine("Sending CTRL_C_EVENT...");
        //processor.CloseProcessByBreak(process);

        //await process.WaitForExitAsync();
        //// Check if the process is still running
        //if (!process.HasExited)
        //{
        //    Console.WriteLine("Process is still running.");
        //}
        //else
        //{
        //    Console.WriteLine("Process has exited.");
        //}

        //Console.WriteLine("process WaitForExitAsync");
    }
}
