using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.ViewModel.common
{
    public class CommonModelView
    {
    }
    public class EncryptionAes256Request
    {
        public EncryptionAes256Request()
        {
            this.Password = string.Empty;
        }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
    }
    public class Aes256KeyPairResponse
    {
        public Aes256KeyPairResponse()
        {

            this.PrivateKey = string.Empty;
            this.PublicKey = string.Empty;
            this.PrivateKey2 = string.Empty;
        }
        /// <summary>
        /// Use for Decryption
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// Use for Entryption
        /// </summary>
        public string PublicKey { get; set; }
        public string PrivateKey2 { get; set; }
    }
    public class EncryptionSha256Request : EncryptionAes256Request
    {


    }
    public class EncryptionSha256Response
    {
        public EncryptionSha256Response()
        {
            this.HashedPassword = string.Empty;
            this.SlatBytes = new byte[0];
        }
        /// <summary>
        /// Hashed password
        /// </summary>
        public string HashedPassword { get; set; }
        /// <summary>
        /// Hashed salt for Decryption
        /// </summary>
        public byte[] SlatBytes { get; set; }
        /// <summary>
        /// Size of bit for data encryption and decryption
        /// </summary>
        public int BitSize { get; set; }
    }
    
}
