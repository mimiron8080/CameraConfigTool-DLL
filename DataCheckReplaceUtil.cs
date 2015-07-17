using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;

namespace CameraConfigTool
{
    public class DataCreateUtil
    {
        private const string CAMERAFTP_ACCOUNT_USERNAME = "cftp_username";
        private const string CAMERAFTP_ACCOUNT_PASSWORD = "cftp_password";
        private const string CAMERAFTP_ACCOUNT_FTP_HOST = "cftp_ftp_host";
        private const string CAMERAFTP_ACCOUNT_FTP_PORT = "cftp_ftp_port";
        private const string CAMERAFTP_ACCOUNT_FTP_MODE = "cftp_ftp_ispassive";
        private const string CAMERAFTP_ACCOUNT_FTP_FOLDER = "cftp_ftp_folderpath";

        private const string CAMERAFTP_CAMERA_VEDIO_HEIGHT = "cftp_camera_height";
        private const string CAMERAFTP_CAMERA_VEDIO_WIDTH = "cftp_camera_width";
        private const string CAMERAFTP_CAMERA_VEDIO_FRAMERATE = "cftp_camera_video_framerate";
        private const string CAMERAFTP_CAMERA_VEDIO_RESOLUTION = "cftp_camera_video_resolution";

        private const string CAMERAFTP_CAMERA_IMAGE_FRAMERATE = "cftp_camera_image_framerate";
        private const string CAMERAFTP_CAMERA_IMAGE_RESOLUTION = "cftp_camera_image_resolution";

        private const string CAMERAFTP_CAMERA_EMAIL_RECIPIENT = "cftp_email_recipient";
        private const string CAMERAFTP_CAMERA_EMAIL_SMTPSERVER = "cftp_email_smtpserver";
        private const string CAMERAFTP_CAMERA_EMAIL_PORT = "cftp_email_port";
        private const string CAMERAFTP_CAMERA_EMAIL_SENDER = "cftp_email_sender";

        private const string CAMERA_ADMIN_USERNAME = "cftp_admin_username";
        private const string CAMERA_ADMIN_PASSWORD = "cftp_admin_password";
        private const string CAMERA_ADMIN_OLD_PASSWORD = "cftp_admin_old_password";

        private static CameraConfigReader _cameraConfigReader = new CameraConfigReader();

        //private static void DataCreateUtil()
        //{
        //    LoadConfiguration();
        //}

        private static void LoadConfiguration()
        {
            string path = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + path;

            string xmlPath = path + "\\configuration.xml";
            if (!File.Exists(xmlPath))
                xmlPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\configuration.xml";

            _cameraConfigReader.OpenConfiguration(xmlPath);
        }

        public static string CheckAndReplace(string data, DataArguemnts dataArgs)
        {
            if (data.Contains(CAMERAFTP_ACCOUNT_USERNAME))
            {
                data = data.Replace(CAMERAFTP_ACCOUNT_USERNAME, dataArgs.AccountUsername);
            }
            if (data.Contains(CAMERAFTP_ACCOUNT_PASSWORD))
            {
                data = data.Replace(CAMERAFTP_ACCOUNT_PASSWORD, dataArgs.AccountPassword);
            }
            if (data.Contains(CAMERAFTP_ACCOUNT_FTP_HOST))
            {
                data = data.Replace(CAMERAFTP_ACCOUNT_FTP_HOST, dataArgs.FtpHost);
            }
            if (data.Contains(CAMERAFTP_ACCOUNT_FTP_PORT))
            {
                data = data.Replace(CAMERAFTP_ACCOUNT_FTP_PORT, dataArgs.FtpPort);
            }
            if (data.Contains(CAMERAFTP_ACCOUNT_FTP_MODE))
            {
                data = data.Replace(CAMERAFTP_ACCOUNT_FTP_MODE, dataArgs.FtpMode);
            }
            if (data.Contains(CAMERAFTP_ACCOUNT_FTP_FOLDER))
            {
                data = data.Replace(CAMERAFTP_ACCOUNT_FTP_FOLDER, dataArgs.FtpFolder);
            }
            if (data.Contains(CAMERAFTP_CAMERA_VEDIO_HEIGHT))
            {
                data = data.Replace(CAMERAFTP_CAMERA_VEDIO_HEIGHT, dataArgs.VedioHeight);
            }
            if (data.Contains(CAMERAFTP_CAMERA_VEDIO_WIDTH))
            {
                data = data.Replace(CAMERAFTP_CAMERA_VEDIO_WIDTH, dataArgs.VedioWidth);
            }
            if (data.Contains(CAMERAFTP_CAMERA_VEDIO_FRAMERATE))
            {
                data = data.Replace(CAMERAFTP_CAMERA_VEDIO_FRAMERATE, dataArgs.VedioFramerate);
            }
            if (data.Contains(CAMERAFTP_CAMERA_VEDIO_RESOLUTION))
            {
                data = data.Replace(CAMERAFTP_CAMERA_VEDIO_RESOLUTION, dataArgs.VedioResolution);
            }
            if (data.Contains(CAMERAFTP_CAMERA_IMAGE_FRAMERATE))
            {
                data = data.Replace(CAMERAFTP_CAMERA_IMAGE_FRAMERATE, dataArgs.ImageFramerate);
            }
            if (data.Contains(CAMERAFTP_CAMERA_IMAGE_RESOLUTION))
            {
                data = data.Replace(CAMERAFTP_CAMERA_IMAGE_RESOLUTION, dataArgs.ImageResolution);
            }
            if (data.Contains(CAMERAFTP_CAMERA_EMAIL_RECIPIENT))
            {
                data = data.Replace(CAMERAFTP_CAMERA_EMAIL_RECIPIENT, dataArgs.EmailRecipient);
            }
            if (data.Contains(CAMERAFTP_CAMERA_EMAIL_SMTPSERVER))
            {
                data = data.Replace(CAMERAFTP_CAMERA_EMAIL_SMTPSERVER, dataArgs.EmailSmtpServer);
            }
            if (data.Contains(CAMERAFTP_CAMERA_EMAIL_PORT))
            {
                data = data.Replace(CAMERAFTP_CAMERA_EMAIL_PORT, dataArgs.EmailPort);
            }
            if (data.Contains(CAMERAFTP_CAMERA_EMAIL_SENDER))
            {
                data = data.Replace(CAMERAFTP_CAMERA_EMAIL_SENDER, dataArgs.EmailSender);
            }
            if (data.Contains(CAMERA_ADMIN_USERNAME))
            {
                data = data.Replace(CAMERA_ADMIN_USERNAME, dataArgs.AdminUserName);
            }
            if (data.Contains(CAMERA_ADMIN_PASSWORD))
            {
                data = data.Replace(CAMERA_ADMIN_PASSWORD, dataArgs.AdminPassword);
            }
            if (data.Contains(CAMERA_ADMIN_OLD_PASSWORD))
            {
                data = data.Replace(CAMERA_ADMIN_OLD_PASSWORD, dataArgs.AdminOldPassword);
            }
            return data;
        }

