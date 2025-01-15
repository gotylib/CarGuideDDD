
using Microsoft.AspNetCore.Http;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IFileManagerService
    {
        public Task UploadFileAsync(Stream fileStream, string fileName, string guid);
        public Task<IFormFile> GetFileAsync(string fileName);
        public Task<List<string>> ListFilesAsync();
    }
}
