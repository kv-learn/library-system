using System.Security.Claims;
using LibraryApi.Entities;

namespace LibraryApi.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetMemberId(this ClaimsPrincipal user)
    {
        return Guid.TryParse(user.FindFirstValue(AuthClaims.Sub), out var id)
        ? id
        : throw new InvalidOperationException("Token has no valid 'sub' claim."); // only on [Authorize] endpoints

    }

    public static bool IsLibrarian(this ClaimsPrincipal user)
     => user.IsInRole(nameof(MemberRole.Librarian));

}
