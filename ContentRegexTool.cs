using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace CameraConfigTool
{
    public class ContentRegexTool
    {
        public static List<string> HttpGetResultWithRegex(string url, string regexStr, bool isExpect100Continue = true, AuthenticationType authentication = AuthenticationType.None, string userName = "", string password = "")
        {
            try
            {
                List<string> regexResult = new List<string>();
                Byte[] buffer = new Byte[1024 * 512];
                int readLength = 0;
                Regex regex = new Regex(FormatRegex(regexStr));
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.ServicePoint.Expect100Continue = isExpect100Continue;
                webRequest.KeepAlive = true;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
                webRequest.Accept = "text/html, application/xhtml+xml, */*";
                webRequest.Referer = url;

                webRequest.Timeout = 3000;

                if (authentication == AuthenticationType.Basic)
                    webRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password));
                if (authentication == AuthenticationType.Digest)
                {
                    NetworkCredential netWorkCredential = new NetworkCredential(userName, password);
                    CredentialCache credentialCache = new CredentialCache();
                    credentialCache.Add(new Uri(url), "Digest", netWorkCredential);
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = credentialCache;
                }
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

                Stream stream = webResponse.GetResponseStream();

                while ((readLength = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string content = Encoding.UTF8.GetString(buffer, 0, readLength);
                    MatchCollection matchList = regex.Matches(content, 0);
                    if (matchList != null && matchList.Count > 0)
                    {
                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if (matchList[i].Success)
                                regexResult.Add(matchList[i].Value);
                        }
                    }
                }
                if (regexResult.Count > 0)
                    regexResult = regexResult.Distinct().ToList();
                return regexResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string FormatRegex(string regexStr)
        {
            return regexStr;
        }
    }
}
