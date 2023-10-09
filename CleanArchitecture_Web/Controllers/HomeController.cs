﻿using CleanArchitecture_Web.Models;
using CleanArchitecture_Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Presentation;
using System.Diagnostics;
using WhiteLagoon.Application.Common.Interface;
using WhiteLagoon.Application.Common.Utility;

namespace CleanArchitecture_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnviroment;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnviroment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnviroment = webHostEnviroment;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateTime.Now,
            };
            return View(homeVM);
        }
        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateTime checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckIn).ToList();

            foreach(var villa in villaList)
            {
               int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0?true:false;
            }
            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };
            return PartialView("_VillaList",homeVM);
        }
        [HttpPost]
        public IActionResult GeneratePTTExport(int id)
        {
            var villa = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").FirstOrDefault(x => x.Id == id);
            if(villa is null)
            {
                return RedirectToAction(nameof(Error));
            }
            string basePath = _webHostEnviroment.WebRootPath;
            string filePath = basePath + @"/exports/ExportVillaDetails.pptx";
             
            using IPresentation presentation = Presentation.Open(filePath);

            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if(shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if(shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy);
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size : {0} Sqft", villa.Sqft);
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Price Per Night : {0} adults", villa.Price.ToString("c")); ;
            }

            shape = slide.Shapes.FirstOrDefault(u=> u.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape is not null)
            {
                List<string> listItems = villa.VillaAmenity.Select(x => x.Name).ToList();
                shape.TextBody.Text = "";

                foreach(var item in listItems)
                {
                    IParagraph paraGraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paraGraph.AddTextPart(item);

                    paraGraph.ListFormat.Type = ListType.Bulleted;
                    paraGraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(14, 148, 152);
                }
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;
            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;
                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream  imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
            }

            MemoryStream stream = new();
            presentation.Save(stream);
            stream.Position = 0;
            return File(stream, "application/pptx", "villa.pptx");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}