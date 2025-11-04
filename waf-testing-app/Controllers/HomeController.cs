using Microsoft.AspNetCore.Mvc;

namespace WafTestingApp.Controllers;

[Route("")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public HomeController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var filePath = Path.Combine(_env.WebRootPath, "index.html");
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("HTML file not found");
        }

        var content = await System.IO.File.ReadAllTextAsync(filePath);
        return Content(content, "text/html; charset=utf-8");
    }
}
