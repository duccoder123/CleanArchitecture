using WhiteLagoon.Domain.Entities;

namespace CleanArchitecture_Web.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Villa> VillaList { get; set; }   
        public DateTime CheckInDate { get; set; }   
        public DateTime? CheckOuteDate { get; set; }
        public int Nights { get; set; }
    }
}
