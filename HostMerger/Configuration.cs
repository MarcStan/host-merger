namespace HostMerger
{
    public class Configuration
    {
        public BlobStorageFileInfo Sources { get; set; }

        public BlobStorageFileInfo Cache { get; set; }

        public BlobStorageFileInfo Output { get; set; }

        public string AzureWebJobsStorage { get; set; }
    }

    public class BlobStorageFileInfo
    {
        public string ContainerName { get; set; }

        public string FileName { get; set; }
    }
}
