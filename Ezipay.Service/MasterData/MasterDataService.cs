using ezeePay.Utility.Enums;
using Ezipay.Database;
using Ezipay.Repository.AdminRepo.ChargeBack;
using Ezipay.Repository.CommonRepo;
using Ezipay.Repository.MasterData;
using Ezipay.Repository.TokenRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Service.AdminService;
using Ezipay.Service.CommonService;
using Ezipay.Service.UserService;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;

using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.BundleViewModel;
using Ezipay.ViewModel.ChannelViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.MasterDataViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using Ezipay.ViewModel.WalletUserVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Ezipay.Service.MasterData
{
    public class MasterDataService : IMasterDataService
    {
        private IMasterDataRepository _masterDataRepository;
        private IWalletUserService _walletUserService;
        private ICommonRepository _commonRepository;
        private ICommonServices _commonServices;
        private ITokenRepository _tokenRepository;
        private IWalletUserRepository _walletUserRepository;
        private ISendEmails _sendEmails;
        private IChargeBackRepository _ChargeBackRepository;
        private IUserApiService _userApiService;

        public MasterDataService()
        {
            _masterDataRepository = new MasterDataRepository();
            _walletUserService = new WalletUserService();
            _commonRepository = new CommonRepository();
            _tokenRepository = new TokenRepository();
            _walletUserRepository = new WalletUserRepository();
            _sendEmails = new SendEmails();
            _commonServices = new CommonServices();
            _ChargeBackRepository = new ChargeBackRepository();
            _userApiService = new UserApiService();
        }

        public async Task<List<IsdCodesResponse>> IsdCodes()
        {
            var result = new List<IsdCodesResponse>();
            return result = await _masterDataRepository.IsdCodes();
            //var response = new List<IsdCodesResponse>();
            //try
            //{
            //    response = await _masterDataRepository.IsdCodes();
            //    if (response != null && response.Count > 0)
            //    {
            //        //string imageUrl = AppSetting.imageUrl;
            //        string imageUrl = "https://api.ezipaygh.com/Content/img/";
            //        foreach (var item in response)
            //        {
            //            item.CountryFlag = imageUrl + item.CountryFlag;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //}

            //return response;
        }

        public async Task<List<MainCategoryResponse>> MainCategory()
        {
            var response = new List<MainCategoryResponse>();
            response = await _masterDataRepository.MainCategory();


            return response;
        }

        public async Task<List<SubCategoryResponse>> SubCategory(SubCategoryRequest request)
        {
            var response = new List<SubCategoryResponse>();
            response = await _masterDataRepository.SubCategory(request);
            return response;
        }

        public async Task<List<WalletServicesList>> WalletServices(WalletServicesRequest request)
        {
            var response = new List<WalletServicesList>();
            response = await _masterDataRepository.WalletServices(request);
            return response;
        }

        public async Task<List<ChannelResponce>> GetChannels(ChannelRequest request, string token)
        {
            var response = new List<ChannelResponce>();
            response = await _masterDataRepository.GetChannels(request);
            ////var UserDetail = await _walletUserService.UserProfile();
            var UserDetail = await _walletUserService.UserProfile(token);
            response.ForEach(x =>
            {
                x.DocumentStatus = UserDetail.DocumetStatus;
                x.channel = x.ServiceName;
            });
            return response;
        }

        public async Task<AppServiceRepositoryResponse> AppServices()
        {
            var response = new AppServiceRepositoryResponse();
            var WalletServices = new List<WalletServiceResponse>();
            var PayServices = new List<WalletServiceResponse>();
            try
            {
                response = await _masterDataRepository.AppServices();
            }
            catch
            {

            }
            return response;
        }

        public async Task<List<MerchantsResponse>> Merchant(string token)
        {
            var response = new List<MerchantsResponse>();
            //var userProfile = await _walletUserService.UserProfile();
            var userProfile = await _walletUserService.UserProfile(token);
            response = await _masterDataRepository.Merchant(userProfile.WalletUserId);
            return response;
        }

        public async Task<BundleResponse> GetBundles(IspBundlesRequest request)
        {
            var response = new List<IspBundlesResponse>();
            var res = new BundleResponse();
            string url = "";
            if (request.IspType.ToLower() == "busy")
            {
                url = "http://52.40.89.233/aggregator/api/bundles?datanumber=" + request.AccountNumber + "&channel=" + request.IspType;
            }
            else if (request.IspType.ToLower() == "surfline")
            {
                url = "http://52.40.89.233/aggregator/api/surflinebundle?customer=" + "233" + request.AccountNumber;
                //url = "http://52.40.89.233/aggregator/api/databundles?apikey=57F68FC7-97AB-403B-8BD0-7BF50AC13423" + "&network=" + request.IspType;
            }
            else if (request.IspType.ToLower() == "mtn fibre")
            {
                url = "http://52.40.89.233/aggregator/api/mtn";
            }
            else
            {
                url = "http://52.40.89.233/aggregator/api/databundles?apikey=57F68FC7-97AB-403B-8BD0-7BF50AC13423" + "&network=" + request.IspType;
            }
            var m_strFilePath = url;
            string xmlStr;
            try
            {
                using (var wc = new WebClient())
                {
                    xmlStr = wc.DownloadString(m_strFilePath);
                }
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    if (request.IspType.ToLower() == "busy")
                    {
                        var data = JsonConvert.DeserializeObject<List<List<ISPBundleDataObject>>>(xmlStr);
                        var objList = new List<IspBundlesResponse>();
                        var accounttype = "";
                        foreach (var list in data)
                        {
                            foreach (var item in list)
                            {
                                var obj = new IspBundlesResponse();
                                obj.Amount = item.Amount + " XOF";
                                obj.BundleId = item.BundleId;
                                obj.Description = item.Description == "null" ? "" : item.Description;
                                obj.Name = item.Name;
                                obj.DisplayContent = item.Amount + " XOF ," + item.Name;
                                obj.AccountType = accounttype;
                                objList.Add(obj);
                            }
                        }
                        if (objList.Count > 0)
                        {
                            res.RstKey = 1;
                            res.ispBundlesResponces = objList;
                        }
                        else
                        {
                            res.RstKey = 2;
                            res.ispBundlesResponces = objList;
                        }
                        //Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);                        
                    }
                    else if (request.IspType.ToLower() == "surfline")
                    {
                        dynamic data = JsonConvert.DeserializeObject<Dictionary<string, object>>(xmlStr);
                        var objList = new List<IspBundlesResponse>();
                        var accounttype = "";
                        foreach (var item in data)
                        {
                            var key = item.Key;
                            var value = item.Value;
                            if (key == "AccountType")
                            {
                                accounttype = value;
                            }
                        }
                        foreach (var item in data)
                        {
                            // var myKey = item.FirstOrDefault(x => x.Value == "one").Key;
                            var key = item.Key;
                            var value = item.Value;
                            if (key == "AccountType" && value != null)
                            {
                                accounttype = value;
                            }
                            if (value != null)
                            {
                                if (value != "" && key.Contains("Bundle"))
                                {
                                    var findData = value.Split('|');
                                    var obj = new IspBundlesResponse();
                                    obj.Amount = findData[1] != null ? findData[1] : string.Empty;
                                    obj.BundleId = findData[3] != null ? findData[3] : string.Empty;
                                    obj.Description = "Validity: " + (findData[2] != null ? findData[2] : string.Empty);
                                    obj.Name = findData[0] != null ? findData[0] : string.Empty;
                                    obj.DisplayContent = obj.Amount + "," + obj.Name;
                                    obj.AccountType = accounttype;
                                    objList.Add(obj);

                                    // Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                                    if (objList.Count > 0)
                                    {
                                        res.RstKey = 1;
                                        res.ispBundlesResponces = objList;
                                    }
                                    else
                                    {
                                        res.RstKey = 2;
                                        res.ispBundlesResponces = objList;
                                    }
                                }
                            }
                        }
                        //Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                        if (objList.Count > 0)
                        {
                            res.RstKey = 1;
                            res.ispBundlesResponces = objList;
                        }
                        else
                        {
                            res.RstKey = 2;
                            res.ispBundlesResponces = objList;
                        }
                    }
                    else if (request.IspType.ToLower() == "mtn fibre")
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(xmlStr);
                        var finalData = JsonConvert.DeserializeObject<MtnBundleResponse>(data);
                        var objList = new List<IspBundlesResponse>();
                        var accounttype = "";

                        //foreach (var d in finalData.bundles)
                        //{
                        foreach (var item in finalData.bundles)
                        {
                            var obj = new IspBundlesResponse();
                            // obj.network_id = item.network_id;
                            obj.BundleId = item.product_id;
                            obj.plan_name = item.name;
                            obj.Amount = item.amount;
                            obj.validity = item.validity;
                            obj.volume = item.product_id;
                            obj.category = item.product_id;
                            obj.Description = item.name + item.amount + " XOF ," + item.product_id;
                            obj.DisplayContent = item.amount + " XOF ," + item.product_id + " ," + item.name;
                            objList.Add(obj);
                        }
                        // }
                        // Response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, objList);
                        if (objList.Count > 0)
                        {
                            res.RstKey = 1;
                            res.ispBundlesResponces = objList;
                        }
                        else
                        {
                            res.RstKey = 2;
                            res.ispBundlesResponces = objList;
                        }
                    }
                    else
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(xmlStr);
                        var finalData = JsonConvert.DeserializeObject<BundleResponseForNew>(data);
                        var objList = new List<IspBundlesResponse>();
                        var accounttype = "";

                        foreach (var item in finalData.bundles)
                        {
                            var obj = new IspBundlesResponse();
                            obj.BundleId = item.plan_id;
                            obj.plan_name = item.plan_name;
                            obj.Amount = item.price;
                            obj.validity = item.validity;
                            obj.volume = item.volume;
                            obj.category = item.category;
                            obj.Name = item.plan_name;
                            obj.Description = item.category + item.price + " XOF ," + item.volume;
                            obj.DisplayContent = item.price + " XOF ," + item.volume + " ," + item.category;
                            objList.Add(obj);
                        }
                        if (objList.Count > 0)
                        {
                            res.RstKey = 1;
                            res.ispBundlesResponces = objList;
                        }
                        else
                        {
                            res.RstKey = 2;
                            res.ispBundlesResponces = objList;
                        }

                    }
                }
                else
                {
                    res.RstKey = 2;
                }
                //  return AES256.Encrypt(keys.PublicKey, JsonConvert.SerializeObject(Response)); //response;
            }
            catch (Exception ex)
            {
                res.RstKey = 2;
            }
            return res;//AES256.Encrypt(keys.PublicKey, JsonConvert.SerializeObject(Response)); //response;
        }

        public async Task<List<commissionOnAmountModel>> ServiceCommissionListForWeb(ChannelRequest request)
        {
            var response = new List<commissionOnAmountModel>();
            var currencyDetail = _masterDataRepository.GetCurrencyRate();
            response = await _masterDataRepository.ServiceCommissionListForWeb(request);
            response.ForEach(x =>
            {
                x.AmountInDollar = Convert.ToDecimal(currencyDetail.CediRate);
            }); //for only option2 add_mone//Add Doller Rate
            response.ForEach(x =>
            {
                x.AmountInNGN = Convert.ToDecimal(currencyDetail.NGNRate);
            });
            //for only option add_mone
            response.ForEach(x =>
            {
                x.AmountInEuro = Convert.ToDecimal(currencyDetail.EuroRate);
            });
            //for only option paymone:- tranfertobank
            response.ForEach(x => {
                x.AmountInSendNGN = Convert.ToDecimal(currencyDetail.SendNGNRate);
            });
            //for only option paymone:- ghana mobmon
            response.ForEach(x => {
                x.AmountInSendGH = Convert.ToDecimal(currencyDetail.SendGHRate);
            });

            return response;
            //var response = new List<commissionOnAmountModel>();
            //response = await _masterDataRepository.ServiceCommissionListForWeb(request);
            //return response;
        }

        public async Task<List<FAQResponse>> FAQ()
        {
            var response = new List<FAQResponse>();
            try
            {
                response = await _masterDataRepository.FAQ();
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("UserProfileRepository.cs", "FAQ");
            }
            return response;
        }

        public async Task<bool> CheckPassword(string password, string token)////
        {
            bool IsMatch = false;
            var response = new CheckLoginResponse();
            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    response = await _commonRepository.CheckPassword(token);
                    if (response != null && !string.IsNullOrEmpty(response.HashedPassword))
                    {
                        var hashedObject = SHA256ALGO.HashPasswordDecryption(password, response.HashedSalt);
                        if (hashedObject.HashedPassword == response.HashedPassword)
                        {
                            IsMatch = true;
                        }
                        else
                        {
                            IsMatch = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CommonServices.cs", "CheckPassword");
            }
            return IsMatch;

        }
        public async Task<List<FeedbackTypeResponse>> FeedBackTypes()
        {
            var response = new List<FeedbackTypeResponse>();
            try
            {
                response = await _commonRepository.FeedBackTypes();
            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("CommonRepository.cs", "FeedBackTypes");
            }
            return response;
        }

        public async Task<bool> SaveFeedBack(FeedBackRequest request)
        {
            bool response = false;
            try
            {
                var adminKeyPair = AES256.AdminKeyPair;
                var KeyPair = _tokenRepository.KeysBySessionToken();
                var AdminEmail = ConfigurationManager.AppSettings["AdminEmail"].ToString();

                var result = await _walletUserRepository.UserProfile(KeyPair.Token);

                // long WalletUserId = (long)await db.SessionTokens.Where(x => x.TokenValue == KeyPair.Token).Select(x => x.WalletUserId).FirstOrDefaultAsync();
                // var sender = new AppUserRepository().UserProfile();

                Feedback _feedBack = new Feedback();
                _feedBack.UserId = result.WalletUserId;
                _feedBack.IsActive = true;
                _feedBack.IsDeleted = false;
                _feedBack.CreatedDate = DateTime.UtcNow;
                _feedBack.UpdatedDate = DateTime.UtcNow;
                _feedBack.FeedbackId = request.FeedbackTypeId;
                _feedBack.FeedBackMessage = request.FeedBackMessage;
                await _commonRepository.SaveFeedBack(_feedBack);
                response = true;
                var req = new EmailModel
                {
                    TO = AdminEmail,
                    Body = "Hi,<br/>" + request.FeedBackMessage + " <br/><br/>" + (result.FirstName + " " + result.LastName).Trim() + "<br/>" + DateTime.UtcNow.ToString("dd-MMM-yyyy"),
                    Subject = "Feedback"
                };
                _sendEmails.SendEmail(req);

                //_IMessageService.SendMail(AdminEmail, "Feedback", "Hi,<br/>" + request.FeedBackMessage + " <br/><br/>" + (result.FirstName + " " + result.LastName).Trim() + "<br/>" + DateTime.UtcNow.ToString("dd-MMM-yyyy"));

            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("UserProfileRepository.cs", "ChangePassword", request);
            }
            return response;
        }

        public async Task<bool> ChangeNotification(string token)
        {
            ////string token = GlobalData.Key;
            var result = await _walletUserRepository.UserProfile(token);
            var _walletUser = await _walletUserRepository.GetCurrentUser(result.WalletUserId);
            if (_walletUser != null)
            {
                if (_walletUser.IsNotification == true)
                {
                    _walletUser.IsNotification = false;
                }
                else
                {
                    _walletUser.IsNotification = true;
                }
                await _commonRepository.ChangeNotification(_walletUser);
                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task<bool> SendRequest(string token)////
        {
            bool IsSent = false;
            try
            {

                ////string token = GlobalData.Key;
                var _response = await _walletUserRepository.UserProfile(token);
                if (_response != null)
                {
                    var requestNumber = await _masterDataRepository.GetInvoiceNumber(6);

                    string FirstName = _response.FirstName;
                    string LastName = _response.LastName;
                    string EmailId = _response.EmailId;
                    string mobileNo = _response.MobileNo;
                    string sdtcode = _response.StdCode;
                    string createdDate = Convert.ToString(DateTime.Now);
                    var TempBody = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/HtmlTemplates/CallBackEmailTemplate.html");
                    string body = string.Format(TempBody, FirstName, LastName, mobileNo, EmailId, createdDate);
                    string fromEmail = ConfigurationManager.AppSettings["CallBackFromEmailId"];
                    string fromPassword = ConfigurationManager.AppSettings["CallBackFromPassword"];
                    string toEmail = ConfigurationManager.AppSettings["CallBackToEmailId"];
                    string subject = "Callback Request ID " + requestNumber.AutoDigit;
                    // IsSent = CommonMethod.CommonSendEmail(fromEmail, fromPassword, toEmail, subject, Body);

                    var req = new EmailModel
                    {
                        TO = toEmail,
                        Body = body,
                        Subject = subject
                    };
                    _sendEmails.SendEmail(req);
                    if (_sendEmails != null)
                    {
                        IsSent = true;
                    }
                    else
                    {
                        IsSent = false;
                    }
                    if (IsSent == true)
                    {
                        Callback _callback = new Callback();
                        _callback.FirstName = _response.FirstName;
                        _callback.LastName = _response.LastName;
                        _callback.EmailId = _response.EmailId;
                        var requestNewId = await _masterDataRepository.GetInvoiceNumber(6);
                        _callback.RequestNumber = requestNewId.AutoDigit;
                        _callback.CreatedDate = DateTime.Now;
                        _callback.UpdatedDate = DateTime.Now;
                        _callback.IsActive = true;
                        _callback.MobileNo = _response.MobileNo;
                        _callback.IsDeleted = false;
                        _callback.Status = (int)CallBackRequestStatus.Pending;

                        _callback = await _commonRepository.SendRequest(_callback);
                        if (_callback.CallbackId > 0)
                        {
                            CallbackListTracking callbackListTracking = new CallbackListTracking();

                            callbackListTracking.CallBackId = _callback.CallbackId;
                            callbackListTracking.Status = (int)CallBackRequestStatus.Pending; ;
                            callbackListTracking.CeatedBy = _response.WalletUserId;
                            callbackListTracking.CreatedOn = DateTime.UtcNow;
                            await _commonRepository.InsertCallbackListTracking(callbackListTracking);
                            IsSent = true;
                            //InsertCallbackListTracking(db, _callback.CallbackId, (int)CallBackRequestStatus.Pending, _response.WalletUserId);
                        }
                        else
                        {
                            IsSent = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return IsSent;
        }

        public async Task<List<BannerVM>> GetBanner()
        {
            var response = new List<BannerVM>();

            response = await _commonRepository.GetBanner();

            return response;
        }

        public async Task<UserDocumentResponse> ViewDocument(UserDocumentRequest request)
        {
            var response = new UserDocumentResponse();
            var AdminKeys = AES256.AdminKeyPair;
            response = await _commonRepository.ViewDocument(request);
            response.WalletUserId = response.WalletUserId;
            response.DocumentStatus = response.DocumentStatus;
            response.CardImage = AES256.Decrypt(AdminKeys.PrivateKey, response.CardImage);
            response.IdProofImage = AES256.Decrypt(AdminKeys.PrivateKey, response.IdProofImage);
            return response;
        }

        public async Task<List<RecentReceiverResponse>> RecentReceiver(RecentReceiverRequest request)
        {
            var result = new List<RecentReceiverResponse>();
            return result = await _commonRepository.RecentReceiver(request);
        }

        public async Task<List<IsdCodesResponse>> IsdCodesFrancCountry()
        {
            var result = new List<IsdCodesResponse>();
            return result = await _masterDataRepository.IsdCodesFrancCountry();
        }


        public async Task<List<IspChannelResponse>> GetChannelsForISP(ChannelRequest request, string token)////
        {
            var response = new List<IspChannelResponse>();
            var chenRes = new List<ChannelProductResponce>();
            var UserDetail = await _walletUserService.UserProfile(token);
            if ((request.ServiceCategoryId == 1) || (request.ServiceCategoryId == 3))
            {
                decimal amtCedi = 0;
                decimal amtDoller = 0;
                if (request.ServiceCategoryId == 1)
                {
                    response = await _masterDataRepository.GetChannelsForISP(request);
                }
                if (request.ServiceCategoryId == 3 && request.IsdCode != null)// && request.IsdCode == "+229")
                {

                    response = await _masterDataRepository.GetChannelsForISP(request);
                }
                else if (request.ServiceCategoryId == 3 && request.IsdCode == "+225")
                {
                    response = await _masterDataRepository.GetChannelsForISP(request);
                }

                response.ForEach(x =>
                {
                    x.DocumentStatus = UserDetail.DocumetStatus;
                    x.AmountInCedi = amtCedi;
                    x.AmountInDoller = amtDoller;
                });
                return response;
            }
            else if (request.ServiceCategoryId == 12 && (request.IsdCode == "+221" || request.IsdCode == "+223" || request.IsdCode == "+225" || request.IsdCode == "+226" || request.IsdCode == "+245" || request.IsdCode == "+227" || request.IsdCode == "+229" || request.IsdCode == "+225" || request.IsdCode == "+228"))
            {
                response = await _masterDataRepository.GetChannelsForISP(request);
            }
            if (request.ServiceCategoryId == 7 && (request.IsdCode == "+245" || request.IsdCode == "+227" || request.IsdCode == "+229" || request.IsdCode == "+225" || request.IsdCode == "+228"))
            {
                if (request.IsdCode == "+225")
                {
                    response = await _masterDataRepository.GetChannelsForISP(request);
                }
                //response = await GetISPOperatorList(request.ServiceCategoryId, request.IsdCode);
                var res = await GetOperators(request.IsdCode, token);

                if (request.IsdCode == "+229")
                {
                    foreach (var data in res)
                    {
                        //test
                        ProductDataResponse productDataResponse = new ProductDataResponse();
                        Dictionary<string, object> value = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.JsonData);
                        var dictresults = JsonConvert.DeserializeObject<Dictionary<string, object>>(value["result"].ToString());
                        var dictresultProduct = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictresults["products"].ToString());
                        var LastproductTypeid = "";
                        var Lastproductid = "";
                        foreach (KeyValuePair<string, object> resultProduct in dictresultProduct)
                        {
                            var valuesData = resultProduct.Value;
                            productDataResponse = JsonConvert.DeserializeObject<ProductDataResponse>(valuesData.ToString());
                            if (productDataResponse.productType.id == "1")
                            {
                                chenRes.Add(new ChannelProductResponce
                                {
                                    productId = Convert.ToInt32(productDataResponse.id),
                                    ProductName = productDataResponse.name,
                                    OperatorId = data.OperatorId,
                                    PriceType = productDataResponse.priceType,
                                    // MinAmount = productDataResponse.price != null ? productDataResponse.price.min.@operator : string.Empty,
                                    MinAmount = productDataResponse.price.min != null ? productDataResponse.price.min.@operator : string.Empty,
                                    MaxAmount = productDataResponse.price.max != null ? productDataResponse.price.max.@operator : string.Empty,
                                    FixAmount = productDataResponse.price.@operator != null ? productDataResponse.price.@operator : string.Empty
                                });
                                LastproductTypeid = productDataResponse.productType.id;
                                Lastproductid = productDataResponse.id;
                            }
                        }

                        response.Add(new IspChannelResponse
                        {
                            ImageUrl = data.ImageUrl,
                            WalletServiceId = data.WalletServiceId,
                            ServiceName = data.ServiceName,
                            ServiceCategoryId = data.ProductId,
                            OperatorId = data.OperatorId,
                            JsonData = data.JsonData,
                            productDataJson = JsonConvert.DeserializeObject<object>(data.JsonData),
                            productId = Convert.ToInt32(Lastproductid),
                            channelProductResponces = chenRes,
                            productTypeId = LastproductTypeid != null ? LastproductTypeid : string.Empty,
                        });
                    }
                }
                else
                {
                    foreach (var data in res)
                    {
                        //test
                        ProductDataResponse productDataResponse = new ProductDataResponse();
                        Dictionary<string, object> value = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.JsonData);
                        var dictresults = JsonConvert.DeserializeObject<Dictionary<string, object>>(value["result"].ToString());
                        var dictresultProduct = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictresults["products"].ToString());
                        foreach (KeyValuePair<string, object> resultProduct in dictresultProduct)
                        {
                            var valuesData = resultProduct.Value;
                            productDataResponse = JsonConvert.DeserializeObject<ProductDataResponse>(valuesData.ToString());

                            chenRes.Add(new ChannelProductResponce
                            {
                                productId = Convert.ToInt32(productDataResponse.id),
                                ProductName = productDataResponse.name,
                                OperatorId = data.OperatorId,
                                PriceType = productDataResponse.priceType,
                                // MinAmount = productDataResponse.price != null ? productDataResponse.price.min.@operator : string.Empty,
                                MinAmount = productDataResponse.price.min != null ? productDataResponse.price.min.@operator : string.Empty,
                                MaxAmount = productDataResponse.price.max != null ? productDataResponse.price.max.@operator : string.Empty,
                                FixAmount = productDataResponse.price.@operator != null ? productDataResponse.price.@operator : string.Empty
                            });
                        }

                        response.Add(new IspChannelResponse
                        {
                            ImageUrl = data.ImageUrl,
                            WalletServiceId = data.WalletServiceId,
                            ServiceName = data.ServiceName,
                            ServiceCategoryId = data.ProductId,
                            OperatorId = data.OperatorId,
                            JsonData = data.JsonData,
                            productDataJson = JsonConvert.DeserializeObject<object>(data.JsonData),
                            productId = Convert.ToInt32(productDataResponse.id),
                            channelProductResponces = chenRes,
                            productTypeId = productDataResponse.productType.id != null ? productDataResponse.productType.id : string.Empty,
                        });
                    }
                }
            }
            else if (request.ServiceCategoryId == 8 && (request.IsdCode == "+223" || request.IsdCode == "+245" || request.IsdCode == "+227" || request.IsdCode == "+229" || request.IsdCode == "+225" || request.IsdCode == "+228"))
            {
                response = await GetISPOperatorList(request.ServiceCategoryId, request.IsdCode, token);////
            }
            else if (request.ServiceCategoryId == 7 || request.ServiceCategoryId == 6)
            {
                response = await _masterDataRepository.GetChannelsForISP(request);
            }
            else if (request.ServiceCategoryId == 10 && request.IsdCode != "")
            {
                response = await _masterDataRepository.GetChannelsForISP(request);
            }

            response.ForEach(x =>
                {
                    x.DocumentStatus = UserDetail.DocumetStatus;
                    // x.channel = x.ServiceName;
                });



            return response;




        }

        public async Task<List<FinalResponse>> GetOperators(string isdCode, string token)////
        {
            var response = new List<FinalResponse>();
            //string response = "";
            if (isdCode == "+245" || isdCode == "+227" || isdCode == "+229" || isdCode == "+225" || isdCode == "+228" || isdCode == "+223")
            {
                string countryCode = "";
                if (isdCode == "+245")
                {
                    countryCode = "GW";
                }
                else if (isdCode == "+227")
                {
                    countryCode = "NE";
                }
                else if (isdCode == "+229")
                {
                    countryCode = "BJ";
                }
                else if (isdCode == "+228")
                {
                    countryCode = "TG";
                }
                else if (isdCode == "+225")
                {
                    countryCode = "CI";
                }
                else if (isdCode == "+223")
                {
                    countryCode = "ML";
                }
                ////var token = GlobalData.Key;

                var data = await _walletUserRepository.UserProfile(token);////no use 
                var invoiceNumber = await _masterDataRepository.GetInvoiceNumber();
                //var invoiceNumber = new ThirdPartyRepository().GetInvoiceNumber();

                //var passHashed = _thirdPartyRepository.SHA1Hash("eazipayapi123");
                var passHashed = _commonServices.SHA1Hash("eazipayapixof1234");
                string reqs = invoiceNumber.InvoiceNumber + passHashed;

                var final = _commonServices.SHA1Hash(reqs);
                try
                {

                    var req = new OperatorRequest
                    {
                        auth = new Auth
                        {
                            username = "eazipayapixof",
                            salt = invoiceNumber.InvoiceNumber,
                            password = final,
                        },
                        command = "getOperators",
                        version = "5",
                        country = countryCode,
                    };


                    var jsonReq = JsonConvert.SerializeObject(req);
                    CommonApi commonApi = new CommonApi();
                    string apiUrl = ThirdPartyAggragatorSettings.AirtimeArtx;
                    string responseString = "";
                    var payData = Task.Run(() => commonApi.GetOperatorAirtime(jsonReq, apiUrl));
                    payData.Wait();
                    responseString = payData.Result.ToString();
                    Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                    // OperatorData operatorData = new OperatorData();
                    var dictresult = JsonConvert.DeserializeObject<Dictionary<string, object>>(values["result"].ToString());
                    foreach (KeyValuePair<string, object> entry in dictresult)
                    {
                        var val = entry.Value;
                        var operatorData = JsonConvert.DeserializeObject<dynamic>(val.ToString());

                        if (operatorData != null)
                        {
                            var invoice = await _masterDataRepository.GetInvoiceNumber();
                            var passwordHashed = _commonServices.SHA1Hash("eazipayapixof1234");
                            string reqForOpt = invoice.InvoiceNumber + passwordHashed;

                            var hashedPass = _commonServices.SHA1Hash(reqForOpt);
                            string aaaa = "{\"auth\":{\"username\":\"" +
                                req.auth.username +
                                "\",\"salt\":\"" +
                                invoice.InvoiceNumber +
                                "\",\"password\":\"" +
                                hashedPass +
                                "\",\"signature\":null},\"version\":\"5\",\"command\":\"getOperatorProducts\",\"operator\":\"" +
                                operatorData.id +
                                "\"}";

                            var operatorDataById = Task.Run(() => commonApi.GetOperatorAirtime(aaaa, apiUrl));
                            operatorDataById.Wait();
                            responseString = operatorDataById.Result.ToString();
                        }
                        response.Add(new FinalResponse
                        {
                            ServiceName = operatorData.name,
                            WalletServiceId = Convert.ToInt32(operatorData.id),
                            ImageUrl = "https://media.sochitel.com/img/operators/" + Convert.ToInt32(operatorData.brandId) + ".png",
                            OperatorId = Convert.ToInt32(operatorData.id),
                            JsonData = responseString
                        });
                    }

                }
                catch (Exception ex)
                {

                }
                // return responce;
            }
            return response;
        }



        public async Task<List<IspChannelResponse>> GetISPOperatorList(int ServiceCategoryId, string IsdCode, string token)
        {
            List<IspChannelResponse> responce = new List<IspChannelResponse>();
            var chenRes = new List<ChannelProductResponce>();
            var res = await GetOperators(IsdCode, token);

            foreach (var data in res)
            {
                //test
                ProductDataResponse productDataResponse = new ProductDataResponse();
                Dictionary<string, object> value = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.JsonData);
                var dictresults = JsonConvert.DeserializeObject<Dictionary<string, object>>(value["result"].ToString());
                var dictresultProduct = JsonConvert.DeserializeObject<Dictionary<string, object>>(dictresults["products"].ToString());
                foreach (KeyValuePair<string, object> resultProduct in dictresultProduct)
                {
                    var valuesData = resultProduct.Value;
                    productDataResponse = JsonConvert.DeserializeObject<ProductDataResponse>(valuesData.ToString());
                    chenRes.Add(new ChannelProductResponce
                    {
                        productId = Convert.ToInt32(productDataResponse.id),
                        ProductName = productDataResponse.name,
                        OperatorId = data.OperatorId,
                        PriceType = productDataResponse.priceType,
                        // MinAmount = productDataResponse.price != null ? productDataResponse.price.min.@operator : string.Empty,
                        MinAmount = productDataResponse.price.min != null ? productDataResponse.price.min.@operator : string.Empty,
                        MaxAmount = productDataResponse.price.max != null ? productDataResponse.price.max.@operator : string.Empty,
                        FixAmount = productDataResponse.price.@operator != null ? productDataResponse.price.@operator : string.Empty,

                    });
                }

                responce.Add(new IspChannelResponse
                {
                    productTypeId = productDataResponse.productType.id != null ? productDataResponse.productType.id : string.Empty,
                    ImageUrl = data.ImageUrl,
                    WalletServiceId = data.WalletServiceId,
                    ServiceName = data.ServiceName,
                    ServiceCategoryId = data.ProductId,
                    OperatorId = data.OperatorId,
                    JsonData = data.JsonData,
                    productDataJson = JsonConvert.DeserializeObject<object>(data.JsonData),
                    productId = Convert.ToInt32(productDataResponse.id),
                    channelProductResponces = chenRes
                });
            }

            return responce;
        }

        public async Task<bool> SaveFeedBackV2(FeedBackWebRequest request)
        {
            bool response = false;

            var adminKeyPair = AES256.AdminKeyPair;
            var KeyPair = _tokenRepository.KeysBySessionToken();
            var AdminEmail = ConfigurationManager.AppSettings["AdminEmail"].ToString();
            long userId = 0;
            if (string.IsNullOrWhiteSpace(request.EmailId) && string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.MobileNo))
            {
                var result = await _walletUserRepository.UserProfile(KeyPair.Token);
                if (result != null)
                {
                    userId = result.WalletUserId;
                    request.EmailId = result.EmailId;
                    request.Name = result.FirstName + " " + result.LastName;
                    request.MobileNo = result.MobileNo;
                }
            }

            Feedback _feedBack = new Feedback();
            if (userId == 0)
            {
                _feedBack.UserName = request.Name;
                _feedBack.EmailId = request.EmailId;
                _feedBack.MobileNo = request.MobileNo;
                _feedBack.IsAnonymousUser = true;
            }
            _feedBack.UserId = userId;
            _feedBack.IsActive = true;
            _feedBack.IsDeleted = false;
            _feedBack.CreatedDate = DateTime.UtcNow;
            _feedBack.UpdatedDate = DateTime.UtcNow;
            _feedBack.FeedbackId = request.FeedbackTypeId;
            _feedBack.FeedBackMessage = request.FeedBackMessage;
            await _commonRepository.SaveFeedBack(_feedBack);
            response = true;
            var req = new EmailModel
            {
                TO = AdminEmail,
                Body = "Hi,<br/>" + request.FeedBackMessage + " <br/><br/>" + (request.Name).Trim() + "<br/>" + DateTime.UtcNow.ToString("dd-MMM-yyyy"),
                Subject = "Feedback"
            };
            _sendEmails.SendEmail(req);

            return response;
        }

        public async Task<List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>> GetcardnoList(long Walletuserid)
        {
            return await _masterDataRepository.GetcardnoList(Walletuserid);
        }

        public async Task<bool> Chargeback(long Walletuserid)
        {
            var result = await _walletUserRepository.GetCurrentUser(Walletuserid);
            //response.CurrentBalance = result.CurrentBalance;
            //response.WalletUserId = result.WalletUserId;

            //get the user id from whokm we get chargeback :- account block & debit
            if (result.WalletUserId != 0)
            {
                ViewModel.AdminViewModel.ChargeBackRequest request1 = new ViewModel.AdminViewModel.ChargeBackRequest();
                request1.Walletuserid = result.WalletUserId;
                var user_id = result.WalletUserId;

                var userChargeBackresult = await _ChargeBackRepository.GetChargeBackListById(request1);
                decimal CurrentBalance = decimal.Parse(result.CurrentBalance);

                if (userChargeBackresult.Count > 0)
                {
                    decimal AmountLimit = decimal.Parse(userChargeBackresult[0].AmountLimit);
                    var ChargeBacksubmitBy = userChargeBackresult[0].Createdby;
                    if (AmountLimit <= CurrentBalance)
                    {
                        ViewModel.AdminViewModel.UserManageRequest request2 = new ViewModel.AdminViewModel.UserManageRequest();
                        request2.UserId = user_id;
                        request2.IsActive = false;
                        request2.Status = 1;
                        request2.Flag = ChargeBacksubmitBy;

                        var i = await _userApiService.EnableDisableUser(request2);
                        if (i == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public async Task<Country> IsdCodesby(string IsdCode)
        {

            return await _masterDataRepository.IsdCodesby(IsdCode);

        }

        public async Task<List<ManageWalletServicesList>> ManageWalletServices(WalletServicesRequest request)
        {
            var response = new List<ManageWalletServicesList>();
            response = await _masterDataRepository.ManageWalletServices(request);
            return response;
        }

        public async Task<int> UpdateWalletServicesStatus(UpdateWalletServicesRequest request)
        {
            int result = 0;

            var response = new WalletService();
            response = await _masterDataRepository.GetWalletServicesForUpdate(request.WalletServiceId);
            if (request.IsActive == false)
            {
                response.IsActive = false;
                response.IsDeleted = true;
                result = 1;
                response = await _masterDataRepository.UpdateWalletServicesStatus(response);

            }
            if (request.IsActive == true)
            {
                response.IsActive = true;
                response.IsDeleted = false;
                response = await _masterDataRepository.UpdateWalletServicesStatus(response);
                result = 2;
            }

            return result;
        }

        public async Task<List<IsdCodesResponse>> IsdCodesForXAFCountry()
        {
            var result = new List<IsdCodesResponse>();
            return result = await _masterDataRepository.IsdCodesForXAFCountry();
        }
        public async Task<List<NGNBankResponse>> GetNGNbankList(int flag)
        {
            var objList = new List<NGNBankResponse>();

            try
            {
                string bnklist = await GetBanklist();

                if (!string.IsNullOrEmpty(bnklist))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic data = JsonConvert.DeserializeObject<Dictionary<string, object>>(bnklist);
                    if (flag == 0)
                    {
                        foreach (var item in data["data"])
                        {
                            //if (item.code == "011") //onli firtsbank
                            if (item.code == "057" || item.code == "033" || item.code == "011")
                            {
                                var obj = new NGNBankResponse();
                                obj.code = item.code;
                                obj.name = item.name;

                                objList.Add(obj);
                            }
                        }
                    }
                    else if (flag == 1)
                    {
                        foreach (var item in data["data"])
                        {

                            var obj = new NGNBankResponse();
                            obj.code = item.code;
                            obj.name = item.name;

                            objList.Add(obj);

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return objList;
        }

        

        public async Task<string> GetBanklist()
        {

            string resBody = "";

            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    //var content = new StringContent(null, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonSetting.flutterFLWSECKey);


                    var url = CommonSetting.flutterFLWBankNGN;
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();


                }
                catch (HttpRequestException e)
                {
                    e.Message.ErrorLog("GetBanklist", e.StackTrace + " " + e.Message);
                }
                return resBody;

            }
        }


        public async Task<List<CurrencyvalueResponseById>> GetCurrencyValue(CurrencyvalueRequestById request)
        {
            //------Get Currency Rate--------------
            
            var currencyDetail = _masterDataRepository.GetCurrencyRate();
            var objList = new List<CurrencyvalueResponseById>();
            var sender = await _walletUserRepository.GetUserDetailById(request.Walletuserid);
            //var senderfreezeamount = await _walletUserRepository.GetUserbalancefreezeById(request.Walletuserid);

            if (request.Walletuserid != 0)
            {
                var response = new CurrencyvalueResponseById();
                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", sender.CurrentBalance));    // "1,234,257";

                decimal amt = (amountWithCommision); //xof to xof
                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                response.finalAmt = finalAmt;
                response.txtfinalAmt = "XOF/XAF " + finalAmt;
                objList.Add(response);
            }
            //if (request.Walletuserid != 0) //freezeamount
            //{
            //    var response = new CurrencyvalueResponseById();
            //    decimal amount = decimal.Parse(string.Format("{0:0,0}", senderfreezeamount.currentbalance));    // "1,234,257";

            //    decimal amt = (amount); //xof to xof
            //    var finalAmt = Decimal.Parse(amt.ToString("0.00"));
            //    response.finalAmt = finalAmt;
            //    response.txtfinalAmt = "Freeze XOF/XAF " + finalAmt;
            //    objList.Add(response);
            //}

            if (request.Walletuserid != 0)
            {
                var response = new CurrencyvalueResponseById();
                decimal AdddollarRate = Convert.ToDecimal(currencyDetail.CediRate);//Add Doller Rate;                                                                                   
                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", sender.CurrentBalance));    // "1,234,257";
                decimal amt = (amountWithCommision * AdddollarRate); //xof to dollar
                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                response.finalAmt = finalAmt;
                response.txtfinalAmt = "USD " + finalAmt;
                objList.Add(response);
            }
            if (request.Walletuserid != 0)
            {
                var response = new CurrencyvalueResponseById();
                decimal AddEuroRate = Convert.ToDecimal(currencyDetail.EuroRate);//Add EuroRate;
                decimal amountWithCommision = decimal.Parse(string.Format("{0:0,0}", sender.CurrentBalance));    // "1,234,257";
                decimal amt = (amountWithCommision * AddEuroRate); //xof to EuroRate
                var finalAmt = Decimal.Parse(amt.ToString("0.00"));
                response.finalAmt = finalAmt;
                response.txtfinalAmt = "EURO " + finalAmt;
                objList.Add(response);
            }
            


            return objList;
        }

        public async Task<List<IsdCodesResponse>> IsdCodesAddMonMobMonCountry()
        {
            var result = new List<IsdCodesResponse>();
            return result = await _masterDataRepository.IsdCodesAddMonMobMonCountry();
        }
        public async Task<List<IsdCodesResponse>> IsdCodesPayGhanaMobMonCountry()
        {
            var result = new List<IsdCodesResponse>();
            return result = await _masterDataRepository.IsdCodesPayGhanaMobMonCountry();
        }

    }
}