using Microsoft.AspNetCore.Mvc;

namespace MidisSqlAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "Midis SQL AI API",
            timestampUtc = DateTime.UtcNow
        });
    }
}