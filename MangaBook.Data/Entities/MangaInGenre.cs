using System;

namespace MangaBook.Data.Entities
{
    public class MangaInGenre
    {
        public Guid Id { get; set; }
        public Guid MangaId { get; set; }
        public string GenreSlug { get; set; }
    }
}
