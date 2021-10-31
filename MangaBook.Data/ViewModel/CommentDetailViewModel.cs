using System;

namespace MangaBook.Data.ViewModel
{
    public class CommentDetailViewModel
    {
        public Guid CommentId { get; set; }
        public string MangaSlug { get; set; }
        public string Content { get; set; }
        public DateTime PublishedDate { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
        public Guid AuthorId { get; set; }
    }
}
