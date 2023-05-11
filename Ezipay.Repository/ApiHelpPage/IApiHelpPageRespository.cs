using Ezipay.ViewModel.ApiHelpPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Repository.ApiHelpPage
{
    public interface IApiHelpPageRespository
    {
        List<ApiHelpPageModel> ApiList();
    }
}
