using Ezipay.ViewModel.common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Utility.common
{
    public class AES256
    {
        /// <summary>
        /// Key use for encrypt and decrypt mobile no and email
        /// </summary>
        public static Aes256KeyPairResponse AdminKeyPair =
            new Aes256KeyPairResponse
            {
                PublicKey = ConfigurationManager.AppSettings["PublicKey"],
                PrivateKey = ConfigurationManager.AppSettings["PrivateKey"]
            };
        /// <summary>
        /// Key use for encrypt and decrypt whole data except mobile no and email
        /// </summary>
        /// <returns></returns>
        public static Aes256KeyPairResponse UserKeyPair()
        {
            string key = string.Empty;
            if (GlobalData.RoleId == 1)
            {
                key = SHA256ALGO.GetKey2();
            }
            else
            {
                key = SHA256ALGO.GetKey();
            }
            return new Aes256KeyPairResponse { PublicKey = key, PrivateKey = key};
        }
        public static string Encrypt(string key, string input)
        {
            try
            {
                //byte[] key,
                byte[] iv;

                // byte[] base64data = Convert.FromBase64String(input);
                byte[] passphrasedata = RawBytesFromString(key);
                byte[] currentHash = new byte[0];
                SHA256Managed hash = new SHA256Managed();
                currentHash = hash.ComputeHash(passphrasedata);

                iv = new byte[16];
                // DeriveKeyAndIV(RawBytesFromString(passphrase), null, 1, out key, out iv);

                return Convert.ToBase64String(EncryptStringToBytes(input, currentHash, iv));
            }
            catch (Exception ex)
            {

                return string.Empty;
            }
        }

        public static string Decrypt(string key, string input)
        {
            try
            {
                //byte[] key, 
                byte[] iv = new byte[16];
                byte[] base64data = Convert.FromBase64String(input);
                byte[] passphrasedata = RawBytesFromString(key);
                byte[] currentHash = new byte[0];
                SHA256Managed hash = new SHA256Managed();
                currentHash = hash.ComputeHash(passphrasedata);

                // DeriveKeyAndIV(RawBytesFromString(passphrase), null, 1, out key, out iv);

                return DecryptStringFromBytes(base64data, currentHash, null);
            }
            catch (Exception ex)
            {

                return string.Empty;
            }
        }

        public static string Encrypt2(string key, string cipherText)
        {
            try
            {
                var keybytes = Encoding.UTF8.GetBytes(key);
                var iv = Encoding.UTF8.GetBytes(key);

                var encrypted = Convert.FromBase64String(cipherText);
                return Convert.ToBase64String(EncryptStringToBytes2(cipherText, keybytes, iv));
            }
            catch (Exception ex)
            {

                return string.Empty;
            }
        }
        public static string Decrypt2(string key, string cipherText)
        {
            try
            {
                var keybytes = Encoding.UTF8.GetBytes(key);
                var iv = Encoding.UTF8.GetBytes(key);

                var encrypted = Convert.FromBase64String(cipherText);
                var decriptedFromJavascript = DecryptStringFromBytes2(encrypted, keybytes, iv);
                return decriptedFromJavascript;
            }catch(Exception ex)
            {
                return string.Empty;
            }
        }


        static byte[] EncryptStringToBytes2(string plainText, byte[] key, byte[] iV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (iV == null || iV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128 / 8;

                rijAlg.Key = key;
                rijAlg.IV = iV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            return encrypted;

        }

        private static string DecryptStringFromBytes2(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128/8;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.  
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream  
                                // and place them in a string.  
                                plaintext = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch(Exception ex)
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }


        private static byte[] RawBytesFromString(string input)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(input);
            //var ret = new List<Byte>();

            //foreach (char x in input)
            //{
            //  var c = (byte)((ulong)x & 0xFF);
            //  ret.Add(c);
            //}

            //return  ret.ToArray();
        }

        private static void DeriveKeyAndIV(byte[] data, byte[] salt, int count, out byte[] key, out byte[] iv)
        {
            List<byte> hashList = new List<byte>();
            byte[] currentHash = new byte[0];

            int preHashLength = data.Length + ((salt != null) ? salt.Length : 0);
            byte[] preHash = new byte[preHashLength];

            System.Buffer.BlockCopy(data, 0, preHash, 0, data.Length);
            if (salt != null)
                System.Buffer.BlockCopy(salt, 0, preHash, data.Length, salt.Length);

            SHA256Managed hash = new SHA256Managed();
            currentHash = hash.ComputeHash(preHash);

            for (int i = 1; i < count; i++)
            {
                currentHash = hash.ComputeHash(currentHash);
            }

            hashList.AddRange(currentHash);

            while (hashList.Count < 48) // for 32-byte key and 16-byte iv
            {
                preHashLength = currentHash.Length + data.Length + ((salt != null) ? salt.Length : 0);
                preHash = new byte[preHashLength];

                System.Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                System.Buffer.BlockCopy(data, 0, preHash, currentHash.Length, data.Length);
                if (salt != null)
                    System.Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + data.Length, salt.Length);

                currentHash = hash.ComputeHash(preHash);

                for (int i = 1; i < count; i++)
                {
                    currentHash = hash.ComputeHash(currentHash);
                }

                hashList.AddRange(currentHash);
            }
            hash.Clear();
            key = new byte[32];
            iv = new byte[16];
            hashList.CopyTo(0, key, 0, 32);
            hashList.CopyTo(32, iv, 0, 16);
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged cipher = new RijndaelManaged())
            {
                cipher.Key = Key;
                cipher.IV = IV;
                //cipher.Mode = CipherMode.CBC;
                //cipher.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = cipher.CreateEncryptor(cipher.Key, cipher.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            return encrypted;

        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            //if (IV == null || IV.Length <= 0)
            //  throw new ArgumentNullException("Key");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (var cipher = new RijndaelManaged())
            {
                cipher.Key = Key;
                cipher.IV = new byte[16];
                //cipher.Mode = CipherMode.CBC;
                //cipher.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = cipher.CreateDecryptor(Key, cipher.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        var bytes = default(byte[]);
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            bytes = srDecrypt.CurrentEncoding.GetBytes(srDecrypt.ReadToEnd());

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            //aintext = srDecrypt.ReadToEnd();
                        }
                        plaintext = ASCIIEncoding.UTF8.GetString(bytes, 0, bytes.Count());
                    }
                }

            }

            return plaintext;

        }

        public static string EncryptURL(string clearText)
        {
            string EncryptionKey = "STUDEBUDDY03092014"; //"MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    //   clearText = System.Web.HttpServerUtility.UrlTokenEncode(ms.ToArray());
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string DecryptURL(string cipherText)
        {
            string EncryptionKey = "STUDEBUDDY03092014";// "MAKV2SPBNI99212";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    //cipherText = System.Web.HttpServerUtility.UrlTokenDecode(cipherText);
                    // cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }

    public static class Encryption
    {
        /// <summary>
        /// Key use for encrypt and decrypt mobile no and email
        /// </summary>
        public static Aes256KeyPairResponse AdminKeyPair =
            new Aes256KeyPairResponse
            {
                PublicKey = ConfigurationManager.AppSettings["PublicKey"],
                PrivateKey = ConfigurationManager.AppSettings["PrivateKey"]
            };

        /// <summary>
        /// Key use for encrypt and decrypt whole data except mobile no and email
        /// </summary>
        /// <returns></returns>
        public static Aes256KeyPairResponse UserKeyPair()
        {
            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(1024, cspParams);
            string PublicKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(false));
            string PrivateKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(true));
            return new Aes256KeyPairResponse { PublicKey = PublicKey, PrivateKey = PrivateKey };
        }

        /// <summary>
        /// Return Bytes of encrypted data
        /// </summary>
        /// <param name="PublicKey"></param>
        /// <param name="valueForEncryption"></param>
        /// <returns></returns>
        public static byte[] EncryptedBytes(string PublicKey, string valueForEncryption)
        {
            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            rsaProvider.ImportCspBlob(Convert.FromBase64String(PublicKey));
            byte[] plainBytes = Encoding.UTF8.GetBytes(valueForEncryption);
            return rsaProvider.Encrypt(plainBytes, false);

        }

        /// <summary>
        /// Return decrypted string from encrypted bytes.
        /// </summary>
        /// <param name="PrivateKey"></param>
        /// <param name="encryptedBytes"></param>
        /// <returns></returns>
        public static string DecryptedBytes(string PrivateKey, byte[] encryptedBytes)
        {
            CspParameters cspParams = new CspParameters { ProviderType = 1 };
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
            rsaProvider.ImportCspBlob(Convert.FromBase64String(PrivateKey));
            byte[] plainBytes = rsaProvider.Decrypt(encryptedBytes, false);
            return System.Text.Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);

        }

        /// <summary>
        /// Return encypted string 
        /// </summary>
        /// <param name="PublicKey"></param>
        /// <param name="valueForEncryption"></param>
        /// <returns></returns>
        public static string Encrypt(string PublicKey, string valueForEncryption)
        {
            return System.Text.Encoding.ASCII.GetString(EncryptedBytes(PublicKey, valueForEncryption));
        }

        /// <summary>
        /// Return decrypted string
        /// </summary>
        /// <param name="PrivateKey"></param>
        /// <param name="valueForEncryption"></param>
        /// <returns></returns>
        public static string Decrypt(string PrivateKey, string valueForEncryption)
        {
            return DecryptedBytes(PrivateKey, Encoding.ASCII.GetBytes(valueForEncryption)).ToString();
        }
    }

    public static class SHA256ALGO
    {


        public static EncryptionSha256Response HashPassword(string passwordToHash)
        {
            return HashPasswordEncryption(passwordToHash);
        }
        static EncryptionSha256Response HashPasswordEncryption(string passwordToHash)
        {
            int bitSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PasswordBitSize"]);
            string PrivateKey = System.Configuration.ConfigurationManager.AppSettings["PrivateKey"];
            int numberOfRounds = 100;
            var sw = new Stopwatch();
            sw.Start();
            byte[] saltBytes = GenerateSalt(bitSize);
            var hashedPassword = HashPassword(Encoding.UTF8.GetBytes(PrivateKey + passwordToHash), saltBytes, numberOfRounds, bitSize);
            sw.Stop();
            EncryptionSha256Response response = new EncryptionSha256Response { HashedPassword = Convert.ToBase64String(hashedPassword), SlatBytes = saltBytes, BitSize = bitSize };
            return response;
        }
        public static EncryptionSha256Response HashPasswordDecryption(string passwordToHash, byte[] saltBytes)
        {
            int bitSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PasswordBitSize"]);
            string PrivateKey = System.Configuration.ConfigurationManager.AppSettings["PrivateKey"];
            int numberOfRounds = 100;
            var sw = new Stopwatch();
            sw.Start();
            var hashedPassword = HashPassword(Encoding.UTF8.GetBytes(PrivateKey + passwordToHash), saltBytes, numberOfRounds, bitSize);
            sw.Stop();
            EncryptionSha256Response response = new EncryptionSha256Response { HashedPassword = Convert.ToBase64String(hashedPassword), SlatBytes = saltBytes, BitSize = bitSize };
            return response;
        }
        static byte[] GenerateSalt(int bitSize)
        {
            using (var randomNumberGenerator = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[bitSize];
                randomNumberGenerator.GetBytes(randomNumber);

                return randomNumber;
            }
        }
        public static string GetKey()
        {
            int bitSize = Convert.ToInt32(ConfigurationManager.AppSettings["PasswordBitSize"]);
            // return Encoding.ASCII.GetString(GenerateSalt(bitSize));
            return CommonSetting.AlphaNumericString(bitSize);
        }

        public static string GetKey2()
        {
            int bitSize = 16; //Convert.ToInt32(ConfigurationManager.AppSettings["PasswordBitSize"]);
            // return Encoding.ASCII.GetString(GenerateSalt(bitSize));
            return CommonSetting.AlphaNumericString(bitSize);
        }

        static byte[] HashPassword(byte[] toBeHashed, byte[] salt, int numberOfRounds, int bitSize)
        {
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(toBeHashed, salt, numberOfRounds))
            {
                return rfc2898DeriveBytes.GetBytes(bitSize);
            }
        }

       
    }
    
}
