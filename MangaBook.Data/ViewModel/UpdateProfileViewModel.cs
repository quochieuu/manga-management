using System;

namespace MangaBook.Data.ViewModel
{
    public class UpdateProfileViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset Birth { get; set; }
        public int Gender { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string UrlAvatar { get; set; }
    }
}
