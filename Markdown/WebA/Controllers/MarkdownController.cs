using Microsoft.AspNetCore.Mvc;
using Markdown;
using System.Threading.Tasks;

public class MarkdownController : Controller
{
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MarkdownText))
        {
            return BadRequest(new { error = "Input text is null or empty." });
        }

        try
        {
            var html = MdProcessor.Render(request.MarkdownText);
            return Json(new { html });
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            // _logger.LogError(ex, "Error while translating Markdown to HTML.");

            return StatusCode(500, new { error = "An error occurred while processing the request." });
        }
    }
}

public class TranslateRequest
{
    public string MarkdownText { get; set; }
}