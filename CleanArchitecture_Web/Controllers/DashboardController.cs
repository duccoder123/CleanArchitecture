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

        public async Task<IActionResult> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
            (u.Status != SD.StatusPending || u.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(u => u.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;
            PieChartVM pieChartVM = new()
            {
                Labels = new string[] { "New Customer", "Returning Customer Booking" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return Json(pieChartVM);
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
            u.BookingDate.Date <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u=> new
                {
                    DateTime = u.Key,
                    NewBookingCount = u.Count(),    
                });
            var customerData = _unitOfWork.User.GetAll(u => u.CreateAt >= DateTime.Now.AddDays(-30) &&
            u.CreateAt <= DateTime.Now)
                .GroupBy(b => b.CreateAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });

            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
                (booking, customer) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewCustomerCount = customer.Select(x => x.NewCustomerCount).FirstOrDefault()
                });

            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
                (customer, booking) => new
                {
                    customer.DateTime,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                    customer.NewCustomerCount
                });

            var mergeData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergeData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergeData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergeData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();
            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                 new ChartData
                {
                    Name = "New Customers",
                    Data = newCustomerData
                }
            };
            LineChartVM lineChartVM = new()
            {
                Categories = categories,
                Series = chartDataList
            };
         

            return Json(bookingData);
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
