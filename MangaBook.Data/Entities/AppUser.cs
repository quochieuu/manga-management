using Microsoft.AspNetCore.Identity;
using System;

namespace MangaBook.Data.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
        public DateTimeOffset Birth { get; set; }
        public int Gender { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string UrlAvatar { get; set; }

        public bool IsActive { get; set; }

    }
}
