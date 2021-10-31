using System;

namespace MangaBook.Data.Entities
{
    public class Manga
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string UrlImage { get; set; }
        public string Author { get; set; }
        public string ReleaseYear { get; set; }

        public bool IsHot { get; set; }

        public int MangaStatus { get; set; } // Dang ra truyen, hay la full
        public int Status { get; set; } // Da xuat ban, nhap, dang cho duyet

        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public AppUser AppUser { get; set; }

    }
}
