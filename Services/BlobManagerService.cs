using SoloX.BlazorJsBlob;

namespace ComicsBlazor.Services;

public class BlobManagerService(IBlobService blobService)
{
    private readonly IBlobService blobService = blobService;

    private Dictionary<string, IBlob> Blobs = new Dictionary<string, IBlob>();

    public async Task<IBlob> AddBlobAsync(string key, string content, string type)
    {
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
        {
            return await AddBlobAsync(key, stream, type);
        }
    }

        public async Task<IBlob> AddBlobAsync(string key, byte[] content, string type)
    {
        using (var stream = new MemoryStream(content))
        {
            return await AddBlobAsync(key, stream, type);
        }
    }
    public async Task<IBlob> AddBlobAsync(string key, Stream stream, string type)
    {
        if (Blobs.ContainsKey(key))
            return Blobs[key];

        var blob = await blobService.CreateBlobAsync(stream, type);
        Blobs[key] = blob;
        return blob;
    }
}