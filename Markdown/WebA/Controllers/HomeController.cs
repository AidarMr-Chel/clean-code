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
    //    // ����� ����� �������� ������ ��� �������� Markdown � HTML
    //    // ��������, ��������� ���������� ��� ��������� Markdown

    //    var html = "# ���������\n\n��� **Markdown** �����."; // ������ ��������

    //    return Ok(new { html = html });
    //}

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SaveFile([FromBody] SaveFileRequest request)
    {
        // ������ ���������� ����� � MinIO
        // �������������� �� ����� � DocumentsController
        return RedirectToAction("SaveFile", "Documents", request);
    }
}
