using Ezipay.Repository.AdminRepo.DashBoardRepo;
using Ezipay.Utility.SendEmail;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.DashBoardViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ezipay.Service.AdminService.DashBoardService
{
    public class DashBoardServices : IDashBoardServices
    {
        private IDashBoardRepository _dashBoardRepository;
        private ISendEmails _sendEmails;
        public DashBoardServices()
        {
            _dashBoardRepository = new DashBoardRepository();
            _sendEmails = new SendEmails();
        }
        public async Task<DashboardResponse> DashboardDetails(DashboardRequest request)
        {
            var response = new DashboardResponse();
            try
            {
                response = await _dashBoardRepository.DashboardDetails(request);
            }
            catch (Exception ex)
            {

            }

            return response;
        }
        public async Task<bool?> EnableTransactions(string sessionToken)
        {
            bool? response = false;
            try
            {
                //var UserDetail = await _walletUserService.UserProfile(sessionToken);
                response = await _dashBoardRepository.EnableTransactions();
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<List<CheckUBATxnNotCaptureOurSideResponse>> CheckUBATxnNotCaptureOurSide(string InvoiceNumber)
        {
            var response = new List<CheckUBATxnNotCaptureOurSideResponse>();
            try
            {
                response = await _dashBoardRepository.CheckUBATxnNotCaptureOurSide(InvoiceNumber);
            }
            catch (Exception ex)
            {

            }

            return response;
        }

        public async Task<UserBlockUnblockDetailResponse> Emailuser()
        {
            var objResponse = new UserBlockUnblockDetailResponse();
            try
            {
                objResponse = await _dashBoardRepository.Emailuser();                

                string textBody = " <table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr bgcolor='#4da6ff'><td><b>EmailId</b></td> <td> <b> Blockdate </b> </td><td> <b> Comment</b> </td></tr>";
                for (int loopCount = 0; loopCount < objResponse.TotalCount; loopCount++)
                {
                    textBody += "<tr><td>" + objResponse.UserList[loopCount].EmailId + "</td>" +
                        "<td> " + objResponse.UserList[loopCount].Blockdate + "</td> " +
                         "<td> " + objResponse.UserList[loopCount].Comment + "</td> " +
                        "</tr>";
                }
                textBody += "</table>";

                var req1 = new EmailModel
                {
                    TO = "pritesh.salla@aituniversal.com",                                        
                    Subject = "SARL Account Blocked List By Using Different Card",
                    Body = textBody
                };
                _sendEmails.SendEmailUsingDifferentCard(req1);
            }
            catch (Exception ex)
            {

            }
            return objResponse;
        }
    }
}
