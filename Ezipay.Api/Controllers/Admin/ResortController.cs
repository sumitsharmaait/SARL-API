using ezeePay.Utility.CommonClass;
using Ezipay.Api.Filters;
using Ezipay.Service.Admin.Resort;
using Ezipay.ViewModel.common;
using Ezipay.ViewModel.ResortViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Ezipay.Api.Controllers.Admin
{
    [RoutePrefix("api/admin")]
    [SessionAuthorization]
    [SessionTokenExceptionFilter]
    public class ResortController : ApiController
    {
        private IResortService _resortService;
        private IHttpActionResult _iHttpActionResult;
        private Converter _converter;
        public ResortController(IResortService resortService)
        {
            _resortService = resortService;
            _converter = new Converter();
        }
        /// <summary>
        /// InsertHotel
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<object>))]
        [Description("InsertHotel.")]
        [Route("InsertHotel")]
        public async Task<IHttpActionResult> InsertHotel()
        {

            var response = new Response<Object>();
            var result = new Object();
            try
            {
                HttpContextWrapper objwrapper = GetHttpContext(this.Request);

                HttpPostedFileBase hotelImage = objwrapper.Request.Files["hotelImage"];
                HttpPostedFileBase pdfFile = objwrapper.Request.Files["pdfFile"];
                var location = HttpContext.Current.Request.Form["Location"];
                var noOfRooms = HttpContext.Current.Request.Form["NoOfRooms"];
                var costOfRooms = HttpContext.Current.Request.Form["CostOfRooms"];
                var maxGuest = HttpContext.Current.Request.Form["MaxGuest"];
                var hotelName = HttpContext.Current.Request.Form["HotelName"];
                var hotelStatus = HttpContext.Current.Request.Form["HotelStatus"];
                var AvailableRooms = HttpContext.Current.Request.Form["AvailableRooms"];
                //string jsonvalue = objwrapper.Request.Form["userId"];
                var request = new HotelRequest();
                request.HotelImage = string.Empty;
                request.CostOfRooms = 0;
                request.HotelName = string.Empty;
                request.Location = string.Empty;
                request.NoOfRooms = 0;
                request.PdfUrl = string.Empty;
                request.MaxGuest = 0;
                request.AvailableRooms = 0;
                bool _response = false;

                if (hotelImage != null && pdfFile != null)
                {
                    var hotelImg = await _resortService.SaveImage(hotelImage, "");
                    var file = await _resortService.SaveImage(pdfFile, "");
                    request.HotelImage = hotelImg;
                    request.PdfUrl = file;
                    request.CostOfRooms = Convert.ToDecimal(costOfRooms);
                    request.HotelName = hotelName;
                    request.Location = location;
                    request.NoOfRooms = Convert.ToInt32(noOfRooms);
                    request.MaxGuest = Convert.ToInt32(maxGuest);
                    if (!string.IsNullOrEmpty(request.HotelImage))
                    {
                        request.HotelImage = ConfigurationManager.AppSettings["ImageUrl"] + request.HotelImage;
                        request.PdfUrl = ConfigurationManager.AppSettings["ImageUrl"] + request.PdfUrl;
                        _response = await _resortService.InsertHotel(request);

                    }
                }
                if (_response)
                {
                    response = response.Create(true, ResponseMessages.DATA_SAVED, HttpStatusCode.OK, result);
                }
                else
                {
                    response = response.Create(false, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotAcceptable, result);
                }
            }
            catch (Exception ex)
            {

            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
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
        /// GetHotels
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<List<HotelMasterResponse>>))]
        [Route("GetHotels")]
        public async Task<IHttpActionResult> GetHotels()
        {
            var response = new Response<List<HotelMasterResponse>>();
            var result = new List<HotelMasterResponse>();
            if (ModelState.IsValid)
            {
                try
                {
                    result = await _resortService.GetHotels();
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


        /// <summary>
        /// DeleteHotel
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [ResponseType(typeof(Response<Object>))]
        [Route("DeleteHotel")]
        public async Task<IHttpActionResult> DeleteHotel(RequestModel requestModel)
        {
            var response = new Response<Object>();
            var result = new Object();
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new EncrDecr<DeleteResortRequest>().Decrypt(requestModel.Value, false, Request);
                    result = await _resortService.DeleteHotel(request.id);
                    if ((bool)result)
                    {
                        response = response.Create(true, ResponseMessages.RESORT_DELETED, HttpStatusCode.OK, result);

                    }
                    else
                    {
                        response = response.Create(false, ResponseMessages.RESORT_NOT_DELETED, HttpStatusCode.NotAcceptable, result);
                    }
                }
                catch (Exception ex)
                {
                    response = response.Create(false, ResponseMessages.RESORT_NOT_DELETED, HttpStatusCode.NotAcceptable, result);
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
                response = response.Create(false, ResponseMessages.RESORT_NOT_DELETED, HttpStatusCode.NotAcceptable, result);
            }
            _iHttpActionResult = _converter.ApiResponseMessage(response, HttpStatusCode.OK, true, false, Request);
            return _iHttpActionResult;
        }
        //[AcceptVerbs("POST")]
        //[ResponseType(typeof(Response<bool>))]
        //[Description("UpdateHotel.")]
        //[Route("UpdateHotel")]
        //public async Task<Response<bool>> UpdateHotel()
        //{

        //    Response<bool> response = new Response<bool>();
        //    try
        //    {


        //        var availableRooms = HttpContext.Current.Request.Form["NoOfRooms"];
        //        var costOfRooms = HttpContext.Current.Request.Form["CostOfRooms"];
        //        var maxGuest = HttpContext.Current.Request.Form["MaxGuest"];
        //        var hotelStatus = HttpContext.Current.Request.Form["HotelStatus"];
        //        var hotelId = HttpContext.Current.Request.Form["hotelId"];
        //        //string jsonvalue = objwrapper.Request.Form["userId"];               
        //        var re = new HotelMaster();
        //        bool _response = false;

        //        if (availableRooms != null || availableRooms != null)
        //        {
        //            re.CostOfRooms = Convert.ToDecimal(costOfRooms);
        //            re.AvailableRooms = Convert.ToInt32(availableRooms);
        //            re.MaxGuest = Convert.ToInt32(maxGuest);
        //            re.Id = Convert.ToInt32(hotelId);
        //            if (!string.IsNullOrEmpty(availableRooms))
        //            {
        //                _response = await _resortService.UpdateHotel(re);
        //            }
        //        }

        //        if (_response)
        //        {
        //            response.Create(true, ResponseMessages.DATA_SAVED, HttpStatusCode.OK, _response);
        //            // CurrentData.uesrprofilePic = request.ATMCard;
        //        }
        //        else
        //        {
        //            response.Create(false, ResponseMessages.DATA_NOT_SAVED, HttpStatusCode.NotFound, _response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return response;
        //}
    }
}
