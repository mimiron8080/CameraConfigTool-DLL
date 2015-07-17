using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace CameraConfigTool
{
    public class UdpCamSearch
    {
        private const string MULTCASTADDRESS = "255.255.255.255";
        private List<string> _serviceStrList=new List<string>();
        private List<IPCamera> _serviceList=new List<IPCamera>();
        private const int PORTNUM = 10500;//10000
        private UdpClient udpC;
        private Thread listener;
        private Byte[] MESSFORCAMBYTES_PART_I = new Byte[27] { 0x4d, 0x4f, 0x5f, 0x49, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01};
        private Byte[] MESSFORCAMBYTES_PART_II = new Byte[5] { 0xb4, 0x9a, 0x70, 0x4d, 0x00 };     
        private Byte[] receiveBytes;
        private const char SPLITCHAR = ';';

        /// <summary>
        /// Send camera research packet and receive camera respones packet
        /// </summary>
        public void SendCamData()
        {
            SendCamSearch();
            ReceiveCam();
        }

        public List<IPCamera> GetServiceList()
        {
            if (_serviceStrList != null)
            {
                HashSet<string> hssSet = new HashSet<string>(_serviceStrList);
                _serviceStrList = hssSet.Distinct().ToList();
                _serviceList = new List<IPCamera>();
                foreach (var str in _serviceStrList)
                {
                    try
                    {
                        IPCamera service = new IPCamera();
                        service.PresentantionUrl = str.Split(SPLITCHAR)[0];
                        service.ModelName = str.Split(SPLITCHAR)[1];
                        service.IsConfigureWorkDone = false;
                        service.IsConfigureWorkSuccess = true;
                        _serviceList.Add(service);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                return _serviceList;
            }
            else
                return null;
        }

        /// <summary>
        /// Send search packet
        /// </summary>
        private void SendCamSearch()
        {
            udpC = new UdpClient(10000);
            try
            {
                udpC.Send(MESSFORCAMBYTES_PART_I, MESSFORCAMBYTES_PART_I.Length, new IPEndPoint(IPAddress.Parse(MULTCASTADDRESS), PORTNUM));

                udpC.Send(MESSFORCAMBYTES_PART_II, MESSFORCAMBYTES_PART_II.Length, new IPEndPoint(IPAddress.Parse(MULTCASTADDRESS), PORTNUM));
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Udp error: "+ex.Message);
            }
        }

        /// <summary>
        /// Receive respones packet
        /// </summary
        private void ReceiveCam()
        {
            if (udpC != null)
            {
                listener = new Thread(UdpReceiveThread);
                listener.IsBackground = true;
                listener.Start();
                listener.Join(2000);
                udpC.Close();
            }
        }

        /// <summary>
        /// Receive thread
        /// </summary>
        private void UdpReceiveThread()
        {
            while (true)
            {
                try
                {
                    IPEndPoint iep = new IPEndPoint(IPAddress.Parse(MULTCASTADDRESS), PORTNUM);
                    //iep = null;
                    receiveBytes = udpC.Receive(ref iep);
                    if (receiveBytes.Length > 27)
                    {
                        string serviceAddress = receiveBytes[57].ToString() + "." + receiveBytes[58].ToString() + "." + receiveBytes[59].ToString() + "." + receiveBytes[60].ToString() + ":" + receiveBytes[75].ToString();
                        if (serviceAddress.Split(':')[0].CompareTo(iep.ToString().Split(':')[0]) == 0) //model HD811W
                        {
                            string serviceName = Encoding.ASCII.GetString(receiveBytes, 36, 6);
                            if (!string.IsNullOrEmpty(serviceName))
                            {
                                _serviceStrList.Add(serviceAddress + SPLITCHAR.ToString() + serviceName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }
    }
}
