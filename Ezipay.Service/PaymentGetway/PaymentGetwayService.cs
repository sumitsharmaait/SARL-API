using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.ViewModel;
using Ezipay.ViewModel.TokenViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.PaymentGetway
{
    public class PaymentGetwayService : IPaymentGetwayService
    {
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly ITokenRepository _tokenRepository;
        public PaymentGetwayService()
        {
            _walletUserRepository = new WalletUserRepository();
            _tokenRepository = new TokenRepository();
        }
        public async Task<SessionInfoResponse> GetWalletSessionInfo(SessionInfoRequest request)
        {
            var result = new SessionInfoResponse();

            result = await _walletUserRepository.GetWalletSessionInfo(request.WalletUserId);
            if (result != null)
            {
                result.WalletBalance = Convert.ToDouble(result.CurrentBalance);
                var token = new TokenRequest { DeviceUniqueId = request.DeviceUniqueId, WalletUserId = request.WalletUserId };
                var sessionToken = await _tokenRepository.GenerateToken(token);
                result.PrivateKey = sessionToken.PrivateKey;
                result.PublicKey = sessionToken.PublicKey;
                result.Token = sessionToken.Token;
            }

            return result;
        }

        public async Task<int> PayMoney(PGPayMoneyVM request)
        {
            int result = 0;
            var walletUser = await _walletUserRepository.GetWalletUser(request.WalletUserId);
            if (walletUser != null)
            {
                double currentBalance = Convert.ToDouble(walletUser.CurrentBalance);
                if (currentBalance < request.Amount)
                {
                    return 2; //----Insufficient wallet balance
                }

                walletUser.CurrentBalance = Convert.ToString(currentBalance - request.Amount);

                await _walletUserRepository.UpdateUserDetail(walletUser);

                var transEntity = new WalletTransaction
                {
                    SenderId = walletUser.WalletUserId,
                    ReceiverId = walletUser.WalletUserId,
                    AccountNo = request.LoanNumber,
                    TransactionStatus = (int)TransactionStatus.Completed,
                    TransactionType = "DEBIT",
                    TransactionTypeInfo = (int)TransactionTypeInfo.LendingAppTransaction,
                    Comments = "Payment for Lending App",
                    IsActive = true,
                    IsDeleted = false,
                    IsAdminTransaction = false,
                    TransactionId = "0",
                    WalletServiceId = 144,
                    TotalAmount = Convert.ToString(request.Amount),
                    CreatedDate = DateTime.UtcNow,
                    WalletAmount = walletUser.CurrentBalance,
                    WalletTransactionId = 0,
                    BankBranchCode = string.Empty,
                    BankTransactionId = "0",
                    BenchmarkCharges = "0",
                    CommisionAmount = "0",
                    CommisionId = 0,
                    CommisionPercent = "0",
                    FlatCharges = "0",
                    InvoiceNo = string.Empty,
                    IsAddDuringPay = false,
                    IsBankTransaction = false,
                    ServiceTax = "0",
                    MerchantCommissionAmount = "0",
                    ServiceTaxRate = 0,
                    UpdatedDate = DateTime.UtcNow,
                    MerchantCommissionId = 0,
                    VoucherCode = string.Empty,
                    UpdatedOn = DateTime.UtcNow


                };

                result = await _walletUserRepository.InsertWalletTransaction(transEntity);
            }
            return result;
        }

        public async Task<CashInCashOutResponse> CashInCashOut(CashInCashOutRequest request)
        {
            var response = new CashInCashOutResponse();

            var tran = new WalletTransaction();
            //var userData = db.WalletUsers.Where(x => x.WalletUserId == request.UserId && x.EmailId == request.EmailId).FirstOrDefault();

            if (request.merchantKey != null && request.apiKey != null)
            {

                //if (request.transactionType.ToLower() == "cashin" && request.amount != null)
                //{
                //    //Merchant details
                //    var merchantData = await _walletUserRepository.GetMerchantApiKey(request.apiKey, request.merchantKey);
                //    long merchantWalletUserId = Convert.ToInt32(merchantData.WalletUserId);
                //    var userData = await _walletUserRepository.GetWalletUser(merchantWalletUserId);
                //    decimal userAmount = Convert.ToDecimal(userData.CurrentBalance);
                //    //User details
                //    long senderWalletId = Convert.ToInt32(request.senderId);
                //    var senderWalletData = await _walletUserRepository.GetWalletUser(senderWalletId);
                //    decimal senderAmount = Convert.ToDecimal(senderWalletData.CurrentBalance);

                //    decimal reqAmount = Convert.ToDecimal(request.amount);
                //    userData.CurrentBalance = Convert.ToString(userAmount + reqAmount);
                //    await _walletUserRepository.UpdateUserDetail(userData);
                //    tran.Comments = "Cash In";
                //    tran.InvoiceNo = "Cash In";
                //    tran.TotalAmount = request.amount;
                //    tran.TransactionType = AggragatorServiceType.CREDIT;
                //    tran.IsBankTransaction = false;
                //    tran.BankBranchCode = string.Empty;
                //    tran.BankTransactionId = string.Empty;
                //    tran.CommisionId = 0;
                //    tran.WalletAmount = request.amount;
                //    tran.ServiceTaxRate = 0;
                //    tran.ServiceTax = "0";
                //    tran.WalletServiceId = 159;
                //    tran.SenderId = userData.WalletUserId;
                //    tran.ReceiverId = senderWalletData.WalletUserId;
                //    tran.AccountNo = string.Empty;
                //    tran.TransactionId = "0";
                //    tran.IsAdminTransaction = false;
                //    tran.IsActive = true;
                //    tran.IsDeleted = false;
                //    tran.CreatedDate = DateTime.UtcNow;
                //    tran.UpdatedDate = DateTime.UtcNow;
                //    tran.TransactionTypeInfo = (int)TransactionTypeInfo.CashIn;
                //    tran.TransactionStatus = (int)TransactionStatus.Completed;
                //    tran.MerchantCommissionAmount = "0";
                //    tran.CommisionAmount = "0";
                //    tran.VoucherCode = string.Empty;
                //    tran.MerchantCommissionId = 0;
                //    tran.UpdatedOn = DateTime.Now;
                //    tran.BenchmarkCharges = 0;
                //    tran.FlatCharges = 0;
                //    tran.CommisionPercent = 0;

                //    response.Message = "Cash in successfully";
                //    response.IsSuccess = true;
                //    response.UserId = userData.WalletUserId.ToString();
                //}
                if (request.transactionType.ToLower() == "cashout" && request.amount != null)
                {
                    //Merchant details
                    var merchantData = await _walletUserRepository.GetMerchantApiKey(request.apiKey, request.merchantKey);
                    long merchantWalletUserId = Convert.ToInt32(merchantData.WalletUserId);
                    var userData = await _walletUserRepository.GetWalletUser(merchantWalletUserId);
                    decimal userAmount = Convert.ToDecimal(userData.CurrentBalance);
                    //User details
                    long senderWalletId = Convert.ToInt32(request.senderId);
                    var senderWalletData = await _walletUserRepository.GetWalletUser(senderWalletId);
                    var sender = await _walletUserRepository.GetUserDetailById(senderWalletId);
                    decimal senderAmount = Convert.ToDecimal(senderWalletData.CurrentBalance);


                    decimal reqAmount = Convert.ToDecimal(request.amount);
                    if (senderAmount > 0 && senderAmount >= reqAmount && senderWalletData.CurrentBalance != null && reqAmount > 0)
                    {
                        decimal finalAmount = userAmount + reqAmount;
                        senderWalletData.CurrentBalance = Convert.ToString(senderAmount - reqAmount);
                        userData.CurrentBalance = finalAmount.ToString();
                        await _walletUserRepository.UpdateUserDetail(senderWalletData);
                        await _walletUserRepository.UpdateUserDetail(userData);
                        tran.Comments = "Cash out";
                        tran.InvoiceNo = "Cash out";
                        tran.TotalAmount = request.amount;
                        tran.TransactionType = AggragatorServiceType.CREDIT;
                        tran.IsBankTransaction = false;
                        tran.BankBranchCode = string.Empty;
                        tran.BankTransactionId = string.Empty;
                        tran.CommisionId = 0;
                        tran.WalletAmount = request.amount;
                        tran.ServiceTaxRate = 0;
                        tran.ServiceTax = "0";
                        tran.WalletServiceId = 159;
                        tran.SenderId = senderWalletData.WalletUserId;
                        tran.ReceiverId = userData.WalletUserId;
                        tran.AccountNo = string.Empty;
                        tran.TransactionId = "0";
                        tran.IsAdminTransaction = false;
                        tran.IsActive = true;
                        tran.IsDeleted = false;
                        tran.CreatedDate = DateTime.UtcNow;
                        tran.UpdatedDate = DateTime.UtcNow;
                        tran.TransactionTypeInfo = (int)TransactionTypeInfo.CashIn;
                        tran.TransactionStatus = (int)TransactionStatus.Completed;
                        tran.MerchantCommissionAmount = "0";
                        tran.CommisionAmount = "0";
                        tran.VoucherCode = string.Empty;
                        tran.MerchantCommissionId = 0;
                        tran.UpdatedOn = DateTime.Now;
                        tran.BenchmarkCharges = "0";
                        tran.FlatCharges = "0";
                        tran.CommisionPercent = "0";
                    }
                    response.Amount = tran.WalletAmount;
                    response.Message = "Cash out successfully";
                    response.IsSuccess = true;
                    response.UserId = userData.WalletUserId.ToString();
                    response.Sender =senderWalletData.WalletUserId.ToString();
                    response.EmailId = sender.EmailId;
                    response.TransactionType = "cashout";
                }
                else
                {
                    response.Message = "Transaction type is not found";
                    response.IsSuccess = false;
                }
            }
            var result = await _walletUserRepository.InsertWalletTransaction(tran);

            return response;
        }
    }
}
