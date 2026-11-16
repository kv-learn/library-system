namespace LibraryApi.Entities;

public class Loan : IHasCreatedAt
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsReturned => ReturnedAt.HasValue;

    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
