using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            this.db = db;
        }

        public IActionResult Index()
        {
            var objVM = new HomeVM()
            {
                Products = db.Product.Include(x => x.Category).Include(x => x.ApplicationType),
                Categories = db.Category
            };
            return View(objVM);
        }

        public IActionResult Details(int id)
        {
            var objVM = new DetailsVM();
            objVM.Product = db.Product.Include(x => x.Category).Include(x => x.ApplicationType).FirstOrDefault(x => x.Id == id);
            objVM.ExistsInCart = false;
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            foreach (var shoppingCart in shoppingCartList)
            {
                if (shoppingCart.ProductId == id)
                {
                    objVM.ExistsInCart = true;
                }
            }
            return View(objVM);
        }

        [HttpPost, ActionName(nameof(Details))]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCart> shoppingCartsList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartsList.Add(new ShoppingCart() { ProductId = id });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartsList);
            return RedirectToAction(nameof(Index));
        }


        public IActionResult RemoveFromCart(int id)
        {
            var objVM = new DetailsVM();
            objVM.Product = db.Product.Include(x => x.Category).Include(x => x.ApplicationType).FirstOrDefault(x => x.Id == id);
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            ShoppingCart shoppingCart = new ShoppingCart();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            shoppingCart = shoppingCartList.SingleOrDefault(x => x.ProductId == id);
            if (shoppingCart != null)
            {
                shoppingCartList.Remove(shoppingCart);
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

