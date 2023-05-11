using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.AdminService;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CheckPasswordViewModel;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.WalletUserVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
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
    /// User Management
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class UserManagementController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        private IUserApiService _userApiService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="userApiService"></param>
        public UserManagementController(IUserApiService userApiService)
        {
            _userApiService = userApiService;
            _converter = new Converter();
        }

        /// <summary>
        /// UserList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserListResponse>))]
        [Route("UserList")]
        public async Task<IHttpActionResult> UserList(RequestModel requestModel)
        {
            var response = new Response<UserListResponse>();
            var result = new UserListResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserListRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.UserList(request);
                    if (result.UserList != null)
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// Change User Status
        /// Status={1-Block/Unblock,2-Delete}
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [Route("ChangeUserStatus")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> ChangeUserStatus(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserManageRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.EnableDisableUser(request);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.USER_BLOCKED, HttpStatusCode.OK, result);

                    }
                    else if ((int)result == 2)
                    {
                        response = response.Create(true, AdminResponseMessages.USER_UNBLOCKED, HttpStatusCode.OK, result);
                    }
                    else if ((int)result == 3)
                    {
                        response = response.Create(true, AdminResponseMessages.USER_DELETED, HttpStatusCode.OK, result);
                    }
                    else if ((int)result == 4)
                    {
                        response = response.Create(true, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(true, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.OK, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(true, AdminResponseMessages.TRANSACTION_NOT_ENABLED, HttpStatusCode.OK, result);
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

        //[AcceptVerbs("POST")]
        //[ResponseType(typeof(Response<bool>))]
        //[Route("Delete")]
        //public async Task<IHttpActionResult> Delete(UserDeleteRequest request)
        //{
        //    var response = new Response<object>();
        //    var result = new object();
        //    // int langId = AppUtils.GetLangId(Request);
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            result = await _userApiService.Delete(request);
        //            if (result != null)
        //            {
        //                response = response.Create(true, AdminResponseMessages.USER_MANAGE_SUCCESS, HttpStatusCode.OK, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //            }
        //            else
        //            {
        //                response = response.Create(false, AdminResponseMessages.USER_MANAGE_FAILURE, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, AdminResponseMessages.USER_MANAGE_FAILURE, HttpStatusCode.NotAcceptable, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
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
        //        response = response.Create(false, AdminResponseMessages.USER_MANAGE_FAILURE, HttpStatusCode.NotAcceptable, result);
        //        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //    }
        //    return _iHttpActionResult;
        //}

        /// <summary>
        /// VerifyUser
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserEmailVerifyResponse>))]
        [Route("VerifyUser")]
        public async Task<IHttpActionResult> VerifyUser(string token)
        {
            var response = new Response<UserEmailVerifyResponse>();
            var result = new UserEmailVerifyResponse();
            // int langId = AppUtils.GetLangId(Request);
            if (ModelState.IsValid)
            {
                try
                {
                    token = HttpUtility.UrlDecode(token);
                    result = await _userApiService.VerfiyByEmailId(token);
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
        /// UserTransactions
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<ViewUserTransactionResponse>))]
        [Route("UserTransactions")]
        public async Task<IHttpActionResult> UserTransactions(RequestModel requestModel)
        {
            var response = new Response<ViewUserTransactionResponse>();
            var result = new ViewUserTransactionResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<ViewUserTransactionRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.UserTransactions(request);
                    if (result.TransactionList != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// CreditDebitUserAccount
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<CreditDebitResponse>))]
        [Route("CreditDebitUserAccount")]
        public async Task<IHttpActionResult> CreditDebitUserAccount(RequestModel requestModel)
        {
            var response = new Response<CreditDebitResponse>();
            var result = new CreditDebitResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<CreditDebitRequest>().Decrypt(requestModel.Value, false, Request);

                    result = await _userApiService.CreditDebitUserAccount(request);
                    switch (result.RstKey)
                    {
                        case 1:
                            response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                            break;
                        case 2:
                            response = response.Create(false, AdminResponseMessages.USER_NOT_FOUND, HttpStatusCode.OK, result);
                            break;
                        case 3:
                            response = response.Create(false, AdminResponseMessages.USER_TRANSACTION_LOW_BALANCE, HttpStatusCode.OK, result);
                            break;
                        case 4:
                            response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.OK, result);
                            break;
                        case 5:
                            response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.OK, result);
                            break;
                        default:
                            response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.OK, result);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.USER_TRANSACTION_FAILURE, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// ManageTransaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<bool>))]
        [Route("ManageTransaction")]
        public async Task<IHttpActionResult> ManageTransaction(RequestModel requestModel)
        {
            var response = new Response<object>();
            var result = new object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<SetTransactionLimitRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.ManageTransaction(request);
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
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
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
                response = response.Create(false, AdminResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// GetTransactionLimitDetails
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserTransactionLimitDetailsResponse>))]
        [Route("GetTransactionLimitDetails")]
        public async Task<IHttpActionResult> GetTransactionLimitDetails(RequestModel requestModel)
        {
            var response = new Response<UserTransactionLimitDetailsResponse>();
            var result = new UserTransactionLimitDetailsResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserDetailsRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.GetTransactionLimitDetails(request);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// UserDetails
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserDetailsResponse>))]
        [Route("UserDetails")]
        public async Task<IHttpActionResult> UserDetails(RequestModel requestModel)
        {
            var response = new Response<UserDetailsResponse>();
            var result = new UserDetailsResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserDetailsRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.UserDetails(request);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// DownloadReportWithData
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<DownloadReportResponse>))]
        [Route("DownloadReportWithData")]
        public async Task<IHttpActionResult> DownloadReportWithData(RequestModel requestModel)
        {
            var response = new Response<DownloadReportResponse>();
            var result = new DownloadReportResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<DownloadReportApiRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.DownloadReportWithData(request);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// DeletedUserList
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserListResponse>))]
        [Route("DeletedUserList")]
        public async Task<IHttpActionResult> DeletedUserList(RequestModel requestModel)
        {
            var response = new Response<UserListResponse>();
            var result = new UserListResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserListRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.DeletedUserList(request);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// SaveUserDocument
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("SaveUserDocument2")]
        public async Task<IHttpActionResult> SaveUserDocument2()
        {
            var response = new Response<Object>();
            var result = new Object();

            try
            {
                HttpContextWrapper objwrapper = GetHttpContext(this.Request);
                HttpPostedFileBase atmcollection = objwrapper.Request.Files["atm"];
                HttpPostedFileBase idcollection = objwrapper.Request.Files["id"];

                string jsonvalue = objwrapper.Request.Form["userId"];
                string type = objwrapper.Request.Form["type"];
                var request = new DocumentUploadRequest();
                request.UserId = Convert.ToInt64(jsonvalue);
                request.ATMCard = string.Empty;
                request.IdCard = string.Empty;

                if (atmcollection != null || idcollection != null)
                {
                    request.ATMCard = await _userApiService.SaveImage(atmcollection, "");
                    request.IdCard = await _userApiService.SaveImage(idcollection, "");
                    if (!string.IsNullOrEmpty(request.ATMCard))
                    {
                        request.ATMCard = ConfigurationManager.AppSettings["ImageUrl"] + request.ATMCard;
                    }
                    if (!string.IsNullOrEmpty(request.IdCard))
                    {
                        request.IdCard = ConfigurationManager.AppSettings["ImageUrl"] + request.IdCard;

                    }
                    result = await _userApiService.SaveUserDocument(request, Convert.ToInt32(type));
                }


                if ((bool)result)
                {
                    response = response.Create(true, ResponseMessages.DOCUMENT_UPLOADED, HttpStatusCode.OK, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DOCUMENT_NOT_UPLOADED, HttpStatusCode.NotAcceptable, result);
                    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }

            return _iHttpActionResult;
        }

        HttpContextWrapper GetHttpContext(HttpRequestMessage request = null)
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
        /// documentdetail
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserDocumentDetailsResponse>))]
        [Route("documentdetail")]
        public async Task<IHttpActionResult> ViewDocumentDetails(RequestModel requestModel)
        {
            var response = new Response<UserDocumentDetailsResponse>();
            var result = new UserDocumentDetailsResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserDetailsRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.ViewDocumentDetails(request);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("ChangeUserDocumentStatus")]
        public async Task<IHttpActionResult> ChangeUserDocumentStatus(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<DocumentChangeRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.ChangeUserDocumentStatus(request);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.STATUS_CHANGED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.UNABLE_TO_PROCESS, HttpStatusCode.NotAcceptable, result);
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

        /// <summary>
        /// PendingKycUserList
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserListResponse>))]
        [Route("PendingKycUserList")]
        public async Task<IHttpActionResult> PendingKycUserList(RequestModel requestModel)
        {
            var response = new Response<UserListResponse>();
            var result = new UserListResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserListRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.PendingKycUserList(request);
                    if (result.UserList != null)
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }


        /// <summary>
        /// ExportUserListReport
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("exportUserListReport")]
        [Description("exportUserListReport")]
        public async Task<HttpResponseMessage> ExportUserListReport(DownloadLogReportRequest request)
        {
            // int langId = AppUtils.GetLangId(Request);
            string filename = "EzipayLog";
            MemoryStream memoryStream = null;
            memoryStream = await _userApiService.ExportUserListReport(request);
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

        /// <summary>
        /// SaveUserDocument
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("SaveUserDocument")]
        public async Task<IHttpActionResult> SaveUserDocument(RequestModel request)
        {
            var requestModel = new EncrDecr<DocumentUploadRequest>().Decrypt(request.Value, false, Request);
            var response = new Response<Object>();
            var result = new Object();
            var req = new DocumentUploadRequest
            {
                UserId = requestModel.UserId,
                ATMCard = requestModel.ATMCard,
                IdCard = requestModel.IdCard
            };
            try
            {
                if (requestModel.ATMCard != null || requestModel.IdCard != null)
                {
                    result = await _userApiService.SaveUserDocument(req, 2);
                }
                if ((bool)result)
                {
                    response = response.Create(true, ResponseMessages.DOCUMENT_UPLOADED, HttpStatusCode.OK, result);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DOCUMENT_NOT_UPLOADED, HttpStatusCode.NotAcceptable, result);
                }
            }
            catch (Exception ex)
            {
                response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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
        [ResponseType(typeof(Response<Object>))]
        [Route("VerifyEmail")]
        public async Task<IHttpActionResult> VerifyEmail(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<DocumentChangeRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _userApiService.VerifyEmail(request);
                    if ((int)result == 1)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.STATUS_CHANGED, HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.UNABLE_TO_PROCESS, HttpStatusCode.NotAcceptable, result);
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


        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<DuplicateCardNoVMResponse>>))]
        [Route("GetduplicatecardnoList")]
        public async Task<IHttpActionResult> GetduplicatecardnoList(RequestModel requestModel)
        {
            var response = new Response<List<DuplicateCardNoVMResponse>>();
            var result = new List<DuplicateCardNoVMResponse>();
            var request = new EncrDecr<DuplicateCardNoVMRequest>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _userApiService.GetduplicatecardnoList(request.Cardno,request.Walletuserid);
                    if (result != null)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_FOUND, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, AdminResponseMessages.DATA_NOT_FOUND, HttpStatusCode.NotAcceptable, result);
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


        [AcceptVerbs("POST")]
        [Route("Insertduplicatecardno")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> Insertduplicatecardno(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            var request = new EncrDecr<DuplicateCardNoVMRequest>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                        return _iHttpActionResult;
                    }
                    result = await _userApiService.Insertduplicatecardno(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, AdminResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, AdminResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
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


        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<UserBlockUnblockDetailResponse>))]
        [Route("EnableDisableUserList")]
        public async Task<IHttpActionResult> EnableDisableUserList(RequestModel requestModel)
        {
            var request = new EncrDecr<UserListRequest>().Decrypt(requestModel.Value, false, Request);

            var response = new Response<UserBlockUnblockDetailResponse>();
            var result = new UserBlockUnblockDetailResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    
                    result = await _userApiService.EnableDisableUserList(request);
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }




      

    }
}
