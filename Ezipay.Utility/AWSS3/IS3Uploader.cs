using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.AWSS3
{
    public interface IS3Uploader
    {
        bool UploadImages(Stream imgstream, string imgname);
    }
}
