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
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;

namespace MangaBook.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/manga")]
    [Authorize(Roles = "Admin")]
    public class MangaController : Controller
    {
        private readonly DataDbContext _context;

        public MangaController(DataDbContext context)
        {
            _context = context;
        }

        [Route("index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Manga.Where(p => p.IsDeleted == false)
                .OrderByDescending(p => p.ModifiedDate)
                .ToListAsync());
        }

        [Route("pending-manga")]
        public async Task<IActionResult> ListPendingManga()
        {
            int PENDING_STATUS = 3;

            return View(await _context.Manga.Where(p => p.IsDeleted == false && p.Status == PENDING_STATUS)
                .OrderByDescending(p => p.ModifiedDate)
                .ToListAsync());
        }

        [Route("approve-manga-{mangaId}")]
        public IActionResult ApproveManga(Guid? mangaId)
        {
            if(mangaId == null)
            {
                return NotFound();
            }

            var manga = _context.Manga.FirstOrDefault(m => m.Id == mangaId);

            if (manga == null)
            {
                return NotFound();
            }

            manga.Status = 1;

            _context.Update(manga);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListPendingManga));
        }

        [Route("{id}")]
        public async Task<IActionResult> ListChapters(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chap = await _context.Chapters
                .OrderByDescending(p => p.ModifiedDate)
                .Where(m => m.MangaId == id)
                .ToListAsync();

            var manga = await _context.Manga
                .FirstOrDefaultAsync(m => m.Id == id);

            ViewBag.MangaId = manga.Id;
            ViewBag.MangaName = manga.Name;

            if (chap == null)
            {
                return NotFound();
            }

            return View(chap);
        }

        [Route("detail/{id}")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manga = await _context.Manga
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manga == null)
            {
                return NotFound();
            }

            return View(manga);
        }

        [Route("create-chapter-{mangaId}")]
        public IActionResult CreateChapter(Guid? mangaId)
        {
            if (mangaId == null)
            {
                return NotFound();
            }

            var manga = _context.Manga.FirstOrDefault(m => m.Id == mangaId);

            ViewBag.MangaId = manga.Id;
            ViewBag.MangaName = manga.Name;
            if (manga == null)
            {
                return NotFound();
            }

            return View();
        }

        [Route("create-chapter-{mangaId}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChapter(Chapter chapter, Guid mangaId)
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

                return Redirect("/admin/manga/"+ mangaId);
            }
            return View(chapter);
        }



        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Manga manga, IFormFile files, string genre)
        {
            if (ModelState.IsValid)
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var slug = StringHelper.UpperToLower(StringHelper.ToUnsignString(manga.Name));

                // Thêm ảnh vào wwwroot 
                var fileName = Path.GetFileName(files.FileName);
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                var fileExtension = Path.GetExtension(fileName);
                var newFileName = String.Concat(myUniqueFileName, fileExtension);

                var filepath =
        new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads/manga")).Root + $@"\{newFileName}";

                using (FileStream fs = System.IO.File.Create(filepath))
                {
                    files.CopyTo(fs);
                    fs.Flush();
                }


                var newImageName = newFileName;

                var createItem = new Manga()
                {
                    Name = manga.Name,
                    Description = manga.Description,
                    Slug = slug,
                    Author = manga.Author,
                    ReleaseYear = manga.ReleaseYear,
                    MangaStatus = manga.MangaStatus,
                    IsHot = manga.IsHot,
                    UrlImage = newImageName.ToString(),
                    CreatedBy = Guid.Parse(userId),
                    CreatedDate = DateTime.Now,
                    ModifiedBy = Guid.Parse(userId),
                    ModifiedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Status = 1
                };
                _context.Add(createItem);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(genre))
                {
                    // Tách chuỗi nhập vào
                    string[] genresList = genre.Split(',');

                    foreach (var genreItem in genresList)
                    {
                        // Kiểm tra genre có trùng hay không, có thì bỏ qua, không thì thêm vào bảng Genre
                        var existedGenre = this.CheckGenre(genreItem);
                        var genreId = StringHelper.ToUnsignString(genreItem);

                        if (!existedGenre)
                        {
                            // Thêm genre bào bảng Genre
                            this.InsertGenre(genreId, genreItem, Guid.Parse(userId));
                        }
                        // Truyền 2 tham số vào MangaInGenre(MangaId, GenreId);
                        this.CreateMangaGenre(createItem.Id, genreId);
                    }
                }


                return RedirectToAction(nameof(Index));
            }
            return View(manga);
        }

        // Kiểm tra genre đã tồn tại trong database hay không?
        [Route("checkgenre/{genre}")]
        public bool CheckGenre(string genre)
        {
            return _context.Genres.Count(x => x.Name.ToLower() == genre.ToLower()) > 0;
        }

        [Route("insert-genre")]
        // Thêm genre bào bảng Genre
        public void InsertGenre(string id, string name, Guid userId)
        {
            var genre = new Genre();
            genre.Id = new Guid();
            genre.Slug = id;
            genre.Name = name;
            genre.CreatedBy = userId;
            genre.ModifiedBy = userId;
            genre.CreatedDate = DateTime.Now;
            genre.ModifiedDate = DateTime.Now;
            genre.IsActive = true;
            genre.IsDeleted = false;
            _context.Genres.Add(genre);
            _context.SaveChanges();
        }

        [Route("create-manga-genre")]
        // Truyền 2 tham số vào MangaInGenre(MangaId, GenreId);
        public void CreateMangaGenre(Guid mangaId, string genreSlug)
        {
            var mangaGenre = new MangaInGenre();
            mangaGenre.MangaId = mangaId;
            mangaGenre.GenreSlug = genreSlug;

            _context.MangaInGenres.Add(mangaGenre);
            _context.SaveChanges();

        }


        [Route("edit-manga-{id}")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manga = await _context.Manga.FindAsync(id);

            var genreByManga = await (from m in _context.Manga
                                      join mg in _context.MangaInGenres on m.Id equals mg.MangaId
                                      join g in _context.Genres on mg.GenreSlug equals g.Slug
                                      where m.Id == id
                                      select new Genre()
                                      {
                                          Name = g.Name,
                                          Description = g.Description,
                                          Slug = g.Slug,
                                          ModifiedDate = g.ModifiedDate
                                      })
                                    .ToListAsync();

            ViewBag.GenreByManga = genreByManga;
            if (manga == null)
            {
                return NotFound();
            }
            return View(manga);
        }

        [Route("edit-manga-{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Manga manga, IFormFile files, string genre)
        {
            if (id != manga.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (files != null)
                    {
                        
                        // Xu ly chinh sua anh
                        var fileName = Path.GetFileName(files.FileName);
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                        var fileExtension = Path.GetExtension(fileName);
                        var newFileName = String.Concat(myUniqueFileName, fileExtension);

                        var filepath =
                new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads/manga")).Root + $@"\{newFileName}";

                        using (FileStream fs = System.IO.File.Create(filepath))
                        {
                            files.CopyTo(fs);
                            fs.Flush();
                        }

                        var newImageName = newFileName;
                        manga.UrlImage = newImageName.ToString();
                        manga.IsActive = true;
                        manga.Status = 1;
                        manga.ModifiedDate = DateTime.Now;
                        manga.ModifiedBy = Guid.Parse(userId);
                    }
                    else if (files == null)
                    {
                        var data = _context.Manga.Where(p => p.Id == id).AsNoTracking().FirstOrDefault();

                        manga.Name = data.Name;
                        manga.ReleaseYear = data.ReleaseYear;
                        manga.Slug = data.Slug;
                        manga.UrlImage = data.UrlImage;
                        manga.Author = data.Author;
                        manga.CreatedBy = data.CreatedBy;
                        manga.CreatedDate = data.CreatedDate;
                        manga.Description = data.Description;
                        manga.IsDeleted = data.IsDeleted;
                        manga.IsHot = data.IsHot;
                        manga.MangaStatus = data.MangaStatus;

                        manga.IsActive = true;
                        manga.Status = 1;
                        manga.ModifiedDate = DateTime.Now;
                        manga.ModifiedBy = Guid.Parse(userId);
                        _context.Manga.Update(manga);
                        _context.SaveChanges();

                    }

                    if (manga.Slug == null)
                    {
                        manga.Slug = StringHelper.UpperToLower(StringHelper.ToUnsignString(manga.Name));
                    }

                    manga.ModifiedDate = DateTime.Now;
                    manga.ModifiedBy = Guid.Parse(userId);

                    _context.Update(manga);
                    

                    // Xóa tất cả record đc map trong bảng tạm của Genre
                    var tagTemp = _context.MangaInGenres.Where(t => t.MangaId == manga.Id);
                        //.Include(t => t.BlogTags).ToList();
                    string[] tagsLists = genre.Split(',');
                    foreach (var ta in tagsLists)
                    {
                        var removeTags = _context.MangaInGenres.Where(t => t.MangaId == manga.Id);
                        _context.MangaInGenres.RemoveRange(removeTags);
                        _context.SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(genre))
                    {
                        // Tách chuỗi nhập vào
                        string[] tagsList = genre.Split(',');

                        foreach (var tag in tagsList)
                        {
                            // Kiểm tra Tag có trùng hay không, có thì bỏ qua, không thì thêm vào bảng BlogTag
                            var existedTag = this.CheckGenre(tag);
                            var tagId = StringHelper.ToUnsignString(tag);

                            if (!existedTag)
                            {
                                // Thêm tag bào bảng BlogTags
                                this.InsertGenre(tagId, tag, Guid.Parse(userId));
                            }
                            // Truyền 2 tham số vào BlogPostTags(PostId, TagId);
                            this.CreateMangaGenre(manga.Id, tagId);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MangaExists(manga.Id))
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
            return View(manga);
        }

        [Route("delete-manga-{id}")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            // Xóa manga thì tất cả các chapter sẽ mất và xử lý các Genre kèm theo
            if (id == null)
            {
                return NotFound();
            }

            var manga = await _context.Manga
                .FirstOrDefaultAsync(m => m.Id == id);

            // Find and delete Manga Genre record
            var fkBlogPostTag = await _context.MangaInGenres.Where(p => p.MangaId == id).ToListAsync();
            foreach (var postTag in fkBlogPostTag)
            {
                if (postTag.MangaId == id)
                {
                    _context.MangaInGenres.Remove(postTag);
                }
            }

            // Đếm thẻ trong Blog Post Tag, nếu chỉ có 1 thẻ thì xóa, 2 thẻ trở lên để lại, vì 2 thẻ trở lên tức là là thẻ đó còn nối với bảng khác
            var listTag = await _context.MangaInGenres.Where(p => p.MangaId == id).ToListAsync();
            foreach (var itemTag in listTag)
            {
                if (itemTag.MangaId == id)
                {
                    var tagList = await _context.Genres.Where(p => p.Slug == itemTag.GenreSlug).ToListAsync();

                    foreach (var tag in tagList)
                    {
                        var numTag = _context.MangaInGenres.Count(p => p.GenreSlug == tag.Slug);
                        if (numTag <= 1)
                        {
                            _context.Genres.Remove(tag);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            // Xóa hết các chap
            var listChapter = _context.Chapters.Where(p => p.MangaId == manga.Id).ToList();
            foreach(var chap in listChapter)
            {
                _context.Chapters.RemoveRange(chap);
                _context.SaveChanges();
            }

            _context.Manga.Remove(manga);
            _context.SaveChanges();

            if (manga == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool MangaExists(Guid id)
        {
            return _context.Manga.Any(e => e.Id == id);
        }

        [Route("set-manga-is-hot-{mangaId}")]
        public IActionResult ChangeMangaIsHot(Guid mangaId)
        {
            var manga = _context.Manga.FirstOrDefault(p => p.Id == mangaId);

            
            if(manga.IsHot == true)
            {
                manga.IsHot = false;
                _context.Update(manga);
                _context.SaveChanges();

            } else if (manga.IsHot == false)
            {
                manga.IsHot = true;
                _context.Update(manga);
                _context.SaveChanges();
            } else
            {
                manga.IsHot = true;
                _context.Update(manga);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("set-manga-active-{mangaId}")]
        public IActionResult ChangeMangaActive(Guid mangaId)
        {
            var manga = _context.Manga.FirstOrDefault(p => p.Id == mangaId);


            if (manga.IsActive == true)
            {
                manga.IsActive = false;
                _context.Update(manga);
                _context.SaveChanges();

            }
            else if (manga.IsActive == false)
            {
                manga.IsActive = true;
                _context.Update(manga);
                _context.SaveChanges();
            }
            else
            {
                manga.IsActive = true;
                _context.Update(manga);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
