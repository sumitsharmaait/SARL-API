using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.AdminViewModel
{
    public class SubadminListRequest : SearchRequest
    {

    }
    public class SubadminListResponse
    {
        public SubadminListResponse()
        {
            TotalCount = 0;
            SubadminList = new List<SubadminList>();
            CompleteNavigationList = new List<NavigationList>();
        }

        public int TotalCount { get; set; }
        public List<SubadminList> SubadminList { get; set; }
        public List<NavigationList> CompleteNavigationList { get; set; }
    }

    public class SubadminList
    {
        public SubadminList()
        {
            FunctionList = new List<ModuleFunctionModel>();
        }
        public long SubadminId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
        public string Password { get; set; }
        public string IsdCode { get; set; }
        public List<NavigationList> NavigationList { get; set; }
        public string Functions { get; set; }
        public List<ModuleFunctionModel> FunctionList { get; set; }

    }
    public class ModuleFunctionModel
    {
        public long Id { get; set; }
        public string FunctionName { get; set; }
    }
    public class SubAdminRequest
    {
        public long SubadminId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public string IsdCode { get; set; }
        public List<NavigationList> NavigationList { get; set; }
        public long AdminId { get; set; } //log key

    }
    public class NavigationList
    {
        public NavigationList()
        {
            FunctionList = new List<ModuleFunctionModel>();
        }
        public long NavigationId { get; set; }
        public string Navigation { get; set; }
        public bool NavigationForUser { get; set; }
        public string Functions { get; set; }
        public List<ModuleFunctionModel> FunctionList { get; set; }
    }



    public class SubadminSaveResponse
    {

        public SubadminSaveResponse()
        {
            this.statusCode = 0;
        }

        public int statusCode;
        public int RstKey { get; set; }

    }



    public class IsdCodeListResponse
    {
        public List<IsdCodeList> isdCodeList;

        public IsdCodeListResponse()
        {
            this.isdCodeList = new List<IsdCodeList>();
        }

    }



    public class IsdCodeList
    {
        public string IsdCode { get; set; }
    }
   
    public class SubAdminManageRequest
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public long AdminId { get; set; } //log key
    }
    public class UpdateSubAdminPermissionRequest
    {
        [Required]
        public long SubAdminId { get; set; }
        [Required]
        public long NavigationId { get; set; }
        public long AdminId { get; set; } //log key
    }
}
