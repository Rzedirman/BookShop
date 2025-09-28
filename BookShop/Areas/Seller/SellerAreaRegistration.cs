// Areas/Seller/SellerAreaRegistration.cs
// Area registration for seller functionality

using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Seller
{
    [Area("Seller")]
    public class SellerAreaRegistration : AreaAttribute
    {
        public SellerAreaRegistration() : base("Seller")
        {
        }
    }
}