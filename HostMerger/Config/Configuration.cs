namespace HostMerger.Config
{
    public class Configuration
    {
        public string ContainerName { get; set; }

        public string Sources { get; set; }

        public string Whitelist { get; set; }

        public string Output { get; set; }

        public string AzureWebJobsStorage { get; set; }
    }
}
