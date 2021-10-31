using Microsoft.AspNetCore.Identity;
using System;

namespace MangaBook.Data.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
        public string Description { get; set; }
    }
}
