using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.ResortViewModel
{
    public class HotelRequest
    {
        public int Id { get; set; }
        public string HotelImage { get; set; }
        public string HotelName { get; set; }
        public string Location { get; set; }
        public string PdfUrl { get; set; }
        public int NoOfRooms { get; set; }
        public decimal CostOfRooms { get; set; }
        public int MaxGuest { get; set; }
        public int AvailableRooms { get; set; }
    }

    public class HotelMasterResponse
    {
        public int Id { get; set; }
        public string HotelName { get; set; }
        public string HotelImage { get; set; }
        public string Location { get; set; }
        public string PdfUrl { get; set; }
        public Nullable<long> AvailableRooms { get; set; }
        public Nullable<long> NoOfRooms { get; set; }
        public Nullable<decimal> CostOfRooms { get; set; }
        public Nullable<long> MaxGuest { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpDatedDate { get; set; }
    }

    public class DeleteResortRequest
    {
        public int id { get; set; }
        public bool IsActive { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class HotelBookingRequest
    {
        public int HotelId { get; set; }
        public long WalletUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public int NoOfGuest { get; set; }
        public string CostOfRoom { get; set; }      
    }
}
