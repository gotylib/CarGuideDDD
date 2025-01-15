
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Reactive.Linq;

namespace CarGuideDDD.Infrastructure.Services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IMinioClient _minioClient;
        private readonly string bucketName = "photostocar";
        private readonly string endpoint = "minio:9000";
        private readonly string accessKey = "Gleb";
        private readonly string secretKey = "Az100Az.";
        public FileManagerService()
        {
              _minioClient = new MinioClient()
             .WithEndpoint(endpoint)
             .WithCredentials(accessKey, secretKey)
             .Build();

                    var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(bucketName);

            // Ensure the bucket exists
            if (!_minioClient.BucketExistsAsync(bucketExistsArgs).GetAwaiter().GetResult())
            {
                var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(bucketName);
                _minioClient.MakeBucketAsync(makeBucketArgs).GetAwaiter().GetResult();
            }
        }


        public async Task<IFormFile> GetFileAsync(string fileName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                          .WithBucket(bucketName)
                                          .WithObject(fileName)
                                          .WithCallbackStream((stream) =>
                                          {
                                              stream.CopyTo(memoryStream);
                                          });

                await _minioClient.GetObjectAsync(getObjectArgs);

                memoryStream.Position = 0;
                // Создаем IFormFile
                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = $"application/{fileName.Split('.')[1]}"
                };
                return formFile;
            }
            
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task UploadFileAsync(Stream fileStream, string fileName, string guid)
        {
            try
            {
                var type = $"{guid}.{fileName.Split('.')[1]}";
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(guid)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType("image/jpeg");

                await _minioClient.PutObjectAsync(putObjectArgs);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e);
            }
        }
        public async Task<List<string>> ListFilesAsync()
        {
            var fileNames = new List<string>();
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true);

            var observable = _minioClient.ListObjectsAsync(listObjectsArgs);
            var enumerator = observable.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                fileNames.Add(item.Key);
            }

            return fileNames;
        }
    }
}

    
