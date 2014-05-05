using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Data;

namespace SMT.Foundation.Log
{
    public enum level
    {
        Warn,
        Debug,
        Error,
        Fatal,
        Info
    }
    public class TXTLogger : ILogger
    {
        StringBuilder sb = new StringBuilder();
        public TXTLogger()
        {

        }

        public void Write(ErrorLog message)
        {
            string logpath = LogConfig.Instance.ErrorLogPath;
            string filepath = string.Empty;
            if (string.IsNullOrEmpty(logpath))
            {
                filepath =AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                filepath = logpath;
            }
            filepath += @"/ErrorLog/";
            if (!Directory.Exists(filepath))    //如果文件夹不存在
            {
                DirectoryInfo dirinfo = System.IO.Directory.CreateDirectory(filepath);//创建文件夹
            }
            filepath += DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            //string filepath = Server.MapPath(@"..\ErrorLog\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            string strMessage = GetMessage(message);

            byte[] fileContent = Encoding.UTF8.GetBytes(strMessage);
            FileStream fs = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.None, 2048, true);

            try
            {
                AutoResetEvent manualEvent = new AutoResetEvent(false);
                IAsyncResult asyncResult = fs.BeginWrite(fileContent, 0, fileContent.Length,
                                                        new AsyncCallback(EndWriteCallback),
                                                        new WriteState(fs, manualEvent));
                manualEvent.WaitOne(3000, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }


        public void Write(string message, level levelEnum, AutoResetEvent myWriteResetEvent)
        {
            //string path = System.IO.Path.Combine(LogConfig.Instance.TracePath + @"/TraceLog", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            string logpath = LogConfig.Instance.ErrorLogPath;
            string filepath = string.Empty;
            if (string.IsNullOrEmpty(logpath))
            {
                filepath = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                filepath = logpath;
            }
            filepath += @"/ErrorLog/";
            if (!Directory.Exists(filepath))    //如果文件夹不存在
            {
                DirectoryInfo dirinfo = System.IO.Directory.CreateDirectory(filepath);//创建文件夹
            }
            filepath += DateTime.Now.ToString("yyyy-MM-dd") + ".txt";


            string strMessage = GetMessage(message, levelEnum);
            byte[] fileContent = Encoding.UTF8.GetBytes(strMessage);
            FileStream fs = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.None, 2048, true);
            try
            {
                //AutoResetEvent manualEvent = new AutoResetEvent(false);
                IAsyncResult asyncResult = fs.BeginWrite(fileContent, 0, fileContent.Length,
                                                        new AsyncCallback(EndWriteCallback),
                                                        new WriteState(fs, myWriteResetEvent));
                myWriteResetEvent.WaitOne();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }
        //public DataSet RetrieveErrorLogs(DateTime dtfrom, DateTime dtto, string strUserID)
        //{
        //    return null;
        //}

        //public ErrorLog RetrieveErrorLogById(System.Guid ErrorLogID)
        //{
        //    return null;
        //}

        //public void DeleteErrorLog(Guid ErrorLogID)
        //{

        //}



        // 异步写
        private static void EndWriteCallback(IAsyncResult asyncResult)
        {
            WriteState stateInfo = (WriteState)asyncResult.AsyncState;
            int workerThreads;
            int portThreads;
            try
            {
                ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
                stateInfo.fStream.EndWrite(asyncResult);
            }
            finally
            {
                stateInfo.autoEvent.Set();
            }
        }
        private string GetMessage(string message, level levelEnum)
        {
            sb.Append(levelEnum.ToString() + "  " + message + "  " + DateTime.Now.ToString() + "\r\n");
            return sb.ToString();
        }
        private string GetMessage(ErrorLog message)
        {

            sb.Append("============================================================================\r\n");
            sb.Append("Error Log\r\n");
            sb.Append("----------------------------------------------------------------------------\r\n");
            sb.Append("Error Log ID: " + message.ErrorLogID + "\r\n");
            sb.Append("Computer Name: " + message.ComputerName + "\r\n");
            sb.Append("Created On: " + message.CreatedOn + "\r\n\r\n");
            sb.Append("Login Information\r\n");
            sb.Append("----------------------------------------------------------------------------\r\n");
            sb.Append("Current Thead ID: " + message.ThreadId + "\r\n\r\n");
            sb.Append("Login User ID: " + message.LoginUser + "\r\n");
            sb.Append("Login User Name: " + message.LoginUser + "\r\n");
            sb.Append("Organization ID: " + message.OrganizationID + "\r\n\r\n");

            sb.Append("Exception Details\r\n");
            sb.Append("----------------------------------------------------------------------------\r\n");
            sb.Append("Error URL: " + message.ErrorURL + "\r\n");
            sb.Append("Error Message: " + message.ErrorMessage + "\r\n");
            sb.Append("Error Stack Trace: " + message.ErrorStackTrace + "\r\n\r\n");
            sb.Append("Execution Trace: " + message.ErrorExecutionTrace + "\r\n\r\n");

            sb.Append("Server Information\r\n");
            sb.Append("-----------------------------------------------------------------------------\r\n");
            sb.Append("Server OS:" + message.ServerOS + "\r\n");
            sb.Append("Server .Net runtime:" + message.ServerNetRuntime + "\r\n");
            //sb.Append("AWB Assemblies Version:" + message.AWBAssembliesVersion + "\r\n\r\n");

            sb.Append("Client Information\r\n");
            sb.Append("-----------------------------------------------------------------------------\r\n");
            sb.Append("Client OS:" + message.ClientOS + "\r\n");
            sb.Append("Client OS Lauguage:" + message.ClientOSLanguage + "\r\n");
            sb.Append("Client Browser:" + message.ClientBrowser + "\r\n");
            sb.Append("Client Browser Lauguage:" + message.ClientBrowserLanguage + "\r\n");
            sb.Append("Client .Net runtime:" + message.ClientNetRuntime + "\r\n");
            sb.Append("Client Support javascript:" + message.ClientJavaScriptSupport + "\r\n");
            sb.Append("============================================================================\r\n\r\n\r\n");
            // There should be no single '\n' or '\n\r' in the email message
            string strMessage = sb.ToString().Replace("\n\r", "\r\n");
            strMessage = strMessage.Replace("\n", "\r\n");
            strMessage = strMessage.Replace("\r\r\n", "\r\n");

            return strMessage;
        }
    }

    internal sealed class WriteState
    {
        /// <summary>
        /// 文件流
        /// </summary>
        public FileStream fStream;

        /// <summary>
        /// 事件
        /// </summary>
        public AutoResetEvent autoEvent;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fStream">文件流</param>
        /// <param name="autoEvent">事件</param>
        public WriteState(FileStream fStream, AutoResetEvent autoEvent)
        {
            this.fStream = fStream;
            this.autoEvent = autoEvent;
        }
    }

}
