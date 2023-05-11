using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.SubAdmin;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// SubAdmin Management
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class SubAdminController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private ISubAdminService _subAdminService;
        private Converter _converter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="subAdminService"></param>
        public SubAdminController(ISubAdminService subAdminService)
        {
            _subAdminService = subAdminService;
            _converter = new Converter();
        }

        /// <summary>
        /// GetSubAdmins
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<SubadminListResponse>))]
        [Route("GetSubAdmins")]
        public async Task<IHttpActionResult> GetSubAdmins(RequestModel model)
        {
            var response = new Response<SubadminListResponse>();
            var result = new SubadminListResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<SubadminListRequest>().Decrypt(model.Value,false,Request);
                    result = await _subAdminService.GetSubAdmins(request);
                    if (result.SubadminList != null)
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
        /// SaveSubAdmin
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<SubadminSaveResponse>))]
        [Route("SaveSubAdmin")]
        public async Task<IHttpActionResult> SaveSubAdmin(RequestModel model)
        {
            var response = new Response<SubadminSaveResponse>();
            var result = new SubadminSaveResponse();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<SubAdminRequest>().Decrypt(model.Value,false,Request);
                    result = await _subAdminService.SaveSubAdmins(request);
                    switch (result.RstKey)
                    {                      
                        case 1:
                            response = response.Create(true, AdminResponseMessages.SUB_ADMIN_CREATED, HttpStatusCode.OK, result);
                            break;
                        case 2:
                            response = response.Create(true, ResponseMessages.EXIST_MOBILE_NO, HttpStatusCode.OK, result);
                            break;
                        case 3:
                            response = response.Create(true, ResponseMessages.EXIST_EMAIL, HttpStatusCode.OK, result);
                            break;                       
                        default:
                            response = response.Create(true, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.OK, result);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.AGGREGATOR_FAILED_ERROR, HttpStatusCode.NotAcceptable, result);                    
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
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK,true,false,Request);
            return _iHttpActionResult;
        }

        /// <summary>
        /// DeleteSubadmin
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("DeleteSubadmin")]
        public async Task<IHttpActionResult> DeleteSubadmin(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<UserDeleteRequest>().Decrypt(model.Value,false,Request);
                    result = await _subAdminService.DeleteSubadmin(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.USER_MANAGE_SUCCESS, "deleted"), HttpStatusCode.OK, result);
                      
                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.USER_MANAGE_FAILURE, "deleted"), HttpStatusCode.NotAcceptable, result);                        
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
        /// EnableDisableSubAdmin
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("EnableDisableSubAdmin")]
        public async Task<IHttpActionResult> EnableDisableSubAdmin(RequestModel model)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<SubAdminManageRequest>().Decrypt(model.Value,false,Request);
                    result = await _subAdminService.EnableDisableSubAdmin(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, string.Format(AdminResponseMessages.MANAGE_SUBADMIN_SUCCESS, request.IsActive ? "activated" : "deactivated"), HttpStatusCode.OK, result);                       
                    }
                    else
                    {
                        response = response.Create(false, string.Format(AdminResponseMessages.MANAGE_SUBADMIN_FAILURE, request.IsActive ? "activated" : "deactivated"), HttpStatusCode.NotAcceptable, result);                        
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
