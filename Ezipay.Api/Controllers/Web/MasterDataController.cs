using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.MasterData;
using Ezipay.Utility.common;

using Ezipay.ViewModel.BannerViewModel;
using Ezipay.ViewModel.BundleViewModel;
using Ezipay.ViewModel.ChannelViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.MasterDataViewModel;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers
{
    /// <summary>
    /// MasterDataController
    /// </summary>
    [RoutePrefix("api/MasterDataController")]
    public class MasterDataController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private IMasterDataService _masterDataService;
        private Converter _converter;
        private object _walletUserService;

        /// <summary>
        /// masterDataService
        /// </summary>
        /// <param name="masterDataService"></param>
        public MasterDataController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
            _converter = new Converter();
        }

        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse>>))]
        [Route("IsdCodes")]
        public async Task<IHttpActionResult> IsdCodes()
        {
            var response = new Response<List<IsdCodesResponse>>();
            var result = new List<IsdCodesResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.IsdCodes();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }


        /// <summary>
        /// MainCategory
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<MainCategoryResponse>>))]
        [Route("MainCategory")]
        [SessionAuthorization]
        public async Task<IHttpActionResult> MainCategory()
        {
            var response = new Response<List<MainCategoryResponse>>();
            var result = new List<MainCategoryResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.MainCategory();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK,false);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, false);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, false);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// SubCategory
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<SubCategoryResponse>>))]
        [Route("SubCategory")]
        [SessionAuthorization]
        public async Task<IHttpActionResult> SubCategory(RequestModel requestModel)
        {
            var response = new Response<List<SubCategoryResponse>>();
            var result = new List<SubCategoryResponse>();

            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<SubCategoryRequest>().Decrypt(requestModel.Value);
                    result = await _masterDataService.SubCategory(request);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// WalletServices
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<WalletServicesList>>))]
        [Route("WalletServices")]
        [SessionAuthorization]
        public async Task<IHttpActionResult> WalletServices(RequestModel requestModel)
        {
            var response = new Response<List<WalletServicesList>>();
            var result = new List<WalletServicesList>();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<WalletServicesRequest>().Decrypt(requestModel.Value);
                    result = await _masterDataService.WalletServices(request);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }


        /// <summary>
        /// GetChannelList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<ChannelResponce>>))]
        [Route("GetChannelList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetChannelList(RequestModel request)
        {
            var response = new Response<List<ChannelResponce>>();
            var result = new List<ChannelResponce>();
            var requestModel = new EncrDecr<ChannelRequest>().Decrypt(request.Value, false);
            // var channels = _masterDataService.GetChannels(request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetChannels(requestModel, sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true);
            return _iHttpActionResult;
        }

        /// <summary>
        /// AppServices
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<AppServiceRepositoryResponse>))]
        [Route("AppServices")]
        public async Task<IHttpActionResult> AppServices()
        {
            var response = new Response<AppServiceRepositoryResponse>();
            var result = new AppServiceRepositoryResponse();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.AppServices();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetMerchantList
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<MerchantsResponse>>))]
        [Route("GetMerchantList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetMerchantList()
        {
            var response = new Response<List<MerchantsResponse>>();
            var result = new List<MerchantsResponse>();
            // int langId = AppUtils.GetLangId(Request);

            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////


            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.Merchant(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetMerchantListForApp
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<MerchantsResponse>>))]
        [Route("GetMerchantListForApp")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetMerchantListForApp()
        {
            var response = new Response<List<MerchantsResponse>>();
            var result = new List<MerchantsResponse>();
            // int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////

            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.Merchant(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetBundles
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<BundleResponse>))]
        [Route("GetBundles")]
        public async Task<IHttpActionResult> GetBundles(RequestModel request)
        {
            var requestModel = new EncrDecr<IspBundlesRequest>().Decrypt(request.Value, false);
            var response = new Response<BundleResponse>();
            var result = new BundleResponse();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetBundles(requestModel);
                    switch (result.RstKey)
                    {
                        case 0:
                            response = response.Create(false, ResponseMessages.INVALID_PASSWORD, HttpStatusCode.OK, result);
                            break;
                        case 1:
                            response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                            break;
                        case 2:
                            response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.OK, result);

                            break;
                        default:
                            response = response.Create(false, ResponseMessages.TRANSACTION_SERVICE_CATEGORY_NOT_FOUND, HttpStatusCode.OK, result);

                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse>>))]
        [Route("IsdCodesForAdmin")]
        [SessionAuthorization]
        public async Task<IHttpActionResult> IsdCodesForAdmin()
        {
            var response = new Response<List<IsdCodesResponse>>();
            var result = new List<IsdCodesResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.IsdCodes();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// ServiceCommissionListForWeb
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<commissionOnAmountModel>>))]
        [Route("ServiceCommissionListForWeb")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> ServiceCommissionListForWeb(RequestModel request)
        {
            var response = new Response<List<commissionOnAmountModel>>();
            var result = new List<commissionOnAmountModel>();
            var requestModel = new EncrDecr<ChannelRequest>().Decrypt(request.Value, false);
            // var channels = _masterDataService.GetChannels(request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.ServiceCommissionListForWeb(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true);
            return _iHttpActionResult;
        }


        /// <summary>
        /// FAQ
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<FAQResponse>>))]
        [Route("FAQ")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> FAQ()
        {
            var response = new Response<List<FAQResponse>>();
            var result = new List<FAQResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.FAQ();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// FeedBackTypes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<FeedbackTypeResponse>>))]
        [Route("FeedBackTypes")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> FeedBackTypes()
        {
            var response = new Response<List<FeedbackTypeResponse>>();
            var result = new List<FeedbackTypeResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.FeedBackTypes();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// SaveFeedBack
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Route("SaveFeedBack")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> SaveFeedBack(RequestModel request)
        {
            var response = new Response<object>();
            var result = new object();
            var requestModel = new EncrDecr<FeedBackRequest>().Decrypt(request.Value, false);
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.SaveFeedBack(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_FEEDBAK_SAVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_FEEDBAK_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_FEEDBAK_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Route("SaveFeedBackV2")]
        public async Task<IHttpActionResult> SaveFeedBackV2(FeedBackWebRequest requestModel)
        {
            var response = new Response<object>();
            var result = new object();

            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.SaveFeedBackV2(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_FEEDBAK_SAVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_FEEDBAK_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_FEEDBAK_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, false, false);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// ChangeNotification
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Route("ChangeNotification")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> ChangeNotification()
        {
            var response = new Response<object>();
            var result = new object();
            // int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.ChangeNotification(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.NOTIFICATION_CHANGE, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.NOTIFICATION_CHANGE_UNSUCCESS, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.INVALID_FORM_DATA, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;

                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.INVALID_FORM_DATA, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// SendRequest
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Route("CallBack")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> CallBack()
        {
            var response = new Response<object>();
            var result = new object();
            // int langId = AppUtils.GetLangId(Request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();////
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.SendRequest(sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.REQUEST_SENT, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.REQUEST_SENT_FAILD, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetBanner
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<BannerVM>>))]
        [Route("GetBanner")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetBanner()
        {
            var response = new Response<List<BannerVM>>();
            var result = new List<BannerVM>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetBanner();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetBanner
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserDocumentResponse>))]
        [Route("ViewDocument")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> ViewDocument(RequestModel request)
        {
            var response = new Response<UserDocumentResponse>();
            var result = new UserDocumentResponse();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<UserDocumentRequest>().Decrypt(request.Value, false);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.ViewDocument(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }

        ///// <summary>
        ///// Get Referal Url for share and earn amount
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[AcceptVerbs("POST")]
        //[ResponseType(typeof(Response<UserDocumentResponse>))]
        //[Route("GetBanner")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //public async Task<IHttpActionResult> GetReferalUrl(RequestModel request)
        //{
        //    var response = new Response<ShareAndEarnResponse>();
        //    var result = new ShareAndEarnResponse();
        //    // int langId = AppUtils.GetLangId(Request);
        //    var requestModel = new EncrDecr<UserDocumentRequest>().Decrypt(request.Value, false);
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            result = await _shareAndEarnService.GetReferalUrl(requestModel.WalletUserId);
        //            if (result != null)
        //            {
        //                response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

        //            }
        //            else
        //            {
        //                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //        }
        //    }
        //    else
        //    {
        //        var errorList = new List<Errorkey>();
        //        foreach (var mod in ModelState)
        //        {
        //            Errorkey objkey = new Errorkey();
        //            objkey.Key = mod.Key;
        //            if (mod.Value.Errors.Count > 0)
        //            {
        //                objkey.Val = mod.Value.Errors[0].ErrorMessage;
        //            }
        //            errorList.Add(objkey);
        //        }
        //        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
        //    return _iHttpActionResult;
        //}

        /// <summary>
        /// RecentReceiver
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<RecentReceiverResponse>))]
        [Route("RecentReceiver")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> RecentReceiver(RequestModel request)
        {
            var response = new Response<List<RecentReceiverResponse>>();
            var result = new List<RecentReceiverResponse>();
            // int langId = AppUtils.GetLangId(Request);
            var requestModel = new EncrDecr<RecentReceiverRequest>().Decrypt(request.Value, false);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.RecentReceiver(requestModel);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false);
            return _iHttpActionResult;
        }


        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse>>))]
        [Route("IsdCodesFrancCountry")]
        public async Task<IHttpActionResult> IsdCodesFrancCountry()
        {
            var response = new Response<List<IsdCodesResponse>>();
            var result = new List<IsdCodesResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.IsdCodesFrancCountry();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse>>))]
        [Route("IsdCodesForXAFCountry")]
        public async Task<IHttpActionResult> IsdCodesForXAFCountry()
        {
            var response = new Response<List<IsdCodesResponse>>();
            var result = new List<IsdCodesResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.IsdCodesForXAFCountry();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }



        /// <summary>
        /// GetChannelsForISP
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<IspChannelResponse>>))]
        [Route("GetChannelsForISP")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetChannelsForISP(RequestModel request)
        {
            var response = new Response<List<IspChannelResponse>>();
            var result = new List<IspChannelResponse>();
            var requestModel = new EncrDecr<ChannelRequest>().Decrypt(request.Value, false);
            // var channels = _masterDataService.GetChannels(request);
            string sessionToken = Request.Headers.GetValues("token").FirstOrDefault();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetChannelsForISP(requestModel, sessionToken);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true);
            return _iHttpActionResult;
        }

        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<FAQResponse>>))]
        [Route("FAQWeb")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> FAQWeb()
        {
            var response = new Response<List<FAQResponse>>();
            var result = new List<FAQResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.FAQ();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>>))]
        [Route("GetcardnoList")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetcardnoList(RequestModel requestModel)
        {
            var response = new Response<List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>>();
            var result = new List<ViewModel.AdminViewModel.DuplicateCardNoVMResponse>();
            var request = new EncrDecr<ViewModel.AdminViewModel.DuplicateCardNoVMRequest>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetcardnoList(request.Walletuserid);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<ManageWalletServicesList>>))]
        [Route("ManageWalletServices")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> ManageWalletServices(RequestModel requestModel)
        {
            var response = new Response<List<ManageWalletServicesList>>();
            var result = new List<ManageWalletServicesList>();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<WalletServicesRequest>().Decrypt(requestModel.Value, false, Request);
                   
                    result = await _masterDataService.ManageWalletServices(request);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        //   _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// UpdateWalletServicesStatus
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateWalletServicesStatus")]
        [ResponseType(typeof(Response<Object>))]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> UpdateWalletServicesStatus(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                     var request = new EncrDecr<UpdateWalletServicesRequest>().Decrypt(requestModel.Value, false, Request);
                   
                    result = await _masterDataService.UpdateWalletServicesStatus(request);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.SERVICE_BLOCKED, HttpStatusCode.OK, result);
                    }
                    else if ((int)result == 2)
                    {
                        response = response.Create(true, AdminResponseMessages.SERVICE_UNBLOCKED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(true, AdminResponseMessages.SERVICE_UNBLOCKED, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(true, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(true, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.OK, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<NGNBankResponse>>))]
        [Route("GetNGNbankList")]
        public async Task<IHttpActionResult> GetNGNbankList()
        {
            var response = new Response<List<NGNBankResponse>>();
            var result = new List<NGNBankResponse>();
            
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetNGNbankList(0);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }



        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<NGNBankResponse>>))]
        [Route("GetPayNGNbankList")]
        public async Task<IHttpActionResult> GetPayNGNbankList()
        {
            var response = new Response<List<NGNBankResponse>>();
            var result = new List<NGNBankResponse>();

            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetNGNbankList(1);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<CurrencyvalueResponseById>>))]
        [Route("GetCurrencyValue")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        public async Task<IHttpActionResult> GetCurrencyValue(RequestModel requestModel)
        {
         
            var response = new Response<List<CurrencyvalueResponseById>>();
            var result = new List<CurrencyvalueResponseById>();
            var request = new EncrDecr<CurrencyvalueRequestById>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.GetCurrencyValue(request);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse>>))]
        [Route("IsdCodesAddMonMobMonCountry")]
        public async Task<IHttpActionResult> IsdCodesAddMonMobMonCountry()
        {
            var response = new Response<List<IsdCodesResponse>>();
            var result = new List<IsdCodesResponse>();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.IsdCodesAddMonMobMonCountry();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }

        /// <summary>
        /// IsdCodes
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(Response<List<IsdCodesResponse>>))]
        [Route("IsdCodesPayGhanaMobMonCountry")]
        public async Task<IHttpActionResult> IsdCodesPayGhanaMobMonCountry()
        {
            var response = new Response<List<IsdCodesResponse>>();
            var result = new List<IsdCodesResponse>();
           
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _masterDataService.IsdCodesPayGhanaMobMonCountry();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                var errorList = new List<Errorkey>();
                foreach (var mod in ModelState)
                {
                    Errorkey objkey = new Errorkey();
                    objkey.Key = mod.Key;
                    if (mod.Value.Errors.Count > 0)
                    {
                        objkey.Val = mod.Value.Errors[0].ErrorMessage;
                    }
                    errorList.Add(objkey);
                }
                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, false, false);
            return _iHttpActionResult;
        }


    }
}

