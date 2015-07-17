using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraConfigTool.Onvif;

namespace CameraConfigTool
{
    public delegate void NotifySearch(IPCamera ipCamera,string msg);

    public class CameraSearch
    {
        public NotifySearch NotifySearchEvent;

        public CameraSearch()
        {
        }

        public List<IPCamera> GetAvailableCameraList()
        {
            List<IPCamera> serviceList = new List<IPCamera>();
            UdpCamSearch udpInstance = new UdpCamSearch();
            udpInstance.SendCamData();
            if (udpInstance.GetServiceList() != null)
                serviceList=udpInstance.GetServiceList();
            ///WSDiscoveryForNVT wsDiscoveryForNVT = new WSDiscoveryForNVT();
            ///wsDiscoveryForNVT.ListenForUri += NotifySearchEvent;
            ///wsDiscoveryForNVT.WSDiscovery();
            UpnpCamSearch upnpInstance = new UpnpCamSearch();
            upnpInstance.NotifySearchEvent += NotifySearchEvent;
            upnpInstance.SendCamData();
            serviceList.AddRange(upnpInstance.GetServiceList());

            return serviceList;
        }
    }
}