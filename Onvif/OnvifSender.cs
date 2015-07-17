using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace CameraConfigTool.Onvif
{
    public class OnvifSender
    {
        struct DigestAuthenticationWS
        {
            public string Username { get; set; }
            public string Nonce { get; set; }
            public string Created { get; set; }
            public string Password { get; set; }
        }

        public static void CallWebService(string _url, bool isNeedAuthentication = false, string username = "", string password = "")
        {
            //var _url = "http://xxxxxxxxx/Service1.asmx";
            //var _action = "http://xxxxxxxx/Service1.asmx?op=HelloWorld";

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
            if (isNeedAuthentication)
            {
                if (!string.IsNullOrEmpty(username))
                    soapEnvelopeXml = CreateSoapEnvelope(CaculateDigestForWS(username, password));
            }

            HttpWebRequest webRequest = CreateWebRequest(_url, "");
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            asyncResult.AsyncWaitHandle.WaitOne();

            string soapResult;
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
                //Console.Write(soapResult);
            }
        }

        private static DigestAuthenticationWS CaculateDigestForWS(string username, string password)
        {
            DigestAuthenticationWS digestAuthWS = new DigestAuthenticationWS();

            string nonce = Guid.NewGuid().ToString();
            byte[] noncebyte = Encoding.ASCII.GetBytes(nonce);
            nonce = Convert.ToBase64String(noncebyte);
            DateTime datetime = DateTime.UtcNow;
            string dtstr = datetime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            byte[] datetimebyte = Encoding.ASCII.GetBytes(dtstr);
            byte[] passwordbyte = Encoding.ASCII.GetBytes(password);
            byte[] degistbyte = new byte[noncebyte.Length + datetimebyte.Length + passwordbyte.Length];
            Buffer.BlockCopy(noncebyte, 0, degistbyte, 0, noncebyte.Length);
            Buffer.BlockCopy(datetimebyte, 0, degistbyte, noncebyte.Length * sizeof(byte), datetimebyte.Length);
            Buffer.BlockCopy(passwordbyte, 0, degistbyte, (noncebyte.Length + datetimebyte.Length) * sizeof(byte), passwordbyte.Length);
            SHA1 hashAlgorithm = new SHA1CryptoServiceProvider();
            degistbyte = hashAlgorithm.ComputeHash(degistbyte);
            string degistpassword = Convert.ToBase64String(degistbyte);

            digestAuthWS.Username = username;
            digestAuthWS.Nonce = nonce;
            digestAuthWS.Created = dtstr;
            digestAuthWS.Password = degistpassword;

            return digestAuthWS;
        }

        private static HttpWebRequest CreateWebRequest(string url, string action = "")
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            if (!string.IsNullOrEmpty(action))
                webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        private static XmlDocument CreateSoapEnvelope()
        {
            XmlDocument soapEnvelop = new XmlDocument();
            soapEnvelop.LoadXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope"" xmlns:tds=""http://www.onvif.org/ver10/device/wsdl""><SOAP-ENV:Body><tds:GetSystemDateAndTime/></SOAP-ENV:Body></SOAP-ENV:Envelope>");
            //soapEnvelop.LoadXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope"" xmlns:tds=""http://www.onvif.org/ver10/device/wsdl""><SOAP-ENV:Body><tds:GetDeviceInformation/></SOAP-ENV:Body></SOAP-ENV:Envelope>");
            return soapEnvelop;
        }

        private static XmlDocument CreateSoapEnvelope(DigestAuthenticationWS digestAuthWS)
        {
            XmlDocument soapEnvelop = new XmlDocument();
            string authInfo = string.Format(@"<SOAP-ENV:Header><wsse:Security><wsse:UsernameToken><wsse:Username>{0}</wsse:Username><wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wssusername-token-profile-1.0#PasswordDigest"">{1}</wsse:Password><wsse:Nonce>{2}</wsse:Nonce><wsu:Created>{3}</wsu:Created></wsse:UsernameToken></wsse:Security></SOAP-ENV:Header>", digestAuthWS.Username, digestAuthWS.Password, digestAuthWS.Nonce, digestAuthWS.Created);
            //soapEnvelop.LoadXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:tt=""http://www.onvif.org/ver10/schema"" xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns:tds=""http://www.onvif.org/ver10/device/wsdl"">" + authInfo + @"<SOAP-ENV:Body><tds:GetDeviceInformation/></SOAP-ENV:Body></SOAP-ENV:Envelope>");
            soapEnvelop.LoadXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2003/05/soap-envelope"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:tt=""http://www.onvif.org/ver10/schema"" xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns:tds=""http://www.onvif.org/ver10/device/wsdl"">" + authInfo + @"<SOAP-ENV:Body><tds:GetCapabilities><tds:Category>All</tds:Category></tds:GetCapabilities></SOAP-ENV:Body></SOAP-ENV:Envelope>");
            return soapEnvelop;
        }
    }
}
