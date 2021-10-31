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

namespace MangaBook.WebApp.Controllers
{
    [Route("")]
    public class BookmarksController : Controller
    {
        private readonly DataDbContext _context;

        public BookmarksController(DataDbContext context)
        {
            _context = context;
        }

        [Route("truyen-yeu-thich")]
        public async Task<IActionResult> Index()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var bookmark = await (from m in _context.Manga
                                  join bm in _context.Bookmarks on m.Id equals bm.MangaId
                                  select new ListBookmarkViewModel() { 
                                      MangaId = m.Id,
                                      BookmarkId = bm.Id,
                                      MangaImage = m.UrlImage,
                                      MangaSlug = m.Slug,
                                      MangaTitle = m.Name,
                                      BookmarkedDate = bm.ModifiedDate
                                  })
                                  .OrderByDescending(p => p.BookmarkedDate)
                                  .ToListAsync();

            return View(bookmark);
        }



        [Route("delete-bookmark-{id}")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(m => m.Id == id);

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            if (bookmark == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
