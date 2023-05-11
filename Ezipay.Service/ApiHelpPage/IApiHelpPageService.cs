using Ezipay.ViewModel.ApiHelpPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.ApiHelpPage
{
    public interface IApiHelpPageService
    {
        List<ApiHelpPageModel> ApiList();
    }
}
