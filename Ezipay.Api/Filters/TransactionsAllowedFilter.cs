using ezeePay.Utility.CommonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using ezeePay.Utility.Enums;
using Newtonsoft.Json;
using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.SendPushViewModel;
using Ezipay.Utility.common;
using Ezipay.Service.WalletSettingService;
using Ezipay.Service.TokenService;
using Ezipay.Service.ApiHelpPage;
using Ezipay.ViewModel.common;
using Ezipay.Utility.SendPush;
using Ezipay.Service.UserService;

namespace Ezipay.Api.Filters
{
    public class TransactionsAllowed : ActionFilterAttribute
    {
        private ITokenService _tokenService;
        private IApiHelpPageService _apiHelpPageService;
        private ISendPushNotification _sendPushNotification;
        private IWalletUserService _walletUserService;
        public TransactionsAllowed()
        {
            _tokenService = new TokenService();
            _apiHelpPageService = new ApiHelpPageService();
            _sendPushNotification = new SendPushNotification();
            _walletUserService = new WalletUserService();
        }
        public override void OnActionExecuting(HttpActionContext context)
        {
            bool isAllowed = false;

            try
            {
                isAllowed = new WalletSettings().IsTransactionAllowed();
            }
            catch (Exception ex)
            {

            }

            if (!isAllowed)
            {
                var tokenHeader = context.Request.Headers.Where(x => x.Key.ToLower() == "token").FirstOrDefault().Value.FirstOrDefault();
                var Userdata = _walletUserService.GetUserProfileForTransaction(tokenHeader);
                //var Userdata = await _walletUserRepository.GetCurrentUser(data.WalletUserId);
                PushNotificationModel push = new PushNotificationModel();
                push.deviceType = (int)Userdata.DeviceType;
                push.deviceKey = Userdata.DeviceToken;
                push.SenderId = Userdata.WalletUserId;

                TransactionDisabledPushModel pushModel1 = new TransactionDisabledPushModel();
                pushModel1.alert = ResponseMessages.TRANSACTION_DISABLED;

                PayMoneyPushModel pushModel = new PayMoneyPushModel();
                pushModel.TransactionDate = DateTime.UtcNow;
                pushModel.TransactionId = "0";
                pushModel.alert = ResponseMessages.TRANSACTION_DISABLED;
                pushModel.Amount = "";
                pushModel.CurrentBalance = "";
                pushModel.pushType = (int)PushType.PAYSERVICES;
                pushModel.TransactionTypeInfo = (int)TransactionTypeInfo.PaidByPayServices;

                if ((int)push.deviceType == (int)DeviceTypes.ANDROID || (int)push.deviceType == (int)DeviceTypes.Web)
                {
                    PushPayload<PayMoneyPushModel> aps = new PushPayload<PayMoneyPushModel>();
                    PushPayloadData<PayMoneyPushModel> _data = new PushPayloadData<PayMoneyPushModel>();
                    _data.notification = pushModel;
                    aps.data = _data;
                    aps.to = Userdata.DeviceToken;
                    aps.collapse_key = string.Empty;
                    push.payload = pushModel;
                    push.message = JsonConvert.SerializeObject(aps);

                }
                if ((int)push.deviceType == (int)DeviceTypes.IOS)
                {
                    NotificationJsonResponse<PayMoneyPushModel> aps = new NotificationJsonResponse<PayMoneyPushModel>();
                    aps.aps = pushModel;

                    push.message = JsonConvert.SerializeObject(aps);
                }
                _sendPushNotification.sendPushNotification(push);


                Response<string> response = new Response<string>();
                //string result = new ApiHelpPageRepository().ApiList().Where(x => x.ApiName == context.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
                string result = _apiHelpPageService.ApiList().Where(x => x.ApiName == context.ActionDescriptor.ActionName).Select(x => x.Response).FirstOrDefault();
                var keys = _tokenService.KeysBySessionToken();
                response.Create(false, ResponseMessages.TRANSACTION_DISABLED, HttpStatusCode.OK, result);
                var resp = AES256.Encrypt(keys.PublicKey, JsonConvert.SerializeObject(response));
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, resp);
                return;
            }

            base.OnActionExecuting(context);

        }
    }
}