using MangaBook.Data.DataContext;
using MangaBook.Data.Entities;
using MangaBook.Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaBook.WebApp.Controllers
{

    [Route("")]
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly DataDbContext _context;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, DataDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        [Route("dang-nhap")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {

                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }

            }
            return RedirectToAction("index", "home");
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    FullName = model.FullName,
                    Email = model.Email
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("index", "home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [Route("ho-so")]
        public IActionResult Profile()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return NotFound();
            }

            var us = _context.AppUsers.FirstOrDefault(us => us.Id == Guid.Parse(userId));

            
            if (us == null)
            {
                return NotFound();
            }
            return View(us);
        }

        [HttpGet]
        [Route("cap-nhat-ho-so")]
        public IActionResult UpdateProfile()
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewBag.UserId = userId;

            if (userId == null)
            {
                return NotFound();
            }

            var us = _context.AppUsers.FirstOrDefault(p => p.Id == Guid.Parse(userId));
            if (us == null)
            {
                return NotFound();
            }
            return View(us);
        }

        [HttpPost]
        [Route("cap-nhat-ho-so")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel user)
        {
            var menu = _context.SysMenus.Where(m => m.IsActive == true).ToList();
            ViewBag.Menu = menu;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                try
                {
                    var us = await _context.AppUsers.FirstOrDefaultAsync(p => p.Id == user.Id);
                    us.Id = user.Id;
                    us.PhoneNumber = user.PhoneNumber;
                    us.UserName = user.UserName;
                    us.Address = user.Address;
                    us.Birth = user.Birth;
                    us.Description = user.Description;
                    us.FullName = user.FullName;
                    us.Gender = user.Gender;

                    _context.AppUsers.Update(us);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(UpdateProfile));
        }

        [HttpPost]
        [Route("cap-nhat-anh")]
        public async Task<IActionResult> UploadProfileAvatar(UpdateProfileViewModel user, IFormFile files)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var fileName = Path.GetFileName(files.FileName);
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                var fileExtension = Path.GetExtension(fileName);
                var newFileName = String.Concat(myUniqueFileName, fileExtension);

                var filepath =
        new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads/avatar")).Root + $@"\{newFileName}";

                using (FileStream fs = System.IO.File.Create(filepath))
                {
                    files.CopyTo(fs);
                    fs.Flush();
                }


                var newAvt = newFileName.ToString();
                var us = await _context.AppUsers.FirstOrDefaultAsync(p => p.Id == user.Id);

                us.UrlAvatar = newAvt;

                _context.Entry(us).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(UpdateProfile));
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        [Route("dang-xuat")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [Route("access-denied")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


    }

}