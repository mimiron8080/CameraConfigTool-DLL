using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace CameraConfigTool
{
    public class CameraSettings
    {
        private CameraFtpUser _cameraFtpUser;
        private CameraConfigReader _cameraConfigReader = new CameraConfigReader();
        private List<ConfigMethod> _configMethodList;
        private List<FirmVersion> _firmVersionList;
        private FirmVersion _currentFirmVersion;
        private DataArguemnts _dataArgs = new DataArguemnts();

        public CameraSettings(string path ,CameraFtpUser cameraFtpUser)
        {
            if (cameraFtpUser == null)
                throw new CustomException("Argument error: cameraFtpUser is null", CustomExceptionType.Argument);
            if (string.IsNullOrEmpty(cameraFtpUser.FtpHost) || string.IsNullOrEmpty(cameraFtpUser.PortNum))
                throw new CustomException("Argument error: cameraFtpUser is invalid", CustomExceptionType.Argument);
            _cameraFtpUser = cameraFtpUser.Clone();
            _cameraConfigReader.OpenConfiguration(path);
            _configMethodList = new List<ConfigMethod>();
            _firmVersionList = new List<FirmVersion>();
        }

        public CameraSettings(string path)
        {
            _cameraConfigReader.OpenConfiguration(path);
            _configMethodList = new List<ConfigMethod>();
            _firmVersionList = new List<FirmVersion>();
        }

        public void CameraConfig(IPCamera ipCamera)
        {
            try
            {
                if (GetMethod(ipCamera) == true)
                {
                    if (!string.IsNullOrEmpty(ipCamera.UserSettingName))
                        _cameraFtpUser.FilePath = ipCamera.UserSettingName;
                    if (string.IsNullOrEmpty(_cameraFtpUser.FilePath))
                    {
                        _cameraFtpUser.FilePath = ipCamera.ModelName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
                        ipCamera.UserSettingName = _cameraFtpUser.FilePath;
                    }
                    _dataArgs = DataCreateUtil.DealWithConfigAndCameraFtpUser(ipCamera, _dataArgs, _cameraFtpUser);
                    PostData(ipCamera);
                    ipCamera.IsConfigureWorkDone = true;
                }
            }
            catch (Exception ex)
            {
                ipCamera.IsConfigureWorkSuccess = false;
                ipCamera.IsConfigureWorkDone = true;
                throw ex;
            }
        }

        public void CameraAuthenticate(IPCamera ipCamera)
        {
            try
            {
                AuthenticationCheck(ipCamera);
                if (ipCamera.IsAuthenticateDone)
                    return;
                VersionCheck(ipCamera);
                if (string.IsNullOrEmpty(ipCamera.VedioInfo))
                {
                    ipCamera.IsAuthentication = false;
                    ipCamera.IsAuthenticateDone = true;
                }
                else
                {
                    ipCamera.IsAuthentication = true;
                }
            }
            catch (Exception ex)
            {
                ipCamera.IsAuthentication = false;
                ipCamera.IsAuthenticateDone = true;
                throw ex;
            }
        }

        public bool CameraVersionChange(IPCamera ipCamera, string username, string password)
        {
            try
            {
                VersionCheck(ipCamera, username, password);
                if (!string.IsNullOrEmpty(_currentFirmVersion.SequenceNum))
                    return true;
                else
                    throw new Exception("Get firm version error: firmware version error");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void VersionCheck(IPCamera ipCamera, string username = "", string password = "")
        {
            string deviceType = ipCamera.ModelName;
            int versionCount = _cameraConfigReader.GetVersionCount(deviceType);
            for (int i = 0; i < versionCount; i++)
            {
                FirmVersion firmVersion = new FirmVersion();
                try
                {
                    firmVersion.Url = _cameraConfigReader.GetVersionUrl(deviceType, i);
                    firmVersion.AuthenticationType = _cameraConfigReader.GetVersionAuthenticationType(deviceType, i);
                    if (firmVersion.AuthenticationType == AuthenticationType.error)
                        throw new CustomException("Configuration xml error: get wrong version authentication type", CustomExceptionType.Xml);
                    firmVersion.RegexStr = _cameraConfigReader.GetVersionRegex(deviceType, i);
                    if (string.IsNullOrEmpty(firmVersion.RegexStr))
                        throw new CustomException("Configuration xml error: version regex is null", CustomExceptionType.Xml);
                    firmVersion.No = _cameraConfigReader.GetVersionNo(deviceType, i);
                    firmVersion.SequenceNum = _cameraConfigReader.GetVersionSequenceNum(deviceType, i);
                }
                catch (Exception ex)
                {
                    if ((ex as CustomException) == null)
                        throw new CustomException("System return: " + ex.Message, CustomExceptionType.System);
                    else
                        throw ex;
                }
                _firmVersionList.Add(firmVersion);
            }
            if (_firmVersionList.Count > 0)
            {
                foreach (var firmVersion in _firmVersionList)
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        username = ipCamera.Username;
                        password = ipCamera.Password;
                    }
                    List<string> versionNo = ContentRegexTool.HttpGetResultWithRegex(PresentationUrlFormat(ipCamera.PresentantionUrl) + firmVersion.Url, firmVersion.RegexStr, false, firmVersion.AuthenticationType, username, password);
                    if (versionNo.Count != 1)
                        throw new Exception("Get firm version from server error");
                    if (versionNo[0] == firmVersion.No)
                    {
                        _currentFirmVersion = firmVersion;
                        ipCamera.FirmVersion = firmVersion;
                    }
                }
            }
            if (string.IsNullOrEmpty(ipCamera.VedioInfo))
            {
                if (!string.IsNullOrEmpty(ipCamera.FirmVersion.SequenceNum))
                    ipCamera.VedioInfo = _cameraConfigReader.GetVedioInfo(ipCamera.FirmVersion.SequenceNum);
            }
        }

        private void AuthenticationCheck(IPCamera ipCamera)
        {
            if (string.IsNullOrEmpty(ipCamera.Username))
            {
                ipCamera.Username = _cameraConfigReader.GetUserName(ipCamera.ModelName);
                ipCamera.Password = _cameraConfigReader.GetPassword(ipCamera.ModelName);
            }
            //string sequenceNum = ipCamera.FirmVersion.SequenceNum;
            string deviceType = ipCamera.ModelName;
            if (_cameraConfigReader.GetAuthenticationType(_cameraConfigReader.GetVersionSequenceNum(deviceType, 0), 0) == AuthenticationType.error)
                throw new CustomException("Configuration xml error: get wrong method authentication type", CustomExceptionType.Xml);

            if (!string.IsNullOrEmpty(_cameraConfigReader.GetCheckAuthenticationUrl(deviceType)))
            {
                string customPassword = "12345678";
                string userName = _cameraConfigReader.GetUserName(deviceType);
                string password = _cameraConfigReader.GetPassword(deviceType);

                string versionUrl = _cameraConfigReader.GetVersionUrl(deviceType, 0);
                string authType = string.Empty;
                AuthenticationType authenticationType = _cameraConfigReader.GetVersionAuthenticationType(deviceType, 0);
                if (authenticationType == AuthenticationType.Basic)
                    authType = "Basic";
                if (authenticationType == AuthenticationType.Digest)
                    authType = "Digest";

                ///4Try
                bool isFirst = false;
                try
                {
                    //isFirst = HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl) + versionUrl, false, "", "", authType, userName, password, 3000, true);
                    isFirst = HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl), false, "", "", authType, userName, password, 3000, true);
                }
                catch (Exception ex)
                {
                    isFirst = false;
                }
                if (isFirst)
                {
                    if (_cameraConfigReader.GetCheckAuthenticationAuthenticationType(deviceType) == AuthenticationType.Digest)
                    {

                        string url = _cameraConfigReader.GetCheckAuthenticationUrl(deviceType);
                        string data = _cameraConfigReader.GetCheckAuthenticationData(deviceType);
                        DataArguemnts dataArgs = new DataArguemnts();
                        dataArgs.AdminUserName = userName;
                        dataArgs.AdminOldPassword = password;
                        dataArgs.AdminPassword = customPassword;/// pending change 2015-01-23 08:33 PM

                        if (string.IsNullOrEmpty(data))
                        {
                            url = DataCreateUtil.CheckAndReplace(url, dataArgs);
                        }
                        //webRequest4Check.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password));

                        if (!string.IsNullOrEmpty(data))
                        {
                            data = DataCreateUtil.CheckAndReplace(data, dataArgs);
                        }
                        
                        ///4Reset
                        try
                        {
                            if (HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl) + url, false, "POST", data, authType, userName, password))
                            {
                                Thread.Sleep(60 * 1000);
                                //ipCamera.IsAuthentication = true;
                            }
                            else
                            {
                                ipCamera.IsAuthenticateDone = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            /// 4Check 
                            if (HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl) + versionUrl, false, "", "", authType, ipCamera.Username, customPassword, 10 * 1000))
                            {
                                ipCamera.Password = customPassword;
                                //ipCamera.IsAuthentication = true;
                            }
                            else
                            {
                                ipCamera.IsAuthenticateDone = true;
                            }
                        }
                    }
                }
                else
                {
                    /// 4Check 
                    if (HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl) + versionUrl, false, "", "", authType, ipCamera.Username, customPassword, 10 * 1000))
                    {
                        ipCamera.Password = customPassword;
                        //ipCamera.IsAuthentication = true;
                    }
                    else
                    {
                        ipCamera.IsAuthenticateDone = true;
                    }
                }
            }
            else
            {
                if (_cameraConfigReader.GetAuthenticationType(_cameraConfigReader.GetVersionSequenceNum(deviceType, 0), 0) == AuthenticationType.Basic)
                {
                    if (HttpMethod(ipCamera.PresentantionUrl, false, "", "", "Basic", ipCamera.Username, ipCamera.Password, 3 * 1000))
                    {
                        //ipCamera.IsAuthentication = true;
                    }
                    else
                    {
                        ipCamera.IsAuthenticateDone = true;
                    }
                }
                else
                {
                    //ipCamera.IsAuthentication = true;
                    ipCamera.IsAuthenticateDone = true;
                }
            }
        }

        private void PostData(IPCamera ipCamera)
        {
            try
            {
                foreach (var configMethod in _configMethodList)
                {
                    PostDataStep(ipCamera, configMethod);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PresentationUrlFormat(string url)
        {
            url = url.Contains("http") ? url : ("http://" + url);
            if (url.LastIndexOf("/", url.Length - 1, 1) == url.Length - 1)
            {
                url = url.Substring(0, url.Length - 1);
            }
            if (url.LastIndexOf("\\", url.Length - 1, 1) == url.Length - 1)
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        private void PostDataStep(IPCamera ipCamera, ConfigMethod configMethod)
        {
            OperationType filterType = OperationType.error;
            if (string.IsNullOrEmpty(_dataArgs.VedioFramerate))
            {
                filterType = OperationType.SetVideo;
                if (configMethod.OperationType == OperationType.SetVideoProfile)
                    filterType = OperationType.SetVideoProfile;
            }
            if (string.IsNullOrEmpty(_dataArgs.ImageFramerate))
            {
                filterType = OperationType.SetImage;
                if (configMethod.OperationType == OperationType.SetImageProfile)
                    filterType = OperationType.SetImageProfile;
            }
            if (!string.IsNullOrEmpty(_dataArgs.VedioFramerate) && !string.IsNullOrEmpty(_dataArgs.ImageFramerate))
            {
                filterType = OperationType.SetImage;
                if (configMethod.OperationType == OperationType.SetImageProfile)
                    filterType = OperationType.SetImageProfile;
            }
            if (configMethod.OperationType == filterType)
                return;
            if (!string.IsNullOrEmpty(ipCamera.Username))
            {
                configMethod.AdminUserName = ipCamera.Username;
                configMethod.AdminPassword = ipCamera.Password;/// may be not right 2014-12-30 06:11 PM
            }
            else
            {
                ipCamera.Username = configMethod.AdminUserName;
                ipCamera.Password = configMethod.AdminPassword;
            }

            if (configMethod.AuthenticationType == AuthenticationType.Url)
            {
                try
                {
                    configMethod.Url = DataCreateUtil.CheckAndReplace(configMethod.Url, _dataArgs);
                    
                    if (HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl) + configMethod.Url))
                    {
                        ///statusCode maybe right ,according to response result
                        /// ok
                    }
                    else
                    {
                        ipCamera.IsConfigureWorkSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (configMethod.AuthenticationType == AuthenticationType.Basic || configMethod.AuthenticationType == AuthenticationType.Digest)
            {
                try
                {
                    //DataArguemnts dataArgs = new DataArguemnts();
                    //dataArgs.FtpHost = _cameraFtpUser.FtpHost;
                    //dataArgs.FtpPort = _cameraFtpUser.PortNum;
                    //dataArgs.AccountUsername = _cameraFtpUser.UserName;
                    //dataArgs.AccountPassword = _cameraFtpUser.Password;
                    //dataArgs.FtpFolder = _cameraFtpUser.FilePath;
                    //dataArgs.FtpMode = "1";/// pending change 2015-01-15 01:10 PM

                    string requestType = string.Empty;
                    if (string.IsNullOrEmpty(configMethod.Data))
                    {
                        configMethod.Url = DataCreateUtil.CheckAndReplace(configMethod.Url, _dataArgs);
                        requestType = "GET";
                    }

                    if (!string.IsNullOrEmpty(configMethod.Data))
                    {
                        configMethod.Data = DataCreateUtil.CheckAndReplace(configMethod.Data, _dataArgs);
                        requestType = "POST";
                    }
                    string type = string.Empty;
                    if (configMethod.AuthenticationType == AuthenticationType.Basic)
                        type = "Basic";
                    if (configMethod.AuthenticationType == AuthenticationType.Digest)
                        type = "Digest";
                    if (HttpMethod(PresentationUrlFormat(ipCamera.PresentantionUrl) + configMethod.Url, false, requestType, configMethod.Data, type, configMethod.AdminUserName, configMethod.AdminPassword))
                    {
                        ///ok
                    }
                    else
                    {
                        ipCamera.IsConfigureWorkSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private bool HttpMethod(string url, bool isExpect100Continue = true, string method = "", string data = "", string authentication = "", string userName = "", string password = "", int timeout = -1, bool isForTry = false)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.ServicePoint.Expect100Continue = isExpect100Continue;
                if (!string.IsNullOrEmpty(method))
                {
                    if (method == "POST")
                        webRequest.Method = "POST";
                    if (method == "GET")
                        webRequest.Method = "GET";
                }
                //webRequest.KeepAlive = true;
                webRequest.KeepAlive = false;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
                webRequest.Accept = "text/html, application/xhtml+xml, */*";
                webRequest.Referer = url;

                if (authentication == "Basic")
                    webRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + password));
                if (authentication == "Digest")
                {
                    NetworkCredential netWorkCredential = new NetworkCredential(userName, password);
                    CredentialCache credentialCache = new CredentialCache();
                    credentialCache.Add(new Uri(url), "Digest", netWorkCredential);
                    webRequest.PreAuthenticate = true;
                    webRequest.Credentials = credentialCache;
                }
                if (timeout != -1)
                    webRequest.Timeout = timeout;

                if (!string.IsNullOrEmpty(data))
                {
                    Stream requestStream = webRequest.GetRequestStream();
                    requestStream.Write(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetBytes(data).Length);
                    requestStream.Close();
                }

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                
                //HttpStatusCode.TemporaryRedirect
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                if (webResponse.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    if (isForTry)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool GetMethod(IPCamera ipCamera)
        {
            string sequenceNum = ipCamera.FirmVersion.SequenceNum;
            int stepCount = _cameraConfigReader.GetStepCount(sequenceNum);

            for (int i = 0; i < stepCount; i++)
            {
                ConfigMethod configMethod = new ConfigMethod();
                try
                {
                    configMethod.Url = _cameraConfigReader.GetUrl(sequenceNum, i);
                    configMethod.AuthenticationType = _cameraConfigReader.GetAuthenticationType(sequenceNum, i);
                    configMethod.Data = _cameraConfigReader.GetData(sequenceNum, i);
                    configMethod.OperationType = _cameraConfigReader.GetOperationType(sequenceNum, i);
                    configMethod.AdminUserName = _cameraConfigReader.GetUserName(ipCamera.ModelName);
                    configMethod.AdminPassword = _cameraConfigReader.GetPassword(ipCamera.ModelName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //if (configMethod.Url == string.Empty)
                //    return false;
                if (configMethod.AuthenticationType == AuthenticationType.error)
                    return false;
                //if (configMethod.AuthenticationType == AuthenticationType.Basic && string.IsNullOrEmpty(configMethod.Data)) ///changed 2015-01-15 04:25 PM
                //    return false;
                _configMethodList.Add(configMethod);
            }

            return true;
        }
    }

    public struct FirmVersion
    {
        public string Url
        {
            get;
            set;
        }
        public AuthenticationType AuthenticationType
        {
            get;
            set;
        }
        public string RegexStr
        {
            get;
            set;
        }
        public string No
        {
            get;
            set;
        }
        public string SequenceNum
        {
            get;
            set;
        }
    }

    public struct ConfigMethod
    {
        public string Url
        {
            get;
            set;
        }

        public AuthenticationType AuthenticationType
        {
            get;
            set;
        }

        public OperationType OperationType
        {
            get;
            set;
        }

        public string Data
        {
            get;
            set;
        }

        public string AdminUserName
        {
            get;
            set;
        }

        public string AdminPassword
        {
            get;
            set;
        }
    }
}