using WhiteLagoon.Domain.Entities;

namespace CleanArchitecture_Web.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Villa> VillaList { get; set; }   
        public DateOnly CheckInDate { get; set; }   
        public DateOnly? CheckOuteDate { get; set; }
        public int Nights { get; set; }
    }
}
