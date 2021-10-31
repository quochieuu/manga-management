using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MangaBook.Data.DataContext;
using MangaBook.Data.Entities;
using MangaBook.Data.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace MangaBook.WebApp.Controllers
{
    [Route("")]
    [AllowAnonymous]
    public class CommentsController : Controller
    {
        private readonly DataDbContext _context;

        public CommentsController(DataDbContext context)
        {
            _context = context;
        }

        [Route("lich-su-binh-luan")]
        public async Task<IActionResult> Index()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var listComment = await (from m in _context.Manga
                                     join cmt in _context.Comments on m.Id equals cmt.MangaId
                                     join u in _context.AppUsers on cmt.CreatedBy equals u.Id
                                     select new CommentDetailViewModel()
                                     {
                                         AuthorName = u.FullName,
                                         AuthorAvatar = u.UrlAvatar,
                                         AuthorId = u.Id,
                                         CommentId = cmt.Id,
                                         Content = cmt.Content,
                                         MangaSlug = m.Slug,
                                         PublishedDate = cmt.ModifiedDate
                                     })
                                     .OrderByDescending(cmt => cmt.PublishedDate)
                                     .ToListAsync();

            return View(listComment);
        }



        [Route("xoa-binh-luan-{id}")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            if (comment == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

       
    }
}
