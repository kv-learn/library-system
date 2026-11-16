using LibraryApi.Entities;

namespace LibraryApi.Data.Seeding;

public static class GenreSeed
{
    public static readonly Guid Fiction        = new("59225385-a68c-4f56-bcd9-728eff6e5ce4");
    public static readonly Guid Fantasy        = new("f5c5c073-cc6a-4a56-a4ff-6bbb015c9c65");
    public static readonly Guid Horror         = new("92403d88-69ac-4363-a995-d75f25208b66");
    public static readonly Guid Mystery        = new("e61d0e27-c76e-4fde-ab38-eb48298bebf4");
    public static readonly Guid Romance        = new("29fd1a92-8580-473b-8040-5facebf19be3");
    public static readonly Guid ScienceFiction = new("a1a2215a-7db5-411c-bb13-cf7d6e8430f4");

    public static Genre[] All =>
    [
        new() { Id = Fiction,        Name = "Fiction" },
        new() { Id = Fantasy,        Name = "Fantasy" },
        new() { Id = Horror,         Name = "Horror" },
        new() { Id = Mystery,        Name = "Mystery" },
        new() { Id = Romance,        Name = "Romance" },
        new() { Id = ScienceFiction, Name = "Science Fiction" },
    ];
}
