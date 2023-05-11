using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ezipay.Service.AdminService
{
    public interface IUserApiService
    {

        Task<UserListResponse> UserList(UserListRequest request);
        Task<int> EnableDisableUser(UserManageRequest request);
        Task<UserEmailVerifyResponse> VerfiyByEmailId(string token);
        Task<ViewUserTransactionResponse> UserTransactions(ViewUserTransactionRequest request);
        Task<CreditDebitResponse> CreditDebitUserAccount(CreditDebitRequest request);
        Task<bool> ManageTransaction(SetTransactionLimitRequest request);
        Task<UserTransactionLimitDetailsResponse> GetTransactionLimitDetails(UserDetailsRequest request);
        Task<UserDetailsResponse> UserDetails(UserDetailsRequest request);
       
        Task<DownloadReportResponse> DownloadReportWithData(DownloadReportApiRequest request);
        Task<UserListResponse> DeletedUserList(UserListRequest request);
        Task<string> SaveImage(HttpPostedFileBase image, string PreviousImage);
        Task<bool> SaveUserDocument(DocumentUploadRequest request, int type);
        Task<UserDocumentDetailsResponse> ViewDocumentDetails(UserDetailsRequest request);
        Task<int> ChangeUserDocumentStatus(DocumentChangeRequest request);
        Task<UserListResponse> PendingKycUserList(UserListRequest request);
        Task<MemoryStream> ExportUserListReport(DownloadLogReportRequest request);
        Task<int> VerifyEmail(DocumentChangeRequest request);
        
        Task<List<DuplicateCardNoVMResponse>> GetduplicatecardnoList(string Cardno,long Walletuserid);
        Task<bool> Insertduplicatecardno(DuplicateCardNoVMRequest request);

        Task<UserBlockUnblockDetailResponse> EnableDisableUserList(UserListRequest request);
    }
}
