namespace LibraryApi.Entities;

public class Reservation : IHasCreatedAt 
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }


    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;

}
