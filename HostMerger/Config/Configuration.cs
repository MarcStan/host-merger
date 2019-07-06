namespace HostMerger.Config
{
    public class Configuration
    {
        /// <summary>
        /// Container name of the input files.
        /// Default: config
        /// </summary>
        public string ContainerName { get; set; } = "config";

        /// <summary>
        /// Name of the blob file to read. It must contain links to the hostfiles/entries to merge.
        /// Defaults to sources.json if not set.
        /// </summary>
        public string Source { get; set; } = "sources.json";

        /// <summary>
        /// Name of the whitelist file. Allows to keep entries even if some blocklists contains them.
        /// Default: whitelist.txt
        /// </summary>
        public string Whitelist { get; set; } = "whitelist.txt";

        /// <summary>
        /// If not set will use <see cref="ContainerName"/>.
        /// Allows to change the container name of the output file (e.g. to put it in a public container).
        /// </summary>
        public string OutputContainerName { get; set; }

        /// <summary>
        /// Name of the output file.
        /// Will be a valid hostfile read to be used by Windows, Pi-Hole, uBlock Origin/Matrix, DNS66, NetGuard and many other blocking solutions.
        /// Default: hosts.txt
        /// </summary>
        public string Output { get; set; } = "hosts.txt";

        /// <summary>
        /// Connection string for the storage account.
        /// </summary>
        public string AzureWebJobsStorage { get; set; }
    }
}
