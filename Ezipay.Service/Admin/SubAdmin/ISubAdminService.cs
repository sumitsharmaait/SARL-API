using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Service.Admin.SubAdmin
{
    public interface ISubAdminService
    {
        Task<SubadminListResponse> GetSubAdmins(SubadminListRequest request);
        Task<SubadminSaveResponse> SaveSubAdmins(SubAdminRequest request);
        Task<bool> DeleteSubadmin(UserDeleteRequest request);
        Task<bool> EnableDisableSubAdmin(SubAdminManageRequest request);
    }
}
