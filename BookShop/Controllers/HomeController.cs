using BookShop.Helpers;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        // GET: Home/Index - Main catalog page with bestsellers
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string searchString,
            int? genreId,
            int? authorId,
            int? languageId,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            int? pageNumber)
        {
            try
            {
                _logger.LogInformation("Loading book catalog");

                // Get bestsellers (top 5 books by order count)
                var bestsellers = await _context.Products
                    .Include(p => p.Author)
                    .Include(p => p.Genre)
                    .Include(p => p.Language)
                    .Include(p => p.Orders)
                    .Where(p => p.InStock > 0) // Only show in-stock books
                    .OrderByDescending(p => p.Orders.Count)
                    .Take(5)
                    .ToListAsync();

                // Start with all products query
                var productsQuery = _context.Products
                    .Include(p => p.Author)
                    .Include(p => p.Genre)
                    .Include(p => p.Language)
                    .Where(p => p.InStock > 0) // Only show in-stock books
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    productsQuery = productsQuery.Where(p =>
                        p.Title.Contains(searchString) ||
                        p.Description.Contains(searchString) ||
                        p.Author.Name.Contains(searchString) ||
                        p.Author.LastName.Contains(searchString));
                }

                // Apply genre filter
                if (genreId.HasValue && genreId.Value > 0)
                {
                    productsQuery = productsQuery.Where(p => p.GenreId == genreId.Value);
                }

                // Apply author filter
                if (authorId.HasValue && authorId.Value > 0)
                {
                    productsQuery = productsQuery.Where(p => p.AuthorId == authorId.Value);
                }

                // Apply language filter
                if (languageId.HasValue && languageId.Value > 0)
                {
                    productsQuery = productsQuery.Where(p => p.LanguageId == languageId.Value);
                }

                // Apply price range filter
                if (minPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
                }

                // Apply sorting
                ViewData["CurrentSort"] = sortOrder;
                ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
                ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";
                ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
                ViewData["AuthorSortParam"] = sortOrder == "author" ? "author_desc" : "author";

                productsQuery = sortOrder switch
                {
                    "title_desc" => productsQuery.OrderByDescending(p => p.Title),
                    "price" => productsQuery.OrderBy(p => p.Price),
                    "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                    "date" => productsQuery.OrderBy(p => p.PublicationDate),
                    "date_desc" => productsQuery.OrderByDescending(p => p.PublicationDate),
                    "author" => productsQuery.OrderBy(p => p.Author.LastName).ThenBy(p => p.Author.Name),
                    "author_desc" => productsQuery.OrderByDescending(p => p.Author.LastName).ThenByDescending(p => p.Author.Name),
                    _ => productsQuery.OrderBy(p => p.Title)
                };

                // Pagination
                int pageSize = 12;
                var paginatedBooks = await PaginatedList<Product>.CreateAsync(productsQuery, pageNumber ?? 1, pageSize);

                // Get filter options for dropdowns
                var genres = await _context.Genres.OrderBy(g => g.GenreName).ToListAsync();
                var authors = await _context.Authors.OrderBy(a => a.LastName).ThenBy(a => a.Name).ToListAsync();
                var languages = await _context.Languages.OrderBy(l => l.LanguageName).ToListAsync();

                // Create view model
                var viewModel = new BookCatalogViewModel
                {
                    Bestsellers = bestsellers,
                    AllBooks = paginatedBooks,
                    SearchTerm = searchString,
                    AvailableGenres = genres,
                    AvailableAuthors = authors,
                    AvailableLanguages = languages,
                    CurrentFilters = new FilterParameters
                    {
                        GenreId = genreId,
                        AuthorId = authorId,
                        LanguageId = languageId,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice
                    }
                };

                // Store filter values in ViewData for view
                ViewData["CurrentFilter"] = searchString;
                ViewData["GenreId"] = genreId;
                ViewData["AuthorId"] = authorId;
                ViewData["LanguageId"] = languageId;
                ViewData["MinPrice"] = minPrice;
                ViewData["MaxPrice"] = maxPrice;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book catalog");
                return View(new BookCatalogViewModel());
            }
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
            if (userFromDB != null)
            {
                var userRoleFromDB = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == userFromDB.RoleId);
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
        public async Task<IActionResult> Signup([Bind("Email,Password,LastName,Name,Phone,BirthDate, IsSeller")] BookShop.ViewModels.CreateUserViewModel usr)
        {
            if (ModelState.IsValid)
            {

                var userFromDB = await _context.Users.FirstOrDefaultAsync(u => u.Email == usr.Email);
                string roleName = usr.IsSeller ? "seller" : "user";
                var roleForNewUser = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
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

                    // Automatically log in the new user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, usr.Email),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, roleForNewUser.RoleName)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

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