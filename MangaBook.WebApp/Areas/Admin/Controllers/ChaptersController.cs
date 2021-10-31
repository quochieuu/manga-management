using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MangaBook.Data.DataContext;
using MangaBook.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MangaBook.Data.Helpers;

namespace MangaBook.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/chapter")]
    [Authorize(Roles = "Admin")]
    public class ChaptersController : Controller
    {
        private readonly DataDbContext _context;

        public ChaptersController(DataDbContext context)
        {
            _context = context;
        }

        [Route("edit-{id}")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }
            return View(chapter);
        }

        
        [HttpPost]
        [Route("edit-{id}")]
        public async Task<IActionResult> Edit(Guid id, Chapter chapter)
        {
            if (id != chapter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var findMangaIdByChapterId = (from m in _context.Manga
                                              join ch in _context.Chapters on m.Id equals ch.MangaId
                                              where ch.Id == id
                                              select m.Id).FirstOrDefault();

                try
                {
                    

                    if (chapter.Slug == null)
                    {
                        chapter.Slug = StringHelper.UpperToLower(StringHelper.ToUnsignString(chapter.Name));
                    }

                    chapter.MangaId = findMangaIdByChapterId;
                    chapter.ModifiedBy = Guid.Parse(userId);
                    chapter.ModifiedDate = DateTime.Now;
                    chapter.IsActive = true;
                    chapter.IsDeleted = false;

                    _context.Update(chapter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapter.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("/admin/manga/" + findMangaIdByChapterId);
            }
            return View(chapter);
        }

        [Route("delete-{id}")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var findMangaIdByChapterId = (from m in _context.Manga
                                          join ch in _context.Chapters on m.Id equals ch.MangaId
                                          where ch.Id == id
                                          select m.Id).FirstOrDefault();


            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(m => m.Id == id);

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            if (chapter == null)
            {
                return NotFound();
            }

            return Redirect("/admin/manga/" + findMangaIdByChapterId);
        }

        [Route("set-chapter-active-{chapterId}")]
        public IActionResult ChangeChapterActive(Guid chapterId)
        {
            var chapter = _context.Chapters.FirstOrDefault(p => p.Id == chapterId);


            if (chapter.IsActive == true)
            {
                chapter.IsActive = false;
                _context.Update(chapter);
                _context.SaveChanges();

            }
            else if (chapter.IsActive == false)
            {
                chapter.IsActive = true;
                _context.Update(chapter);
                _context.SaveChanges();
            }
            else
            {
                chapter.IsActive = true;
                _context.Update(chapter);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("ChapterExists-{id}")]
        private bool ChapterExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id);
        }
    }
}
