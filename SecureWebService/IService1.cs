using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;


namespace SecureWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(Namespace = Constants.Namespace)]
    public interface IService1
    {

        [OperationContract]
        string GetPublicKey();

        [OperationContract]
        bool SetStatus(officerInfo info);

    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract(Namespace = Constants.Namespace)]
    public class officerInfo
    {
        private byte[] _officerId;
        private byte[] _latitude;
        private byte[] _longitude;
        private byte[] _Status;
        //Symmetric keys
        private byte[] RijnKey;
        private byte[] RijnIV;
        // Asymmetric ecrypted Symmetric keys
        private byte[] EncryptedRijnKey;
        private byte[] EncryptedRijnIV;
        //Servers Public Key
        private string Public_key;

        [DataMember]
        public string officerId
        {
            get { return DecryptStringFromBytes(_officerId, RijnKey, RijnIV); }
            set { _officerId = EncryptStringToBytes(value, RijnKey, RijnIV); EncryptKeysLast(); }
        }

        [DataMember]
        public double latitude
        {
            get { return Convert.ToDouble(DecryptStringFromBytes(_latitude, RijnKey, RijnIV)); }
            set { _latitude = EncryptStringToBytes(Convert.ToString(value), RijnKey, RijnIV); EncryptKeysLast(); }
        }

        [DataMember]
        public double longitude
        {
            get { return Convert.ToDouble(DecryptStringFromBytes(_longitude, RijnKey, RijnIV)); }
            set { _longitude = EncryptStringToBytes(Convert.ToString(value), RijnKey, RijnIV); EncryptKeysLast(); }
        }

        [DataMember]
        public int Status
        {
            get { return Convert.ToInt16(DecryptStringFromBytes(_Status, RijnKey, RijnIV)); }
            set { _Status = EncryptStringToBytes(Convert.ToString(value), RijnKey, RijnIV); EncryptKeysLast(); }
        } 

        [DataMember]
        public string Publickey
        {
            set { Public_key = value; }
            get { return Public_key; }
        }

        private void RijnGenerateKeys()
        {
            //Generate symmetric key
            try
            {
                if (RijnKey == null || RijnIV == null)
                {
                    using (RijndaelManaged myRijndael = new RijndaelManaged())
                    {
                        myRijndael.KeySize = 256;
                        myRijndael.BlockSize = 256;
                        myRijndael.Mode = CipherMode.CBC;
                        //Generate a new random key for each use
                        myRijndael.GenerateKey();
                        RijnKey = myRijndael.Key;
                        //Generate a new random IV for each use
                        myRijndael.GenerateIV();
                        RijnIV = myRijndael.IV;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        
        private void _EncryptKeys()
        {
            try
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    if (Public_key == null || Public_key.Length <= 0) { throw new ArgumentNullException("Public_key"); }
                    //encrypt the symmetric key using the public key information
                    RSA.FromXmlString(Public_key);
                    EncryptedRijnKey = RSA.Encrypt(this.RijnKey, true);
                    //After Encrypting the key, set the un-encrypted key to null
                    RijnKey = null;
                    EncryptedRijnIV = RSA.Encrypt(this.RijnIV, true);
                    //After Encrypting the IV, set the un-encrypted IV to null
                    RijnIV = null;
                }
            }
            catch (Exception e)
            {
                //Catch this exception in case the encryption did
                //not succeed.
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        private void EncryptKeysLast()
        {
            //Check to make sure all properties are set before allowing key encryption
            if ( _officerId != null && _latitude != null && _longitude != null && _Status != null)
            {
                _EncryptKeys();
            }
        }

        public void DecryptKeys(string Private_key)
        {
            try
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Decrypt the symmetric key using the servers private key information
                    RSA.FromXmlString(Private_key);
                    RijnKey = RSA.Decrypt(EncryptedRijnKey, true);
                    EncryptedRijnKey = null;
                    RijnIV  = RSA.Decrypt(EncryptedRijnIV, true);
                    EncryptedRijnIV = null;
                }
            }
            catch (Exception e)
            {
                //Catch this exception in case the encryption did
                //not succeed.
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
    

        private byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            if (Key == null || Key.Length <= 0 || IV == null || IV.Length <= 0)
            {
                //Generate the symetric key
                RijnGenerateKeys();
                Key = RijnKey;
                IV = RijnIV;
            }
            if (plainText == null || plainText.Length <= 0) { throw new ArgumentNullException("plainText"); }
            if (Key == null || Key.Length <= 0) { throw new ArgumentNullException("Key"); }
            if (IV == null || IV.Length <= 0) { throw new ArgumentNullException("Key"); }

            byte[] encrypted;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.KeySize = 256;
                rijAlg.BlockSize = 256;

                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        private string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0) { throw new ArgumentNullException("cipherText"); }
            if (Key == null || Key.Length <= 0) { throw new ArgumentNullException("Key"); }
            if (IV == null || IV.Length <= 0) { throw new ArgumentNullException("Key"); }

            string plaintext = null;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.KeySize = 256;
                rijAlg.BlockSize = 256;

                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;

        }
    }

    public class Constants
    {
        // Ensures consistency in the name declarations across services
        public const string Namespace = "SecureWebService";
        public const string FilePath = "c:/log/officers.xml";
    }
}
