using CleanArchitecture_Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interface;
using WhiteLagoon.Application.Common.Utility;

namespace CleanArchitecture_Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending || u.Status == SD.StatusCancelled);
            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate &&
            u.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate &&
            u.BookingDate <= currentMonthStartDate);

            //RadialBarChartVM radialBarChartVM = new();
            //int increaseDecreaseRatio = 100;
            //if(countByPreviousMonth != 0)
            //{
            //    increaseDecreaseRatio = Convert.ToInt32((countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth *100);
            //}
            //radialBarChartVM.TotalCount = totalBookings.Count();
            //radialBarChartVM.CountInCurrentMonth = countByCurrentMonth ;
            //radialBarChartVM.HasRatioIncreased = currentMonthStartDate > previousMonthStartDate;
            //radialBarChartVM.Series = new int[] { increaseDecreaseRatio };


            return Json(GetRadialChartDataModel(totalBookings.Count(),countByCurrentMonth,countByPreviousMonth));
        }


        public async Task<IActionResult> GetRegisterUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();
            var countByCurrentMonth = totalUsers.Count(u => u.CreateAt >= currentMonthStartDate &&
            u.CreateAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(u => u.CreateAt >= previousMonthStartDate &&
            u.CreateAt <= currentMonthStartDate);

            return Json(GetRadialChartDataModel(totalUsers.Count(), countByPreviousMonth, countByCurrentMonth));
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending
                || u.Status == SD.StatusCancelled);
            var totalRevenue =Convert.ToInt32(totalBookings.Sum(u => u.TotalCost));
            var countByCurrentMonth = totalBookings.Where(u => u.BookingDate >= currentMonthStartDate &&
            u.BookingDate <= DateTime.Now).Sum(u=>u.TotalCost);
            var countByPreviousMonth = totalBookings.Where(u => u.BookingDate >= previousMonthStartDate &&
            u.BookingDate <= currentMonthStartDate).Sum(u => u.TotalCost);

            return Json(GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        private static RadialBarChartVM GetRadialChartDataModel(int totalCount, double currentMonthCount,double prevMonthCount)
        {

            RadialBarChartVM radialBarChartVM = new();
            int increaseDecreaseRation = 100;
            if (prevMonthCount != 0)
            {
                increaseDecreaseRation = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }

            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth =Convert.ToInt32(currentMonthCount);
            radialBarChartVM.HasRatioIncreased = currentMonthCount > prevMonthCount;
            radialBarChartVM.Series = new int[] { increaseDecreaseRation };
            return radialBarChartVM;
        } 
    }
}
