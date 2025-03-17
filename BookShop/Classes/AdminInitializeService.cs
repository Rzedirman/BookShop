using BookShop.Models;
using BookShop.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace BookShop.Classes
{
    public class AdminInitializeService : IUserInitializeService//Creating an Admin user
    {
        //private readonly myShopContext _context = new myShopContext();
        private readonly myShopContext _context;
        public AdminInitializeService(myShopContext context)
        {
            _context = context;
        }
        public void Initialize()
        {
            string adminEmail = "admin@gmail.com";
            string adminPassword = "12345";
            string hashedPassword = String.Empty;

            if (_context.Roles.FirstOrDefault(r => r.RoleName == "admin") ==null)
            {
                Role adminRole = new Role {RoleName="admin"};
                _context.Roles.Add(adminRole);
                _context.SaveChanges();
            }
            if (_context.Users.FirstOrDefault(u=>u.Email== adminEmail) ==null)
            {
                Role? userRole = _context.Roles.FirstOrDefault(r => r.RoleName == "admin");
                using (var myHash = SHA256.Create())
                {
                    var byteArrayResultOfRawData =Encoding.UTF8.GetBytes(adminPassword);
                    var byteArrayResult =myHash.ComputeHash(byteArrayResultOfRawData);
                    hashedPassword = string.Concat(Array.ConvertAll(byteArrayResult,h => h.ToString("X2")));
                }

                User adminUser = new User { Name="admin",
                    LastName="admin",
                    Email = adminEmail,
                    BirthDate = DateTime.Today,
                    Password = hashedPassword,
                    Role = userRole
                };
                _context.Users.Add(adminUser);
                _context.SaveChanges();
            }
        }
    }   
}
