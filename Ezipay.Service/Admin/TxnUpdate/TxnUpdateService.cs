using Ezipay.Repository.AdminRepo.TxnUpdate;
using Ezipay.Repository.CardPayment;
using Ezipay.Repository.CommisionRepo;
using Ezipay.Repository.ThridPartyApiRepo;
using Ezipay.Repository.UserRepo;
using Ezipay.Utility.common;
using Ezipay.Utility.Extention;
using Ezipay.Utility.SendEmail;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using Ezipay.ViewModel.SendEmailViewModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Ezipay.Service.Admin.TxnUpdate
{
    public class TxnUpdateService : ITxnUpdateService
    {
        private readonly ITxnUpdateRepository _TxnUpdateRepository;
        private IWalletUserRepository _walletUserRepository;
        private IThridPartyApiRepository _thridPartyApiRepository;
        
        private ISendEmails _sendEmails;
        
        public TxnUpdateService()
        {
            _thridPartyApiRepository = new ThridPartyApiRepository();
            _TxnUpdateRepository = new TxnUpdateRepository();
            _walletUserRepository = new WalletUserRepository();

            _sendEmails = new SendEmails();
        }


        public async Task<List<WalletTxnResponse>> GetWalletTxnPendingList()
        {

            var result = new List<WalletTxnResponse>();


            var objList = await _TxnUpdateRepository.GetWalletTxnPendingList();


            foreach (var item in objList)
            {
                var obj = new WalletTxnResponse();
                obj.WalletTxnId = item.WalletTransactionId.ToString();
                obj.TxnId = item.TransactionId;
                obj.CreatedDate = item.CreatedDate;
                obj.WalletUserId = item.SenderId.ToString();
                obj.WalletTxnStatus = item.TransactionStatus.ToString();
                obj.InvoiceNo = item.InvoiceNo;
                obj.TransactionType = item.TransactionType;
                obj.TotalAmount = item.TotalAmount;
                obj.WalletServiceId = item.WalletServiceId;
                //
                if (item.IsdCode == "+229")
                {
                    obj.TxnCountry = "Benin (Former Dahomey)";
                }
                else if (item.IsdCode == "+226")
                {
                    obj.TxnCountry = "Burkina Faso (Former Upper Volta)";
                }
                else if (item.IsdCode == "+225")
                {
                    obj.TxnCountry = "Cote D'Ivoire (Former Ivory Coast)";
                }
                else if (item.IsdCode == "+245")
                {
                    obj.TxnCountry = "Guinea-Bissau (Former Portuguese Guinea)";
                }
                else if (item.IsdCode == "+223")
                {
                    obj.TxnCountry = "Mali (Former French Sudan and Sudanese Republic)";
                }
                else if (item.IsdCode == "+227")
                {
                    obj.TxnCountry = "Niger";
                }
                else if (item.IsdCode == "+221")
                {
                    obj.TxnCountry = "Senegal";
                }
                else if (item.IsdCode == "+228")
                {
                    obj.TxnCountry = "Togo";
                }
                else
                {
                    obj.TxnCountry = "";
                }
                result.Add(obj);
            }


            return result;
        }

        public async Task<bool> UpdatePendingWalletTxn(WalletTxnRequest request)
        {
            var result = false;

            int rowAffected = await _TxnUpdateRepository.UpdatePendingWalletTxn(request);
            if (rowAffected > 0)
            {
                result = true;
            }

            return result;
        }
        public async Task<bool> UpdateBankPendingWalletTxn(WalletTxnRequest request)
        {
            var result = false;

            int rowAffected = await _TxnUpdateRepository.UpdateBankPendingWalletTxn(request);
            if (rowAffected > 0)
            {
                if (request.Txnstatus == 1)
                {
                    var transaction = await _thridPartyApiRepository.GetWalletTransaction(request.InvoiceNo);

                    try
                    {
                        //--------send mail on success transaction--------                          
                        var senderdata = await _walletUserRepository.GetUserDetailById(request.UserId);
                        string filename = CommonSetting.successfullTransaction;
                        var body = _sendEmails.ReadEmailformats(filename);
                        body = body.Replace("$$FirstName$$", senderdata.FirstName + " " + senderdata.LastName);
                        body = body.Replace("$$DisplayContent$$", "Flutter CARD");
                        body = body.Replace("$$customer$$", transaction.AccountNo);
                        body = body.Replace("$$amount$$", "XOF " + transaction.WalletAmount);
                        body = body.Replace("$$ServiceTaxAmount$$", "XOF " + transaction.CommisionAmount);
                        body = body.Replace("$$AmountWithCommission$$", "XOF " + transaction.TotalAmount);
                        body = body.Replace("$$TransactionId$$", transaction.InvoiceNo);
                        var req = new EmailModel
                        {
                            TO = senderdata.EmailId,
                            Subject = "Transaction Successfull",
                            Body = body
                        };
                        _sendEmails.SendEmail(req);
                    }
                    catch
                    {

                    }
                }
                result = true;
            }

            return result;
        }

      

    }
}
