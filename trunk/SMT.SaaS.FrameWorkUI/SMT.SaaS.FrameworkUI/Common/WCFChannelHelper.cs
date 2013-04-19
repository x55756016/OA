using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace SMT.SaaS.FrameworkUI
{
    public static class WCFChannelHelper
    {
        /// <summary>
        /// 关闭Channel
        /// </summary>
        /// <param name="channel">channel</param>
        public static void DoClose(this ICommunicationObject channel)
        {
            if (channel == null)
                return;

            switch (channel.State)
            {
                case CommunicationState.Faulted:
                    try
                    {
                        //string msg = "WCFChannelHelper:CloseChannel Faulted 调用 channel.Abort";
                        //LogHelper.WriteDebug(msg);
                        channel.Abort();
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.WriteError(ex.Message, ex);
                    }
                    break;
                case CommunicationState.Closed:
                case CommunicationState.Closing:
                    break;
                default:
                    //Closing 指示通信对象正转换到 Closed 状态。 Closed 指示通信对象已关闭，且不再可用。 
                    try
                    {
                        channel.Close();

                        //string msg = "WCFChannelHelper:CloseChannel";
                        //LogHelper.WriteDebug(msg);
                    }
                    catch (CommunicationException ex)
                    {
                        //状态为 Faulted 的对象并没有关闭，可能仍在占用资源。应该使用 Abort 方法来关闭出错的对象。
                        // 如果对状态为 Faulted 的对象调用 Close，则会引发 CommunicationObjectFaultedException，这是因为没有正常关闭对象。
                        //string msg = "WCFChannelHelper.DoClose,调用channel.Abort " + ex.Message;
                        //LogHelper.WriteDebug(msg);
                        channel.Abort();
                    }
                    catch (TimeoutException ex)
                    {
                        //string msg = "WCFChannelHelper.DoClose,调用channel.Abort " + ex.Message;
                        //LogHelper.WriteDebug(msg);
                        channel.Abort();
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.WriteError("DoClose:" + ex.Message, ex);
                        channel.Abort();
                    }
                    break;
            }
        }
    }
}
