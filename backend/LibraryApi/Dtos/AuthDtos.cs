using System.ComponentModel.DataAnnotations;
using LibraryApi.Entities;

namespace LibraryApi.Dtos;

public record RegisterRequest( 
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MaxLength(255)] string FullName,
    [Required, MinLength(8), MaxLength(128)] string Password);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAt);

public record MemberDto(Guid Id, string Email, string FullName, MemberRole Role);
