using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Xml;
using System.Net.NetworkInformation;

namespace CameraConfigTool
{
    public class UpnpCamSearch
    {
        private List<string> CameraDescriptionUrlList = new List<string>();
        private const string MULTCASTADDRESS = "239.255.255.250";//"255.255.255.255";
        private const int PORTNUM = 1900;
        private const string MESSFORCAM_ALL = "M-SEARCH * HTTP/1.1\r\n" +
                             "HOST: 239.255.255.250:1900\r\n" +
                             "ST: ssdp:all\r\n" +
                             "MAN: \"ssdp:discover\"\r\n" +
                             "MX: 3\r\n\r\n";
        private const string MESSFORCAM_BASIC = "M-SEARCH * HTTP/1.1\r\n" +
                              "HOST: 239.255.255.250:1900\r\n" +
                              "ST: urn:schemas-upnp-org:device:Basic:1.0\r\n" +
                              "MAN: \"ssdp:discover\"\r\n" +
                              "MX: 3\r\n\r\n";
        private const string MESSFORCAM_MEDIASERVER = "M-SEARCH * HTTP/1.1\r\n" +
                              "HOST: 239.255.255.250:1900\r\n" +
                              "ST: urn:schemas-upnp-org:device:MediaServer:1\r\n" +
                              "MAN: \"ssdp:discover\"\r\n" +
                              "MX: 3\r\n\r\n";
        private UdpClient udpC;
        private Thread listener;
        private Byte[] receiveBytes;
        private ManualResetEvent mre;


        public NotifySearch NotifySearchEvent;
        /// <summary>
        /// Send camera research packet and receive camera respones packet
        /// </summary>
        public void SendCamData()
        {
            SendCamSearch();
            ReceiveCam();
        }

