namespace LibraryApi.Entities;

public class Member : IHasCreatedAt
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string PasswordHash { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
    public DateTime CreatedAt { get; set; }
    public  ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
