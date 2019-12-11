using System.Collections.Generic;
using System.Threading.Tasks;

namespace HostMerger.Helper
{
    public interface ICloudBlobManager
    {
        string ContainerName { get; }

        Task<IReadOnlyList<string>> ReadLinesAsync(string blobName);

        Task<T> ReadAsync<T>(string blobName);

        Task WriteAsync(string blobName, string content);

        Task WriteAsync(string blobName, string[] lines);
    }
}
