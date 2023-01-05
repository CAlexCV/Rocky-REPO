using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        //[BindProperty]
        //public ProductUserVM ProductUserVM { get; set; }
        private readonly ApplicationDbContext db;

        public CartController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            List<int> shoppingCartId = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> productsInCart = db.Product.Where(x => shoppingCartId.Contains(x.Id));

            return View(productsInCart);
        }

        [HttpPost, ActionName(nameof(Index)), ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            List<int> shoppingCartId = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> productsInCart = db.Product.Where(x => shoppingCartId.Contains(x.Id));
            ProductUserVM ProductUserVM = new ProductUserVM()
            {
                ProductList = productsInCart.ToList(),
                ApplicationUser = db.ApplicationUser.FirstOrDefault(x => x.Id == claim.Value)
            };
            return View(ProductUserVM);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            ShoppingCart shoppingCart = shoppingCartList.SingleOrDefault(x => x.ProductId == id);
            shoppingCartList.Remove(shoppingCart);
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
