using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Ezipay.Utility.common;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace Ezipay.Utility.AWSS3
{
    public class S3Uploader : IS3Uploader
    {
        // private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSouth1;
        private static IAmazonS3 client;

        //public S3Uploader()
        //{
        //    client = new AmazonS3Client(bucketRegion);
        //}

        public bool UploadImages(Stream imgstream, string imgname)
        {
            bool result = false;

            result = WritingAnObjectAsync(
                imgstream,
                imgname,
                CommonSetting.AWS_ACCESS_KEY_ID,
                CommonSetting.AWS_SECRET_ACCESS_KEY,
                CommonSetting.AWS_BUCKET,
                CommonSetting.S3ServiceURL);
            if (result == true)
            {
                result = true;
            }
            return result;
        }

        public bool WritingAnObjectAsync(Stream imgstream, string imgname, string AWS_ACCESS_KEY_ID, string AWS_SECRET_ACCESS_KEY, string AWS_BUCKET, string S3ServiceURL)
        {
            //try
            //{
            //    AmazonS3Config config = new AmazonS3Config();
            //    config.ServiceURL = S3ServiceURL;
            //    config.RegionEndpoint = RegionEndpoint.APSouth1;
            //    using (client = new AmazonS3Client(AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, config))
            //    {
            //        PutObjectRequest request = new PutObjectRequest()
            //        {
            //            BucketName = AWS_BUCKET.Trim('/'),
            //            CannedACL = S3CannedACL.PublicRead,
            //            InputStream = imgstream,
            //            Key = imgname,                       
            //        };                    
            //        PutObjectResponse response = client.PutObject(request);
            //    }
            //}
            //catch (AmazonS3Exception e)
            //{
            //}
            //catch (Exception e)
            //{
            //}
            bool result = false;
            try
            {
                IAmazonS3 client;

                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = S3ServiceURL; //ConfigurationManager.AppSettings["AWSurl"];
                string AWSAccessKey = AWS_ACCESS_KEY_ID; /*ConfigurationManager.AppSettings["AWSAccessKey"];*/
                string AWSSecretKey = AWS_SECRET_ACCESS_KEY; /*ConfigurationManager.AppSettings["AWSSecretKey"];*/
                client = AWSClientFactory.CreateAmazonS3Client(
                  AWSAccessKey,
                  AWSSecretKey,
                   config
                   );
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = AWS_BUCKET.Trim('/'),
                    CannedACL = S3CannedACL.PublicRead,
                    InputStream = imgstream,
                    Key = imgname
                };
                PutObjectResponse response1 = client.PutObject(request);
                result= true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    result= false;
                }
                else
                {
                    result= false;
                }
            }
            return result;
        }


    }
}
