using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly MinioService _minioService;

    public HomeController(MinioService minioService)
    {
        _minioService = minioService;
    }

    public IActionResult Index()
    {
        return View();
    }

    //[HttpPost]
    //public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
    //{
    //    // Здесь можно добавить логику для перевода Markdown в HTML
    //    // Например, используя библиотеку для обработки Markdown

    //    var html = "# Заголовок\n\nЭто **Markdown** текст."; // Пример перевода

    //    return Ok(new { html = html });
    //}

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SaveFile([FromBody] SaveFileRequest request)
    {
        // Логика сохранения файла в MinIO
        // Перенаправляем на метод в DocumentsController
        return RedirectToAction("SaveFile", "Documents", request);
    }
}
