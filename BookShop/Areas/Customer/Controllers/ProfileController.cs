// Areas/Customer/Controllers/ProfileController.cs
// Controller for customer profile management and wallet operations

using BookShop.Interfaces;
using BookShop.Models;
using BookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "user")]
    public class ProfileController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly myShopContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ICustomerService customerService,
            myShopContext context,
            ILogger<ProfileController> logger)
        {
            _customerService = customerService;
            _context = context;
            _logger = logger;
        }

        // GET: Customer/Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading customer profile");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Get wallet balance
                var walletBalance = await _customerService.GetWalletBalanceAsync(currentUser.UserId);

                // Create view model
                var viewModel = new CustomerProfileViewModel
                {
                    UserInfo = currentUser,
                    WalletBalance = walletBalance,
                    TopUpAmount = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer profile");
                TempData["ErrorMessage"] = "An error occurred while loading your profile.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // POST: Customer/Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CustomerProfileViewModel model)
        {
            try
            {
                _logger.LogInformation($"Updating profile for user: {model.UserInfo.Email}");

                // Get current user to verify ownership
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null || currentUser.UserId != model.UserInfo.UserId)
                {
                    return Forbid();
                }

                // Remove validation for fields we're not updating
                ModelState.Remove("UserInfo.Password");
                ModelState.Remove("UserInfo.Role");
                ModelState.Remove("UserInfo.CreatedDate");
                ModelState.Remove("UserInfo.Balance");
                ModelState.Remove("TopUpAmount");

                if (ModelState.IsValid)
                {
                    // Check if email is being changed and if it's already taken
                    if (currentUser.Email != model.UserInfo.Email)
                    {
                        var emailExists = await _context.Users
                            .AnyAsync(u => u.Email == model.UserInfo.Email && u.UserId != currentUser.UserId);

                        if (emailExists)
                        {
                            TempData["ErrorMessage"] = "This email address is already registered.";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // Update user properties
                    currentUser.Name = model.UserInfo.Name;
                    currentUser.LastName = model.UserInfo.LastName;
                    currentUser.Email = model.UserInfo.Email;
                    currentUser.Phone = model.UserInfo.Phone;
                    currentUser.BirthDate = model.UserInfo.BirthDate;

                    var updateSuccess = await _customerService.UpdateUserProfileAsync(currentUser);

                    if (updateSuccess)
                    {
                        _logger.LogInformation($"Profile updated successfully for user ID: {currentUser.UserId}");
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the errors in the form.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer profile");
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Profile/TopUpWallet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUpWallet(decimal topUpAmount)
        {
            try
            {
                _logger.LogInformation($"Processing wallet top-up of ${topUpAmount}");

                // Get current user
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Home", new { area = "" });
                }

                // Validate amount
                if (topUpAmount <= 0 || topUpAmount > 10000)
                {
                    TempData["ErrorMessage"] = "Top-up amount must be between $1 and $10,000.";
                    return RedirectToAction(nameof(Index));
                }

                // Process top-up
                var success = await _customerService.TopUpWalletAsync(currentUser.UserId, topUpAmount);

                if (success)
                {
                    _logger.LogInformation($"Wallet topped up successfully for user ID: {currentUser.UserId}");
                    TempData["SuccessMessage"] = $"Wallet topped up with ${topUpAmount:F2} successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to top up wallet. Please try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing wallet top-up");
                TempData["ErrorMessage"] = "An error occurred while processing your top-up.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}