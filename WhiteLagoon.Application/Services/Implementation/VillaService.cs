﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using WhiteLagoon.Application.Common.Interface;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        
      

        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");
                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);
                villa.ImageUrl = @"\images\VillaImage\" + fileName;

            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600x400";
            }
            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Save();
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                Villa? objFrDb = _unitOfWork.Villa.Get(u => u.Id == id);
                if (objFrDb is not null)
                {
                    if (!string.IsNullOrEmpty(objFrDb.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFrDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _unitOfWork.Villa.Remove(objFrDb);
                    _unitOfWork.Save();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
        }

        public Villa GetVillaById(int id)
        {
            return _unitOfWork.Villa.Get( u=> u.Id == id, includeProperties:"VillaAmenity");  
        }

        public IEnumerable<Villa> GetVillasAvailablilityByDate(int nights, DateTime checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties:"VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckIn).ToList();

            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }
            return villaList;
        }

        public bool IsVillaAvailableByDate(int villaId, int nights, DateTime checkInDate)
        {
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count(villaId, villaNumberList, checkInDate, nights, bookedVillas);
            return roomAvailable > 0;
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);
                villa.ImageUrl = @"\images\VillaImage\" + fileName;

            }
            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Save();
        }
    }

   
}