        public static DataArguemnts DealWithConfigAndCameraFtpUser(IPCamera camera, DataArguemnts dataArgs, CameraFtpUser cameraFtpUser)
        {
            LoadConfiguration();
            dataArgs.FtpHost = cameraFtpUser.FtpHost;
            dataArgs.FtpPort = cameraFtpUser.PortNum;
            dataArgs.AccountUsername = cameraFtpUser.UserName;
            dataArgs.AccountPassword = cameraFtpUser.Password;
            dataArgs.FtpFolder = cameraFtpUser.FilePath;
            dataArgs.FtpMode = "1";

            if (cameraFtpUser.ImageSettings.IsFreePlan != null && cameraFtpUser.ImageSettings.IsFreePlan == "false")
            {
                string imageResolution = BuildSuitableResolution(camera, cameraFtpUser.ImageSettings.Resolution);
                string imageFramerate = BuildSuitableFramerate(camera, (1000 / int.Parse(cameraFtpUser.ImageSettings.Frequency)).ToString());
                if (imageResolution.Contains("x"))
                {
                    dataArgs.ImageWidth = imageResolution.Split('x')[0];
                    dataArgs.ImageHeight = imageResolution.Split('x')[1];
                }
                dataArgs.ImageResolution = imageResolution;
                dataArgs.ImageFramerate = imageFramerate;
            }
            if (cameraFtpUser.VedioSettings.IsFreePlan != null && cameraFtpUser.VedioSettings.IsFreePlan == "false")
            {
                string videoReslution = BuildSuitableResolution(camera, cameraFtpUser.VedioSettings.Resolution);
                string videoFramerate = BuildSuitableFramerate(camera, (1000 / int.Parse(cameraFtpUser.VedioSettings.Frequency)).ToString());
                if (videoReslution.Contains("x"))
                {
                    dataArgs.VedioWidth = videoReslution.Split('x')[0];
                    dataArgs.VedioHeight = videoReslution.Split('x')[1];
                }
                dataArgs.VedioResolution = videoReslution;
                dataArgs.VedioFramerate = videoFramerate;
            }
            return dataArgs;
        }

        private static string BuildSuitableFramerate(IPCamera camera,string framerate)
        {
            int framerateDigit = int.Parse(framerate);
            string sequenceNum = camera.FirmVersion.SequenceNum;
            string configFramerate = _cameraConfigReader.GetFrameRate(sequenceNum);
            List<string> framerateList = configFramerate.Split(',').OfType<string>().ToList();
            int index = 0;
            for (int i = 0; i < framerateList.Count; i++)// small count erithin be ok
            {
                if (i + 1 == framerateList.Count)
                    index = i;
                if (framerateDigit < int.Parse(framerateList[i]))
                    continue;
                else
                {
                    if (i == 0)
                    {
                        index = i;
                    }
                    else
                    {
                        if ((framerateDigit - int.Parse(framerateList[i])) <= (int.Parse(framerateList[i - 1]) - framerateDigit))
                        {
                            index = i;
                        }
                        else
                        {
                            index = i - 1;
                        }
                    }
                    break;
                }
            }
            return framerateList[index];
        }

