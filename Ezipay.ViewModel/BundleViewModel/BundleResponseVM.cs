using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.BundleViewModel
{
    public class IspBundlesResponse : ISPBundleDataObject
    {
      
        public string status { get; set; }
        public string message { get; set; }
        public string trxn { get; set; }
        public string DisplayContent { get; set; }
        public string AccountType { get; set; }
        public string Result { get; set; }
       
    }
    public class IspBundlesRequest
    {
        public string IspType { get; set; }
        public string AccountNumber { get; set; }

    }
    public class ISPBundleData
    {
        public ISPBundleDataObject[] Property1 { get; set; }
    }

    public class ISPBundleDataObject
    {
        public string Name { get; set; }
        public string Amount { get; set; }
        public string BundleId { get; set; }
        public string Description { get; set; }
        public int network_id { get; set; }
        public string plan_id { get; set; }
        public string validity { get; set; }
        public string plan_name { get; set; }
        public string type { get; set; }
        public string volume { get; set; }
        public string category { get; set; }
        public string price { get; set; }
    }
    public class IspBundlesResponce : ISPBundleDataObject
    {
        //public IspBundlesResponce()
        //{
        //    bundles = new List<Bundle>();
        //}
        public string status { get; set; }
        public string message { get; set; }
        public string trxn { get; set; }
        // public List<Bundle> bundles { get; set; }
        public string DisplayContent { get; set; }
        public string AccountType { get; set; }
        public string Result { get; set; }
    }
    public class Bundle
    {
        public int network_id { get; set; }
        public string plan_id { get; set; }
        public string validity { get; set; }
        public string plan_name { get; set; }
        public string type { get; set; }
        public string volume { get; set; }
        public string category { get; set; }
        public string price { get; set; }
        public string name { get; set; }
        public string product_id { get; set; }
        public string amount { get; set; }
    }

    public class BundleResponseForNew
    {
        public BundleResponseForNew()
        {
            bundles = new List<Bundle>();
        }
        public string status { get; set; }
        public string message { get; set; }
        public string trxn { get; set; }
        public List<Bundle> bundles { get; set; }
    }

    //public class Bundle
    //{
    //    public string name { get; set; }
    //    public string product_id { get; set; }
    //    public string amount { get; set; }
    //    public object validity { get; set; }
    //}

    public class MtnBundleResponse
    {
        public List<Bundle> bundles { get; set; }
        public object error_code { get; set; }
        public bool success { get; set; }
    }

    public class BundleResponse
    {
        public BundleResponse()
        {
            this.RstKey = 0;
        }       
        public int RstKey { get; set; }
        public List<IspBundlesResponse> ispBundlesResponces { get; set; }
    }
}
