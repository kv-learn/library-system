namespace LibraryApi.Entities;

public class Review : IHasCreatedAt
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public int Rating { get; set; } // 1 to 5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;

}
