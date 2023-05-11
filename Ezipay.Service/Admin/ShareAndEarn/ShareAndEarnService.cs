using Ezipay.Database;
using Ezipay.Repository.AdminRepo.ShareAndEarn;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.ViewModel.ShareAndEarnViewModel;
using Ezipay.ViewModel.WalletUserVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.Admin.ShareAndEarn
{
    public class ShareAndEarnService : IShareAndEarnService
    {
        private IShareAndEarnRepository _shareAndEarnRepository;
        private IWalletUserRepository _walletUserRepository;

        public ShareAndEarnService()
        {
            _shareAndEarnRepository = new ShareAndEarnRepository();
            _walletUserRepository = new WalletUserRepository();
        }

        public async Task<int> InsertReward(InsertShareRewardRequest request)
        {
            int result = 0;
            var getRewards = await GetRewardList();
            if (getRewards.Id > 0)
            {
                var data = await _shareAndEarnRepository.GetRewardById(getRewards.Id);
                data.IsActive = false;
                await _shareAndEarnRepository.UpdateRewards(data);
            }
            var req = new ShareAndEarnMaster
            {
                ConversionPointsValue = request.ConversionPointsValue,
                ReceiverPoints = request.ReceiverPoints,
                ConversionPoint = request.ConversionPoint,
                MinimumRedeemablePoint = request.MinimumRedeemablePoint,
                SenderPoints = request.SenderPoints,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            result = await _shareAndEarnRepository.InsertReward(req);
            return result;
        }

        public async Task<ShareAndEarnMasterResponse> GetRewardList()
        {
            var res = new ShareAndEarnMasterResponse();
            var result = await _shareAndEarnRepository.GetReward();
            if (result != null)
            {
                res.Id = result.Id;
                res.ConversionPoint = result.ConversionPoint;
                res.ReceiverPoints = result.ReceiverPoints;
                res.ConversionPointsValue = result.ConversionPointsValue;
                res.MinimumRedeemablePoint = result.MinimumRedeemablePoint;
                res.SenderPoints = result.SenderPoints;
                res.IsActive = result.IsActive;
                res.IsDeleted = result.IsDeleted;
                res.CreatedDate = result.CreatedDate;
                res.UpdatedDate = result.UpdatedDate;
            }
            return res;
        }

        public async Task<Object> GetReferalUrl(long walletuserId)
        {
            var UserDetail = await _walletUserRepository.GetUserDetailById(walletuserId);

            var response = new ShareAndEarnResponse();
            // string ShareLink = "";

            var tagData = new TagsData
            {
                FirstName = UserDetail.FirstName,
                LastName = UserDetail.LastName,
                MobileNo = UserDetail.MobileNo,
                WalletUserId = UserDetail.WalletUserId.ToString()
            };
            var ds = JsonConvert.SerializeObject(tagData);
            var ShareLink1 = new Object();

            try
            {
                var req = new ShareAndEarnRequest();
                req.branch_key = CommonSetting.branch_key;
                req.channel = "facebook";
                req.feature = "signup";
                req.campaign = "promotion";
                req.stage = "new user";
                req.tags = UserDetail.WalletUserId.ToString();// new string[4] { UserDetail.FirstName, UserDetail.LastName, UserDetail.WalletUserId.ToString(), UserDetail.MobileNo };
                req.data = new Data();
                req.data.canonical_identifier = "Content/" + (new DateTime().GetDateTimeFormats());
                req.data.og_title = "Title from Deep Link";
                req.data.og_description = "Description from Deep Link";
                req.data.og_image_url = "http://www.lorempixel.com/400/400/";
                req.data.desktop_url = "http://www.example.com";
                req.data.android_url = CommonSetting.android_url;
                req.data.custom_boolean = true;
                req.data.custom_integer = 1;
                req.data.custom_string = "";
                req.data.custom_array = new int[6] { 1, 2, 3, 4, 5, 6 };
                req.data.custom_object = new Custom_Object();
                req.data.custom_object.random = "dictionary";
                var jsonReq = JsonConvert.SerializeObject(req);
                var url = CommonSetting.branchio_url;
                var result = await HttpPostRequest(url, jsonReq);
                var ShareLink = JsonConvert.DeserializeObject<dynamic>(result);
                if (result != null)
                {
                    var saveReq = new ShareAndEarnDetail()
                    {
                        SenderId = UserDetail.WalletUserId,
                        ReferUrl = result,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    //db.ShareAndEarnDetails.Add(saveReq);
                    //int res = await db.SaveChangesAsync();
                    int res = await _shareAndEarnRepository.SaveData(saveReq);
                    if (res > 0)
                    {
                        ShareLink1 = ShareLink;
                    }
                    else
                    {
                        ShareLink1 = "Failed";
                    }
                }

            }
            catch (Exception ex)
            {

                ex.Message.ErrorLog("ShareAndEarnService.cs", "ShareAndEarn");
            }
            return ShareLink1;

        }

        public async Task<string> HttpPostRequest(string url, string req)
        {
            string resString = "";
            string resBody = "";
            // RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(resBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return resBody;
            }
        }

        public async Task<object> GetUserData(string url)
        {
            string ReqUrl = "https://api2.branch.io/v1/url?url=" + url + "&branch_key=key_test_ikRQxAl3Ml7Ca5m7ldS1SbekyslGkEIz";
            var result = await GetShareAndEarnData(ReqUrl);

            var jsonData = JsonConvert.DeserializeObject<object>(result);

            return jsonData;
        }

        public async Task<string> GetShareAndEarnData(string url)
        {
            string resString = "";
            string resBody = "";
            // RootObject responseData = new RootObject();
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    //var content = new StringContent(req, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    resBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(resBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
                return resBody;
            }
        }


        public async Task<bool> RedeemPoints(RedeemPointsRequest request)
        {
            bool res = false;
            var userData = await _walletUserRepository.GetCurrentUser(request.WalletUserId);

            decimal rewardPoints = Convert.ToDecimal(userData.EarnedPoints);
            decimal rewardAmount = Convert.ToDecimal(userData.EarnedAmount);
            userData.EarnedPoints = rewardPoints - request.RedeemPoints;
            userData.EarnedAmount = rewardAmount - request.RedeemAmount;
            decimal currentBalance = Convert.ToDecimal(userData.CurrentBalance) + rewardAmount;
            userData.CurrentBalance = currentBalance.ToString();
            var result = await _walletUserRepository.UpdateUserDetail(userData);
            if (result != null)
            {
                res = true;
                var req = new RedeemPointsHistory
                {
                    RedeemAmount = request.RedeemAmount.ToString(),
                    WalletUserId = userData.WalletUserId,
                    TransactionType = "DEBIT",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                };
                await _walletUserRepository.InsertEarnedHistory(req);
            }
            return res;
        }


        public async Task<List<RedeemPointsHistoryResponse>> GetRedeemHistory(long walletUserId)
        {           
            var result = new List<RedeemPointsHistoryResponse>();
            result = await _shareAndEarnRepository.GetRedeemHistory(walletUserId);           
            return result;
        }



    }
}
