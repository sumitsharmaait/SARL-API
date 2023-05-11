using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

namespace EziPay.AWSUtils
{
    public class S3Uploader : IS3Uploader
    {
        public async Task<bool> UploadImage(Stream imgstream, string imgname, string bucket)
        {

            try
            {
                IAmazonS3 client;

                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = ConfigurationManager.AppSettings["AWSurl"];
                string AWSAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
                string AWSSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
                client = new AmazonS3Client(
                  AWSAccessKey,
                  AWSSecretKey,
                   config
                   );
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = bucket.Trim('/'),
                    CannedACL = S3CannedACL.PublicRead,
                    InputStream = imgstream,
                    Key = imgname

                };
                PutObjectResponse response1 = await client.PutObjectAsync(request);
                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteObject(string bucketName, string keyName)
        {
            try
            {
                IAmazonS3 client;
                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = ConfigurationManager.AppSettings["AWSurl"];
                string AWSAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
                string AWSSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
                config.ServiceURL = ConfigurationManager.AppSettings["AWSurl"];
                DeleteObjectRequest deleteObjectRequest =
                    new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName
                    };

                using (client = new AmazonS3Client(
                     AWSAccessKey, AWSSecretKey, config))
                {
                    client.DeleteObject(deleteObjectRequest);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


    }
}
