using System;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class ChargeBackResponse
    {
        public long id { get; set; }
        public long Walletuserid { get; set; }
        public string UserEmail { get; set; }
        public string UserMobileNo { get; set; }
        public string IsActiveStatus { get; set; }
        public string CurrentBalance { get; set; }
        public string AmountLimit { get; set; }
        public string Createdby { get; set; }
        public DateTime Createddate { get; set; }
        public string DeleteFlag { get; set; }
        public string Comment { get; set; }
    }

    public class ChargeBackRequest
    {

        public long id { get; set; }
        public long Walletuserid { get; set; }
        public string Amount { get; set; }
        public string Createdby { get; set; }
        public string DeleteFlag { get; set; }
        public string Deleteby { get; set; }
        public string Comment { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class freezeRequest
    {
        public long id { get; set; }
        public long Walletuserid { get; set; }
        public string Amount { get; set; }

        public string FreezeComment { get; set; }

        public string UnFreezeComment { get; set; }
        public string Createdby { get; set; }
        public string DeleteFlag { get; set; }
        public string Deleteby { get; set; }
        public long AdminId { get; set; } //log key
    }

}
