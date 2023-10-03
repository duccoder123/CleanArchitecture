using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interface
{
    public interface IBookingRepository : IRepository<Booking>
    {
        void Update(Booking entities);
        void UpdateStatus(int bookingId, string orderStatus, int villaNumber);
        void UpdateStripePaymentID(int bookingId,    string sessionId, string paymentIntentId); 
    }
}
