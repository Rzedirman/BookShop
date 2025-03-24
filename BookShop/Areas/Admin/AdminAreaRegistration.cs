// Areas/Admin/AdminAreaRegistration.cs
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Admin
{
    [Area("Admin")]
    public class AdminAreaRegistration : AreaAttribute
    {
        public AdminAreaRegistration() : base("Admin")
        {
        }
    }
}