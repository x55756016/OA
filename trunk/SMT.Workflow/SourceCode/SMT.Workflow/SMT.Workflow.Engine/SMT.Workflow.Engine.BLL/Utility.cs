/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Utility.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 14:42:39   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SMT.Workflow.Engine.IContract;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.Data;
using SMT.Workflow.Engine.DAL;
using SMT.Workflow.Engine.BLL.PersonnelService;
using SMT.Workflow.Common.DataAccess;

namespace SMT.Workflow.Engine.BLL
{
    public static class Utility
    {
        /// <summary>
        /// 通过岗位查找第一个人
        /// </summary>
        /// <param name="strPostID"></param>
        /// <returns></returns>
        public static string ReceiveUser(string strPostID)
        {
            PersonnelServiceClient HRClient = new PersonnelServiceClient();
            string[] Employees = HRClient.GetEmployeeIDsByPostID(strPostID);
            if (Employees != null && Employees.Count() > 0)
            {
                return Employees[0];
            }
            return string.Empty;
        }

        /// <summary>
        /// 调用WCF
        /// </summary>
        /// <param name="ContractType"></param>
        /// <param name="BindingName"></param>
        /// <param name="Endaddress"></param>
        /// <param name="FuncName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static object CallEventWCFService(string ContractType, string BindingName, string Endaddress, string FuncName, string strValue)
        {

            EndpointAddress endPoint = new EndpointAddress(Endaddress);//地址
            string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                                    "<System>" + "\r\n" +
                                    "{0}" +
                                    "</System>";
            if (ContractType.ToUpper() == "ENGINE")
            {
                IApplicationService contract = new ChannelFactory<IApplicationService>(WCfBindingName(BindingName), endPoint).CreateChannel();
                Type type = contract.GetType();
                MethodInfo methodInfo = type.GetMethod(FuncName);
                strValue = string.Format(XmlTemplete, strValue);
                string[] param = new string[1];
                param[0] = strValue;
                return methodInfo.Invoke(contract, param);
            }
            else
            {             
                using (ChannelFactory<IEventTriggerProcess> channel = new ChannelFactory<IEventTriggerProcess>(WCfBindingName(BindingName), endPoint))
                {
                    IEventTriggerProcess instance = channel.CreateChannel();
                    using (instance as IDisposable)
                    {
                        try
                        {
                            Type type = typeof(IEventTriggerProcess);
                            MethodInfo mi = type.GetMethod(FuncName);
                            strValue = string.Format(XmlTemplete, strValue);
                            string[] param = new string[1];
                            param[0] = strValue;
                            //Log.WriteLog("调用服务验证Endaddress:" + Endaddress + "strValue：" + strValue);
                            return mi.Invoke(instance, param);
                        }
                        catch (TimeoutException tm)
                        {
                            (instance as ICommunicationObject).Abort();
                            //Log.WriteLog("调用服务验证TimeoutException" + tm.StackTrace);
                            throw;
                        }
                        catch (CommunicationException com)
                        {
                            (instance as ICommunicationObject).Abort();
                            //Log.WriteLog("调用服务验证CommunicationException" + com.StackTrace);
                            throw;
                        }
                        catch (Exception vErr)
                        {
                            //Log.WriteLog("调用服务验证Exception" + vErr.StackTrace);
                            (instance as ICommunicationObject).Abort();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 流程调用的方法
        /// </summary>
        /// <param name="BindingName">bindingName</param>
        /// <param name="Endaddress">endaddress</param>
        /// <param name="FuncName">funcName</param>
        /// <param name="strValue">strValue</param>
        /// <returns>object</returns>
        public static object CallWCFService(string bindingName, string endaddress, string funcName, string strValue,ref string erroMesssage)
        {
            //绑定不同的协议未测试，讨论一下最好的方法，或者或者做一个CASE，让用户填好
            try
            {
                EndpointAddress endPoint = new EndpointAddress(endaddress);//地址
                IApplicationService contract = new ChannelFactory<IApplicationService>(WCfBindingName(bindingName), endPoint).CreateChannel();
                Type type = contract.GetType();
                MethodInfo methodInfo = type.GetMethod(funcName);
                string[] param = new string[1];
                param[0] = strValue;               
                return methodInfo.Invoke(contract, param);
            }
            catch (Exception e)
            {
                string cMessage = "\r\n 动态　调用第三方服务出错　命名空间：SMT.Workflow.Engine.BLL 类方法：CallWCFService（） WCF地址:" + endaddress + "方法名称:" +funcName + " " +
                                   "WCF协议:" + bindingName + "\r\n" +
                                   "参数：" + strValue + "\r\n";
                LogHelper.WriteLog(cMessage+e.ToString());
                erroMesssage += cMessage;
                return null;
                //throw new Exception("命名空间：SMT.Workflow.Engine.BLL 类方法：CallWCFService（）" + e.Message);  //暂时不抛出异常也能往下走             
            }

        }


        /// <summary>
        /// 绑定的协议
        /// </summary>
        /// <param name="strBindingName">strBindingName</param>
        /// <returns>Binding</returns>
        public static Binding WCfBindingName(string strBindingName)
        {
            switch (strBindingName.ToUpper())
            {
                case "WSHTTPBINDING":
                    WSHttpBinding wsbinding = new WSHttpBinding();
                    wsbinding.MaxReceivedMessageSize = 4048000;
                    wsbinding.MaxBufferPoolSize = 1048576;
                    wsbinding.ReaderQuotas.MaxStringContentLength = 4048000;
                    wsbinding.ReaderQuotas.MaxArrayLength = 4048000;
                    wsbinding.ReaderQuotas.MaxBytesPerRead = 4048000;
                    wsbinding.SendTimeout = new TimeSpan(0, 3, 0);
                    return wsbinding;
                case "WSDUALHTTPBINDING":
                    WSDualHttpBinding wsdbinding = new WSDualHttpBinding();
                    wsdbinding.MaxReceivedMessageSize = 4048000;
                    wsdbinding.MaxBufferPoolSize = 1048576;
                    wsdbinding.ReaderQuotas.MaxStringContentLength = 4048000;
                    wsdbinding.ReaderQuotas.MaxArrayLength = 4048000;
                    wsdbinding.ReaderQuotas.MaxBytesPerRead = 4048000;
                    wsdbinding.SendTimeout = new TimeSpan(0, 3, 0);
                    return wsdbinding;
                case "WSFEDERATIONHTTPBINDING":
                    WSFederationHttpBinding wsfe = new WSFederationHttpBinding();
                    wsfe.MaxReceivedMessageSize = 4048000;
                    wsfe.MaxBufferPoolSize = 1048576;
                    wsfe.ReaderQuotas.MaxStringContentLength = 4048000;
                    wsfe.ReaderQuotas.MaxArrayLength = 4048000;
                    wsfe.ReaderQuotas.MaxBytesPerRead = 4048000;
                    wsfe.SendTimeout = new TimeSpan(0, 3, 0);
                    return wsfe;
                case "NETTCPBINDING":
                    NetTcpBinding netTcpBinding = new NetTcpBinding();
                    netTcpBinding.MaxReceivedMessageSize = 4048000;
                    netTcpBinding.MaxBufferPoolSize = 1048576;
                    netTcpBinding.ReaderQuotas.MaxStringContentLength = 4048000;
                    return netTcpBinding;
                case "NETNAMEDPIPEBINDING":

                    NetNamedPipeBinding netNamePipeBinding = new NetNamedPipeBinding();
                    netNamePipeBinding.MaxReceivedMessageSize = 4048000;
                    netNamePipeBinding.MaxBufferPoolSize = 1048576;
                    netNamePipeBinding.ReaderQuotas.MaxStringContentLength = 4048000;
                    return netNamePipeBinding;
                case "NETMSMQBINDING":
                    return new NetMsmqBinding();
                case "NETPEERTCPBINDING":
                    return new NetPeerTcpBinding();
                case "CUSTOMBINDING":
                    BinaryMessageEncodingBindingElement encodingBindingElement = new BinaryMessageEncodingBindingElement();
                    encodingBindingElement.ReaderQuotas.MaxStringContentLength = 4048000;
                    HttpTransportBindingElement transportBindingElement = new HttpTransportBindingElement();                  
                    transportBindingElement.MaxReceivedMessageSize = 4048000;
                    transportBindingElement.MaxBufferSize = 4048000;
                    transportBindingElement.MaxBufferPoolSize = 4048000;
                    CustomBinding customBinding = new CustomBinding(encodingBindingElement, transportBindingElement);
                    customBinding.SendTimeout = new TimeSpan(0, 3, 0);
                    return customBinding;
                default:
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.MaxReceivedMessageSize = 4048000;
                    binding.MaxBufferSize = 4048000;
                    binding.MaxBufferPoolSize = 1048576;
                    binding.ReaderQuotas.MaxStringContentLength = 4048000;
                    binding.ReaderQuotas.MaxArrayLength = 4048000;
                    binding.ReaderQuotas.MaxBytesPerRead = 4048000;
                    binding.SendTimeout = new TimeSpan(0, 3, 0);
                    return binding;
            }
        }


        public static string GetColumnString(DataRow dr, string colname)
        {
            try
            {
                object value = GetDataRowValue(dr, colname);
                if (value == null)
                {
                    return string.Empty;
                }
                switch (value.GetType().ToString())
                {
                    case "System.String":
                        return (string)value;
                    case "System.Int64":
                        return ((long)value).ToString();
                    case "System.Decimal":
                        return ((decimal)value).ToString();
                    case "System.Int32":
                        return ((int)value).ToString();
                    case "System.Double":
                        return ((double)value).ToString();
                    case "System.Boolean":
                        return ((bool)value).ToString();
                    case "System.DateTime":
                        return ((DateTime)value).ToString();
                    default:
                        return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        private static object GetDataRowValue(DataRow dr, string colname)
        {
            if (dr[colname] == DBNull.Value)
            {
                switch ((dr[colname]).GetType().ToString())
                {
                    case "System.String":
                        return "";
                    case "System.Int64":
                        return 0;
                    case "System.Decimal":
                        return 0;
                    case "System.Int32":
                        return 0;
                    case "System.Double":
                        return 0;
                    case "System.Boolean":
                        return false;
                    case "System.DateTime":
                        return string.Empty;
                    default:
                        return string.Empty;
                }
            }
            return dr[colname];
        }
    }
}
