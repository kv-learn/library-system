using LibraryApi.Authentication;
using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AuthService(LibraryDbContext dbContext, IPasswordHasher<Member> hasher, TokenService tokenService)
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);

        if (await dbContext.Members.AnyAsync(m => m.Email == email, ct))
            return Result<AuthResponse>.Fail(ErrorType.Conflict, "Email is already registered");

        var newMember = new Member { Email = email, FullName = request.FullName.Trim(), PasswordHash = "" };
        newMember.PasswordHash = hasher.HashPassword(newMember, request.Password);
        dbContext.Members.Add(newMember);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {

            return Result<AuthResponse>.Fail(ErrorType.Conflict, "Email is already registered");
        }

        return Result<AuthResponse>.Ok(CreateResponse(newMember));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        var member = await dbContext.Members.FirstOrDefaultAsync(m => m.Email == email, ct);
        if (member is null)
            return InvalidCredentials();

        var result = hasher.VerifyHashedPassword(member, member.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return InvalidCredentials();

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            member.PasswordHash = hasher.HashPassword(member, request.Password);
            await dbContext.SaveChangesAsync(ct);
        }

        return Result<AuthResponse>.Ok(CreateResponse(member));
    }

    public async Task<Result<MemberDto>> GetMemberAsync(Guid id, CancellationToken ct)
    {
        var member = await dbContext.Members.Where(m => m.Id == id)
            .Select(m => new MemberDto(m.Id, m.Email, m.FullName, m.Role))
            .FirstOrDefaultAsync(ct);

        return member is null
            ? Result<MemberDto>.Fail(ErrorType.Unauthorized, "Member no longer exists.")
            : Result<MemberDto>.Ok(member);
    }

    private AuthResponse CreateResponse(Member member)
    {
        var token = tokenService.CreateAccessToken(member);
        return new AuthResponse(token.Token, token.ExpiresAt);
    }

    private static Result<AuthResponse> InvalidCredentials()
    {
        return Result<AuthResponse>.Fail(ErrorType.Unauthorized, "Invalid email or password.");
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
