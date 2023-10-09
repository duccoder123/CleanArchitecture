using CleanArchitecture_Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility
{
    public class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending"; // chưa giải quyết
        public const string StatusApproved = "Approved"; // xác nhận
        public const string StatusCheckIn = "CheckedIn"; // đi vào
        public const string StatusCompleted = "Completed"; // hoàn thành
        public const string StatusCancelled = "Cancelled"; // huy bo
        public const string StatusRefunded = "Refunded"; // đền bù 

        public static int VillaRoomsAvailable_Count(int villaId,
            List<VillaNumber> villaNumberList, DateTime checkInDate, int nights,
            List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomForAllNight = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();
            for (int i = 0; i < nights; i++)
            {
                var villasBooked = bookings.Where(u => u.CheckInDate <= checkInDate.AddDays(i)
                && u.CheckOutDate >= checkInDate.AddDays(i) && u.VillaId == villaId);
                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.VillaId))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if (totalAvailableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if(finalAvailableRoomForAllNight > totalAvailableRooms)
                    {
                        finalAvailableRoomForAllNight = totalAvailableRooms;
                    }
                }
            }
            return finalAvailableRoomForAllNight;
        }

        public static RadialBarChartDTO GetRadialChartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {

            RadialBarChartDTO radialBarChartVM = new();
            int increaseDecreaseRation = 100;
            if (prevMonthCount != 0)
            {
                increaseDecreaseRation = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }

            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            radialBarChartVM.HasRatioIncreased = currentMonthCount > prevMonthCount;
            radialBarChartVM.Series = new int[] { increaseDecreaseRation };
            return radialBarChartVM;
        }
    }
}
