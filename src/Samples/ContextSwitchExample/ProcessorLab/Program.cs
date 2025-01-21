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


using MessagePack;
using MessagePack.Resolvers;
using ShareLib;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;


class Program
{
    /// <summary>
    /// Encapsulate message from MQ service
    /// </summary>
    [MessagePackObject]
    public class MessageOutputTask
    {
        /// <summary>
        /// Output message from process
        /// </summary>
        [Key("0")]
        public string Message { get; set; }
        [Key("1")]
        public int Status { get; set; }
        /// <summary>
        /// Reply information that we want to store for continue execution message.
        /// </summary>
        [Key("2")]
        [MessagePackFormatter(typeof(PrimitiveObjectResolver))]
        public IDictionary<string, object> Headers { get; set; }
        /// <summary>
        /// Default use BasicProperties.Reply To queue name, task processor can overwrite reply queue name.
        /// </summary>
        /// <value>Default use BasicProperties.Reply</value>
        [Key("3")]
        public string ReplyQueueName { get; set; }
    }

    [MessagePackObject]
    public class ValueModel
    {
        [Key(0)]
        public int Value { get; set; }

        [Key(1)]
        public string Name { get; set; }
    }

    //NamedPipeClientStream _operationPipe;
    //NamedPipeClientStream _dataPipe;
    private static CancellationToken CreateMessageCancellationToken(int timeoutMilliseconds)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationTokenSource cts1 = new CancellationTokenSource();
        cts.CancelAfter(timeoutMilliseconds < 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeoutMilliseconds));
        var tokens = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cts1.Token);
        return tokens.Token;
    }

    static async Task Main(string[] args)
    {
        /*
         * {
  "0": "New OutPut Message!",
  "1": "MessageDone",
  "2": null,
  "3": null
}
         */
        var aaa = new MessageOutputTask() {
            Headers = new Dictionary<string, object> {
                { "test", "testval"}
            },
            Message = "New OutPut Message!",
            Status = 200,
            ReplyQueueName = "testQueue"
        };
        var data = MessagePackSerializer.Serialize(aaa);
        Console.WriteLine($"[{string.Join(",", data.Select(x => x.ToString()))}]");
        string dataPipeName = $"workerPipe_data_test";
        int i = 0;
        using (var server = new NamedPipeServerStream(dataPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0))
        using (var pipeStream = new PipeStreamWrapper(server))
        {
            //ProcessorCreator processor = new ProcessorCreator();
            //var process = processor.CreateProcess("dotnet", @$"C:\gitRepo\MessageWorkerPool\examples\rust_examples\ClientSamlpe\target\debug\ClientSamlpe.exe");
            //await process.StandardInput.WriteAsync(dataPipeName);

            await server.WaitForConnectionAsync();
            //using (var memoryStream = new MemoryStream()) {
            while (true)
            {

                var obj = new ValueModel()
                {
                    Name = $"Daniel_{i}",
                    Value = i
                };
                await pipeStream.WriteAsync(obj);
                //await WriteToStreamAsync(server, buffer);
                Console.WriteLine($"Input: {i}");
                //await server.WriteAsync(buffer, 0, buffer.Length);
                var res = await pipeStream.ReadAsync<ValueModel>();
                Console.WriteLine(res);
                i++;
            }
        }

        //var ss = MessagePackSerializer.Deserialize<ValueModel>(new byte[] { 145, 5 });
        //ProcessorCreator processor = new ProcessorCreator();
        //List<Task> tasks = new List<Task>();
        //for (int v = 0; v < 1; v++)
        //{
        //    int k = v;
        //    var task = Task.Run(async () =>
        //    {
        //        string operationPipeName = $"workerPipe_ops_{k}";
        //        string dataPipeName = $"workerPipe_data_{k}";

        //        var process = processor.CreateProcess("dotnet", @$"C:\gitRepo\BlogSample\src\Samples\ContextSwitchExample\WorkerProcess\ExampleWorkerApp\ExampleWorker.dll {operationPipeName} {dataPipeName}");
        //        Console.WriteLine($"Started task process with ID: {process.Id}");
        //        //string pipeLineName = $"{Guid.NewGuid().ToString("N")}_{process.Id}";
        //        int isClose = 0;
        //        Task.Delay(30000).ContinueWith(async (task) =>
        //        {
        //            Interlocked.Exchange(ref isClose, 1);
        //            await SendingCloseSignal(operationPipeName);
        //        });

        //        Console.WriteLine($"[{process.Id}] operationPipeName : {operationPipeName}, dataPipeName : {dataPipeName}");

        //        int i = 0;

        //        using (var server = new NamedPipeServerStream(dataPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0))
        //        using (var pipeStream = new PipeStreamWrapper(server))
        //        {
        //            Console.WriteLine($"[{process.Id}] Waiting for client to connect...");
        //            await server.WaitForConnectionAsync();
        //            Console.WriteLine($"[{process.Id}] Client connected.");
        //            //using (var memoryStream = new MemoryStream()) {
        //            while (Interlocked.CompareExchange(ref isClose, 1, 1) == 0)
        //            {
        //                //byte[] buffer = MessagePackSerializer.Serialize(new ValueModel()
        //                //{
        //                //    Value = i
        //                //});



        //                await pipeStream.WriteAsync(i);
        //                //await WriteToStreamAsync(server, buffer);
        //                Console.WriteLine($"Input: {i}");
        //                //await server.WriteAsync(buffer, 0, buffer.Length);
        //                var res = await pipeStream.ReadAsync<int>();
        //                //                        var outputBuffer = await ReadFromStreamAsync(server);
        //                //var outputModel = MessagePackSerializer.Deserialize<ValueModel>(outputBuffer);
        //                Console.WriteLine(res);
        //                //var len = ReadLength(server);
        //                //byte[] buffer2 = new byte[len];
        //                //await server.ReadAsync(buffer2, 0, len);
        //                //var resModel = MessagePackSerializer.Deserialize<ValueModel>(buffer2);
        //                //Console.WriteLine($"Loop Read: {await server.ReadLineAsync()}");
        //                i++;
        //                //memoryStream.SetLength(0);
        //            }
        //            //}

        //            //using (var reader = new BinaryReader(server))
        //            //using (var writer = new BinaryWriter(server))
        //            //{

        //            //    while (Interlocked.CompareExchange(ref isClose, 1, 1) == 0)
        //            //    {
        //            //        byte[] buffer = MessagePackSerializer.Serialize(new ValueModel()
        //            //        {
        //            //            Value = i
        //            //        });

        //            //        Console.WriteLine($"Input: {i}");
        //            //        writer.WriteLine(buffer);
        //            //        Console.WriteLine($"Loop Read: {await reader.ReadLineAsync()}");
        //            //        i++;
        //            //    }

        //            //   //Console.WriteLine($"IsConnected : {server.IsConnected}, IsConnected : {server.IsAsync}, CanRead : {server.CanRead}");
        //            //}
        //        }

        //    });
        //    tasks.Add(task);
        //}

        //await Task.WhenAll(tasks.ToArray());
    }

    private static async Task<byte[]> ReadFromStreamAsync(Stream stream)
    {
        // Read the payload length (4 bytes for an int)
        var lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
        int payloadLength = BitConverter.ToInt32(lengthBuffer, 0);

        // Read the payload based on its length
        var payloadBuffer = new byte[payloadLength];
        await stream.ReadAsync(payloadBuffer, 0, payloadLength);
        return payloadBuffer;
    }

    private static async Task WriteToStreamAsync(Stream stream, byte[] data)
    {
        // Write the length of the payload
        byte[] lengthBuffer = BitConverter.GetBytes(data.Length);
        await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length);

        // Write the actual payload
        await stream.WriteAsync(data, 0, data.Length);
        await stream.FlushAsync();
    }


    private static int ReadLength(NamedPipeServerStream stream)
    {
        const int lensize = sizeof(int);
        var lenbuf = new byte[lensize];
        var bytesRead = stream.Read(lenbuf, 0, lensize);
        if (bytesRead == 0)
        {
            return 0;
        }
        if (bytesRead != lensize)
            throw new IOException(string.Format("Expected {0} bytes but read {1}", lensize, bytesRead));
        return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenbuf, 0));
    }

    private static NamedPipeServerStream CreatePipe(string pipeName) {
        return new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0);
    }

    private static async Task SendingCloseSignal(string pipeName)
    {
        using (var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0))
        {
            server.WaitForConnection();
            Console.WriteLine("Sending context switch singal...");
            using (var writer = new StreamWriter(server))
            {
                writer.AutoFlush = true;
                await writer.WriteLineAsync("context-switch");
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
