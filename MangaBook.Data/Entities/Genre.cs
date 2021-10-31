using System;

namespace MangaBook.Data.Entities
{
    public class Genre
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public AppUser AppUser { get; set; }
    }
}
