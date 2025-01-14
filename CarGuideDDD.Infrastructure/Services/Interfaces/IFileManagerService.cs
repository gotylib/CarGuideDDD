
namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IFileManagerService
    {
        public Task UploadFileAsync(Stream fileStream, string fileName, string guid);
        public Task<Stream> GetFileAsync(string fileName);
        public Task<List<string>> ListFilesAsync();
    }
}
