using MangaBook.Data.DataContext;
using MangaBook.Data.Entities;
using MangaBook.Data.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaBook.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/menu")]
    [Authorize(Roles = "Admin")]
    public class SysMenuController : Controller
    {
        private readonly DataDbContext _context;

        public SysMenuController(DataDbContext context)
        {
            _context = context;
        }


        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            ViewBag.Menu = _context.SysMenus.ToList();
            return View();
        }

        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }


        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SysMenu cmsMenu)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                cmsMenu.Slug = StringHelper.UpperToLower(StringHelper.ToUnsignString(cmsMenu.Name));
                cmsMenu.CreatedDate = DateTime.Now;
                cmsMenu.ModifiedDate = DateTime.Now;
                cmsMenu.IsActive = true;
                cmsMenu.IsDeleted = false;
                cmsMenu.CreatedBy = Guid.Parse(userId);
                cmsMenu.ModifiedBy = Guid.Parse(userId);

                _context.Add(cmsMenu);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // THÊM MỚI SUB MENU
        [Route("addsubmenu-{id}")]
        [HttpGet]
        public IActionResult AddSubMenu(int? id)
        {

            if (id == null)
                return NotFound();

            SysMenu menuInfo = _context.SysMenus.Find(id);

            if (menuInfo == null)
                NotFound();

            return View(menuInfo);

        }

        [Route("addsubmenu-{id}")]
        [HttpPost]
        public async Task<IActionResult> AddSubMenu(SysMenu menu, string subMenu, string subMenuLink)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var slug = StringHelper.UpperToLower(StringHelper.ToUnsignString(menu.Name));

                SysMenu addSubMenu = new SysMenu
                {
                    Slug = slug,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsDeleted = false,
                    CreatedBy = Guid.Parse(userId),
                    ModifiedBy = Guid.Parse(userId),
                    Name = subMenu,
                    ParentId = menu.Id,
                    Link = subMenuLink,
                    IsActive = true,
                    DisplayOrder = 0
                };


                _context.Add(addSubMenu);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));


            }

            return View(menu);
        }

        // Switch Active and InActive
        [HttpGet]
        [Route("set-featured/{menuId}")]
        public async Task<IActionResult> SetFeatured(int menuId)
        {
            var cmsMenu = await _context.SysMenus.FindAsync(menuId);

            if (cmsMenu.IsActive == true)
            {
                cmsMenu.IsActive = true;
            }
            else
            {
                cmsMenu.IsActive = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 
        [HttpGet]
        [Route("addlink/{menuId}")]
        public IActionResult AddLink(int menuId)
        {
            var menu = _context.SysMenus.Find(menuId);

            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        [HttpPost]
        [Route("addlink/{menuId}")]
        public async Task<IActionResult> AddLink(int menuId, string link)
        {
            var menu = await _context.SysMenus.FindAsync(menuId);

            if (menu == null)
            {
                return NotFound();
            }

            menu.Link = link;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [Route("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cmsMenu = await _context.SysMenus.FindAsync(id);
            if (cmsMenu == null)
            {
                return NotFound();
            }
            return View(cmsMenu);
        }
        [HttpPost]
        [Route("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SysMenu cmsMenu)
        {
            if (id != cmsMenu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    cmsMenu.CreatedDate = DateTime.Now;
                    _context.Update(cmsMenu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CmsMenuExists(cmsMenu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cmsMenu);
        }

        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cmsMenu = await _context.SysMenus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cmsMenu == null)
            {
                return NotFound();
            }

            _context.SysMenus.Remove(cmsMenu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CmsMenuExists(int id)
        {
            return _context.SysMenus.Any(e => e.Id == id);
        }
    }
}
