using LibraryApi.Authentication;
using LibraryApi.Dtos;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[Route("api/auth")]
public class AuthController(AuthService auth) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MemberDto>> Me(CancellationToken ct)
    {
        // The middleware already validated the token; "sub" is the member id we put in it
        if (!Guid.TryParse(User.FindFirst(AuthClaims.Sub)?.Value, out var memberId))
        {
            return Unauthorized();
        }

        var me = await auth.GetMemberAsync(memberId, ct);

        return me is null ? Unauthorized() : Ok(me);
    }
}


