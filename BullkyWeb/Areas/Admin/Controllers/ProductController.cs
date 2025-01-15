namespace BullkyWeb.Areas.Admin.Controllers
{
    using BulkyWeb.DataAccess.Data;
    using Microsoft.AspNetCore.Mvc;
    using Bulky.DataAccess.Repository.IRepository;
    using BulkyBook.Models;
    using BulkyBook.DataAccess.Repository.IRepository;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using BulkyBook.DataAccess.Repository;
    using BulkyBook.Models.ViewModel;
    using NuGet.Protocol.Plugins;
    using Bulky.Utility;
    using Microsoft.AspNetCore.Authorization;

    namespace BullkyWeb.Areas.Admin.Controllers
    {
        [Area("Admin")]
        //[Authorize(Roles = SD.Role_Admin)]

        public class ProductController : Controller
        {
            private readonly IUnitOfWork _unitofwork;
            private readonly IWebHostEnvironment _webHostEnvironment;
            public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment webHostEnvironment)
            {
                _unitofwork = unitofwork;
                _webHostEnvironment = webHostEnvironment;
            }
            public IActionResult Index()
            {
                List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
                return View(objProductList);
            }

            public IActionResult Upsert(int? id)
            {
                ProductVM productVM = new()
                {
                    CategoryList = _unitofwork.Category.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }),
                    Product = new Product()
                };
                if (id == null || id == 0)
                {
                    //create
                    return View(productVM);
                }
                else
                {
                    //update
                    productVM.Product = _unitofwork.Product.Get(u => u.Id == id);
                    return View(productVM);
                }

            }
            [HttpPost]
            public IActionResult Upsert(ProductVM productVM, IFormFile? file)
            {
                if (ModelState.IsValid)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"images\product");

                        if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                        {
                            //delete old image
                            var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        productVM.Product.ImageUrl = @"\images\product\" + fileName;
                    }
                    if (productVM.Product.Id == 0)
                    {
                        _unitofwork.Product.Add(productVM.Product);
                    }
                    else
                    {
                        _unitofwork.Product.Update(productVM.Product);
                    }
                    _unitofwork.Save();
                    TempData["success"] = "Product Created successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    productVM.CategoryList = _unitofwork.Category.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });
                    return View(productVM);
                }

            }
            public IActionResult Edit(int? id)
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                Product productFromDb = _unitofwork.Product.Get(u => u.Id == id);
                if (productFromDb == null)
                {
                    return NotFound();
                }
                return View(productFromDb);
            }
            [HttpPost]
            public IActionResult Edit(Product obj)
            {

                if (ModelState.IsValid)
                {
                    _unitofwork.Product.Update(obj);
                    _unitofwork.Save();
                    TempData["success"] = "Product updated successfully";
                    return RedirectToAction("Index");
                }
                return View();
            }
            #region API CALLS
            [HttpGet]
            public IActionResult GetAll()
            {
                List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
                return Json(new { data = objProductList });
            }
            [HttpDelete]
            public IActionResult Delete(int? id)
            {
                var productToBeDeleted = _unitofwork.Product.Get(u => u.Id == id);

                if (productToBeDeleted == null) 
                { 
                    return Json(new {success = false , message = "Error While deleting"});
                }

                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                    productToBeDeleted.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                _unitofwork.Product .Remove(productToBeDeleted);
                _unitofwork.Save ();

                return Json(new { success = true, message = "Deleted Successfully" });
            }
            #endregion
        }

    }
}
