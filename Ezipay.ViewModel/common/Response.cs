using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.common
{
    [Serializable]
    public class Response<T>
    {

        public bool isSuccess;
        public string message;
        public int status;

        public T result;

        public Response<T> Create(bool success, string message, HttpStatusCode status, T result)
        {
            Response<T> response = new Response<T>();
            response.isSuccess = success;
            response.message = message;
            response.result = result;
            response.status = (int)status;
            return response;
        }
    }

    public class Request
    {
        public string request { get; set; }
    }

    public class RequestModel
    {
        public string Value { get; set; }
    }

    public class Errorkey
    {
        public Errorkey() { }
        public Errorkey(string _key, string _value)
        {
            Key = _key;
            Val = _value;
        }

        public string Key { get; set; }
        public string Val { get; set; }

    }

    public static class GlobalData
    {
       //// public static string Key { get; set; }
        public static int  RoleId { get; set; }
        public static int AppVersion { get; set; }
        public static int AppId { get; set; }
    }
}
