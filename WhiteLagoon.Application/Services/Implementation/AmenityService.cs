using System;
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
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Add(amenity);
            _unitOfWork.Save();
        }

        public bool DeleteAmenity(int id)
        {
            try
            {

                Amenity? objFrDb = _unitOfWork.Amenity.Get(u => u.Id == id);
                if (objFrDb is not null)
                {
                    _unitOfWork.Amenity.Remove(objFrDb);
                    _unitOfWork.Save();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<Amenity> GetAllAmenity()
        {
            return _unitOfWork.Amenity.GetAll(includeProperties: "Villa");
        }

        public Amenity GetAmenityById(int id)
        {
            return _unitOfWork.Amenity.Get(u => u.Id == id);
        }

        public void UpdateAmenity(Amenity amenity)
        {
            _unitOfWork.Amenity.Update(amenity);
            _unitOfWork.Save();
        }
    }


}
