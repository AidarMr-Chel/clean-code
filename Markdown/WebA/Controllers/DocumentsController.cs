using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Minio.DataModel.Args;
using System.Text;

[Authorize]
public class DocumentsController : Controller
{
    private readonly MinioService _minioService;

    public DocumentsController(MinioService minioService)
    {
        _minioService = minioService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.Identity.Name; // Получаем ID текущего пользователя
        var files = await _minioService.ListFilesAsync(userId);
        return View(files);
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            var userId = User.Identity.Name; // Получаем ID текущего пользователя
            var objectName = await _minioService.UploadFileAsync(userId, file.FileName, file.OpenReadStream(), file.ContentType);
            ViewBag.Message = $"Файл '{objectName}' успешно загружен.";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Download(string objectName)
    {
        var userId = User.Identity.Name; // Получаем ID текущего пользователя
        var stream = await _minioService.GetFileAsync(userId, objectName);
        var fileInfo = await _minioService.GetFileInfoAsync(userId, objectName);
        return File(stream, fileInfo.ContentType, objectName);
    }


    [HttpGet]
    public async Task<IActionResult> Edit(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return RedirectToAction("Index");
        }

        var userId = User.Identity.Name; // Получаем ID текущего пользователя
        var fileInfo = await _minioService.GetFileInfoAsync(userId, objectName);
        if (fileInfo == null)
        {
            return NotFound("File not found.");
        }

        var fileStream = await _minioService.GetFileAsync(userId, objectName);
        if (fileStream == null)
        {
            return NotFound("File content not found.");
        }

        ViewBag.ObjectName = objectName;
        ViewBag.FileName = objectName;
        ViewBag.ContentType = fileInfo.ContentType;

        // Если файл текстовый, считываем его содержимое
        string fileContent;
        if (fileInfo.ContentType.StartsWith("text/") || fileInfo.ContentType == "application/json" || fileInfo.ContentType == "application/javascript" || fileInfo.ContentType == "application/xml" || fileInfo.ContentType == "text/html")
        {
            using (var reader = new StreamReader(fileStream))
            {
                fileContent = await reader.ReadToEndAsync();
            }
        }
        else
        {
            // Для нетекстовых файлов можно использовать base64 или другую обработку
            fileContent = Convert.ToBase64String(await FileToBytes(fileStream));
        }

        return View(new EditFileViewModel
        {
            FileName = objectName,
            Content = fileContent,
            ContentType = fileInfo.ContentType
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string objectName, string newContent)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return RedirectToAction("Index");
        }

        var userId = User.Identity.Name; // Получаем ID текущего пользователя
        var fileInfo = await _minioService.GetFileInfoAsync(userId, objectName);
        if (fileInfo == null)
        {
            return NotFound("File not found.");
        }

        // Обновляем содержимое файла
        await _minioService.ReplaceFileAsync(userId, objectName, new MemoryStream(Encoding.UTF8.GetBytes(newContent)), fileInfo.ContentType);

        return RedirectToAction("Index");
    }

    private async Task<byte[]> FileToBytes(Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }

    // Класс для передачи данных в представление
    public class EditFileViewModel
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    //[HttpGet]
    //public async Task<IActionResult> Edit(string objectName)
    //{
    //    var userId = User.Identity.Name; // Получаем ID текущего пользователя
    //    var fileInfo = await _minioService.GetFileInfoAsync(userId, objectName);
    //    var fileStream = await _minioService.GetFileAsync(userId, objectName);

    //    ViewBag.ObjectName = objectName;
    //    ViewBag.FileName = objectName;
    //    ViewBag.ContentType = fileInfo.ContentType;

    //    return View(fileStream);
    //}

    //[HttpPost]
    //public async Task<IActionResult> Edit(string objectName, IFormFile file)
    //{
    //    if (file != null && file.Length > 0)
    //    {
    //        var userId = User.Identity.Name; // Получаем ID текущего пользователя
    //        await _minioService.ReplaceFileAsync(userId, objectName, file.OpenReadStream(), file.ContentType);
    //    }

    //    return RedirectToAction("Index");
    //}

    [HttpPost]
    public async Task<IActionResult> Delete(string objectName)
    {
        var userId = User.Identity.Name; // Получаем ID текущего пользователя
        await _minioService.DeleteFileAsync(userId, objectName);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> SaveFile([FromBody] SaveFileRequest request)
    {
        var userId = User.Identity.Name; // Получаем ID текущего пользователя
        var sanitizedFileName = _minioService.SanitizeFileName(request.fileName);
        var bucketName = $"md-bucket-{_minioService.NormalizeBucketName(userId)}";

        // Проверяем, существует ли корзина, если нет — создаем
        var bucketExists = await _minioService.ObjectExistsAsync(bucketName, "dummy"); // Проверка на существование корзины
        if (!bucketExists)
        {
            await _minioService._minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }

        // Генерируем уникальное имя файла
        var objectName = await _minioService.GenerateUniqueObjectName(bucketName, sanitizedFileName);

        // Преобразуем строку Markdown в поток
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(request.content));

        // Загружаем файл в MinIO
        await _minioService._minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(memoryStream)
            .WithObjectSize(memoryStream.Length)
            .WithContentType("text/markdown"));

        return Ok(new { success = true, message = "File saved successfully!" });
    }
}

public class SaveFileRequest
{
    public string fileName { get; set; }
    public string content { get; set; }
}