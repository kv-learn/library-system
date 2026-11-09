namespace LibraryApi.Entities;
public class Book
{
    public Guid Id { get; set; }   
    public required string Isbn { get; set; }      
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int? PublishedYear { get; set; }

    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
    public ICollection<BookGenre> BookGenres { get; set; } = [];
}
