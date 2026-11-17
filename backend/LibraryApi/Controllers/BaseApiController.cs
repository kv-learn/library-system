using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ObjectResult ToProblem(Error error) => Problem(
    statusCode: error.Type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError,
    },
    title: error.Message);
}
