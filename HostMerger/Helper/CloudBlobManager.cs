using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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

        public CloudBlobManager(string connectionString, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _client = storageAccount.CreateCloudBlobClient();
            ContainerName = containerName;
        }

        public string ContainerName { get; }

        public async Task<IReadOnlyList<string>> ReadLinesAsync(string blobName)
        {
            using (var reader = await ReadAsync(blobName))
            {
                var content = await reader.ReadToEndAsync();
                return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public async Task<T> ReadAsync<T>(string blobName)
        {
            using (var reader = await ReadAsync(blobName))
            {
                var content = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        private async Task<StreamReader> ReadAsync(string blobName)
        {
            var blob = await GetBlobAsync(blobName);
            if (!await blob.ExistsAsync())
                return new StreamReader(new MemoryStream());

            var stream = await blob.OpenReadAsync();
            return new StreamReader(stream);
        }

        public async Task WriteAsync(string blobName, string content)
        {
            var blob = await GetBlobAsync(blobName);
            await blob.UploadTextAsync(content);
        }

        public Task WriteAsync(string blobName, string[] lines)
            => WriteAsync(blobName, string.Join(Environment.NewLine, lines));

        private async Task<CloudBlockBlob> GetBlobAsync(string blobName)
        {
            var container = _client.GetContainerReference(ContainerName);
            await container.CreateIfNotExistsAsync();
            return container.GetBlockBlobReference(blobName);
        }
    }
}

