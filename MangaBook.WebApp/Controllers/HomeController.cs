using MangaBook.Data.DataContext;
using MangaBook.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MangaBook.WebApp.Controllers
{
    [Route("")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly DataDbContext _context;

        public HomeController(DataDbContext context)
        {
            _context = context;
        }

        [Route("trang-chu")]
        [Route("")]
        public IActionResult Index()
        {
            var recentManga = _context.Manga.Where(m => m.IsActive == true && m.IsDeleted == false)
                .OrderByDescending(m => m.ModifiedDate)
                .Take(5)
                .ToList();

            ViewBag.RecentManga = recentManga;

            var hotManga = _context.Manga.Where(m => m.IsActive == true && m.IsDeleted == false && m.IsHot == true)
                .OrderByDescending(m => m.ModifiedDate)
                .Take(12)
                .ToList();

            ViewBag.HotManga = hotManga;

            var listManga = _context.Manga.Where(m => m.IsActive == true && m.IsDeleted == false)
                .OrderByDescending(m => m.ModifiedDate)
                .Take(12)
                .ToList();

            ViewBag.ListManga = listManga;

            var genre = _context.Genres.Where(g => g.IsActive == true && g.IsDeleted == false)
                .OrderByDescending(m => m.ModifiedDate)
                .Take(20)
                .ToList();

            ViewBag.ListGenre = genre;

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            return View();
        }

        [Route("tim-kiem")]
        [HttpGet]
        public IActionResult Search(string s)
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            ViewBag.Keyword = s;

            var result = _context.Manga
                .Where(p => p.IsActive == true && p.Name.Contains(s))
                .OrderByDescending(p => p.ModifiedDate).ToList();

            return View(result);
        }

    }
}
