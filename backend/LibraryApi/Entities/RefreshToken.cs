namespace LibraryApi.Entities;

public class RefreshToken : IHasCreatedAt
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public required string TokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }  
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public Member Member { get; set; } = null!;

}
