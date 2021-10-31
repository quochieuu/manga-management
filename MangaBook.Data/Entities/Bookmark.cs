using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaBook.Data.Entities
{
    public class Bookmark
    {
        public Guid Id { get; set; }

        public Guid MangaId { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public AppUser AppUser { get; set; }
    }
}
