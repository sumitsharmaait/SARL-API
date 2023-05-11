using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo
{
    public interface IUserApiRepository
    {
        Task<UserListResponse> UserList(UserListRequest request);
        Task<bool> EnableDisableUser(UserManageRequest request);
        Task<bool> Delete(UserManageRequest request);
        Task<UserEmailVerifyResponse> VerfiyByEmailId(string token);
        Task<ViewUserTransactionResponse> UserTransactions(ViewUserTransactionRequest request);
        Task<CreditDebitResponse> CreditDebitUserAccount(CreditDebitRequest request, int WalletTransactionSubTypesId, long AdminUserType);
        Task<bool> ManageTransaction(SetTransactionLimitRequest request);
        Task<UserTransactionLimitDetailsResponse> GetTransactionLimitDetails(UserDetailsRequest request);
        Task<UserDetailsResponse> UserDetails(UserDetailsRequest request);
       
        Task<DownloadReportResponse> DownloadReportWithData(DownloadReportApiRequest request);
        Task<List<UserList>> DeletedUserList(UserListRequest request);
        Task<WalletUser> GetUserById(long userId);
        Task<UserDocument> GetUserDocumentByUserId(long userId);
        Task<int> InsertDocument(UserDocument entity);
        Task<int> UpdateUser(WalletUser walletUser);
        Task<int> UpdateDocument(UserDocument usrDoc);
        Task<UserDocumentDetailsResponse> GetUserDocuments(long userId);
        Task<bool> CheckEmail(string emailId,string mobile);
        Task<List<UserList>> PendingKycUserList(UserListRequest request);
        Task<UserListResponse> GenerateUserList(DownloadLogReportRequest request);
        Task<List<DocModel>> GetMerchantDocuments(long walletUserId);
        Task<List<DuplicateCardNoVMResponse>> GetduplicatecardnoList(string Cardno,long Walletuserid);
        Task<int> Insertduplicatecardno(DuplicateCardNoVMRequest request);

        Task<int> SaveBlockUnblockDetails(UserManageRequest request);

        Task<UserBlockUnblockDetailResponse> EnableDisableUserList(UserListRequest request);
    }
}
