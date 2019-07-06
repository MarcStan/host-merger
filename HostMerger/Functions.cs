using HostMerger.Config;
using HostMerger.Extensions;
using HostMerger.Helper;
using HostMerger.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HostMerger
{
    public static class Functions
    {
        /// <summary>
        /// Helper to allow running the function via API call
        /// </summary>
        /// <param name="req"></param>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("execute")]
        public static Task RunOnceAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
            => MergeHostsAsync(null, context, log);

        [FunctionName("merge")]
        public static async Task MergeHostsAsync(
            [TimerTrigger(Constants.EveryDay, RunOnStartup = true)] TimerInfo timer,
            ExecutionContext context,
            ILogger log)
        {
            var config = LoadConfig(context, log);

            // ugly setup here but ensures better testability of components..
            var merger = new HostMergerLogic(new HttpClient(new System.Net.Http.HttpClient()), log);
            var cloudBlobManager = new CloudBlobManager(config.AzureWebJobsStorage, config.ContainerName);
            var outputCloudBlobManager =
                string.IsNullOrEmpty(config.OutputContainerName) ||
                config.OutputContainerName == config.ContainerName
                    ? cloudBlobManager
                    : new CloudBlobManager(config.AzureWebJobsStorage, config.OutputContainerName);

            await merger.RunHostMergingAsync(cloudBlobManager, outputCloudBlobManager, config);
        }

        /// <summary>
        /// Helper that loads the config values from file, environment variables and keyvault in that order.
        /// Last setting wins.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        private static Configuration LoadConfig(ExecutionContext context, ILogger log)
        {
            using (log.MeasureDuration("ConfigLoading"))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                var config = new Configuration();
                var cfg = builder.Build();
                cfg.Bind(config);
                return config;
            }
        }
    }
}
