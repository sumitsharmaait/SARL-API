using ezeePay.Utility.CommonClass;
using ezeePay.Utility.Enums;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Merchant;
using Ezipay.Service.AdminService;
using Ezipay.Service.UserService;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.WalletUserVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// Merchant Management
    /// </summary>
    [RoutePrefix("api/admin")]
    public class MerchantManagementController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private IMerchantService _merchantService;
        private IUserApiService _userApiService;
        private IWalletUserService _walletUserService;
        private Converter _converter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="merchantService"></param>
        /// <param name="userApiService"></param>
        /// <param name="walletUserService"></param>
        public MerchantManagementController(IWalletUserService walletUserService, IMerchantService merchantService, IUserApiService userApiService)
        {
            _walletUserService = walletUserService;
            _userApiService = userApiService;
            _merchantService = merchantService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetMerchantList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<MerchantListResponse>))]
        [Route("GetMerchantList")]
        public async Task<IHttpActionResult> GetMerchantList(RequestModel request)
        {
            var response = new Response<MerchantListResponse>();
            var result = new MerchantListResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<MerchantListRequest>().Decrypt(request.Value, false, Request);
                    result = await _merchantService.GetMerchantList(requestModel);
                    if (result.MerchantList != null)
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
                    //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
                //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        ///// <summary>
        ///// SaveMerchant
        ///// </summary>
        ///// <returns></returns>
        //[AcceptVerbs("POST")]
        //[SessionAuthorization]
        //[SessionTokenExceptionFilter]
        //[ResponseType(typeof(Response<MerchantSaveResponse>))]
        //[Route("SaveMerchant")]
        //public async Task<IHttpActionResult> SaveMerchant()
        //{
        //    var response = new Response<MerchantSaveResponse>();
        //    var result = new MerchantSaveResponse();
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var context = HttpContext.Current.Request;
        //            HttpContextWrapper objwrapper = GetHttpContext(this.Request);
        //            HttpPostedFileBase collection = objwrapper.Request.Files["merchantLogo"];
        //            var request = new MerchantRequest();
        //            string jsonvalue = objwrapper.Request.Form["json"];
        //            if (string.IsNullOrEmpty(jsonvalue))
        //            {
        //                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //                return _iHttpActionResult;
        //            }
        //            request = JsonConvert.DeserializeObject<MerchantRequest>(jsonvalue);
        //            if (collection != null)
        //            {
        //                request.LogoUrl = await _userApiService.SaveImage(collection, "");

        //            }


        //            result = await _merchantService.SaveMerchant(request);
        //            if (request.MerchantId == 0 && result.statusCode == 1)
        //            {
        //                response = response.Create(true, AdminResponseMessages.MERCHANT_CREATED, HttpStatusCode.OK, result);

        //            }
        //            else if (request.MerchantId > 0 && result.statusCode == 1)
        //            {
        //                response = response.Create(true, AdminResponseMessages.MERCHANT_UPDATED, HttpStatusCode.NotAcceptable, result);
        //                //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //            }
        //            else if (result.statusCode == 2)
        //            {
        //                response = response.Create(false, AdminResponseMessages.DUPLICATE_USER, HttpStatusCode.NotAcceptable, result);
        //                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //            }
        //            else
        //            {
        //                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);
        //                //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);
        //            //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        //        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}

        /// <summary>
        /// SaveMerchant
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<MerchantSaveResponse>))]
        [Route("SaveMerchant")]
        public async Task<IHttpActionResult> SaveMerchant(RequestModel request)
        {
            var response = new Response<MerchantSaveResponse>();
            var result = new MerchantSaveResponse();
            var requestModel = new EncrDecr<MerchantRequest>().Decrypt(request.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    //var context = HttpContext.Current.Request;
                    //HttpContextWrapper objwrapper = GetHttpContext(this.Request);
                    //HttpPostedFileBase collection = objwrapper.Request.Files["merchantLogo"];
                    //var request = new MerchantRequest();
                    //string jsonvalue = objwrapper.Request.Form["json"];
                    if (string.IsNullOrEmpty(requestModel.EmailId))
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                        return _iHttpActionResult;
                    }
                    //request = JsonConvert.DeserializeObject<MerchantRequest>(jsonvalue);
                    //if (collection != null)
                    //{
                    //    request.LogoUrl = await _userApiService.SaveImage(collection, "");

                    //}


                    result = await _merchantService.SaveMerchant(requestModel);
                    if (requestModel.MerchantId == 0 && result.statusCode == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.MERCHANT_CREATED, HttpStatusCode.OK, result);
                    }
                    else if (requestModel.MerchantId > 0 && result.statusCode == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.MERCHANT_UPDATED, HttpStatusCode.NotAcceptable, result);
                    }
                    else if (result.statusCode == 2)
                    {
                        response = response.Create(false, AdminResponseMessages.DUPLICATE_USER, HttpStatusCode.NotAcceptable, result);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);
                    }

                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);
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
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        private HttpContextWrapper GetHttpContext(HttpRequestMessage request = null)
        {
            request = request ?? Request;
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]);
            }
            else if (HttpContext.Current != null)
            {
                return new HttpContextWrapper(HttpContext.Current);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// EnableDisableMerchant
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        [Route("EnableDisableMerchant")]
        public async Task<IHttpActionResult> EnableDisableMerchant(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<MerchantManageRequest>().Decrypt(model.Value, false, Request);
                    result = await _merchantService.EnableDisableMerchant(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.USER_MANAGE_SUCCESS, request.IsActive ? "activated" : "deactivated"), HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.USER_MANAGE_FAILURE, request.IsActive ? "activated" : "deactivated"), HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        /// DeleteMarchant
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        [Route("DeleteMarchant")]
        public async Task<IHttpActionResult> DeleteMarchant(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<MarchantDeleteRequest>().Decrypt(request.Value, false, Request);
                    result = await _merchantService.DeleteMarchant(requestModel);
                    if ((bool)result)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.MANAGE_MARCHANT_SUCCESS, "deleted"), HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.MANAGE_MARCHANT_FAILURE, "deleted"), HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        /// EnableDisableTransaction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        [Route("EnableDisableTransaction")]
        public async Task<IHttpActionResult> EnableDisableTransaction(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<MerchantEnableTransactionRequest>().Decrypt(model.Value, false, Request);
                    result = await _merchantService.EnableDisableTransaction(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.MERCHANT_MANAGE_SUCCESS, request.IsDisabledTransaction ? "Transaction activated " : "Transaction deactivated "), HttpStatusCode.OK, true);
                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.MERCHANT_MANAGE_FAILURE, request.IsDisabledTransaction ? " Transaction activated " : "Transaction deactivated "), HttpStatusCode.NotFound, false);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, false);
                    //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        /// ViewMerchantTransactions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<ViewMarchantTransactionResponse>))]
        [Route("ViewMerchantTransactions")]
        public async Task<IHttpActionResult> ViewMerchantTransactions(RequestModel model)
        {
            var response = new Response<ViewMarchantTransactionResponse>();
            var result = new ViewMarchantTransactionResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<ViewMarchantTransactionRequest>().Decrypt(model.Value, false, Request);
                    result = await _merchantService.ViewMerchantTransactions(request);
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        /// exportMerchantListReport
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [Route("exportMerchantListReport")]
        [Description("exportMerchantListReport")]
        public async Task<HttpResponseMessage> ExportMerchantListReport(DownloadLogReportRequest request)
        {
            // int langId = AppUtils.GetLangId(Request);
            string filename = "EzipayLog";
            MemoryStream memoryStream = null;
            memoryStream = await _merchantService.ExportMerchantListReport(request);
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(memoryStream.ToArray())
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue
                      ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Content.Headers.ContentDisposition =
                   new ContentDispositionHeaderValue("attachment")
                   {
                       FileName = $"{filename}_{DateTime.Now.Ticks.ToString()}.xls"
                   };
            //response.Content.Headers.ContentLength = stream.Length;
            memoryStream.WriteTo(memoryStream);
            memoryStream.Close();
            return response;
        }


        ///// <summary>
        ///// MerchantOnBoardRequest
        ///// </summary>
        ///// <returns></returns>
        //[AcceptVerbs("POST")]
        //[TempSessionAuthorization]
        //[TempTokenExceptionFilter]
        //[ResponseType(typeof(Response<MerchantSaveResponse>))]
        //[Route("MerchantOnBoardRequest")]
        //public async Task<IHttpActionResult> MerchantOnBoardRequest()
        //{
        //    var response = new Response<MerchantSaveResponse>();
        //    var result = new MerchantSaveResponse();
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var context = HttpContext.Current.Request;
        //            HttpContextWrapper objwrapper = GetHttpContext(this.Request);
        //            HttpPostedFileBase collection = objwrapper.Request.Files["merchantLogo"];
        //            HttpPostedFileBase atmcollection = objwrapper.Request.Files["atm"];
        //            HttpPostedFileBase idcollection = objwrapper.Request.Files["id"];
        //            var request = new MerchantRequest();
        //            string jsonvalue = objwrapper.Request.Form["json"];
        //            if (string.IsNullOrEmpty(jsonvalue))
        //            {
        //                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //                return _iHttpActionResult;
        //            }
        //            request = JsonConvert.DeserializeObject<MerchantRequest>(jsonvalue);

        //            #region Mobile Verification
        //            var OtpVerification = await _walletUserService.VerifyOtp(new VerifyOtpRequest { MobileNo = (request.IsdCode + request.MobileNo), Otp = request.Otp });
        //            if (OtpVerification.Status != 2)
        //            {
        //                response = response.Create(false, ResponseMessages.OTP_NOT_VERIFIED, HttpStatusCode.OK, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //                return _iHttpActionResult;
        //            }
        //            #endregion




        //            if (collection != null)
        //            {
        //                request.LogoUrl = await _userApiService.SaveImage(collection, "");

        //            }
        //            if (atmcollection != null || idcollection != null)
        //            {
        //                request.ATMCard = await _userApiService.SaveImage(atmcollection, "");
        //                request.IdCard = await _userApiService.SaveImage(idcollection, "");
        //                if (!string.IsNullOrEmpty(request.ATMCard))
        //                {
        //                    request.ATMCard = ConfigurationManager.AppSettings["ImageUrl"] + request.ATMCard;
        //                }
        //                if (!string.IsNullOrEmpty(request.IdCard))
        //                {
        //                    request.IdCard = ConfigurationManager.AppSettings["ImageUrl"] + request.IdCard;

        //                }

        //            }

        //            //-------Merchant additional documents-----
        //            request.Documents = new List<DocModel>();
        //            for (int i = 0; i < request.AddrsFileCount; i++)
        //            {
        //                HttpPostedFileBase doc_collection = objwrapper.Request.Files["addrs" + i.ToString()];
        //                string image = await _userApiService.SaveImage(doc_collection, "");
        //                request.Documents.Add(new DocModel { DocType = (int)EnumDocType.Address, DocName = image });
        //            }

        //            for (int i = 0; i < request.ShareholderIdFileCount; i++)
        //            {
        //                HttpPostedFileBase doc_collection = objwrapper.Request.Files["shareholdersID" + i.ToString()];
        //                string image = await _userApiService.SaveImage(doc_collection, "");
        //                request.Documents.Add(new DocModel { DocType = (int)EnumDocType.ShareholderId, DocName = image });
        //            }

        //            for (int i = 0; i < request.ShareholderImageFileCount; i++)
        //            {
        //                HttpPostedFileBase doc_collection = objwrapper.Request.Files["shareholdersImage" + i.ToString()];
        //                string image = await _userApiService.SaveImage(doc_collection, "");
        //                request.Documents.Add(new DocModel { DocType = (int)EnumDocType.ShareholderImage, DocName = image });
        //            }


        //            result = await _merchantService.MerchantOnBoardRequest(request);
        //            if (request.MerchantId == 0 && result.statusCode == 1)
        //            {
        //                response = response.Create(true, AdminResponseMessages.MERCHANT_CREATED, HttpStatusCode.OK, result);

        //            }
        //            else if (request.MerchantId > 0 && result.statusCode == 1)
        //            {
        //                response = response.Create(true, AdminResponseMessages.MERCHANT_UPDATED, HttpStatusCode.NotAcceptable, result);

        //            }
        //            else if (result.statusCode == 2)
        //            {
        //                response = response.Create(false, AdminResponseMessages.DUPLICATE_USER, HttpStatusCode.NotAcceptable, result);

        //            }
        //            else
        //            {
        //                response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);

        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);

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
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true, Request);
        //    return _iHttpActionResult;
        //}

        /// <summary>
        /// SaveStore
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        [Route("SaveStore")]
        public async Task<IHttpActionResult> SaveStore(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<AddStoreRequest>().Decrypt(model.Value, false, Request);
                    result = await _merchantService.SaveStore(request);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ex.Message, HttpStatusCode.NotAcceptable, result);
                    // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false);
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
        /// GetStores
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<List<StoreResponse>>))]
        [Route("GetStores")]
        public async Task<IHttpActionResult> GetStores(RequestModel model)
        {
            var response = new Response<List<StoreResponse>>();
            var result = new List<StoreResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<StoreSearchRequest>().Decrypt(model.Value, false, Request);
                    result = await _merchantService.GetStores(request);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);

                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                    // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
                //  _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// EnableDisableStore
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        [Route("EnableDisableStore")]
        public async Task<IHttpActionResult> EnableDisableStore(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<StoreManageRequest>().Decrypt(model.Value, false, Request);
                    result = await _merchantService.EnableDisableStore(request);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.USER_MANAGE_SUCCESS, request.IsActive ? "activated" : "deactivated"), HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.USER_MANAGE_FAILURE, request.IsActive ? "activated" : "deactivated"), HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        /// DeleteStore
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SessionAuthorization]
        [SessionTokenExceptionFilter]
        [ResponseType(typeof(Response<Object>))]
        [Route("DeleteStore")]
        public async Task<IHttpActionResult> DeleteStore(RequestModel request)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var requestModel = new EncrDecr<StoreDeleteRequest>().Decrypt(request.Value, false, Request);
                    result = await _merchantService.DeleteStore(requestModel);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.MANAGE_MARCHANT_SUCCESS, "deleted"), HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.MANAGE_MARCHANT_FAILURE, "deleted"), HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        /// MerchantOnBoardRequest
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [TempSessionAuthorization]
        [TempTokenExceptionFilter]
        [ResponseType(typeof(Response<MerchantSaveResponse>))]
        [Route("MerchantOnBoardRequest")]
        public async Task<IHttpActionResult> MerchantOnBoardRequest(MerchantRequest request)
        {
            var response = new Response<MerchantSaveResponse>();
            var result = new MerchantSaveResponse();
            // var image = "";
            if (ModelState.IsValid)
            {
                try
                {
                    //var context = HttpContext.Current.Request;
                    //HttpContextWrapper objwrapper = GetHttpContext(this.Request);
                    //HttpPostedFileBase collection = objwrapper.Request.Files["merchantLogo"];
                    //HttpPostedFileBase atmcollection = objwrapper.Request.Files["atm"];
                    //HttpPostedFileBase idcollection = objwrapper.Request.Files["id"];
                    //var request = new MerchantRequest();
                    //string jsonvalue = objwrapper.Request.Form["json"];
                    //if (string.IsNullOrEmpty(jsonvalue))
                    //{
                    //    response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                    //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    //    return _iHttpActionResult;
                    //}
                    //request = JsonConvert.DeserializeObject<MerchantRequest>(jsonvalue);

                    #region Mobile Verification change below otp 

                    ////var OtpVerification = await _walletUserService.VerifyOtp(new VerifyOtpRequest { MobileNo = (request.IsdCode + request.MobileNo), Otp = request.Otp });
                    ////if (OtpVerification.Status != 2)
                    ////{
                    ////    response = response.Create(false, ResponseMessages.OTP_NOT_VERIFIED, HttpStatusCode.OK, result);
                    ////    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    ////    return _iHttpActionResult;
                    ////}
                    #endregion

                    request.LogoUrl = request.Documents.Where(x => x.DocType == 0).FirstOrDefault().DocName;
                    request.ATMCard = request.Documents.Where(x => x.DocType == 1).FirstOrDefault().DocName;
                    request.IdCard = request.Documents.Where(x => x.DocType == 2).FirstOrDefault().DocName;
                    //  request.AddrsFileCount= request.Documents.Where(x => x.DocType == 3).ToList();                    
                    if (request.ATMCard != null || request.IdCard != null)
                    {
                        //request.ATMCard = await _userApiService.SaveImage(atmcollection, "");
                        //request.IdCard = await _userApiService.SaveImage(idcollection, "");
                        if (!string.IsNullOrEmpty(request.ATMCard))
                        {
                            request.ATMCard = request.ATMCard;
                        }
                        if (!string.IsNullOrEmpty(request.IdCard))
                        {
                            request.IdCard = request.IdCard;

                        }

                    }

                    //-------Merchant additional documents-----
                    //   request.Documents = new List<DocModel>();
                    if (request != null)
                    {
                        var add = request.Documents.Where(x => x.DocType == 3).ToList();
                        foreach (var image in add)
                        {
                            request.Documents.Add(new DocModel { DocType = (int)EnumDocType.Address, DocName = image.DocName });
                        }
                        // HttpPostedFileBase doc_collection = objwrapper.Request.Files["addrs" + i.ToString()];
                        // string image = await _userApiService.SaveImage(doc_collection, "");

                    }

                    if (request != null)
                    {
                        //HttpPostedFileBase doc_collection = objwrapper.Request.Files["shareholdersID" + i.ToString()];
                        //string image = await _userApiService.SaveImage(doc_collection, "");
                        var add = request.Documents.Where(x => x.DocType == 4).ToList();
                        foreach (var image in add)
                        {
                            request.Documents.Add(new DocModel { DocType = (int)EnumDocType.ShareholderId, DocName = image.DocName });
                        }
                    }

                    if (request != null)
                    {
                        var add = request.Documents.Where(x => x.DocType == 5).ToList();
                        foreach (var image in add)
                        {
                            //HttpPostedFileBase doc_collection = objwrapper.Request.Files["shareholdersImage" + i.ToString()];
                            //string image = await _userApiService.SaveImage(doc_collection, "");
                            request.Documents.Add(new DocModel { DocType = (int)EnumDocType.ShareholderImage, DocName = image.DocName });
                        }
                    }


                    result = await _merchantService.MerchantOnBoardRequest(request);
                    if (request.MerchantId == 0 && result.statusCode == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.MERCHANT_CREATED, HttpStatusCode.OK, result);

                    }
                    else if (request.MerchantId > 0 && result.statusCode == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.MERCHANT_UPDATED, HttpStatusCode.NotAcceptable, result);

                    }
                    else if (result.statusCode == 2)
                    {
                        response = response.Create(false, AdminResponseMessages.DUPLICATE_USER, HttpStatusCode.NotAcceptable, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);

                    }

                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.EXCEPTION_OCCURED, HttpStatusCode.NotAcceptable, result);

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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, true, Request);
            return _iHttpActionResult;
        }
    }
}
