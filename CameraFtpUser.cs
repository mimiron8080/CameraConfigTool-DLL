using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraConfigTool
{
    public class CameraFtpUser : Object
	{
        public CameraFtpUser Clone()
        {
            return (CameraFtpUser)this.MemberwiseClone();
        }

        private string userName;
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        private string password;
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        private string ftpHost;
        public string FtpHost
        {
            get
            {
                return ftpHost;
            }
            set
            {
                ftpHost = value;
            }
        }

        private string portNum;
        public string PortNum
        {
            get
            {
                return portNum;
            }
            set
            {
                portNum = value;
            }
        }

        private string filePath;
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
            }
        }

        private AccountSettings imageSettings;
        public AccountSettings ImageSettings
        {
            get
            {
                return imageSettings;
            }
            set
            {
                imageSettings = value;
            }
        }

        private AccountSettings vedioSettings;
        public AccountSettings VedioSettings
        {
            get
            {
                return vedioSettings;
            }
            set
            {
                vedioSettings = value;
            }
        }
	}

    public class AccountSettings
    { 
        /// pending change 2015-01-20 03:47 PM
        ///
        private string resolution;
        public string Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                resolution = value;
            }
        }

        private string frequency;
        public string Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
            }
        }

        private string videoUploadFrequency;
        public string VideoUploadFrequency
        {
            get
            {
                return videoUploadFrequency;
            }
            set
            {
                videoUploadFrequency = value;
            }
        }

        private string videoClipDuration;
        public string VideoClipDuration
        {
            get
            {
                return videoClipDuration;
            }
            set
            {
                videoClipDuration = value;
            }
        }

        private string isFreePlan;
        public string IsFreePlan
        {
            get
            {
                return isFreePlan;
            }
            set
            {
                isFreePlan = value;
            }
        }
    }
}
