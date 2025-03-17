using BookShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly myShopContext _context;
        //private readonly HttpContext _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, myShopContext context)
        {
            _logger = logger;
            _context = context;
            //_httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            //HttpContext? context = _httpContextAccessor.HttpContext;
            var userFromDB = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            var userRoleFromDB = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId== userFromDB.RoleId);
            if (userFromDB != null)
            {
                string hashedPassword = String.Empty;
                string hashedPasswordFromDB = userFromDB.Password;
                using (var myHash = SHA256.Create())
                {
                    var byteArrayResultOfRawData = Encoding.UTF8.GetBytes(password);
                    var byteArrayResult = myHash.ComputeHash(byteArrayResultOfRawData);
                    hashedPassword = string.Concat(Array.ConvertAll(byteArrayResult, h => h.ToString("X2")));
                }

                if (hashedPassword==hashedPasswordFromDB)
                {

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, email),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, userRoleFromDB.RoleName)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,claimsPrincipal);
                    return RedirectToAction("Index", "Home");
                }

                
            }



            ViewBag.Message = "Email or Password are incorrect";
            return View();
        }



        public IActionResult Signup()
        {
            //ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup([Bind("Email,Password,LastName,Name,Phone,BirthDate")] BookShop.ViewModels.CreateUserViewModel usr)
        {
            if (ModelState.IsValid)
            {

                var userFromDB = await _context.Users.FirstOrDefaultAsync(u => u.Email == usr.Email);
                var roleForNewUser = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "user");
                if (userFromDB == null)
                {
                    string hashedPassword = String.Empty;
                    using (var myHash = SHA256.Create())
                    {
                        var byteArrayResultOfRawData = Encoding.UTF8.GetBytes(usr.Password);
                        var byteArrayResult = myHash.ComputeHash(byteArrayResultOfRawData);
                        hashedPassword = string.Concat(Array.ConvertAll(byteArrayResult, h => h.ToString("X2")));
                    }

                    User newUser = new User
                    {
                        Name = usr.Name,
                        LastName = usr.LastName,
                        Email = usr.Email,
                        Phone = usr.Phone,
                        BirthDate = usr.BirthDate,
                        Password = hashedPassword,
                        Role = roleForNewUser
                    };
                    _context.Users.Add(newUser);
                    _context.SaveChanges();

                    return RedirectToAction("Index", "Home");
                    
                }
                ViewBag.Message = "This Email already exist";

            }
            
            return View(usr);
        }






        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}