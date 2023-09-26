using CleanArchitecture_Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace CleanArchitecture_Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _db;
        public VillaNumberController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var villaNumber = _db.VillaNumber.Include(u => u.Villa).ToList();
            return View(villaNumber);
        }

        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(villaNumberVM);
        }
        [HttpPost]
        public IActionResult Create(VillaNumberVM obj)
        {
            bool roomNumberExists = _db.VillaNumber.Any(u => u.Villa_Number == obj.VillaNumber.Villa_Number);


            if (ModelState.IsValid && !roomNumberExists)
            {
                _db.VillaNumber.Add(obj.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "Villa Number Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            if (roomNumberExists)
            {
                TempData["error"] = "The Villa Number already exists.";
            }
            obj.VillaList = _db.Villas.ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(obj);
        }

        public IActionResult Update(int villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _db.VillaNumber.FirstOrDefault(u => u.Villa_Number == villaNumberId)
            };

            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM obj)
        {
           
            if (ModelState.IsValid)
            {
                _db.VillaNumber.Update(obj.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "VillaNumber has been Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            obj.VillaList = _db.Villas.ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(obj);
        }
        public IActionResult Delete(int villaNumberId)
        {

            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _db.VillaNumber.FirstOrDefault(u => u.Villa_Number == villaNumberId)
            };

            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM obj)
        {
            VillaNumber? objFrDb = _db.VillaNumber.FirstOrDefault(u => u.Villa_Number == obj.VillaNumber.Villa_Number);
            if (objFrDb is not null)
            {
                _db.VillaNumber.Remove(objFrDb);
                _db.SaveChanges();
                TempData["success"] = "Villa Number Deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted";
            return View();
        }
    }
}
