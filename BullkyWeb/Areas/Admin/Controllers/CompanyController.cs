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

        public class CompanyController : Controller
        {
            private readonly IUnitOfWork _unitofwork;
            public CompanyController(IUnitOfWork unitofwork)
            {
                _unitofwork = unitofwork;
            }
            public IActionResult Index()
            {
                List<Product> objProductList = _unitofwork.Product.GetAll().ToList();
                return View(objProductList);
            }

            public IActionResult Upsert(int? id)
            {
                
                if (id == null || id == 0)
                {
                    //create
                    return View(new Company() );
                }
                else
                {
                    //update
                    Company companyObj = _unitofwork.Company.Get(u => u.Id == id);
                    return View(companyObj);
                }

            }
            [HttpPost]
            public IActionResult Upsert(Company CompanyObj)
            {
                if (ModelState.IsValid)
                {                   
                    if (CompanyObj.Id == 0)
                    {
                        _unitofwork.Company.Add(CompanyObj);
                    }
                    else
                    {
                        _unitofwork.Company.Update(CompanyObj);
                    }
                    _unitofwork.Save();
                    TempData["success"] = "Company Created successfully";
                    return RedirectToAction("Index");
                }
                else
                {    
                    return View(CompanyObj);
                }

            }
            public IActionResult Edit(int? id)
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                Company CompanyFromDb = _unitofwork.Company.Get(u => u.Id == id);
                if (CompanyFromDb == null)
                {
                    return NotFound();
                }
                return View(CompanyFromDb);
            }
            [HttpPost]
            public IActionResult Edit(Company obj)
            {

                if (ModelState.IsValid)
                {
                    _unitofwork.Company.Update(obj);
                    _unitofwork.Save();
                    TempData["success"] = "Company updated successfully";
                    return RedirectToAction("Index");
                }
                return View();
            }
            #region API CALLS
            [HttpGet]
            public IActionResult GetAll()
            {
                List<Company> objCompanyList = _unitofwork.Company.GetAll().ToList();
                return Json(new { data = objCompanyList });
            }
            [HttpDelete]
            public IActionResult Delete(int? id)
            {
                var CompanyToBeDeleted = _unitofwork.Company.Get(u => u.Id == id);

                if (CompanyToBeDeleted == null)
                {
                    return Json(new { success = false, message = "Error While deleting" });
                }


                _unitofwork.Company.Remove(CompanyToBeDeleted);
                _unitofwork.Save();

                return Json(new { success = true, message = "Deleted Successfully" });
            }
            #endregion
        }

    }
}
