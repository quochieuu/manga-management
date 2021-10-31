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
using MangaBook.Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace MangaBook.WebApp.Controllers
{
    [Route("")]
    [AllowAnonymous]
    public class MangaController : Controller
    {
        private readonly DataDbContext _context;

        public MangaController(DataDbContext context)
        {
            _context = context;
        }

        #region Danh sách manga ở client
        [Route("truyen-moi-nhat")]
        public async Task<IActionResult> Index()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var genre = _context.Genres.Where(g => g.IsActive == true && g.IsDeleted == false)
                .OrderByDescending(m => m.ModifiedDate)
                .Take(20)
                .ToList();

            ViewBag.ListGenre = genre;

            return View(await _context.Manga
                .Where(p => p.IsActive == true)
                .OrderByDescending(p => p.ModifiedDate)
                .ToListAsync());
        }
        #endregion


        #region Danh sách truyện thuộc thể loại
        [Route("manga-{genreSlug}")]
        public async Task<IActionResult> ListMangaByGenre(string genreSlug)
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var gerneName = _context.Genres.FirstOrDefault(p => p.Slug == genreSlug).Name;
            ViewBag.GenreName = gerneName;

            var manga = await (from m in _context.Manga
                               join mg in _context.MangaInGenres on m.Id equals mg.MangaId
                               join g in _context.Genres on mg.GenreSlug equals g.Slug
                               where g.Slug == genreSlug
                               select new Manga() { 
                                    Id = m.Id,
                                    Slug = m.Slug,
                                    Name = m.Name,
                                    Description = m.Description,
                                    ModifiedDate = m.ModifiedDate,
                                    IsActive = m.IsActive
                               })
                               .OrderByDescending(p => p.ModifiedDate)
                               .ToListAsync();

            var genre = _context.Genres.Where(g => g.IsActive == true && g.IsDeleted == false)
                .OrderByDescending(m => m.ModifiedDate)
                .Take(20)
                .ToList();

            ViewBag.ListGenre = genre;

            return View(manga);
        }

        #endregion

        #region Xem mô tả truyện
        [Route("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (slug == null)
            {
                return NotFound();
            }

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var manga = await _context.Manga
                .FirstOrDefaultAsync(m => m.Slug == slug);

            var listGenreByManga = await (from m in _context.Manga
                                    join mg in _context.MangaInGenres on m.Id equals mg.MangaId
                                    join g in _context.Genres on mg.GenreSlug equals g.Slug
                                    where m.Slug == slug
                                    select new Genre() {
                                        Name = g.Name,
                                        Description = g.Description,
                                        Slug = g.Slug,
                                        ModifiedDate = g.ModifiedDate
                                    })
                                    .ToListAsync();

            ViewBag.ListGenreByManga = listGenreByManga;

            var listChapterByManga = await (from m in _context.Manga
                                            join ch in _context.Chapters on m.Id equals ch.MangaId
                                            where m.Slug == slug
                                            select new Chapter() { 
                                                Id = ch.Id,
                                                Name = ch.Name,
                                                Slug = ch.Slug,
                                                Number = ch.Number,
                                                ModifiedDate = ch.ModifiedDate
                                            })
                                            .ToListAsync();

            ViewBag.ListChapterByManga = listChapterByManga;

            var listChapterByMangaFirst = await (from m in _context.Manga
                                                 join ch in _context.Chapters on m.Id equals ch.MangaId
                                                 where m.Slug == slug
                                                 select new Chapter()
                                                 {
                                                     Id = ch.Id,
                                                     Name = ch.Name,
                                                     Slug = ch.Slug,
                                                     Number = ch.Number,
                                                     ModifiedDate = ch.ModifiedDate
                                                 })
                                                 .Take(1)
                                            .ToListAsync();

            ViewBag.ListChapterByMangaFirst = listChapterByMangaFirst;


            var listComment = await (from m in _context.Manga
                                     join cmt in _context.Comments on m.Id equals cmt.MangaId
                                     join u in _context.AppUsers on cmt.CreatedBy equals u.Id
                                     where m.Slug == slug && cmt.IsActive == true
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

            ViewBag.ListComment = listComment;

            var ratingCount = await (from m in _context.Manga
                                     join r in _context.Rating on m.Id equals r.MangaId
                                     where m.Slug == slug
                                     select new Rating()
                                     {
                                         Id = r.Id
                                     }).CountAsync();

            ViewBag.RatingScore =  ratingCount;

            var rated = await (from m in _context.Manga
                               join r in _context.Rating on m.Id equals r.MangaId
                               join u in _context.AppUsers on r.CreatedBy equals u.Id
                               where m.Slug == slug && r.CreatedBy == Guid.Parse(userId)
                               select new Rating()
                               {
                                   Score = r.Score
                               }).FirstOrDefaultAsync();

            if(rated == null)
            {
                ViewBag.UserReatedScore = 0;
            } else
            {
                ViewBag.UserReatedScore = rated.Score;
            }
            



            if (manga == null)
            {
                return NotFound();
            }

            return View(manga);
        }

        #endregion

        #region Đọc truyện
        [Route("chap-{chapterSlug}")]
        public async Task<IActionResult> ChapterDetail(string chapterSlug)
        {
            if (chapterSlug == null)
            {
                return NotFound();
            }


            var listComment = await (from m in _context.Manga
                                     join cmt in _context.Comments on m.Id equals cmt.MangaId
                                     join u in _context.AppUsers on cmt.CreatedBy equals u.Id
                                     join ch in _context.Chapters on m.Id equals ch.MangaId
                                     where ch.Slug == chapterSlug && cmt.IsActive == true
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

            ViewBag.ListComment = listComment;

            // Tim manga slug bang chapter slug
            var mangaSlug = await (from m in _context.Manga
                                   join ch in _context.Chapters on m.Id equals ch.MangaId
                                   where ch.Slug == chapterSlug
                                   select m.Id).FirstOrDefaultAsync();

            ViewBag.MangaId = mangaSlug;

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var chapter = await _context.Chapters.FirstOrDefaultAsync(p => p.Slug == chapterSlug);

            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }
        #endregion

        #region Bình luận
        [Route("add-manga-comment")]
        [HttpPost]
        public async Task<IActionResult> AddMangaComment(Comment comment, Guid mangaId, string chapSlug)
        {
            var mangaSlug = _context.Manga.FirstOrDefault(p => p.Id == mangaId);

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var createItem = new Comment()
                {
                    
                    Content = comment.Content,
                    CreatedBy = Guid.Parse(userId),
                    CreatedDate = DateTime.Now,
                    ModifiedBy = Guid.Parse(userId),
                    ModifiedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    MangaId = mangaId
                };
                _context.Comments.Add(createItem);
                await _context.SaveChangesAsync();

                if(chapSlug != null)
                {
                    return Redirect("/chap-" + chapSlug);
                } else
                {
                    return Redirect("/" + mangaSlug.Slug);
                }
                
            }
            return Redirect("/" + mangaSlug.Slug);
        }

        #endregion

        #region Bookmark
        [Route("bookmark-manga-{mangaId}")]
        [HttpGet]
        public async Task<IActionResult> BookmarkManga(Bookmark bookmark, Guid mangaId)
        {
            var mangaSlug = _context.Manga.FirstOrDefault(p => p.Id == mangaId);

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var createItem = new Bookmark()
                {
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

                return Redirect("/" + mangaSlug.Slug);
            }
            return Redirect("/" + mangaSlug.Slug);
        }

        #endregion


        #region Danh sách truyện được tạo bởi user
        [Route("truyen-cua-ban")]
        public async Task<IActionResult> ListManga()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(await _context.Manga.Where(p => p.IsDeleted == false)
                .Where(m => m.CreatedBy == Guid.Parse(userId))
                .OrderByDescending(p => p.ModifiedDate)
                .ToListAsync());
        }
        #endregion

        #region Danh sách chap thuộc 1 truyện được tạo bởi user
        [Route("chappter-{id}")]
        public async Task<IActionResult> ListChapters(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

        #endregion

        #region Tạo truyện
        [Route("tao-truyen")]
        public IActionResult Create()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            return View();
        }

        [Route("tao-truyen")]
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
                    IsHot = false,
                    UrlImage = newImageName.ToString(),
                    CreatedBy = Guid.Parse(userId),
                    CreatedDate = DateTime.Now,
                    ModifiedBy = Guid.Parse(userId),
                    ModifiedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Status = 3
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


                return RedirectToAction(nameof(ListManga));
            }
            return View(manga);
        }

        // Kiểm tra genre đã tồn tại trong database hay không?
        [Route("check-genre/{genre}")]
        public bool CheckGenre(string genre)
        {
            return _context.Genres.Count(x => x.Name.ToLower() == genre.ToLower()) > 0;
        }

        [Route("add-genre")]
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

        [Route("add-manga-genre")]
        // Truyền 2 tham số vào MangaInGenre(MangaId, GenreId);
        public void CreateMangaGenre(Guid mangaId, string genreSlug)
        {
            var mangaGenre = new MangaInGenre();
            mangaGenre.MangaId = mangaId;
            mangaGenre.GenreSlug = genreSlug;

            _context.MangaInGenres.Add(mangaGenre);
            _context.SaveChanges();

        }

        #endregion

        #region Sửa truyện
        [Route("sua-truyen-{id}")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

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

        [Route("sua-truyen-{id}")]
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
                        manga.Status = 3;
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

        #endregion

        #region Xóa truyện
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
            foreach (var chap in listChapter)
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

        #endregion

        #region Chuyển đổi trạng thái truyện
        [Route("luu-nhap-{mangaId}")]
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

        #endregion

        [Route("danh-gia-{mangaId}")]
        [HttpPost]
        public IActionResult Rating(Guid mangaId, int score)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var manga = _context.Manga.FirstOrDefault(p => p.Id == mangaId);


            if (userId == null)
            {
                return Redirect("/" + manga.Slug);
            }

            var rated = (from m in _context.Manga
                              join r in _context.Rating on m.Id equals r.MangaId
                              join u in _context.AppUsers on r.CreatedBy equals u.Id
                              where m.Id == mangaId && r.CreatedBy == Guid.Parse(userId)
                         select new Rating()
                              {
                                  Score = r.Score
                              }).Count();

            if(rated > 0)
            {
                return Redirect("/" + manga.Slug);
            } else
            {
                Rating rate = new Rating()
                {
                    Score = score,
                    MangaId = mangaId,
                    CreatedBy = Guid.Parse(userId),
                    CreatedDate = DateTime.Now,
                    ModifiedBy = Guid.Parse(userId),
                    ModifiedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false

                };

                _context.Rating.Add(rate);
                _context.SaveChanges();

                return Redirect("/" + manga.Slug);
            }

            
        }

    }
}
