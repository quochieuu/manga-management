using System;
using System.Collections.Generic;

namespace MangaBook.Data.Entities
{
    public class SysMenu
    {
        public SysMenu()
        {
            InverseParents = new HashSet<SysMenu>();
        }

        public int Id { set; get; }
        public string Name { set; get; }
        public string Link { set; get; }
        public string Icon { set; get; }
        public string Slug { set; get; }
        public int DisplayOrder { set; get; }
        public int? ParentId { set; get; }
        public SysMenu Parent { set; get; }

        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<SysMenu> InverseParents { get; set; }
    }
}
