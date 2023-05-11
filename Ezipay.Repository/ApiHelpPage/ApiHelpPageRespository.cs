using Ezipay.ViewModel.ApiHelpPage;
using Ezipay.ViewModel.CardPaymentViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.MasterDataViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.TokenViewModel;
using Ezipay.ViewModel.WalletUserVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.ApiHelpPage
{
    public class ApiHelpPageRespository: IApiHelpPageRespository
    {
        public List<ApiHelpPageModel> ApiList()
        {
            var response = new Response<object>();
            response.isSuccess = true;
            response.message = string.Empty;
            response.status = 200;
            List<ApiHelpPageModel> list = new List<ApiHelpPageModel>();

            #region Session Controller
            response.result = new TempTokenResponse();
            list.Add(new ApiHelpPageModel { ControllerName = "Session", ApiName = "TempToken", Request = JsonConvert.SerializeObject(new TempTokenRequest()), Response = JsonConvert.SerializeObject(response) });
            #endregion

            #region User Controller
            response.result = new Boolean();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "SendOtp", Request = JsonConvert.SerializeObject(new OtpRequest()), Response = JsonConvert.SerializeObject(response) });
            //
            response.result = new Boolean();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "SendVerificationEmail", Request = JsonConvert.SerializeObject(new SendVerificationEmailRequest()), Response = JsonConvert.SerializeObject(response) });

            response.result = new UserSignupResponse();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "SignUp", Request = JsonConvert.SerializeObject(new UserSignupRequest()), Response = JsonConvert.SerializeObject(response) });

            response.result = new UserLoginResponse();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "Login", Request = JsonConvert.SerializeObject(new UserLoginRequest()), Response = JsonConvert.SerializeObject(response) });

            response.result = new Boolean();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "Logout", Response = JsonConvert.SerializeObject(response) });

            //response.result = new ForgotPasswordRequest();
            //list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "ForgetPassword", Request = JsonConvert.SerializeObject(new ForgotPasswordRequest()), Response = JsonConvert.SerializeObject(response) });

            response.result = new Boolean();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "VerifyOtp", Request = JsonConvert.SerializeObject(new VerifyOtpRequest()), Response = JsonConvert.SerializeObject(response) });

            var IsdCode = new List<IsdCodesResponse>();
            IsdCode.Add(new IsdCodesResponse());
            response.result = IsdCode;
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "IsdCodes", Response = JsonConvert.SerializeObject(response) });



            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "CountryIsdCodes", Response = JsonConvert.SerializeObject(response) });

            response.result = new Boolean();
            list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "ChangeNotification", Response = JsonConvert.SerializeObject(response) });

            //var PaymentTransactions = new List<PaymentTransactionResponse>();
            //PaymentTransactions.Add(new PaymentTransactionResponse());
            //response.result = PaymentTransactions;
            //list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "PaymentTransactions", Request = JsonConvert.SerializeObject(new PaymentTransactionRequest()), Response = JsonConvert.SerializeObject(response) });

            //new change
            //var link = new List<ShareAndEarnResponse>();
            //link.Add(new ShareAndEarnResponse());
            //response.result = link;
            //list.Add(new ApiHelpPageModel { ControllerName = "User", ApiName = "ShareAndEarn", Response = JsonConvert.SerializeObject(response) });
            //response.result = new Boolean();
            #endregion

            #region Payment Controller
            response.result = new CardAddMoneyResponse();
            list.Add(new ApiHelpPageModel { ControllerName = "Payment", ApiName = "CardPayment", Request = JsonConvert.SerializeObject(new CardAddMoneyRequest()), Response = JsonConvert.SerializeObject(response) });
            #endregion

            #region UserProfile Controller
            //response.result = new Boolean();
            //list.Add(new ApiHelpPageModel { ControllerName = "UserProfile", ApiName = "ChangePassword", Request = JsonConvert.SerializeObject(new ChangePasswordRequest()), Response = JsonConvert.SerializeObject(response) });

            //var FAQ = new List<FAQResponse>();
            //FAQ.Add(new FAQResponse());
            //response.result = FAQ;
            //list.Add(new ApiHelpPageModel { ControllerName = "UserProfile", ApiName = "FAQ", Response = JsonConvert.SerializeObject(response) });

            //var FeedBackTypes = new List<FeedbackTypeResponse>();
            //FeedBackTypes.Add(new FeedbackTypeResponse());
            //response.result = FeedBackTypes;
            //list.Add(new ApiHelpPageModel { ControllerName = "UserProfile", ApiName = "FeedBackTypes", Response = JsonConvert.SerializeObject(response) });

            //response.result = new Boolean();
            //list.Add(new ApiHelpPageModel { ControllerName = "UserProfile", Request = JsonConvert.SerializeObject(new FeedBackRequest()), ApiName = "SaveFeedBack", Response = JsonConvert.SerializeObject(response) });

            #endregion

            #region WalletService Controller
            //var AppServices = new AppServiceResponse();
            //var pserviceList = new List<WalletServiceResponse>();
            //pserviceList.Add(new WalletServiceResponse());
            //AppServices.PayServices = pserviceList;
            //AppServices.WalletServices = pserviceList;

            //response.result = AppServices;
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletService", ApiName = "AppServices", Response = JsonConvert.SerializeObject(response) });
            //var MerchantList = new List<MerchantsResponse>();
            //MerchantList.Add(new MerchantsResponse());
            //response.result = MerchantList;
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletService", ApiName = "MerchantList", Response = JsonConvert.SerializeObject(response) });



            #endregion

            #region WalletTransactions Controller
            //response.result = new UserDetailByQrCodeResponse();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "UserDetailById", Request = JsonConvert.SerializeObject(new UserDetailByQrCodeRequest()), Response = JsonConvert.SerializeObject(response) });

            //response.result = new WalletTransactionResponse();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "PayMoney", Request = JsonConvert.SerializeObject(new WalletTransactionRequest()), Response = JsonConvert.SerializeObject(response) });

            //response.result = new WalletTransactionResponse();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "MarchantPayment", Request = JsonConvert.SerializeObject(new WalletTransactionRequest()), Response = JsonConvert.SerializeObject(response) });

            //response.result = new WalletTransactionResponse();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "PaymentRequest", Request = JsonConvert.SerializeObject(new WalletTransactionRequest()), Response = JsonConvert.SerializeObject(response) });

            //response.result = new UserDetailResponse();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "UserProfile", Response = JsonConvert.SerializeObject(response) });

            //response.result = new Boolean();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "UpdateUserProfile", Request = JsonConvert.SerializeObject(new UserDetailResponse()), Response = JsonConvert.SerializeObject(response) });

            //var ViewPaymentRequests = new List<PayResponse>();
            //ViewPaymentRequests.Add(new PayResponse());
            //response.result = ViewPaymentRequests;
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "ViewPaymentRequests", Response = JsonConvert.SerializeObject(response) });


            //response.result = new WalletTransactionResponse();
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "ManagePaymentRequest", Request = JsonConvert.SerializeObject(new PayResponse()), Response = JsonConvert.SerializeObject(response) });

            //response.result = new { DateFrom = DateTime.MinValue, DateTo = DateTime.MinValue, DownloadType = 0, TransactionType = 0, Code = 0 };
            //list.Add(new ApiHelpPageModel { HttpVerb = "GET", ControllerName = "WalletTransactions", ApiName = "TransactionStatement", Request = JsonConvert.SerializeObject(new PayResponse()), Response = JsonConvert.SerializeObject(response) });

            //var DownloadReport = new DownloadReportResponse();
            //DownloadReport.ReportData.Add(new ReportData());
            //response.result = DownloadReport;
            //list.Add(new ApiHelpPageModel { ControllerName = "WalletTransactions", ApiName = "DownloadReport", Request = JsonConvert.SerializeObject(new DownloadReportRequest()), Response = JsonConvert.SerializeObject(response) });

            #endregion

            return list;
        }
    }
}
