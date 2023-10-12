using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Drawing;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interface;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace CleanArchitecture_Web.Controllers
{

    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public BookingController(UserManager<ApplicationUser> userManager,IBookingService bookingService,IVillaService villaService,IVillaNumberService villaNumberService, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _bookingService = bookingService;
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _webHostEnviroment = webHostEnvironment;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateTime checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _villaService.GetVillaById(villaId),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _villaService.GetVillaById(booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            

            if(!_villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
            {
                TempData["success"] = "Room has been sold out";
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkIndDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            _bookingService.CreateBooking(booking);

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId{booking.VillaId}&checkInDate={booking.CheckInDate}&night={booking.Nights}",
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string> { domain + villa.ImageUrl},
                    },
                },
                Quantity = 1
            });
            var service = new SessionService();
            Session session = service.Create(options);

            _bookingService.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFrDb = _bookingService.GetBookingById(bookingId);
            if (bookingFrDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFrDb.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _bookingService.UpdateStatus(bookingFrDb.Id, SD.StatusApproved, 0);
                    _bookingService.UpdateStripePaymentID(bookingFrDb.Id, session.Id, session.PaymentIntentId);
                }
            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);
            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == bookingFromDb.VillaId
                && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
            }
            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType)
        {
            string basePath = _webHostEnviroment.WebRootPath;
            WordDocument document = new WordDocument();
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);
            Booking bookingFrDb = _bookingService.GetBookingById(id);
            TextSelection textSelection = document.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.Name;

            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.Phone;

            textSelection = document.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.Email;

            textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text ="BOOKING ID - " + bookingFrDb.Id;

            textSelection = document.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingFrDb.BookingDate.ToShortDateString();

            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.PaymentDate.ToShortDateString();

            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.CheckInDate.ToShortDateString();

            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.CheckOutDate.ToShortDateString();

            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFrDb.TotalCost.ToString();

            WTable table = new(document);

            table.TableFormat.Borders.LineWidth = 1f;
            //table.TableFormat.Borders.Color = Color.Red;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;

            int rows = bookingFrDb.VillaNumber > 0 ? 3 : 2;
            table.ResetCells(rows, 4);
            WTableRow row0 = table.Rows[0];

            row0.Cells[0].AddParagraph().AppendText("NIGHT");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].AddParagraph().AppendText("TOTAL");
            row0.Cells[3].Width = 80;

            WTableRow row1 = table.Rows[1];

            row1.Cells[0].AddParagraph().AppendText(bookingFrDb.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingFrDb.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((bookingFrDb.TotalCost/bookingFrDb.Nights).ToString("c"));
            row1.Cells[3].AddParagraph().AppendText(bookingFrDb.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;

            if(bookingFrDb.VillaNumber > 0)
            {
                WTableRow row2 = table.Rows[1];
                row1.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingFrDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }

            WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;


            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;

            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);

            document.Replace("<ADDTABLEHERE>", bodyPart, false, false);

            using DocIORenderer renderer = new();
            MemoryStream stream = new();

            if (downloadType == "word")
            {
            document.Save(stream, FormatType.Docx);
            stream.Position = 0;
            return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfDocument = renderer.ConvertToPDF(document);
                pdfDocument.Save(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", "BookingDetails.pdf");
            }
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCheckIn, booking.VillaNumber);
            TempData["success"] = "Booking Updated Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            TempData["success"] = "Booking Completed Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            TempData["success"] = "Booking Cancelled Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();
            var villaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == villaId);
            var checkedInVilla = _bookingService.GetCheckedInVillaNumbers(villaId);
            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }


        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBooking;
            string userId = "";
            if(string.IsNullOrEmpty(status)) 
            {
                status = "";
            }
            if (User.IsInRole(SD.Role_Admin))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                 userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            objBooking = _bookingService.GetAllBookings(userId,status);
            return Json(new { data = objBooking });
        }
        #endregion
    }
}
