namespace LibraryApi.Entities;

public class Author
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Biography { get; set; }

    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
}
