using Ezipay.Database;
using Ezipay.Repository.MobileMoneyRepo;
using Ezipay.Utility.common;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.ViewModel.CommisionViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;


namespace Ezipay.Repository.CommisionRepo
{
    public class SetCommisionRepository : ISetCommisionRepository
    {
        public async Task<bool> SetCommision(CommisionMaster request)
        {
            bool response = false;
            using (DB_9ADF60_ewalletEntities db = new DB_9ADF60_ewalletEntities())
            {
                var commission = db.CommisionMasters.AsQueryable().Where(x => x.WalletServiceId == request.WalletServiceId);
                if (commission.Any())
                {
                    foreach (var c in commission)
                    {
                        c.IsActive = false;
                    }
                }
                db.CommisionMasters.Add(request);
                int result = await db.SaveChangesAsync();
                if (result > 0)
                {
                    response = true;
                }
            }
            return response;
        }

        public async Task<CalculateCommissionResponse> CalculateCommission(CalculateCommissionRequest request)
        {
            var response = new CalculateCommissionResponse();
            decimal tAmount1 = 0;
            response.TransactionAmount = request.TransactionAmount;

            response.AmountWithCommission = request.TransactionAmount;
            response.CurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;

            response.UpdatedCurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;
            try
            {
                if (request.TransactionAmount > CommonSetting.LimitAmount)
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        var commision = await db.CommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && x.IsActive == true).FirstOrDefaultAsync();
                        if (commision != null && commision.CommisionPercent > 0)
                        {
                            decimal tAmount = (commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);
                            if (commision.VATCharges > 0)
                            {
                                decimal commisionOfAmt = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100);
                                decimal flate = commisionOfAmt + tAmount;
                                decimal VatCharge = (tAmount + commisionOfAmt) / Convert.ToDecimal(commision.VATCharges);
                                //include vat charges on all commision
                                tAmount1 = flate + VatCharge;
                            }
                            response.CommissionId = (int)commision.CommisionMasterId;
                            response.CommisionPercent = Convert.ToDecimal(commision.CommisionPercent);
                            response.FlatCharges = commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0;
                            response.BenchmarkCharges = commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0;
                            if (request.IsRoundOff)
                            {
                                if (commision.VATCharges > 0)
                                {
                                    response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                    response.AmountWithCommission = Math.Round(response.TransactionAmount + tAmount1, 2);
                                }
                                else
                                {
                                    response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                    response.AmountWithCommission = Math.Round(response.TransactionAmount + response.CommissionAmount, 2);
                                }
                            }
                            else
                            {
                                if (commision.VATCharges > 0)
                                {
                                    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                    response.AmountWithCommission = response.TransactionAmount + tAmount1;
                                }
                                else
                                {
                                    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                    response.AmountWithCommission = response.TransactionAmount + response.CommissionAmount;
                                }
                            }
                        }
                        if (await db.WalletServices.AnyAsync(x => x.MerchantId == request.WalletServiceId && (bool)x.IsActive))
                        {
                            var merchantCommission = db.MerchantCommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && (bool)x.IsActive).FirstOrDefault();
                            if (merchantCommission != null)
                            {
                                response.MerchantCommissionId = merchantCommission.CommisionMasterId;
                                response.MerchantCommissionRate = (decimal)merchantCommission.CommisionPercent;

                                if (request.IsRoundOff)
                                {
                                    response.MerchantCommissionAmount = Math.Round(((request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100), 2);
                                    //response.MerchantCommissionAmount = MerchantCommissionAmount + response.FlatCharges + response.BenchmarkCharges;
                                }
                                else
                                {
                                    response.MerchantCommissionAmount = (request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100;
                                }
                            }
                        }
                        if (request.CurrentBalance > 0)
                        {
                            if (request.IsRoundOff)
                            {
                                response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            }
                            else
                            {
                                response.UpdatedCurrentBalance = response.CurrentBalance - response.AmountWithCommission;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return response;
        }

        public async Task<CalculateCommissionResponse> CalculateCommissionForMobileMoney(CalculateCommissionRequest request, long UserId, long transactionCount, string isdcode)
        {
            var c = new MobileMoneyRepository();

            var response = new CalculateCommissionResponse();
            decimal tAmount1 = 0;
            response.TransactionAmount = request.TransactionAmount;

            response.AmountWithCommission = request.TransactionAmount;
            response.CurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;

            response.UpdatedCurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;
            try
            {
                if (request.TransactionAmount > CommonSetting.LimitAmount)
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        var commision = await db.CommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && x.IsActive == true).FirstOrDefaultAsync();

                        #region commented code
                        //if (commision != null && commision.CommisionPercent > 0 && transactionCount <2)
                        //{
                        //    decimal tAmount = 0;
                        //    if (commision.VATCharges > 0)
                        //    {
                        //        decimal commisionOfAmt = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100);
                        //        decimal flate = commisionOfAmt + tAmount;
                        //        decimal VatCharge = (tAmount + commisionOfAmt) / Convert.ToDecimal(commision.VATCharges);
                        //        //include vat charges on all commision
                        //        tAmount1 = flate + VatCharge;
                        //    }
                        //    response.CommissionId = (int)commision.CommisionMasterId;
                        //    response.CommisionPercent = Convert.ToDecimal(commision.CommisionPercent);
                        //    response.FlatCharges = 0;
                        //    response.BenchmarkCharges = 0;
                        //    if (request.IsRoundOff)
                        //    {
                        //        if (commision.VATCharges > 0)
                        //        {
                        //            response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                        //            response.AmountWithCommission = Math.Round(response.TransactionAmount + tAmount1, 2);
                        //        }
                        //        else
                        //        {
                        //            response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                        //            response.AmountWithCommission = Math.Round(response.TransactionAmount + response.CommissionAmount, 2);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (commision.VATCharges > 0)
                        //        {
                        //            response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                        //            response.AmountWithCommission = response.TransactionAmount + tAmount1;
                        //        }
                        //        else
                        //        {
                        //            response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                        //            response.AmountWithCommission = response.TransactionAmount + response.CommissionAmount;
                        //        }
                        //    }
                        //}
                        #endregion
                        //take minimumcharges on given amount
                        AdminMobileMoneyLimitRequest obj = new AdminMobileMoneyLimitRequest
                        {
                            MinimumAmount = request.TransactionAmount.ToString(),
                            Service = isdcode
                        };
                        var newCommisionMinChargesonGivenAmount = await c.VerifyMobileMoneyLimit(obj);

                        if (commision != null && commision.CommisionPercent > 0)
                        {
                            //take flatcharges by amit & replace minimum amount with admin panel-set mo-mo limit
                            //decimal tAmount = (commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);

                            decimal tAmount = (newCommisionMinChargesonGivenAmount.MinimumCharges != null ? Convert.ToDecimal(newCommisionMinChargesonGivenAmount.MinimumCharges) : 0) + (commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);
                            if (commision.VATCharges > 0)
                            {
                                decimal commisionOfAmt = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100);
                                decimal flate = commisionOfAmt + tAmount;
                                decimal VatCharge = (tAmount + commisionOfAmt) / Convert.ToDecimal(commision.VATCharges);
                                //include vat charges on all commision
                                tAmount1 = flate + VatCharge;
                            }
                            response.CommissionId = (int)commision.CommisionMasterId;
                            response.CommisionPercent = Convert.ToDecimal(commision.CommisionPercent);
                            response.FlatCharges = commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0;

                            response.BenchmarkCharges = commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0;
                            if (request.IsRoundOff)
                            {
                                if (commision.VATCharges > 0)
                                {
                                    response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                    response.AmountWithCommission = Math.Round(response.TransactionAmount + tAmount1, 2);
                                }
                                else
                                {
                                    response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                    response.AmountWithCommission = Math.Round(response.TransactionAmount + response.CommissionAmount, 2);
                                }
                            }
                            else
                            {
                                if (commision.VATCharges > 0)
                                {
                                    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                    response.AmountWithCommission = response.TransactionAmount + tAmount1;
                                }
                                else
                                {
                                    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                    response.AmountWithCommission = response.TransactionAmount + response.CommissionAmount;
                                }
                            }
                        }
                        //if (await db.WalletServices.AnyAsync(x => x.MerchantId == request.WalletServiceId && (bool)x.IsActive))
                        //{
                        //    var merchantCommission = db.MerchantCommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && (bool)x.IsActive).FirstOrDefault();
                        //    if (merchantCommission != null)
                        //    {
                        //        response.MerchantCommissionId = merchantCommission.CommisionMasterId;
                        //        response.MerchantCommissionRate = (decimal)merchantCommission.CommisionPercent;

                        //        if (request.IsRoundOff)
                        //        {
                        //            response.MerchantCommissionAmount = Math.Round(((request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100), 2);
                        //            //response.MerchantCommissionAmount = MerchantCommissionAmount + response.FlatCharges + response.BenchmarkCharges;
                        //        }
                        //        else
                        //        {
                        //            response.MerchantCommissionAmount = (request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100;
                        //        }
                        //    }
                        //}
                        if (request.CurrentBalance > 0)
                        {
                            //if (request.IsRoundOff)
                            //{
                            //    if (isdcode == "+221" || isdcode == "+223" || isdcode == "+226" || isdcode == "+227" || isdcode == "+245") //for sengal,mali, burkina,niger,guinea - 0 %age & 0 vat 
                            //    {
                            //        decimal tAmount11 = (newCommisionMinChargesonGivenAmount.MinimumCharges != null ? Convert.ToDecimal(newCommisionMinChargesonGivenAmount.MinimumCharges) : 0) + (commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);
                            //        //decimal tAmount11 = (newCommisionMinChargesonGivenAmount.MinimumCharges != null ? Convert.ToDecimal(newCommisionMinChargesonGivenAmount.MinimumCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);
                            //        response.AmountWithCommission = Math.Round(tAmount11 + response.AmountWithCommission, 2);

                            //        response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            //    }
                            //    else if (isdcode == "+229" || isdcode == "+228" || isdcode == "+225") //for benin,civ,togo - %age h
                            //    {
                            //        response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            //    }
                            //}
                            //else
                            //{
                            //    response.UpdatedCurrentBalance = response.CurrentBalance - response.AmountWithCommission;
                            //}
                            if (request.IsRoundOff)
                            {
                                response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            }
                            else
                            {
                                response.UpdatedCurrentBalance = response.CurrentBalance - response.AmountWithCommission;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return response;
        }

        public async Task<CalculateCommissionResponse> CalculateAddMoneyCommission(CalculateCommissionRequest request)
        {
            var response = new CalculateCommissionResponse();
            response.TransactionAmount = request.TransactionAmount;
            response.AmountWithCommission = request.TransactionAmount;
            response.CurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;
            response.UpdatedCurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;
            try
            {
                if (request.TransactionAmount > CommonSetting.LimitAmount)
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        var commision = await db.CommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && x.IsActive == true).FirstOrDefaultAsync();
                        if (commision != null && commision.CommisionPercent > 0)
                        {
                            response.CommissionId = (int)commision.CommisionMasterId;
                            response.CommisionPercent = (decimal)commision.CommisionPercent;

                            if (request.IsRoundOff)
                            {
                                response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2);
                                response.AmountWithCommission = Math.Round(response.TransactionAmount + response.CommissionAmount, 2);
                            }
                            else
                            {
                                response.CommissionAmount = (request.TransactionAmount * (decimal)commision.CommisionPercent) / 100;
                                response.AmountWithCommission = response.TransactionAmount + response.CommissionAmount;
                            }
                        }
                        if (request.CurrentBalance > 0)
                        {
                            if (request.IsRoundOff)
                            {
                                response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            }
                            else
                            {
                                response.UpdatedCurrentBalance = response.CurrentBalance - response.AmountWithCommission;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }


        public async Task<CalculateCommissionResponse> CalculatePayNGNTransferSendMoneyCommission(CalculateCommissionRequest request)
        {
            var response = new CalculateCommissionResponse();
            decimal tAmount1 = 0;
            response.TransactionAmount = request.TransactionAmount;

            response.AmountWithCommission = request.TransactionAmount;
            response.CurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;

            response.UpdatedCurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;
            try
            {
                if (request.TransactionAmount > CommonSetting.LimitAmount)
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        var commision = await db.CommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && x.IsActive == true).FirstOrDefaultAsync();
                        if (commision != null && commision.CommisionPercent > 0)
                        {
                            decimal tAmount = (commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);
                            if (commision.VATCharges > 0)
                            {
                                decimal commisionOfAmt = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100);
                                decimal flate = commisionOfAmt + tAmount;
                                decimal VatCharge = (tAmount + commisionOfAmt) / Convert.ToDecimal(commision.VATCharges);
                                //include vat charges on all commision
                                tAmount1 = flate + VatCharge;
                            }
                            response.CommissionId = (int)commision.CommisionMasterId;
                            response.CommisionPercent = Convert.ToDecimal(commision.CommisionPercent);
                            response.FlatCharges = commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0;
                            response.BenchmarkCharges = commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0;
                            if (request.IsRoundOff)
                            {
                                //if (commision.VATCharges > 0)
                                //{
                                //    response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                //    response.AmountWithCommission = Math.Round(response.TransactionAmount + tAmount1, 2);
                                //}
                                //else
                                //{
                                response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                response.AmountWithCommission = Math.Round(response.TransactionAmount + response.CommissionAmount, 2);
                                //ngn will receive
                                response.ServiceTaxAmount = Math.Round(response.TransactionAmount - response.CommissionAmount, 2);
                                //}
                            }
                            else
                            {
                                //if (commision.VATCharges > 0)
                                //{
                                //    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                //    response.AmountWithCommission = response.TransactionAmount + tAmount1;
                                //}
                                //else
                                //{
                                //    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                //    response.AmountWithCommission = response.TransactionAmount + response.CommissionAmount;
                                //}
                            }
                        }
                        //if (await db.WalletServices.AnyAsync(x => x.MerchantId == request.WalletServiceId && (bool)x.IsActive))
                        //{
                        //    var merchantCommission = db.MerchantCommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && (bool)x.IsActive).FirstOrDefault();
                        //    if (merchantCommission != null)
                        //    {
                        //        response.MerchantCommissionId = merchantCommission.CommisionMasterId;
                        //        response.MerchantCommissionRate = (decimal)merchantCommission.CommisionPercent;

                        //        if (request.IsRoundOff)
                        //        {
                        //            response.MerchantCommissionAmount = Math.Round(((request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100), 2);
                        //            //response.MerchantCommissionAmount = MerchantCommissionAmount + response.FlatCharges + response.BenchmarkCharges;
                        //        }
                        //        else
                        //        {
                        //            response.MerchantCommissionAmount = (request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100;
                        //        }
                        //    }
                        //}
                        if (request.CurrentBalance > 0)
                        {
                            if (request.IsRoundOff)
                            {
                                response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            }
                            else
                            {
                                response.UpdatedCurrentBalance = response.CurrentBalance - response.AmountWithCommission;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return response;
        }


        public async Task<CalculateCommissionResponse> CalculatePayNGNTransferAddMoneyCommission(CalculateCommissionRequest request)
        {
            var response = new CalculateCommissionResponse();
           
            response.TransactionAmount = request.TransactionAmount;

            response.AmountWithCommission = request.TransactionAmount;
            response.CurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;

            response.UpdatedCurrentBalance = request.CurrentBalance > 0 ? request.CurrentBalance : 0;
            try
            {
                if (request.TransactionAmount > CommonSetting.LimitAmount)
                {
                    using (var db = new DB_9ADF60_ewalletEntities())
                    {
                        var commision = await db.CommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && x.IsActive == true).FirstOrDefaultAsync();
                        if (commision != null)
                        {
                            decimal tAmount = (commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0) + (commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0);
                            
                            response.CommissionId = (int)commision.CommisionMasterId;
                            response.CommisionPercent = Convert.ToDecimal(commision.CommisionPercent);
                            response.FlatCharges = commision.FlatCharges != null ? Convert.ToDecimal(commision.FlatCharges) : 0;
                            response.BenchmarkCharges = commision.BenchmarkCharges != null ? Convert.ToDecimal(commision.BenchmarkCharges) : 0;
                            if (request.IsRoundOff)
                            {
                                //if (commision.VATCharges > 0)
                                //{
                                //    response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                //    response.AmountWithCommission = Math.Round(response.TransactionAmount + tAmount1, 2);
                                //}
                                //else
                                //{
                                response.CommissionAmount = Math.Round(((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100), 2) + tAmount;
                                response.AmountWithCommission = Math.Round(response.TransactionAmount + response.CommissionAmount, 2);
                                
                                //}
                            }
                            else
                            {
                                //if (commision.VATCharges > 0)
                                //{
                                //    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                //    response.AmountWithCommission = response.TransactionAmount + tAmount1;
                                //}
                                //else
                                //{
                                //    response.CommissionAmount = ((request.TransactionAmount * (decimal)commision.CommisionPercent) / 100) + tAmount;
                                //    response.AmountWithCommission = response.TransactionAmount + response.CommissionAmount;
                                //}
                            }
                        }
                        //if (await db.WalletServices.AnyAsync(x => x.MerchantId == request.WalletServiceId && (bool)x.IsActive))
                        //{
                        //    var merchantCommission = db.MerchantCommisionMasters.Where(x => x.WalletServiceId == request.WalletServiceId && (bool)x.IsActive).FirstOrDefault();
                        //    if (merchantCommission != null)
                        //    {
                        //        response.MerchantCommissionId = merchantCommission.CommisionMasterId;
                        //        response.MerchantCommissionRate = (decimal)merchantCommission.CommisionPercent;

                        //        if (request.IsRoundOff)
                        //        {
                        //            response.MerchantCommissionAmount = Math.Round(((request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100), 2);
                        //            //response.MerchantCommissionAmount = MerchantCommissionAmount + response.FlatCharges + response.BenchmarkCharges;
                        //        }
                        //        else
                        //        {
                        //            response.MerchantCommissionAmount = (request.TransactionAmount * (decimal)merchantCommission.CommisionPercent) / 100;
                        //        }
                        //    }
                        //}
                        if (request.CurrentBalance > 0)
                        {
                            if (request.IsRoundOff)
                            {
                                response.UpdatedCurrentBalance = Math.Round(response.CurrentBalance - response.AmountWithCommission, 2);
                            }
                            else
                            {
                                response.UpdatedCurrentBalance = response.CurrentBalance - response.AmountWithCommission;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return response;
        }


        //public async Task<CommissionCalculationResponse> CalculateCommission(decimal rate, int ServiceId, string amount, decimal flatCharges = 0, decimal benchmarkCharges = 0)
        //{
        //    var res = new CommissionCalculationResponse();
        //    res.CommissionServiceId = ServiceId;
        //    decimal Amount = Convert.ToDecimal(amount);
        //    res.Rate = rate;
        //    try
        //    {

        //        if (Amount > 0)
        //        {
        //            res.CommissionAmount = Convert.ToString(Math.Round(((Amount * rate) / 100 + flatCharges + benchmarkCharges), 2));
        //            res.AmountWithCommission = Convert.ToString(Math.Round(Convert.ToDecimal(Amount) + Convert.ToDecimal(res.CommissionAmount), 2));
        //            res.AfterDeduction = Convert.ToString(Math.Round(Convert.ToDecimal(Amount), 2));// - Convert.ToDecimal(res.CommissionAmount), 2));
        //        }
        //    }
        //    catch (Exception ex)
        //    {


        //        res.AmountWithCommission = Convert.ToString(Math.Round(Convert.ToDecimal(Amount), 2));
        //        res.AfterDeduction = res.AmountWithCommission;

        //    }
        //    return res;
        //}


    }
}
