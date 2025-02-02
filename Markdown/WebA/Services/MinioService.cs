using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Minio.ApiEndpoints;
using Minio.Exceptions;
using System.Reactive.Linq;

public class MinioService
{
    public readonly MinioClient _minioClient;
    private readonly string _bucketNamePrefix;

    public MinioService(IConfiguration configuration)
    {
        var minioConfig = configuration.GetSection("Minio");
        _minioClient = (MinioClient?)new MinioClient()
            .WithEndpoint(minioConfig["Endpoint"])
            .WithCredentials(minioConfig["AccessKey"], minioConfig["SecretKey"])
            .WithSSL(bool.Parse(minioConfig["UseSSL"]))
            .Build();
        _bucketNamePrefix = "md-bucket"; // Префикс для бакетов
    }

    // Нормализация имени корзины
    public string NormalizeBucketName(string userId)
    {
        var normalized = userId.ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^a-z0-9.-]", "");
        normalized = normalized.Trim('.', '-');
        if (normalized.Length > 63)
        {
            normalized = normalized.Substring(0, 63);
        }
        return normalized;
    }

    // Очистка имени файла от недопустимых символов
    public string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Where(ch => !invalidChars.Contains(ch))
            .ToArray());
        return sanitized;
    }

    // Генерация уникального имени файла
    public async Task<string> GenerateUniqueObjectName(string bucketName, string fileName)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var objectName = sanitizedFileName;

        // Проверяем, существует ли файл с таким именем
        var counter = 1;
        while (await ObjectExistsAsync(bucketName, objectName))
        {
            var extension = Path.GetExtension(sanitizedFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitizedFileName);
            objectName = $"{nameWithoutExtension} ({counter++}){extension}";
        }

        return objectName;
    }

    // Проверка существования объекта
    public async Task<bool> ObjectExistsAsync(string bucketName, string objectName)
    {
        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName));
            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }

    // Загрузка файла
    public async Task<string> UploadFileAsync(string userId, string fileName, Stream fileStream, string contentType)
    {
        var bucketName = $"{_bucketNamePrefix}-{NormalizeBucketName(userId)}";

        // Проверяем, существует ли корзина, если нет — создаем
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }

        // Генерируем уникальное имя файла
        var objectName = await GenerateUniqueObjectName(bucketName, fileName);

        // Загружаем файл
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType));

        return objectName;
    }

    // Получение файла
    public async Task<Stream> GetFileAsync(string userId, string objectName)
    {
        var bucketName = $"{_bucketNamePrefix}-{NormalizeBucketName(userId)}";

        var memoryStream = new MemoryStream();
        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream)));

        memoryStream.Position = 0;
        return memoryStream;
    }

    // Получение информации о файле
    public async Task<ObjectStat> GetFileInfoAsync(string userId, string objectName)
    {
        var bucketName = $"{_bucketNamePrefix}-{NormalizeBucketName(userId)}";

        return await _minioClient.StatObjectAsync(new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName));
    }

    // Удаление файла
    public async Task DeleteFileAsync(string userId, string objectName)
    {
        var bucketName = $"{_bucketNamePrefix}-{NormalizeBucketName(userId)}";

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName));
    }

    // Получение списка файлов пользователя
    public async Task<List<string>> ListFilesAsync(string userId)
    {
        var bucketName = $"{_bucketNamePrefix}-{NormalizeBucketName(userId)}";
        var files = new List<string>();

        // Проверяем, существует ли корзина
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!bucketExists)
        {
            return files; // Возвращаем пустой список, если корзина не существует
        }

        // Получаем список объектов
        var listArgs = new ListObjectsArgs()
            .WithBucket(bucketName)
            .WithRecursive(false);

        var observable = _minioClient.ListObjectsAsync(listArgs);

        // Используем реактивный подход для обработки элементов
        await observable.ForEachAsync(item =>
        {
            files.Add(item.Key);
        });

        return files;
    }

    // Замена файла
    public async Task ReplaceFileAsync(string userId, string objectName, Stream fileStream, string contentType)
    {
        var bucketName = $"{_bucketNamePrefix}-{NormalizeBucketName(userId)}";

        // Удаляем старый файл, если он существует
        await DeleteFileAsync(userId, objectName);

        // Загружаем новый файл
        await UploadFileAsync(userId, objectName, fileStream, contentType);
    }
}