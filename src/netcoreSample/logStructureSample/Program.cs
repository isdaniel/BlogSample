using System;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace logStructureSample
{
    class Program
    {
        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new JsonFormatter(renderMessage: true))
                //.WriteTo.Seq("http://localhost:5341/")
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .CreateLogger();
            
            var host = Host.CreateDefaultBuilder(args)
                        .UseSerilog()
                        .Build();
            var data = new[] { 1, 2, 3 };
            Log.Information("Received {Data}", data);
            Log.Information("Received {@Data}", data);
            host.Run();
        }
    }
}
