using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            this.db = db;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objList = db.Product.Include(x => x.Category).Include(x => x.ApplicationType);
            return View(objList);
        }

        public IActionResult Upsert(int? id)
        {
            var objVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = db.Category.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                ApplicationSelectList = db.ApplicationType.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            if (id == null)
            {
                return View(objVM);
            }

            else
            {
                objVM.Product = db.Product.Include(x => x.Category).Include(x => x.ApplicationType).FirstOrDefault(x => x.Id == id);

                if (objVM.Product == null)
                {
                    return NotFound();
                }
                return View(objVM);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM objVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                var imagePath = webHostEnvironment.WebRootPath + WC.PartImagePath;

                if (objVM.Product.Id == 0)
                {
                    var guidImage = Guid.NewGuid().ToString();
                    var fileName = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(imagePath, guidImage + fileName), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    objVM.Product.Image = guidImage + fileName;
                    db.Product.Add(objVM.Product);
                }

                else
                {
                    var objVMFromDB = db.Product.AsNoTracking().FirstOrDefault(x => x.Id == objVM.Product.Id);
                    if (files.Count() > 0)
                    {
                        var guidImage = Guid.NewGuid().ToString();
                        var fileName = Path.GetExtension(files[0].FileName);
                        var oldImage = Path.Combine(imagePath, objVMFromDB.Image);

                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }

                        using (var fileStream = new FileStream(Path.Combine(imagePath, guidImage + fileName), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        objVM.Product.Image = guidImage + fileName;
                        db.Product.Update(objVM.Product);
                    }

                    else
                    {
                        objVM.Product.Image = objVMFromDB.Image;
                        db.Product.Update(objVM.Product);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            else
            {
                objVM.CategorySelectList = db.Category.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                objVM.ApplicationSelectList = db.ApplicationType.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                return View(objVM);
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                var obj = db.Product.Include(x => x.Category).Include(x => x.ApplicationType).FirstOrDefault(x => x.Id == id);
                if (obj == null)
                {
                    return NotFound();
                }
                return View(obj);
            }
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = db.Product.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            var imagePath = webHostEnvironment + WC.PartImagePath;
            var image = Path.Combine(imagePath, obj.Image);
            if (System.IO.File.Exists(image))
            {
                System.IO.File.Delete(image);
            }

            db.Product.Remove(obj);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

