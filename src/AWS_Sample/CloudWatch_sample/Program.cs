using System;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using LogLevel = NLog.LogLevel;

namespace CloudWatch_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            

            var config = new LoggingConfiguration();

            var awsTarget = new AWSTarget()
            {
                LogGroup = ".netcore-group",
                Region = "ap-northeast-1",
                Layout = "${message}",
                LogStreamNameSuffix = "Daniel-logstream"
            };
            config.AddTarget("aws", awsTarget);

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, awsTarget));

            LogManager.Configuration = config;
            
            //write your log
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello World by Daniel");
        }
    }
}
