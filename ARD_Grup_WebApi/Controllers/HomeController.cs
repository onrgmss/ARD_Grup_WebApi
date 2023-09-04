using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ARDGrupRapor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return Ok(new { Message = "Welcome to the API Home page" });
        }

        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            return Ok(new { Message = "Privacy page" });
        }

        [HttpGet("Error")]
        public IActionResult Error()
        {
            return Ok(new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
