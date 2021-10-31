using System;

namespace MangaBook.Data.Entities
{
    public class Chapter
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }
        public int Number { get; set; }
        public string Content { get; set; }
        public Guid MangaId { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public AppUser AppUser { get; set; }
    }
}
