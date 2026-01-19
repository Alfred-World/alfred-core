using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

[ApiController]
[Route("")]
[AllowAnonymous]
public class HealthController : BaseApiController
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return OkResponse(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }
}
