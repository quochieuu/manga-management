using System;

namespace MangaBook.WebApp.Areas.Admin.ViewModel
{
    public class UserRolesViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
