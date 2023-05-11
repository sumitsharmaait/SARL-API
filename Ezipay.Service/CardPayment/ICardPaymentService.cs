
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.MerchantPaymentViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.ThridPartyApiVIewModel;
using Ezipay.ViewModel.WalletUserVM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.CardPayment
{
    public interface ICardPaymentService
    {
        Task<CardAddMoneyResponse> CardPayment(CardAddMoneyRequest request, string sessionToken);
        Task<AddMoneyAggregatorResponse> MobileServicesAggregator(AddMoneyAggregatoryRequest Request, string sessionToken, long WalletUserId = 0);

        Task<int> MerchantContent(MerchantTransactionRequest request, string TransactionId, long? WalletUserId, int? TransactionStatus);

        Task<int> PayMoneyContent(PayMoneyContent request, string TransactionId, long? WalletUserId, int? TransactionStatus);
        Task<CardPaymentSaveResponse> SavePaymentResponse(CardPaymentWebResponse request);

        Task<AddCashDepositToBankResponse> AddCashDepositToBankServices(AddCashDepositToBankRequest Request, string sessionToken, long WalletUserId = 0);


        Task<DuplicateCardNoVMResponse> AddNewCardNo(DuplicateCardNoVMRequest Request, string sessionToken);
        Task<OtpResponse> WalletSendOtp(OtpRequest request, string sessionToken);
        Task<UserExistanceResponse> WalletVerifyOtp(VerifyOtpRequest request, string sessionToken);

        Task<List<MobileNoListResponse>> GetMobileNoList(string sessionToken);

        Task<MasterCardPaymentUBAResponse> NewMasterCardPayment(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse> SaveMasterCardPaymentResponse(MasterCardPaymentResponse request);

        Task<SeerbitResponse> GetSeerbitCardPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<SeerbitResponse> SaveSeerbitPaymentResponse(SeerbitRequest request);

        Task<MasterCardPaymentUBAResponse> GetGTBCIVPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken);


        Task<NgeniunsResponse> GetngeniusCardPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse> SavengeniusPaymentResponse(string reference);
        Task<MasterCardPaymentUBAResponse> NewMasterCardPayment2(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse> SaveMasterCardPayment2Response(MasterCardPaymentResponse request);

        Task<flutterPaymentUrlResponse> GetCardPaymentUrlForflutterwave(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse> SaveflutterCardPaymentResponse(fluttercallbackResponse request);

        //Task<flutterbankResponse> GetCardPaymentUrlForNGNbankflutter(ThirdpartyPaymentByCardRequest request, string headerToken);
        //Task<AddMoneyAggregatorResponse> SaveflutterBankPaymentResponse(BankPaymentWebResponse request);

        Task<UpdateTransactionResponse> SaveflutterPayBankTransferPaymentResponse(string txnreverifystatus, string invoiceno);

        Task<flutterPaymentUrlResponse> GetCardPaymentUrlForNGNbankflutterUSD(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<flutterPaymentUrlResponse> GetCardPaymentUrlForNGNbankflutterEuro(ThirdpartyPaymentByCardRequest request, string headerToken);

        //Task<flutterbankResponse> GetZenithBankUrlByOTP(ZenithBankOTPRequest request, string headerToken);

        Task<flutterbanktransferauthorization> GetCardPaymentUrlForNGNbanktransferflutter(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse> SaveflutteraddmoneNGNBankTransferPaymentResponse(string txnreverifystatus, string invoiceno, string txt);

        Task<binancePaymentUrlResponse> GetCardPaymentUrlForbinance(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<binancewalletResponse> GetCardPaymentUrlForbinancewallet(ThirdpartyPaymentByCardRequest request, string headerToken);

        Task<FXKUDIPaymentUrlResponse> GetCardPaymentUrlForFXKUDI(ThirdpartyPaymentByCardRequest request, string headerToken);

        Task<AddMoneyAggregatorResponse> SaveflutteraddmoneGlobalNigeriaBankTransferResponse(string txnreverifystatus, string invoiceno, string currency, string payment_type);
        Task<AddMoneyAggregatorResponse> SaveflutterPayGlobalNigeriaBankTransferPaymentResponse(string txnreverifystatus, string invoiceno, string currency, string payment_type);


        //Task<AddMoneyAggregatorResponse> SaveflutterCardPaymentResponsewebhook(fluttercallbackResponsewebhook request);



        Task<merchantPaymentUrlResponse> merchantNewFlowPaymentUrl(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse> SaveMerchantPaymentResponse(string txt_ref);

        Task<string> notificationsarl();
    }
}
