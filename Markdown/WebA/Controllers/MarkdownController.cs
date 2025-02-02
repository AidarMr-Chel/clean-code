using Microsoft.AspNetCore.Mvc;
using Markdown;
//using Markdig;

public class MarkdownController : Controller
{
    [HttpPost]
    public IActionResult Translate([FromBody] string markdownText)
    {
        //var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        //var html = Markdown.ToHtml(markdownText, pipeline);
        var html = MdProcessor.Render(markdownText);
        return Json(new { html });
    }
}
