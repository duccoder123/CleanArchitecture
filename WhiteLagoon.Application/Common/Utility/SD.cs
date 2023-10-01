using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon.Application.Common.Utility
{
    public class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin  = "Admin";

        public const string StatusPending = "Pending"; // chưa giải quyết
        public const string StatusApproved = "Approved"; // xác nhận
        public const string StatusCheckIn = "CheckedIn"; // đi vào
        public const string StatusCompleted = "Completed"; // hoàn thành
        public const string StatusCancelled = "Cancelled"; // huy bo
        public const string StatusRefunded = "Refunded"; // đền bù 
    }
}
