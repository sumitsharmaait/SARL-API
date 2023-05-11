using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EziPay.AWSUtils
{
    public interface IS3Uploader
    {
        Task<bool> UploadImage(Stream imgstream, string imgname, string bucket);
        bool DeleteObject(string bucketName, string keyName);
    }
}
