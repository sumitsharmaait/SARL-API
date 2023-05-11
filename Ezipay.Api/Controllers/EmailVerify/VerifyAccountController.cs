using Ezipay.Service.UserService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ezipay.Api.Controllers.EmailVerify
{
    public class VerifyAccountController : Controller
    {
        private IWalletUserService _accountService;
        /// <summary>
        /// ctor
        /// </summary>
        public VerifyAccountController()
        {
            _accountService = new WalletUserService();
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Index(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var res = await _accountService.VerfiyByEmailId(id);
                if (res.RstKey == 2)
                {
                    ViewBag.Status = true;
                    ViewBag.Success = "Email verified successfully.";
                }
                else
                {
                    ViewBag.Status = false;
                    ViewBag.Failure = "Email already verified.";
                }


            }
            return View();
        }
    }
}