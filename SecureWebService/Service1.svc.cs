using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.IO;


namespace SecureWebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(Namespace = Constants.Namespace)]
    public class Service1 : IService1
    {

        private static RSAParameters RSAParams;
        private static string publicKey = "";
        private static string privateKey = "";


        public Service1()
        {
            CreateKeys();
        }

        private void CreateKeys()
        {
            if (publicKey.Length == 0 || privateKey.Length == 0)
            {
                try
                {
                    //Create a new RSACryptoServiceProvider object.
                    using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(3072))
                    {
                        // Export Parameters
                        RSAParams = RSA.ExportParameters(false);
                        // Export public key
                        publicKey = RSA.ToXmlString(false);
                        // Export private/public key pair 
                        privateKey = RSA.ToXmlString(true);
                    }
                }
                catch (CryptographicException e)
                {
                    //Catch this exception in case the encryption did
                    //not succeed.
                    Console.WriteLine(e.Message);
                }
            }
        }


        public bool SetStatus(officerInfo info)
        {
            try
            {
                info.DecryptKeys(privateKey);
                string oId = info.officerId.ToUpper();
                double lat = info.latitude;
                double lon = info.longitude;
                int Stat = info.Status;
                XmlDocument xmlDoc = openFile();
                XmlNode node = xmlDoc.SelectSingleNode(string.Format("Officers/Officer[@id = '{0}']", oId));
                if (node != null)
                {
                    node.Attributes["latitude"].Value = lat.ToString();
                    node.Attributes["longitude"].Value = lon.ToString();
                    node.Attributes["status"].Value = Stat.ToString();
                }
                else
                {
                    XmlElement root = xmlDoc.DocumentElement;
                    using (XmlWriter writer = root.CreateNavigator().AppendChild())
                    {
                        writer.WriteStartElement("Officer");
                        writer.WriteAttributeString("status", Stat.ToString());
                        writer.WriteAttributeString("longitude", lon.ToString());
                        writer.WriteAttributeString("latitude", lat.ToString());
                        writer.WriteAttributeString("id", oId.ToString());
                        writer.WriteEndElement();
                    }
                }

                xmlDoc.Save(Constants.FilePath);

                return true;
            }
            catch
            {
                return false;
            }

        }

        private XmlDocument openFile()
        {
            if (!System.IO.File.Exists(Constants.FilePath))
            {
                using (XmlWriter writer = XmlWriter.Create(Constants.FilePath))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Officers");
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            XmlDocument xmlTemp = new XmlDocument();
            xmlTemp.Load(Constants.FilePath);
            return xmlTemp;
        }

        public string GetPublicKey()
        {
            return publicKey;
        }

        
    }
}
