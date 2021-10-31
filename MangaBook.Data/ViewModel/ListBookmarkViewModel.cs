using System;

namespace MangaBook.Data.ViewModel
{
    public class ListBookmarkViewModel
    {
        public string MangaSlug { get; set; }
        public Guid MangaId { get; set; }
        public Guid BookmarkId { get; set; }
        public string MangaTitle { get; set; }
        public string MangaImage { get; set; }
        public DateTime BookmarkedDate { get; set; }
    }
}