        /// <summary>
        /// Get camera web interface address
        /// </summary>
        /// <returns>Camera web interface address list</returns>
        public List<IPCamera> GetServiceList()
        {
            if (CameraDescriptionUrlList != null && CameraDescriptionUrlList.Count > 0)
            {
                CameraDescriptionUrlList = CameraDescriptionUrlList.Distinct().ToList();
                List<IPCamera> cameraList = new List<IPCamera>();
                foreach (var serviceUrl in CameraDescriptionUrlList)
                {
                    try
                    {
                        IPCamera ipCamera = new IPCamera();
                        XmlDocument responesXml = new XmlDocument();
                        responesXml.Load(WebRequest.Create(serviceUrl).GetResponse().GetResponseStream());
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(responesXml.NameTable);
                        nsmgr.AddNamespace("tts", "urn:schemas-upnp-org:device-1-0");
                        XmlNode deviceTypeNode = responesXml.SelectSingleNode("//tts:device/tts:deviceType/text()", nsmgr);
                        if (deviceTypeNode == null || string.IsNullOrEmpty(deviceTypeNode.Value))
                            continue;
                        else
                        {
                            if (deviceTypeNode.Value.Contains("urn:schemas-upnp-org:device:Basic:1") || deviceTypeNode.Value.Contains("urn:schemas-upnp-org:device:MediaServer:1"))
                            { }
                            else
                                continue;
                        }
                        XmlNode modelNode = responesXml.SelectSingleNode("//tts:device/tts:modelName/text()", nsmgr);
                        if (modelNode == null || String.IsNullOrEmpty(modelNode.Value))
                            continue;
                        ipCamera.ModelName = modelNode.Value;
                        XmlNode urlNode = responesXml.SelectSingleNode("//tts:device/tts:presentationURL/text()", nsmgr);
                        if (urlNode == null || String.IsNullOrEmpty(urlNode.Value))
                            continue;
                        ipCamera.PresentantionUrl = urlNode.Value;
                        XmlNode uuidNode = responesXml.SelectSingleNode("//tts:device/tts:UDN/text()", nsmgr);
                        if (uuidNode == null || string.IsNullOrEmpty(uuidNode.Value))
                            continue;
                        ipCamera.UUID = uuidNode.Value;
                        InitialCamera(ipCamera);
                        if (cameraList.Count(va => va.UUID == uuidNode.Value) == 0)
                            cameraList.Add(ipCamera);

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                return cameraList;

            }
            else
                return null;
        }

        public IPCamera GetIPCameraByDescriptionUrl(string serviceUrl)
        {
            try
            {
                IPCamera ipCamera = new IPCamera();
                XmlDocument responesXml = new XmlDocument();
                responesXml.Load(WebRequest.Create(serviceUrl).GetResponse().GetResponseStream());
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(responesXml.NameTable);
                nsmgr.AddNamespace("tts", "urn:schemas-upnp-org:device-1-0");
                XmlNode deviceTypeNode = responesXml.SelectSingleNode("//tts:device/tts:deviceType/text()", nsmgr);
                if (deviceTypeNode == null || string.IsNullOrEmpty(deviceTypeNode.Value))
                    return null;
                else
                {
                    if (deviceTypeNode.Value.Contains("urn:schemas-upnp-org:device:Basic:1") || deviceTypeNode.Value.Contains("urn:schemas-upnp-org:device:MediaServer:1"))
                    { }
                    else
                        return null;
                }
                XmlNode modelNode = responesXml.SelectSingleNode("//tts:device/tts:modelName/text()", nsmgr);
                if (modelNode == null || String.IsNullOrEmpty(modelNode.Value))
                    return null;
                ipCamera.ModelName = modelNode.Value;
                XmlNode urlNode = responesXml.SelectSingleNode("//tts:device/tts:presentationURL/text()", nsmgr);
                if (urlNode == null || String.IsNullOrEmpty(urlNode.Value))
                    return null;
                ipCamera.PresentantionUrl = urlNode.Value;
                XmlNode uuidNode = responesXml.SelectSingleNode("//tts:device/tts:UDN/text()", nsmgr);
                if (uuidNode == null || string.IsNullOrEmpty(uuidNode.Value))
                    return null;
                ipCamera.UUID = uuidNode.Value;
                InitialCamera(ipCamera);
                return ipCamera;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void InitialCamera(IPCamera camera)
        {
            camera.IsConfigureWorkDone = false;
            camera.IsConfigureWorkSuccess = true;/// for multi-steps configuration
            camera.IsAuthentication = false;
            camera.IsAuthenticateDone = false;
            camera.IsSurpported = false;
            camera.ResultDetailMessage = string.Empty;
            camera.AuthenticateDetailResultMessage = string.Empty;
            if (!camera.IsOnceConfigured)
                camera.UserSettingName = string.Empty;
            camera.FirmVersion = new FirmVersion();
        }

        /// <summary>
        /// Send search packet
        /// </summary>
        private void SendCamSearch()
        {
            udpC = new UdpClient(11200);

            try
            {
                List<IPAddress> broadcastList = GetSubnetBroadcastIP();
                IPEndPoint iep;
                if (broadcastList != null && broadcastList.Count > 0)
                {
                    foreach (var address in broadcastList)
                    {
                        iep = new IPEndPoint(address, PORTNUM);
                        udpC.Send(Encoding.ASCII.GetBytes(MESSFORCAM_ALL), Encoding.ASCII.GetBytes(MESSFORCAM_ALL).Length, iep);
                        //udpC.Send(Encoding.ASCII.GetBytes(MESSFORCAM_BASIC), Encoding.ASCII.GetBytes(MESSFORCAM_BASIC).Length, iep);
                        //udpC.Send(Encoding.ASCII.GetBytes(MESSFORCAM_MEDIASERVER), Encoding.ASCII.GetBytes(MESSFORCAM_MEDIASERVER).Length, iep);
                    }
                }
                else
                {
                    iep = new IPEndPoint(IPAddress.Parse(MULTCASTADDRESS), PORTNUM);
                    udpC.Send(Encoding.ASCII.GetBytes(MESSFORCAM_ALL), Encoding.ASCII.GetBytes(MESSFORCAM_ALL).Length, iep);
                    //udpC.Send(Encoding.ASCII.GetBytes(MESSFORCAM_BASIC), Encoding.ASCII.GetBytes(MESSFORCAM_BASIC).Length, iep);
                    //udpC.Send(Encoding.ASCII.GetBytes(MESSFORCAM_MEDIASERVER), Encoding.ASCII.GetBytes(MESSFORCAM_MEDIASERVER).Length, iep);
                }
            }
            catch
            {

            }
        }

        private List<IPAddress> GetSubnetBroadcastIP()
        {
            List<IPAddress> ipList = new List<IPAddress>();
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                var ipv4Addrs = properties.UnicastAddresses.Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);
                foreach (var addr in ipv4Addrs)
                {
                    var network = CalculateValidNetwork(addr);
                    if (network != null)
                        ipList.Add(network);
                }
            }
            return ipList;
        }

        private static IPAddress CalculateValidNetwork(UnicastIPAddressInformation addr)
        {
            if (CheckValidNetworkAddress(addr))
            {
                var ip = addr.Address.GetAddressBytes();
                var mask = addr.IPv4Mask.GetAddressBytes();

                var result = new Byte[4];
                for (int i = 0; i < 4; ++i)
                {
                    result[i] = (Byte)(ip[i] & mask[i]);
                    result[i] = (Byte)(result[i] + (~mask[i]));
                }
                return new IPAddress(result);
            }
            else
                return null;
        }

        private static bool CheckValidNetworkAddress(UnicastIPAddressInformation addr)
        {
            if (addr.IPv4Mask == null)
                return false;
            if (addr.IPv4Mask.ToString().CompareTo("255.255.255.255") == 0)
                return false;
            var ip = addr.Address.GetAddressBytes();
            var mask = addr.IPv4Mask.GetAddressBytes();
            if (ip[0].CompareTo(Byte.Parse("10")) == 0 && mask[0].CompareTo(Byte.Parse("255")) == 0)
                return true;
            if (ip[0].CompareTo(Byte.Parse("172")) == 0 && ip[1].CompareTo(Byte.Parse("16")) >= 0 && ip[1].CompareTo(Byte.Parse("31")) <= 0)
            {
                if (mask[0].CompareTo(Byte.Parse("255")) == 0 && mask[1].CompareTo(Byte.Parse("240")) == 0)
                    return true;
            }
            if (ip[0].CompareTo(Byte.Parse("192")) == 0 || ip[1].CompareTo(Byte.Parse("168")) == 0)
            {
                if (mask[0].CompareTo(Byte.Parse("255")) == 0 && mask[1].CompareTo(Byte.Parse("255")) == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Receive respones packet
        /// </summary>
        private void ReceiveCam()
        {
            if (udpC != null)
            {
                mre = new ManualResetEvent(false);
                listener = new Thread(UpnpReceiveThread);
                listener.IsBackground = true;
                listener.Start();
                //listener.Join(2000);
                mre.WaitOne(2 * 1000);
                udpC.Close();
                if (NotifySearchEvent != null)
                {
                    NotifySearchEvent(null, "SE");
                }
            }
        }

        /// <summary>
        /// Receive thread
        /// </summary>
        private void UpnpReceiveThread()
        {
            string result;
            const string splitStr = "location:";
            while (true)
            {
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(MULTCASTADDRESS), PORTNUM);
                iep = null;
                try
                {
                    receiveBytes = udpC.Receive(ref iep);
                    result = Encoding.ASCII.GetString(receiveBytes);
                    result = result.Substring(result.ToLower().IndexOf(splitStr) + splitStr.Length);
                    result = result.Substring(0, result.IndexOf("\r")).Trim();

                    if (!CameraDescriptionUrlList.Contains(result))
                    {
                        if (NotifySearchEvent != null)
                        {
                            /// notify camera number add
                            NotifySearchEvent(null, "FNC");
                            ///
                            Thread t = new Thread(new ThreadStart(() =>
                            {
                                IPCamera ipCamera = GetIPCameraByDescriptionUrl(result);
                                if (ipCamera != null)
                                    NotifySearchEvent(ipCamera, "");
                            }));
                            t.Start();
                        }
                    }
                    CameraDescriptionUrlList.Add(result);
                }
                catch
                {
                    mre.Set();
                    break;
                }
            }
        }
    }
}
