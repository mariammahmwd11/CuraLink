using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CuraLink.MVC.Controllers;

[Authorize]
public class RealtimeController : Controller
{
    [HttpGet]
    public IActionResult Token()
    {
        var token = User
            .FindFirst("AccessToken")
            ?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        return Ok(new
        {
            token
        });
    }
}