using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class QueryController : ControllerBase
{
    [HttpGet]
    public IActionResult Query([FromQuery] string? query)
    {
        var result = ParserService.Parse(query ?? string.Empty);

        if (result.IsFailure)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Value);
    }
}
