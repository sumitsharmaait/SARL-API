using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Banner;
using Ezipay.Service.AdminService;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    /// <summary>
    /// Banner
    /// </summary>
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class BannerController : ApiController
    {
        private IHttpActionResult _iHttpActionResult;
        private readonly IBannerService _bannerService;
        private readonly IUserApiService _userApiService;
        private Converter _converter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bannerService"></param>
        public BannerController(IBannerService bannerService, IUserApiService userApiService)
        {
            _userApiService = userApiService;
            _bannerService = bannerService;
            _converter = new Converter();
        }

        ///// <summary>
        ///// GetMerchantList
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[AcceptVerbs("POST")]
        //[ResponseType(typeof(Response<Object>))]
        //[Route("InsertBanner")]
        //public async Task<IHttpActionResult> InsertBanner()
        //{
        //    var response = new Response<Object>();
        //    var result = new Object();
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var context = HttpContext.Current.Request;
        //            HttpContextWrapper objwrapper = GetHttpContext(this.Request);
        //            HttpPostedFileBase collection = objwrapper.Request.Files["bnr"];
        //            var request = new BannerRequest();
        //            string jsonvalue = objwrapper.Request.Form["json"];
        //            if (string.IsNullOrEmpty(jsonvalue))
        //            {
        //                response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //                _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //                return _iHttpActionResult;
        //            }
        //            request = JsonConvert.DeserializeObject<BannerRequest>(jsonvalue);
        //            if (collection != null)
        //            {
        //                request.BannerImage = await _userApiService.SaveImage(collection, "");

        //            }
        //            result = await _bannerService.InsertBanner(request);
        //            if ((bool)result)
        //            {
        //                response = response.Create(true, ResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);

        //            }
        //            else
        //            {
        //                response = response.Create(false, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
        //                //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
        //            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError, true, false, Request);
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
        //        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
        //    }
        //    _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
        //    return _iHttpActionResult;
        //}
        /// <summary>
        /// InsertBanner
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("InsertBanner")]
        public async Task<IHttpActionResult> InsertBanner(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            var request = new EncrDecr<BannerRequest>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    //var context = HttpContext.Current.Request;
                    //HttpContextWrapper objwrapper = GetHttpContext(this.Request);
                    //HttpPostedFileBase collection = objwrapper.Request.Files["bnr"];
                    //var request = new BannerRequest();
                    //string jsonvalue = objwrapper.Request.Form["json"];
                    if (request == null)
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                        return _iHttpActionResult;
                    }
                    //request = JsonConvert.DeserializeObject<BannerRequest>(jsonvalue);
                    //if (collection != null)
                    //{
                    //    request.BannerImage = await _userApiService.SaveImage(collection, "");

                    //}
                    result = await _bannerService.InsertBanner(request);
                    if ((bool)result)
                    {
                        response = response.Create(true, ResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);
                        _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
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
        /// GetBanner
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<BannerResponse>>))]
        [Route("GetBanner")]
        public async Task<IHttpActionResult> GetBanner()
        {
            var response = new Response<List<BannerResponse>>();
            var result = new List<BannerResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _bannerService.GetBanner();
                    if (result != null)
                    {
                        response = response.Create(true, ResponseMessages.DATA_RECEIVED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.DATA_NOT_RECEIVED, HttpStatusCode.NotAcceptable, result);
                        // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
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
        /// DeleteBanner
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("DeleteBanner")]
        public async Task<IHttpActionResult> DeleteBanner(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<DeleteBannerRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _bannerService.DeleteBanner(request.id);
                    if ((bool)result)
                    {
                        response = response.Create(true, ResponseMessages.BANNER_DELETED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.BANNER_NOT_DELETED, HttpStatusCode.NotAcceptable, result);
                        //_iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.BANNER_NOT_DELETED, HttpStatusCode.NotAcceptable, result);
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
                response = response.Create(false, ResponseMessages.BANNER_NOT_DELETED, HttpStatusCode.NotAcceptable, result);
                // _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.InternalServerError);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }

    }
}
