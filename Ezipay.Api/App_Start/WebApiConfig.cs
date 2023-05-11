using Ezipay.Api.Filters;
using Ezipay.Api.Resolver;
using Ezipay.Repository.Admin.ReversalUBA;
using Ezipay.Repository.AdminRepo.ChargeBack;
using Ezipay.Service;
using Ezipay.Service.Admin.AdminMobileMoneyLimit;
using Ezipay.Service.Admin.Banner;
using Ezipay.Service.Admin.Callback;
using Ezipay.Service.Admin.Cashdepositrequest;
using Ezipay.Service.Admin.Commission;
using Ezipay.Service.Admin.Currency;
using Ezipay.Service.Admin.Merchant;
using Ezipay.Service.Admin.Report;
using Ezipay.Service.Admin.Resort;
using Ezipay.Service.Admin.ShareAndEarn;
using Ezipay.Service.Admin.SubAdmin;
using Ezipay.Service.Admin.Subscription;
using Ezipay.Service.Admin.TransactionLimitAU;
using Ezipay.Service.Admin.TransactionLog;
using Ezipay.Service.Admin.TxnUpdate;
using Ezipay.Service.AdminService;
using Ezipay.Service.AdminService.AuthenticationService;
using Ezipay.Service.AdminService.DashBoardService;
using Ezipay.Service.AfroBasket;
using Ezipay.Service.AirtimeService;
using Ezipay.Service.BillPaymentService;
using Ezipay.Service.CardPayment;
using Ezipay.Service.CommonService;
using Ezipay.Service.EzipayPartner;
using Ezipay.Service.EzipayWebhookService;
using Ezipay.Service.FlightHotelService;
using Ezipay.Service.InternatinalRechargeServ;
using Ezipay.Service.InterNetProviderService;
using Ezipay.Service.MasterData;
using Ezipay.Service.MerchantPayment;
using Ezipay.Service.MobileMoneyService;
using Ezipay.Service.PaymentGetway;
using Ezipay.Service.PaymentRequestService;
using Ezipay.Service.PayMoney;
using Ezipay.Service.ThridPartyApiService;
using Ezipay.Service.TokenService;
using Ezipay.Service.TransferTobankService;
using Ezipay.Service.TvService;
using Ezipay.Service.UserService;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Unity;
using Unity.Lifetime;
namespace Ezipay.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = DI();
            config.DependencyResolver = new UnityResolver(container);
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
            name: "swagger_root",
            routeTemplate: "",
            defaults: null,
            constraints: null,
            handler: new RedirectHandler((message => message.RequestUri.ToString()), "swagger"));
            // Web API routes
            // config.MapHttpAttributeRoutes();
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
            // config.MessageHandlers.Add(new CustomMessageHandler());

            //config.MessageHandlers.Add(new CustomLogHandler());
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static UnityContainer DI()
        {
            var container = new UnityContainer();
            container.RegisterType<IWalletUserService, WalletUserService>(new HierarchicalLifetimeManager());
            container.RegisterType<ITokenService, TokenService>(new HierarchicalLifetimeManager());
            container.RegisterType<IMasterDataService, MasterDataService>(new HierarchicalLifetimeManager());
            container.RegisterType<IAirtimeService, AirtimeService>(new HierarchicalLifetimeManager());
            container.RegisterType<IInterNetProviderService, InterNetServiceProviderService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICardPaymentService, CardPaymentService>(new HierarchicalLifetimeManager());
            container.RegisterType<IPayMoneyService, PayMoneyService>(new HierarchicalLifetimeManager());
            container.RegisterType<IMerchantPaymentService, MerchantPaymentService>(new HierarchicalLifetimeManager());
            container.RegisterType<ITvServices, TvServices>(new HierarchicalLifetimeManager());
            container.RegisterType<IMobileMoneyServices, MobileMoneyServices>(new HierarchicalLifetimeManager());
            container.RegisterType<ICommonServices, CommonServices>(new HierarchicalLifetimeManager());
            container.RegisterType<IPaymentRequestServices, PaymentRequestServices>(new HierarchicalLifetimeManager());
            container.RegisterType<IThridPartyApiServices, ThridPartyApiServices>(new HierarchicalLifetimeManager());
            container.RegisterType<ITransferToBankServices, TransferToBankServices>(new HierarchicalLifetimeManager());
            container.RegisterType<IUserApiService, UserApiService>(new HierarchicalLifetimeManager());
            container.RegisterType<IDashBoardServices, DashBoardServices>(new HierarchicalLifetimeManager());
            container.RegisterType<IAuthenticationApiService, AuthenticationApiService>(new HierarchicalLifetimeManager());
            container.RegisterType<ISubAdminService, SubAdminService>(new HierarchicalLifetimeManager());
            container.RegisterType<IMerchantService, MerchantService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICallbackService, CallbackService>(new HierarchicalLifetimeManager());
            container.RegisterType<ITransactionLogService, TransactionLogService>(new HierarchicalLifetimeManager());
            container.RegisterType<ISubscriptionService, SubscriptionService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICommissionService, CommissionService>(new HierarchicalLifetimeManager());
            container.RegisterType<IBannerService, BannerService>(new HierarchicalLifetimeManager());
            container.RegisterType<IReportService, ReportService>(new HierarchicalLifetimeManager());
            container.RegisterType<IFlightBookingPaymentService, FlightBookingPaymentService>(new HierarchicalLifetimeManager());
            container.RegisterType<IPaymentGetwayService, PaymentGetwayService>(new HierarchicalLifetimeManager());
            container.RegisterType<IAppDownloadLogService,AppDownloadLogService >(new HierarchicalLifetimeManager());
            container.RegisterType<IResortService, ResortService>(new HierarchicalLifetimeManager());
            container.RegisterType<IAfroBasketService, AfroBasketService>(new HierarchicalLifetimeManager());
            container.RegisterType<IShareAndEarnService, ShareAndEarnService>(new HierarchicalLifetimeManager());
            container.RegisterType<IBillPaymentService, BillPaymentServices>(new HierarchicalLifetimeManager());
            container.RegisterType<IInternatinalRechargeService, InternatinalRechargeService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICurrencyConvertService, CurrencyConvertService>(new HierarchicalLifetimeManager());
            container.RegisterType<IAdminMobileMoneyLimitService, AdminMobileMoneyLimitService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICashdepositrequestService, CashdepositrequestService>(new HierarchicalLifetimeManager());
            container.RegisterType<IEzipayPartnerService, EzipayPartnerService>(new HierarchicalLifetimeManager());
            container.RegisterType<ITransactionLimitAUService,TransactionLimitAUService >(new HierarchicalLifetimeManager());
            container.RegisterType<ITxnUpdateService, TxnUpdateService>(new HierarchicalLifetimeManager());
           
            container.RegisterType<IChargeBackService, ChargeBackService>(new HierarchicalLifetimeManager());

            container.RegisterType<IReversalUBAService, ReversalUBAService>(new HierarchicalLifetimeManager());
            container.RegisterType<IEzipayWebhookService, EzipayWebhookService>(new HierarchicalLifetimeManager());
            return container;
        }
    }
}
