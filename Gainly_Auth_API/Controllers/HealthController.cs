using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }
}