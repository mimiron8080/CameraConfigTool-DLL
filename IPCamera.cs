using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraConfigTool
{
    public class WorkMessage : EventArgs
    {
        public bool IsSuccess
        {
            get;
            set;
        }
    }
    public class IPCamera : Object
    {
        public IPCamera Clone()
        {
            return (IPCamera)this.MemberwiseClone();
        }

        public FirmVersion FirmVersion
        {
            get;
            set;
        }

        public string UserSettingName
        {
            get;
            set;
        }
        public string Percent
        {
            get;
            set;
        }
        public string ModelName
        {
            get;
            set;
        }
        public string PresentantionUrl
        {
            get;
            set;
        }
        public string VedioSourceUrl
        {
            get;
            set;
        }
        public string UUID
        {
            get;
            set;
        }
        public string Username
        {
            get;
            set;
        }
        public string Password
        {
            get;
            set;
        }
        public string VedioInfo
        {
            get;
            set;
        }
        public bool IsSurpportOnvif
        {
            get;
            set;
        }
        public bool IsAuthentication
        {
            get;
            set;
        }
        public bool IsAuthenticateDone
        {
            get;
            set;
        }
        public bool IsConfigureWorkDone
        {
            get;
            set;
        }
        public bool IsConfigureWorkSuccess
        {
            get;
            set;
        }
        public bool IsSurpported
        {
            get;
            set;
        }
        public bool IsOnceConfigured
        {
            get;
            set;
        }
        public string ResultMessage
        {
            get;
            set;
        }
        public string ResultDetailMessage
        {
            get;
            set;
        }
        public string AuthenticateResultMessage
        {
            get;
            set;
        }
        public string AuthenticateDetailResultMessage
        {
            get;
            set;
        }
        public DateTime LastConfigureTime
        {
            get;
            set;
        }
    }
}