        private static string PresentationUrlFormat(string url)
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
        private static string BuildSuitableResolution(IPCamera camera, string resolution)
        {
            string sequenceNum = camera.FirmVersion.SequenceNum;
            int sqaure=0;
            if (resolution.Split('x').Length == 2)
            {
                sqaure = int.Parse(resolution.Split('x')[0]) * int.Parse(resolution.Split('x')[1]);
            }
            else
            {
                throw new Exception("resolution parse error");
            }

            string url = _cameraConfigReader.GetResolutionUrl(sequenceNum);
            AuthenticationType authenticationType = _cameraConfigReader.GetResolutionAuthenticationType(sequenceNum);
            string regex = _cameraConfigReader.GetResolutionRegex(sequenceNum);
            string configFileResolution = _cameraConfigReader.GetResolutionValue(sequenceNum);
            CommonValueType commonValueType = _cameraConfigReader.GetResolutionValueType(sequenceNum);
            if (commonValueType == CommonValueType.error)
                throw new CustomException("Configuration xml error: xml file encounter a error, please update your configuration.xml", CustomExceptionType.Xml);
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(configFileResolution))
            {
                throw new CustomException("Configuration xml error: xml file encounter a error, please update your configuration.xml", CustomExceptionType.Xml);
            }

            List<string> resolutionList = new List<string>();

            if (!string.IsNullOrEmpty(url))
            {
                if (authenticationType == AuthenticationType.Basic || authenticationType == AuthenticationType.Digest)
                {
                    string username = string.Empty;
                    string password = string.Empty;
                    if (string.IsNullOrEmpty(camera.Username))
                    {
                        username = _cameraConfigReader.GetUserName(camera.ModelName);
                        password = _cameraConfigReader.GetPassword(camera.ModelName);
                    }
                    else
                    {
                        username = camera.Username;
                        password = camera.Password;
                    }

                    resolutionList = ContentRegexTool.HttpGetResultWithRegex(PresentationUrlFormat(camera.PresentantionUrl) + url, regex, false, authenticationType, username, password);
                }
            }
            if (!string.IsNullOrEmpty(configFileResolution))
            {
                resolutionList = configFileResolution.Split(',').OfType<string>().ToList();
            }
            
            if (resolutionList.Count > 0)
            {
                Hashtable set = new Hashtable();
                for (int i = 0; i < resolutionList.Count; i++)
                {
                    int width = 0, height = 0;
                    string[] strArray = resolutionList[i].Split('x');
                    if (strArray.Length == 2)
                    {
                        width = int.Parse(strArray[0]);
                        height = int.Parse(strArray[1]);
                    }
                    else
                    {
                        throw new Exception("resolution parse error");
                    }

                    set.Add(i, width * height);
                }
                List<DictionaryEntry> sortedList = set.Cast<DictionaryEntry>().OrderBy(va => va.Value).ToList();
                int index = 0;
                for (int i = 0; i < sortedList.Count; i++)// small count erithin be ok
                {
                    if (i + 1 == sortedList.Count)
                        index = (int)sortedList[i].Key;
                    if (sqaure > (int)sortedList[i].Value)
                        continue;
                    else
                    {
                        if (i == 0)
                        {
                            index = (int)sortedList[i].Key;
                        }
                        else
                        {
                            if ((sqaure - (int)sortedList[i - 1].Value) <= ((int)sortedList[i].Value - sqaure))
                            {
                                index = (int)sortedList[i - 1].Key;
                            }
                            else
                            {
                                index = (int)sortedList[i].Key;
                            }
                        }
                        break;
                    }

                }
                if (commonValueType == CommonValueType.VALUE)
                    return resolutionList[index];
                if (commonValueType == CommonValueType.INDEX)
                    return index.ToString();
                throw new Exception("unknown error");
            }
            else
            {
                throw new Exception("unknown error");
            }
        }
    }

    public struct DataArguemnts
    {
        public string AccountUsername
        {
            get;
            set;
        }
        public string AccountPassword
        {
            get;
            set;
        }
        public string FtpHost
        {
            get;
            set;
        }
        public string FtpPort
        {
            get;
            set;
        }
        public string FtpMode
        {
            get;
            set;
        }
        public string FtpFolder
        {
            get;
            set;
        }
        public string VedioHeight
        {
            get;
            set;
        }
        public string VedioWidth
        {
            get;
            set;
        }
        public string ImageHeight
        {
            get;
            set;
        }
        public string ImageWidth
        {
            get;
            set;
        }
        public string VedioFramerate
        {
            get;
            set;
        }
        public string VedioResolution
        {
            get;
            set;
        }
        public string ImageFramerate
        {
            get;
            set;
        }
        public string ImageResolution
        {
            get;
            set;
        }
        public string EmailRecipient
        {
            get;
            set;
        }
        public string EmailSmtpServer
        {
            get;
            set;
        }
        public string EmailPort
        {
            get;
            set;
        }
        public string EmailSender
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
        public string AdminOldPassword
        {
            get;
            set;
        }
    }
}