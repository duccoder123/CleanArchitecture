using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace CleanArchitecture_Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaRepository _villaRepo;
        public VillaController(IVillaRepository villaRepo)
        {
            _villaRepo = villaRepo;
        }
        public IActionResult Index()
        {
            var villa = _villaRepo.GetAll();
            return View(villa);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("name", "The description cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _villaRepo.Add(obj);
                _villaRepo.Save();
                TempData["success"] = "Villa Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int villaId)
        {

            Villa? obj = _villaRepo.Get(u => u.Id == villaId);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Update(Villa obj)
        {

            if (ModelState.IsValid)
            {
                _villaRepo.Update(obj);
                _villaRepo.Save();
                TempData["success"] = "Villa Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        public IActionResult Delete(int villaId)
        {

            Villa? obj = _villaRepo.Get(u => u.Id == villaId);
            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objFrDb = _villaRepo.Get(u => u.Id == obj.Id);
            if (objFrDb is not null)
            {
                _villaRepo.Remove(objFrDb);
                _villaRepo.Save();
                TempData["success"] = "Villa Deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
