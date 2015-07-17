using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Discovery;
using System.Xml;
using System.Threading;

namespace CameraConfigTool.Onvif
{
    /// <summary>
    /// Discovery Network Video Transmitter - NVT
    /// use Udp
    /// </summary>

    /// soap message example
    /// <s:Envelope xmlns:s="http://www.w3.org/2003/05/soap-envelope" xmlns:a="http://schemas.xmlsoap.org/ws/2004/08/addressing">
    ///     <s:Header>
    ///     <a:Action s:mustUnderstand="1">http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</a:Action>
    ///     <a:MessageID>urn:uuid:ecaa80ca-ef33-45dd-99ab-2fb3f7289406</a:MessageID>
    ///     <a:ReplyTo>
    ///         <a:Address>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</a:Address>
    ///     </a:ReplyTo>
    ///     <a:To s:mustUnderstand="1">urn:schemas-xmlsoap-org:ws:2005:04:discovery</a:To>
    ///     </s:Header>
    ///     <s:Body>
    ///         <Probe xmlns="http://schemas.xmlsoap.org/ws/2005/04/discovery">
    ///             <d:Types xmlns:d="http://schemas.xmlsoap.org/ws/2005/04/discovery" xmlns:dp0="http://www.onvif.org/ver10/network/wsdl">
    ///             dp0:NetworkVideoTransmitter
    ///             </d:Types>
    ///         </Probe>
    ///         </s:Body>
    /// </s:Envelope>

    public class WSDiscoveryForNVT
    {
        public void WSDiscovery()
        {
            DiscoveryEndpoint endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);

            DiscoveryClient discoveryClient = new DiscoveryClient(endPoint);

            discoveryClient.FindProgressChanged += DiscoveryClient_FindProgressChanged;

            FindCriteria findCriteria = new FindCriteria();
            findCriteria.Duration = TimeSpan.MaxValue;
            findCriteria.MaxResults = int.MaxValue;
            findCriteria.ContractTypeNames.Add(new XmlQualifiedName("NetworkVideoTransmitter", @"http://www.onvif.org/ver10/network/wsdl"));
            discoveryClient.FindAsync(findCriteria);
        }

        private void GetIPCameraByWSAddress(string serviceUrl)
        { 

        }

        private void DiscoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            EndpointDiscoveryMetadata eDMetadata = e.EndpointDiscoveryMetadata;
            /// userful eDMetadata.ListenUris.FirstOrDefault().AbsoluteUri 
            string uri = e.EndpointDiscoveryMetadata.ListenUris.FirstOrDefault().AbsoluteUri;
            if (ListenForUri != null)
            {
                ///
                /// get IPCamera Info
                /// use thread
                /// 
            }
        }

        public NotifySearch ListenForUri;
    }
}