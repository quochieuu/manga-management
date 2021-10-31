using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MangaBook.Data.DataContext;
using MangaBook.Data.Entities;
using System.Security.Claims;
using MangaBook.Data.Helpers;

namespace MangaBook.WebApp.Controllers
{
    public class ChaptersController : Controller
    {
        private readonly DataDbContext _context;

        public ChaptersController(DataDbContext context)
        {
            _context = context;
        }

        [Route("tao-chapter-{mangaId}")]
        public IActionResult Create(Guid? mangaId)
        {
            if (mangaId == null)
            {
                return NotFound();
            }

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var manga = _context.Manga.FirstOrDefault(m => m.Id == mangaId);

            ViewBag.MangaId = manga.Id;
            ViewBag.MangaName = manga.Name;
            if (manga == null)
            {
                return NotFound();
            }

            return View();
        }

        [Route("tao-chapter-{mangaId}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Chapter chapter, Guid mangaId)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var slug = StringHelper.UpperToLower(StringHelper.ToUnsignString(chapter.Name));

                var createItem = new Chapter()
                {
                    Name = chapter.Name,
                    Number = chapter.Number,
                    Slug = slug,
                    Content = chapter.Content,
                    CreatedBy = Guid.Parse(userId),
                    CreatedDate = DateTime.Now,
                    ModifiedBy = Guid.Parse(userId),
                    ModifiedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    MangaId = mangaId
                };
                _context.Add(createItem);
                await _context.SaveChangesAsync();

                return Redirect("/chappter-" + mangaId);
            }
            return View(chapter);
        }

        [Route("sua-chapter-{id}")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;


            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }
            return View(chapter);
        }


        [HttpPost]
        [Route("sua-chapter-{id}")]
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
                    throw;
                }
                return Redirect("/chappter-" + findMangaIdByChapterId);
            }
            return View(chapter);
        }

        [Route("xoa-chap-{id}")]
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

            return Redirect("/chappter-" + findMangaIdByChapterId);
        }

        [Route("luu-nhap-chapter-{chapterId}")]
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

    }
}
