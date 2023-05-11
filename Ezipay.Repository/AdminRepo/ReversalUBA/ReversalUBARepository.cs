using Ezipay.Database;
using Ezipay.ViewModel.AdminViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.AdminRepo.ReversalUBA
{
    public class ReversalUBARepository: IReversalUBARepository
    {
        public async Task<List<UBATxnVerificationResponse>> Getresponse(UBATxnVerificationRequest cr)
        {
            using (var context = new DB_9ADF60_ewalletEntities())
            {
                return await context.Database.SqlQuery<UBATxnVerificationResponse>
                        ("EXEC usp_DepositorList @SearchText,@PageNo,@PageSize",
                        new SqlParameter("@SearchText", cr.Apikey),
                        new SqlParameter("@PageNo", cr.CardNumber),
                        new SqlParameter("@PageSize", cr.CountryCode)
                        ).ToListAsync();
            }
        }

    }
}
