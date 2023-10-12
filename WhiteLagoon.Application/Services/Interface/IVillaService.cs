﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interface
{
    public interface IVillaService
    {
        IEnumerable<Villa> GetAllVillas();
        Villa GetVillaById(int id);
        void CreateVilla(Villa villa);
        void UpdateVilla(Villa villa);
        bool DeleteVilla(int id);
        IEnumerable<Villa> GetVillasAvailablilityByDate(int nights, DateTime checkInDate);
        bool IsVillaAvailableByDate(int villaId, int nights, DateTime checkInDate);

    }
}
