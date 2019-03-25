using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HostMerger.Helper
{
    public class CloudBlobManager : ICloudBlobManager
    {
        private readonly CloudBlobClient _client;

        public CloudBlobManager(string connectionString)
        {
            var storageAccount = StorageAccount.NewFromConnectionString(connectionString);
            _client = storageAccount.CreateCloudBlobClient();
        }

        public async Task<IReadOnlyList<string>> ReadLinesAsync(BlobStorageFileInfo fi)
        {
            using (var reader = await ReadAsync(fi))
            {
                var content = await reader.ReadToEndAsync();
                return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public async Task<T> ReadAsync<T>(BlobStorageFileInfo fi)
        {
            using (var reader = await ReadAsync(fi))
            {
                var content = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        private async Task<StreamReader> ReadAsync(BlobStorageFileInfo fi)
        {
            var blob = await GetBlobAsync(fi);
            var stream = await blob.OpenReadAsync();
            return new StreamReader(stream);
        }

        public async Task WriteAsync(BlobStorageFileInfo fi, string content)
        {
            var blob = await GetBlobAsync(fi);
            await blob.UploadTextAsync(content);
        }

        public Task WriteAsync(BlobStorageFileInfo fi, string[] lines)
            => WriteAsync(fi, string.Join(Environment.NewLine, lines));

        private async Task<CloudBlockBlob> GetBlobAsync(BlobStorageFileInfo fi)
        {
            var container = _client.GetContainerReference(fi.ContainerName);
            await container.CreateIfNotExistsAsync();
            return container.GetBlockBlobReference(fi.FileName);
        }
    }
}

