using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CameraConfigTool
{
    public enum AuthenticationType
    {
        Basic,
        Digest,
        Url,
        None,
        error
    }

    public enum CommonValueType
    {
        INDEX,
        VALUE,
        error
    }

    public enum OperationType
    {
        SetFtp,
        SetVideo,
        SetImage,
        SetImageProfile,
        SetVideoProfile,
        error
    }

    public class CameraConfigReader
    {
        private const int CONFIGVERSIONNODE_CHILDCOUNT = 2;
        private const int METHODNODE_CHILDCOUNT = 6;
        private const int DEVICENODE_CHILDCOUNT = 6;

        private const int DEVICECHECKAUTHENTICATIONNODE_CHILDCOUNT = 3;
        private const int METHODGETRESOLUTIONNODE_CHILDCOUNT = 5;

        /// methodlist 
        private const int VEDIOINFONODE_INDEX = 2;
        private const int GETFRAMERATENODE_INDEX = 3;
        private const int GETRESOLUTIONNODE_INDEX = 4;
        private const int STEPSNODE_INDEX = 5;

        /// device
        private const int CAMERATYPENODE_INDEX = 0;
        private const int USERNAMENODE_INDEX = 2;
        private const int PASSWORDNODE_INDEX = 3;
        private const int CHECKAUTHENTICATIONNODE_INDEX = 4;
        private const int VERSIONSNODE_INDEX = 5;

        /// version 
        private const int NONODE_INDEX = 3;
        private const int SEQUENCENUMNODE_INDEX = 5;

        /// checkAuthentication, getresolution, step ,version
        private const int URLNODE_INDEX = 0;
        private const int AUTHENTICATIONTYPENODE_INDEX = 1;

        /// checkAuthentication, step 
        private const int DATANODE_INDEX = 2;

        /// getresolution ,version
        private const int REGEXNODE_INDEX = 2;

        /// getresolution
        private const int RESOLUTIONNODE_INDEX = 3;
        private const int RESOLUTIONVALUETYPENODE_INDEX = 4;


        /// step 
        private const int OPERATIONTYPENODE_INDEX = 3;
        private const int RESULTNODE_INDEX = 4;

        enum ValueType
        {
            NO,
            USERNAME,
            PASSWORD,
            SEQUENCENUM,
            FRAMERATE,
            REGEX,
            RESOLUTION,
            RESOLUTIONVALUETYPE,
            URL,
            AUTHENTICATIONTYPE,
            DATA,
            OPERATIONTYPE,
            RESULT,
            MAJOR,
            MINOR,
            Null
        };

        enum DeviceRootType
        { 
            CAMERATYPE,
            DESCRIPTION,
            USERNAME,
            PASSWORD,
            CHECKAUTHENTICATION,
            VERSIONS
        }

        enum MethodRootType
        {
            NAME,
            SEQUENCENUM,
            VEDIOINFO,
            GETFRAMERATE,
            GETRESOLUTION,
            STEPS
        }

        enum HierarchyVaule
        { 
            CHILD,
            GRANDCHILD,
            GREATGRANDCHILD
        }

        private XmlDocument _configXml;
        private XmlNamespaceManager _nsmgr;

        public CameraConfigReader()
        {
            _configXml = new XmlDocument();
        }

        /// <summary>
        /// Open camera configuration xml file
        /// </summary>
        /// <param name="path">File path</param>
        public void OpenConfiguration(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new CustomException("Argument error: Path is null or empty", CustomExceptionType.Argument);
            try
            {
                _configXml.Load(path);
                _nsmgr = new XmlNamespaceManager(_configXml.NameTable);
            }
            catch (Exception ex)
            {
                if ((ex as CustomException) == null)
                    throw new CustomException("System return: " + ex.Message, CustomExceptionType.System);
                else
                    throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public string GetUserName(string deviceType)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.USERNAME, ValueType.Null, HierarchyVaule.CHILD);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public string GetPassword(string deviceType)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.PASSWORD, ValueType.Null, HierarchyVaule.CHILD);
        }

        public string GetVersionUrl(string deviceType,int versionNum)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.VERSIONS, ValueType.URL, HierarchyVaule.GREATGRANDCHILD, versionNum);
        }

        public AuthenticationType GetVersionAuthenticationType(string deviceType, int versionNum)
        {
            string authenticationType = GetDeviceChildValue(deviceType, DeviceRootType.VERSIONS, ValueType.AUTHENTICATIONTYPE, HierarchyVaule.GREATGRANDCHILD, versionNum);
            return ReturnAuthenticationType(authenticationType);
        }
        public string GetVersionRegex(string deviceType, int versionNum)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.VERSIONS, ValueType.REGEX, HierarchyVaule.GREATGRANDCHILD, versionNum);
        }
        public string GetVersionNo(string deviceType, int versionNum)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.VERSIONS, ValueType.NO, HierarchyVaule.GREATGRANDCHILD, versionNum);
        }

        public string GetVersionSequenceNum(string deviceType, int versionNum)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.VERSIONS, ValueType.SEQUENCENUM, HierarchyVaule.GREATGRANDCHILD, versionNum);
        }

        public string GetVedioInfo(string sequenceNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.VEDIOINFO, ValueType.Null, HierarchyVaule.CHILD);
        }

        public string GetFrameRate(string sequenceNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.GETFRAMERATE, ValueType.Null, HierarchyVaule.CHILD);
        }

        public string GetCheckAuthenticationUrl(string deviceType)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.CHECKAUTHENTICATION, ValueType.URL, HierarchyVaule.GRANDCHILD);
        }

        public AuthenticationType GetCheckAuthenticationAuthenticationType(string deviceType)
        {
            string authenticationType = GetDeviceChildValue(deviceType, DeviceRootType.CHECKAUTHENTICATION, ValueType.AUTHENTICATIONTYPE, HierarchyVaule.GRANDCHILD);
            return ReturnAuthenticationType(authenticationType);
        }

        public string GetCheckAuthenticationData(string deviceType)
        {
            return GetDeviceChildValue(deviceType, DeviceRootType.CHECKAUTHENTICATION, ValueType.DATA, HierarchyVaule.GRANDCHILD);
        }

        public string GetResolutionUrl(string sequenceNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.GETRESOLUTION, ValueType.URL, HierarchyVaule.GRANDCHILD);
        }

        public AuthenticationType GetResolutionAuthenticationType(string sequenceNum)
        {
            string authenticationType = GetMethodChildValue(sequenceNum, MethodRootType.GETRESOLUTION, ValueType.AUTHENTICATIONTYPE, HierarchyVaule.GRANDCHILD);
            return ReturnAuthenticationType(authenticationType);
        }

        public string GetResolutionRegex(string sequenceNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.GETRESOLUTION, ValueType.REGEX, HierarchyVaule.GRANDCHILD);
        }

        public string GetResolutionValue(string sequenceNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.GETRESOLUTION, ValueType.RESOLUTION, HierarchyVaule.GRANDCHILD);
        }

        public CommonValueType GetResolutionValueType(string sequenceNum)
        {
            string valueType = GetMethodChildValue(sequenceNum, MethodRootType.GETRESOLUTION, ValueType.RESOLUTIONVALUETYPE, HierarchyVaule.GRANDCHILD);
            if (valueType == "index")
                return CommonValueType.INDEX;
            if (valueType == "value")
                return CommonValueType.VALUE;
            return CommonValueType.error;
        }

        /// <summary>
        /// Get request url 
        /// </summary>
        /// <param name="deviceType">Camera model</param>
        /// <returns>Suffix of request url to configure camera</returns>
        public string GetUrl(string sequenceNum, int stepNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.STEPS, ValueType.URL, HierarchyVaule.GREATGRANDCHILD, stepNum);
        }

        /// <summary>
        /// Get authentication type
        /// </summary>
        /// <param name="deviceType">Camera model</param>
        /// <returns>Authentication type when login camera web interface</returns>
        public AuthenticationType GetAuthenticationType(string sequenceNum, int stepNum)
        {
            string authenticationType = GetMethodChildValue(sequenceNum, MethodRootType.STEPS, ValueType.AUTHENTICATIONTYPE, HierarchyVaule.GREATGRANDCHILD, stepNum);
            return ReturnAuthenticationType(authenticationType);
        }

        /// <summary>
        /// Get data to post
        /// </summary>
        /// <param name="deviceType">Camera model</param>
        /// <returns>Data to post when configure camera settings </returns>
        public string GetData(string sequenceNum, int stepNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.STEPS, ValueType.DATA, HierarchyVaule.GREATGRANDCHILD, stepNum);
        }

        public OperationType GetOperationType(string sequenceNum, int stepNum)
        {
            string operationType = GetMethodChildValue(sequenceNum, MethodRootType.STEPS, ValueType.OPERATIONTYPE, HierarchyVaule.GREATGRANDCHILD, stepNum);
            if (operationType == "SetFtp")
                return OperationType.SetFtp;
            if (operationType == "SetVideo")
                return OperationType.SetVideo;
            if (operationType == "SetImage")
                return OperationType.SetImage;
            if (operationType == "SetImageProfile")
                return OperationType.SetImageProfile;
            if (operationType == "SetVideoProfile")
                return OperationType.SetVideoProfile;
            return OperationType.error;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public string GetResult(string sequenceNum, int stepNum)
        {
            return GetMethodChildValue(sequenceNum, MethodRootType.STEPS, ValueType.RESULT, HierarchyVaule.GREATGRANDCHILD, stepNum);
        }

        public int GetVersionCount(string deviceType)
        {
            XmlNode deviceNode = GetDeviceNode(deviceType);
            if (deviceNode != null)
            {
                return deviceNode.ChildNodes.Item(VERSIONSNODE_INDEX).ChildNodes.Count;
            }
            else
                throw new CustomException("Configuration xml error: Not Found, device type", CustomExceptionType.Xml);
        }

        public int GetStepCount(string sequenceNum)
        {
            XmlNode methodNode = GetMethodNode(sequenceNum);

            return methodNode.ChildNodes.Item(STEPSNODE_INDEX).ChildNodes.Count;
        }

        public string GetMajor()
        {
            return GetVersion(ValueType.MAJOR);
        }

        public string GetMinor()
        {
            return GetVersion(ValueType.MINOR);
        }

        public bool IsDeviceExist(string deviceType)
        {
            try
            {
                GetDeviceNode(deviceType);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private AuthenticationType ReturnAuthenticationType(string authenticationType)
        {
            if (authenticationType == "Basic")
                return AuthenticationType.Basic;
            if (authenticationType == "Digest")
                return AuthenticationType.Digest;
            if (authenticationType == "Url")
                return AuthenticationType.Url;
            return AuthenticationType.error;
        }

        private string GetVersion(ValueType valueType)
        {
            int index = -1;
            if (valueType == ValueType.MAJOR)
                index = 0;
            if (valueType == ValueType.MINOR)
                index = 1;
            XmlNode versionNode = GetVersionNode();
            XmlNode valueNode = versionNode.ChildNodes.Item(index);
            if (valueNode.HasChildNodes)
            {
                return valueNode.FirstChild.Value;
            }
            else
            {
                throw new CustomException("Configuration xml error: The node value is null", CustomExceptionType.Xml);
            }
        }

        private XmlNode GetVersionNode()
        {
            if (_configXml == null)
                throw new CustomException("Abused error:The configuration xml have not loaded successfully, please use void OpenConfiguration(string path)", CustomExceptionType.Code);
            if (_nsmgr == null)
                throw new CustomException("Abused error:The configuration xml have not loaded successfully, please use void OpenConfiguration(string path)", CustomExceptionType.Code);

            try
            {
                _nsmgr.AddNamespace("tts", "camera-configuration-DHQ-1.0");
                XmlNodeList configVersionNodeList = _configXml.SelectNodes("//tts:configVersion", _nsmgr);
                if (configVersionNodeList == null)
                    throw new CustomException("Configuration xml error: Can not find 'configVersion' node in specified root node", CustomExceptionType.Xml);
                if (configVersionNodeList.Count != 1)
                    throw new CustomException("Configuration xml error: xml file encounter a error, please update your configuration.xml", CustomExceptionType.Xml);
                if (configVersionNodeList[0].ChildNodes.Count != CONFIGVERSIONNODE_CHILDCOUNT)
                    throw new CustomException("Configuration xml error: xml file encounter a error, please update your configuration.xml", CustomExceptionType.Xml);
                return configVersionNodeList[0];
            }
            catch (Exception ex)
            {
                if ((ex as CustomException) == null)
                    throw new CustomException("System return: " + ex.Message, CustomExceptionType.System);
                else
                    throw ex;
            }
        }

        private XmlNode GetDeviceNode(string deviceType)
        {
            if (string.IsNullOrEmpty(deviceType))
            {
                throw new CustomException("Agument error: Device type is null or empty", CustomExceptionType.Argument);
            }

            if (_configXml == null)
                throw new CustomException("Abused error:The configuration xml have not loaded successfully, please use  void OpenConfiguration(string path)", CustomExceptionType.Code);
            if (_nsmgr == null)
                throw new CustomException("Abused error:The configuration xml have not loaded successfully, please use  void OpenConfiguration(string path)", CustomExceptionType.Code);

            try
            {
                _nsmgr.AddNamespace("tts", "camera-configuration-DHQ-1.0");
                XmlNodeList deviceNodeList = _configXml.SelectNodes("//tts:deviceList/tts:device", _nsmgr);
                if (deviceNodeList == null)
                    throw new CustomException("Configuration xml error: Can not find 'device' node in specified root node", CustomExceptionType.Xml);
                foreach (var node in deviceNodeList)
                {
                    if (deviceType.CompareTo((node as XmlNode).ChildNodes.Item(CAMERATYPENODE_INDEX).FirstChild.Value) == 0)
                    {
                        XmlNode deviceNode = node as XmlNode;
                        if (deviceNode.ChildNodes.Count != DEVICENODE_CHILDCOUNT)
                            throw new CustomException("Code error: Get the wrong node, this is not 'device' node", CustomExceptionType.Code);
                        return deviceNode;
                    }
                }
            }
            catch (Exception ex)
            {
                if ((ex as CustomException) == null)
                    throw new CustomException("System return: " + ex.Message, CustomExceptionType.System);
                else
                    throw ex;
            }
            throw new CustomException("Configuration xml error: Not Found, device type", CustomExceptionType.Xml);
        }

        private XmlNode GetMethodNode(string sequenceNum)
        {
            int index = int.Parse(sequenceNum);
            if (index > 0)
            {
                try
                {
                    _nsmgr.AddNamespace("tts", "camera-configuration-DHQ-1.0");
                    XmlNodeList methodNodeList = _configXml.SelectNodes("//tts:methodList/tts:method", _nsmgr);
                    if (methodNodeList == null)
                        throw new CustomException("Configuration xml error: Can not find 'method' node in specified root node", CustomExceptionType.Xml);
                    if (index > methodNodeList.Count)
                        throw new CustomException("Configuration xml error: the index over 'method' node count boundary", CustomExceptionType.Xml);

                    XmlNode methodNode = methodNodeList.Item(index - 1);
                    if (methodNode.ChildNodes.Count != METHODNODE_CHILDCOUNT)
                        throw new CustomException("Code error: Get the wrong node, this is not 'method' node", CustomExceptionType.Code);
                    if (methodNode.ChildNodes.Item(STEPSNODE_INDEX).ChildNodes.Count < 1)
                        throw new CustomException("Configuration xml error: 'steps' node is null", CustomExceptionType.Code);
                    return methodNode;
                }
                catch (Exception ex)
                {
                    if ((ex as CustomException) == null)
                        throw new CustomException("System return: " + ex.Message, CustomExceptionType.System);
                    else
                        throw ex;
                }
            }
            else
            {
                throw new CustomException("Code error: sequenceNum over boudary", CustomExceptionType.Code);
            }
        }

        private string GetDeviceChildValue(string deviceType, DeviceRootType deviceRootType, ValueType valueType, HierarchyVaule hierarchyValue,int versionNum=-1)
        {
            int valueIndex = -1, rootIndex = -1;
            if (deviceRootType == DeviceRootType.CAMERATYPE)
                rootIndex = CAMERATYPENODE_INDEX;
            if (deviceRootType == DeviceRootType.DESCRIPTION)
                rootIndex = 1;
            if (deviceRootType == DeviceRootType.USERNAME)
                rootIndex = USERNAMENODE_INDEX;
            if (deviceRootType == DeviceRootType.PASSWORD)
                rootIndex = PASSWORDNODE_INDEX;
            if (deviceRootType == DeviceRootType.CHECKAUTHENTICATION)
                rootIndex = CHECKAUTHENTICATIONNODE_INDEX;
            if (deviceRootType == DeviceRootType.VERSIONS)
                rootIndex = VERSIONSNODE_INDEX;
            if (rootIndex == -1)
                throw new CustomException("Code error: The specified DeviceRootType is invalid", CustomExceptionType.Code);
            if (valueType == ValueType.USERNAME)
                valueIndex = USERNAMENODE_INDEX;
            if (valueType == ValueType.PASSWORD)
                valueIndex = PASSWORDNODE_INDEX;
            if (valueType == ValueType.URL)
                valueIndex = 0;
            if (valueType == ValueType.AUTHENTICATIONTYPE)
                valueIndex = 1;
            if (valueType == ValueType.DATA)
                valueIndex = 2;
            if (valueType == ValueType.REGEX)
                valueIndex = 2;
            if (valueType == ValueType.NO)
                valueIndex = 3;
            if (valueType == ValueType.SEQUENCENUM)
                valueIndex = 5;
            if (valueIndex == -1 && valueType != ValueType.Null)
                throw new CustomException("Code error: The specified ValueType is invalid", CustomExceptionType.Code);

            XmlNode deviceNode = GetDeviceNode(deviceType);
            if (deviceNode != null)
            {
                if (hierarchyValue == HierarchyVaule.CHILD)
                {
                    XmlNode valueNode = deviceNode.ChildNodes.Item(rootIndex);
                    if (valueNode.HasChildNodes)
                    {
                        return valueNode.FirstChild.Value;
                    }
                    else
                    {
                        if (rootIndex != PASSWORDNODE_INDEX)
                            throw new CustomException("Configuration xml error: The node value is null", CustomExceptionType.Xml);
                        else
                            return string.Empty;
                    }
                }
                if (hierarchyValue == HierarchyVaule.GRANDCHILD)
                {
                    if (deviceNode.ChildNodes.Item(rootIndex).HasChildNodes)
                    {
                        XmlNode valueNode = deviceNode.ChildNodes.Item(rootIndex).ChildNodes.Item(valueIndex);
                        if (rootIndex == CHECKAUTHENTICATIONNODE_INDEX)
                            if (deviceNode.ChildNodes.Item(rootIndex).ChildNodes.Count != DEVICECHECKAUTHENTICATIONNODE_CHILDCOUNT)
                                throw new CustomException("Configuration xml error: xml file encounter a error, please update your configuration.xml", CustomExceptionType.Xml);
                        if (valueNode.HasChildNodes)
                        {
                            return valueNode.FirstChild.Value;
                        }
                        else
                        {
                            if (valueType != ValueType.DATA)
                                throw new CustomException("Configuration xml error: '" + deviceRootType.ToString() + "' child node value is null", CustomExceptionType.Xml);
                            else
                                return string.Empty;
                        }
                    }
                    else
                        return string.Empty;
                }
                if (hierarchyValue == HierarchyVaule.GREATGRANDCHILD)
                {
                    if (versionNum != -1)
                    {
                        XmlNode valueNode = deviceNode.ChildNodes.Item(rootIndex).ChildNodes.Item(versionNum).ChildNodes.Item(valueIndex);
                        if (valueNode.HasChildNodes)
                        {
                            return valueNode.FirstChild.Value;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        throw new CustomException("Version num error: not done yet", CustomExceptionType.Code);
                    }
                }
                throw new CustomException("Hierarchy error: to the last", CustomExceptionType.Code);
            }
            else
                return string.Empty;
        }

        private string GetMethodChildValue(string sequenceNum, MethodRootType methodRootType, ValueType valueType, HierarchyVaule hierarchyValue, int stepNum = -1)
        {
            int valueIndex = -1, rootIndex = -1;
            if (methodRootType == MethodRootType.NAME)
                rootIndex = 0;
            if (methodRootType == MethodRootType.SEQUENCENUM)
                rootIndex = 1;
            if (methodRootType == MethodRootType.VEDIOINFO)
                rootIndex = VEDIOINFONODE_INDEX;
            if (methodRootType == MethodRootType.GETFRAMERATE)
                rootIndex = GETFRAMERATENODE_INDEX;
            if (methodRootType == MethodRootType.GETRESOLUTION)
                rootIndex = GETRESOLUTIONNODE_INDEX;
            if (methodRootType == MethodRootType.STEPS)
                rootIndex = STEPSNODE_INDEX;
            if (rootIndex == -1)
                throw new CustomException("Code error: The specified MethodRootType is invalid", CustomExceptionType.Code);
            if (valueType == ValueType.URL)
                valueIndex = URLNODE_INDEX;
            if (valueType == ValueType.AUTHENTICATIONTYPE)
                valueIndex = AUTHENTICATIONTYPENODE_INDEX;
            if (valueType == ValueType.REGEX)
                valueIndex = REGEXNODE_INDEX;
            if (valueType == ValueType.RESOLUTION)
                valueIndex = RESOLUTIONNODE_INDEX;
            if (valueType == ValueType.RESOLUTIONVALUETYPE)
                valueIndex = RESOLUTIONVALUETYPENODE_INDEX;
            if (valueType == ValueType.DATA)
                valueIndex = DATANODE_INDEX;
            if (valueType == ValueType.OPERATIONTYPE)
                valueIndex = OPERATIONTYPENODE_INDEX;
            if (valueType == ValueType.RESULT)
                valueIndex = RESULTNODE_INDEX;
            if (valueIndex == -1 && valueType != ValueType.Null)
                throw new CustomException("Code error: The specified ValueType is invalid", CustomExceptionType.Code);
            XmlNode methodNode = GetMethodNode(sequenceNum);
            if (hierarchyValue == HierarchyVaule.CHILD)
            {
                if (methodNode.ChildNodes.Item(rootIndex).HasChildNodes)
                {
                    XmlNode valueNode = methodNode.ChildNodes.Item(rootIndex);
                    if (valueNode.ChildNodes.Count != 1)
                        throw new CustomException("Code error: get wrong node", CustomExceptionType.Code);
                    if (valueNode.HasChildNodes)
                    {
                        return valueNode.FirstChild.Value;
                    }
                    else
                    {
                        //return string.Empty;/// pending change 2015-01-27 10:51 AM
                        throw new CustomException("Configuration xml error: '" + methodRootType.ToString() + "' node value is null", CustomExceptionType.Xml);
                    }
                }
                else
                {
                    throw new CustomException("Configuration xml error: '" + methodRootType.ToString() + "' node value is null", CustomExceptionType.Xml);
                }
            }
            if (hierarchyValue == HierarchyVaule.GRANDCHILD)
            {
                if (methodNode.ChildNodes.Item(rootIndex).HasChildNodes)
                {
                    XmlNode valueNode = methodNode.ChildNodes.Item(rootIndex).ChildNodes.Item(valueIndex);
                    if (rootIndex == GETRESOLUTIONNODE_INDEX)
                        if (methodNode.ChildNodes.Item(rootIndex).ChildNodes.Count != METHODGETRESOLUTIONNODE_CHILDCOUNT)
                            throw new CustomException("Configuration xml error: xml file encounter a error, please update your configuration.xml", CustomExceptionType.Xml);
                    if (valueNode.HasChildNodes)
                    {
                        return valueNode.FirstChild.Value;
                    }
                    else
                    {
                        if (valueType != ValueType.URL && valueType != ValueType.AUTHENTICATIONTYPE && valueType != ValueType.REGEX && valueType != ValueType.RESOLUTION)
                            throw new CustomException("Configuration xml error: '" + methodRootType.ToString() + "' child node value is null", CustomExceptionType.Xml);
                        else
                            return string.Empty;
                    }
                }
                else
                    return string.Empty;
            }
            if (hierarchyValue == HierarchyVaule.GREATGRANDCHILD)
            {
                if (stepNum != -1)
                {
                    if (methodNode.ChildNodes.Item(rootIndex).ChildNodes.Count > stepNum)
                    {
                        XmlNode valueNode = methodNode.ChildNodes.Item(rootIndex).ChildNodes.Item(stepNum).ChildNodes.Item(valueIndex);
                        if (valueNode.HasChildNodes)
                        {
                            return valueNode.FirstChild.Value;
                        }
                        else
                        {
                            if (valueIndex != DATANODE_INDEX && valueIndex != RESULTNODE_INDEX)
                                throw new CustomException("Configuration xml error: '" + methodRootType.ToString() + "' child node value is null", CustomExceptionType.Xml);
                            else
                                return string.Empty;
                        }
                    }
                    else
                    {
                        throw new CustomException("Arguement error: " + methodRootType.ToString() + " number over boundary", CustomExceptionType.Argument);
                    }
                }
                else
                {
                    throw new CustomException("Step num error: not done yet", CustomExceptionType.Code);
                }
            }
            throw new CustomException("Hierarchy error: to the last", CustomExceptionType.Code);
        }
    }
}