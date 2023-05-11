using Ezipay.Repository.ApiHelpPage;
using Ezipay.ViewModel.ApiHelpPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.ApiHelpPage
{
    public class ApiHelpPageService : IApiHelpPageService
    {
        private IApiHelpPageRespository _apiHelpPageRespository;
        public ApiHelpPageService()
        {
            _apiHelpPageRespository = new ApiHelpPageRespository();
        }
        public List<ApiHelpPageModel> ApiList()
        {
            var response = new List<ApiHelpPageModel>();
            response = _apiHelpPageRespository.ApiList();
            return response;
        }
    }
}
