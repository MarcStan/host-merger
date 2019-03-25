using System.Collections.Generic;
using System.Threading.Tasks;

namespace HostMerger.Helper
{
    public interface ICloudBlobManager
    {
        Task<IReadOnlyList<string>> ReadLinesAsync(BlobStorageFileInfo fi);

        Task<T> ReadAsync<T>(BlobStorageFileInfo fi);

        Task WriteAsync(BlobStorageFileInfo fi, string content);

        Task WriteAsync(BlobStorageFileInfo fi, string[] lines);
    }
}
