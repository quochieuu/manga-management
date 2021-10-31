using System;

namespace MangaBook.WebApp.Areas.Admin.ViewModel
{
    public class UserRoleViewModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
