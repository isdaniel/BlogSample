using MessagePack;
using ShareLib;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

class Program
{
    static async Task Main(string[] args)
    {
        //NamedPipeClientStream _operationPipe;
        //NamedPipeClientStream _dataPipe;
        string operationPipeName = args[0];
        string dataPipeName = args[1];
        //await Task.Delay(12000);
        var task = Task.Run(async () => {
            using (var client = new NamedPipeClientStream(".", operationPipeName, PipeDirection.InOut,PipeOptions.Asynchronous))
            {
                await client.ConnectAsync();
                using (var reader = new StreamReader(client))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null && line != "context-switch")
                    {
                    }
                }
            }
        });

        int currVal = 0;

        using (var client = new NamedPipeClientStream(".", dataPipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough))
        using (var pipeStream = new PipeStreamWrapper(client))
        {
            await client.ConnectAsync();
            
            while (!task.IsCompleted)
            {
                //var buffer = await ReadFromStreamAsync(client);
                //var resModel = MessagePackSerializer.Deserialize<ValueModel>(buffer);
                var res = await pipeStream.ReadAsync<int>(); 
                Thread.Sleep(1000);
                res += 100;
                //await WriteToStreamAsync(client, MessagePackSerializer.Serialize(resModel));
                await pipeStream.WriteAsync(res);
            }
            
           

            await task;
        }

        Console.WriteLine();
    }

    private static async Task<byte[]> ReadFromStreamAsync(Stream stream)
    {
        // Read the payload length (4 bytes for an int)
        var lengthBuffer = new byte[4];
        await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
        int payloadLength = BitConverter.ToInt32(lengthBuffer, 0);

        // Read the payload based on its length
        var payloadBuffer = new byte[payloadLength];
        int bytesRead = 0;
        while (bytesRead < payloadLength)
        {
            bytesRead += await stream.ReadAsync(payloadBuffer, bytesRead, payloadLength - bytesRead);
        }

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
}
