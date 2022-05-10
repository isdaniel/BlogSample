using Amazon.Lambda.S3Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AwsDotnetCsharp;
public class Handler
{

    private ServiceProvider _serviceProvider;
    private IConfiguration _configuration;
    private ILogger<Handler> _logger;
    public Handler()
    {
        var serviceCollection = new ServiceCollection();
        this.ConfigureServices(serviceCollection);
        this._serviceProvider = serviceCollection.BuildServiceProvider();
        this._logger = _serviceProvider.GetRequiredService<ILogger<Handler>>();
    }

    private void ConfigureServices(IServiceCollection serviceCollection)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(new JsonFormatter(renderMessage: true))
            .CreateLogger();

        serviceCollection.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
    }
    public string Hello(S3Event evnt, ILambdaContext context)
    {
        var s3Event = evnt.Records?[0]?.S3;
        if (s3Event == null)
        {
            return null;
        }
        _logger.LogInformation($"Data Body：{JsonConvert.SerializeObject(s3Event)}");
        
        return "Go Serverless v1.0! Your function executed successfully!";
    }
}
