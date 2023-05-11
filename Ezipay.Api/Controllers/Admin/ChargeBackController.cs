using Ezipay.Api.Filters;
using Ezipay.Repository.AdminRepo.ChargeBack;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Net;
using ezeePay.Utility.CommonClass;

namespace Ezipay.Api.Controllers.Admin
{
    [RoutePrefix("api/adminChargeBack")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]

    public class ChargeBackController : ApiController
    {

        private IHttpActionResult _iHttpActionResult;
        private IChargeBackService _ChargeBack_Service;
        private Converter _converter;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ChargeBackService"></param>
        public ChargeBackController(IChargeBackService ChargeBack_Service)
        {
            _ChargeBack_Service = ChargeBack_Service;
            _converter = new Converter();
        }

        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<Database.ChargeBack>>))]
        [Route("GetChargeBackList")]
        public async Task<IHttpActionResult> GetChargeBackList()
        {
            var response = new Response<List<Database.ChargeBack>>();
            var result = new List<Database.ChargeBack>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _ChargeBack_Service.GetChargeBackList();
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
        [Route("InsertChargeBackDetail")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> InsertChargeBackDetail(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            var request = new EncrDecr<ChargeBackRequest>().Decrypt(requestModel.Value, false, Request);
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
                    result = await _ChargeBack_Service.InsertChargeBackDetail(request);
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


        //for web use by id
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<Database.ChargeBack>>))]
        [Route("GetChargeBackListById")]
        public async Task<IHttpActionResult> GetChargeBackListById(RequestModel requestModel)
        {
            var response = new Response<List<Database.ChargeBack>>();
            var result = new List<Database.ChargeBack>();           
            
            var request = new EncrDecr<ChargeBackRequest>().Decrypt(requestModel.Value, false, Request);
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _ChargeBack_Service.GetChargeBackListById(request);
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


        //////////
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<Database.freezeuser>>))]
        [Route("GetfreezeList")]
        public async Task<IHttpActionResult> GetfreezeList()
        {
            var response = new Response<List<Database.freezeuser>>();
            var result = new List<Database.freezeuser>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _ChargeBack_Service.GetfreezeList();
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
        [Route("InsertfreezeDetail")]
        [ResponseType(typeof(Response<Object>))]
        public async Task<IHttpActionResult> InsertfreezeDetail(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            var request = new EncrDecr<freezeRequest>().Decrypt(requestModel.Value, false, Request);
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
                    result = await _ChargeBack_Service.InsertfreezeDetail(request);
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

    }
}
