using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BookShop.Helpers;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly myShopContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(myShopContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber)
        {
            _logger.LogInformation("Loading user management");

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var usersQuery = _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.LastName)
                .AsQueryable();

            // Apply search filter if specified
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Email.Contains(searchString) ||
                    u.Name.Contains(searchString) ||
                    u.LastName.Contains(searchString));
            }

            int pageSize = 10;
            var users = await PaginatedList<User>.CreateAsync(usersQuery, pageNumber ?? 1, pageSize);

            return View(users);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Map User to UserEditViewModel
            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                BirthDate = user.BirthDate,
                RoleId = user.RoleId
            };

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(viewModel);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(model.UserId);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Check if email already exists for a different user
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId))
                    {
                        ModelState.AddModelError("Email", "This email is already registered");
                        ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", model.RoleId);
                        return View(model);
                    }

                    // Update only the properties we want to change
                    existingUser.Name = model.Name;
                    existingUser.LastName = model.LastName;
                    existingUser.Email = model.Email;
                    existingUser.Phone = model.Phone;
                    existingUser.BirthDate = model.BirthDate;
                    existingUser.RoleId = model.RoleId;

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated user with ID: {existingUser.UserId}");

                    TempData["SuccessMessage"] = "User updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.UserId))
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

            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", model.RoleId);
            return View(model);
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            // Prevent deleting the admin user
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "admin");
            if (user.RoleId == adminRole.RoleId && await _context.Users.CountAsync(u => u.RoleId == adminRole.RoleId) <= 1)
            {
                TempData["ErrorMessage"] = "Cannot delete the only admin user";
                return RedirectToAction(nameof(Index));
            }

            // Check if user has orders or other dependencies
            bool hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Cannot delete user with existing orders";
                return RedirectToAction(nameof(Index));
            }

            // Delete user's cart items
            var cartItems = await _context.Carts.Where(c => c.UserId == id).ToListAsync();
            _context.Carts.RemoveRange(cartItems);

            // Delete user's favorites
            var favorites = await _context.Favorites.Where(f => f.UserId == id).ToListAsync();
            _context.Favorites.RemoveRange(favorites);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Deleted user with ID: {id}");

            TempData["SuccessMessage"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserPasswordViewModel { UserId = user.UserId };
            return View(model);
        }

        // POST: Admin/Users/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Hash the new password
                string hashedPassword = String.Empty;
                using (var myHash = SHA256.Create())
                {
                    var byteArrayResultOfRawData = Encoding.UTF8.GetBytes(model.NewPassword);
                    var byteArrayResult = myHash.ComputeHash(byteArrayResultOfRawData);
                    hashedPassword = string.Concat(Array.ConvertAll(byteArrayResult, h => h.ToString("X2")));
                }

                // Update the user's password
                user.Password = hashedPassword;
                _context.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Changed password for user with ID: {user.UserId}");

                TempData["SuccessMessage"] = "Password changed successfully";
                return RedirectToAction(nameof(Edit), new { id = model.UserId });
            }

            return View(model);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}