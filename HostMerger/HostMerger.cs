using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HostMerger
{
    public static class HostMerger
    {
        [FunctionName("HostMerger")]
        public static async Task HostMergerAsync(
            [TimerTrigger(Constants.EveryDay)] TimerInfo timer,
            ExecutionContext context,
            ILogger log)
        {
            var config = LoadConfig(context, log);

            var merger = new HostMergerLogic();

            await merger.RunHostMergingAsync(config);
        }

        /// <summary>
        /// Helper that loads the config values from file, environment variables and keyvault in that order.
        /// Last setting wins.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        private static Configuration LoadConfig(ExecutionContext context, ILogger log)
        {
            using (var init = log.BeginScope("ConfigLoading"))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                var tmpConfig = builder.Build();

                var tokenProvider = new AzureServiceTokenProvider();
                var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));
                builder.AddAzureKeyVault($"https://{tmpConfig["KeyVaultName"]}.vault.azure.net", kvClient, new DefaultKeyVaultSecretManager());

                var config = new Configuration();
                var cfg = builder.Build();
                cfg.Bind(config);
                return config;
            }
        }
    }
}
