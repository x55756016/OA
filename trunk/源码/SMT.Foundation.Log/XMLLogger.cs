using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Threading;

namespace SMT.Foundation.Log
{
    public class XMLLogger : ILogger
    {
        public XMLLogger()
        {

        }

        public delegate void AsyncWriteLogCaller(ErrorLog message, string filepath);

        public void Write(ErrorLog message)
        {
            string logpath = LogConfig.Instance.ErrorLogPath;
            //string filepath = logpath + "\\ErrorLog\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".xml";
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
            filepath += DateTime.Now.ToString("yyyy-MM-dd") + Thread.CurrentThread.ManagedThreadId.ToString() + ".xml";

            // 异步写
            AutoResetEvent manualEvent = new AutoResetEvent(false);
            AsyncWriteLogCaller caller = new AsyncWriteLogCaller(WriteLog);
            IAsyncResult result = caller.BeginInvoke(message, filepath, null, null);
            manualEvent.WaitOne(3000, false);
        }

        /// <summary>
        /// Retrieve ErrorLogs according to the datetime period and userid.
        /// </summary>
        /// <param name="dtfrom"></param>
        /// <param name="dtto"></param>
        /// <param name="strUserID"></param>
        /// <returns></returns>
        //public DataSet RetrieveErrorLogs(DateTime dtfrom, DateTime dtto, string strUserID)
        //{
        //    DataSet rds = new DataSet();

        //    int interval = DateTime.Compare(dtto, dtfrom);
        //    string path;
        //    for (int i = 0; i <= interval; i++)
        //    {
        //        path = LogConfig.Instance.ErrorLogPath + "ErrorLog" + dtfrom.AddDays(i).ToString("yyyy-MM-dd") + ".xml";
        //        DataSet ds = new DataSet();
        //        if (File.Exists(path))
        //        {
        //            ds.ReadXml(path);

        //            if (rds.Tables.Count == 0)
        //            {
        //                DataTable dt = new DataTable();
        //                dt = ds.Tables[0].Copy();
        //                dt.Clear();
        //                rds.Tables.Add(dt);
        //            }

        //            DataRow[] rows = ds.Tables[0].Select("LOG_LoginUser='" + strUserID + "'");
        //            if (rows.Length > 0)
        //            {
        //                foreach (DataRow row in rows)
        //                {
        //                    DataRow newrow = rds.Tables[0].NewRow();
        //                    for (int j = 0; j < rds.Tables[0].Columns.Count; j++)
        //                    {
        //                        newrow[j] = row[j];
        //                    }
        //                    rds.Tables[0].Rows.Add(newrow);
        //                }
        //            }
        //            ds.Dispose();

        //        }
        //    }
        //    return rds;
        //}

        ///// <summary>
        ///// Retrieve a ErrorLog according to the ErrorLogID
        ///// </summary>
        ///// <param name="ErrorLogID"></param>
        ///// <returns></returns>
        //public ErrorLog RetrieveErrorLogById(System.Guid ErrorLogID)
        //{
        //    string[] files = Directory.GetFiles(LogConfig.Instance.ErrorLogPath, "ErrorLog*.xml", SearchOption.TopDirectoryOnly);
        //    XmlDocument doc = new XmlDocument();

        //    ErrorLog log = new ErrorLog();
        //    foreach (string file in files)
        //    {
        //        doc.Load(file);
        //        XmlElement root = doc.DocumentElement;
        //        foreach (XmlNode node in root.ChildNodes)
        //        {
        //            if (node.FirstChild.InnerText == ErrorLogID.ToString() && node.ChildNodes.Count == 19)
        //            {
        //                log.ErrorLogID = ErrorLogID.ToString();
        //                log.AWBVersion = node.ChildNodes[1].InnerText;
        //                log.AWBAssembliesVersion = node.ChildNodes[2].InnerText;
        //                log.ClientBrowser = node.ChildNodes[3].InnerText;
        //                log.ClientBrowserLanguage = node.ChildNodes[4].InnerText;
        //                log.ClientJavaScriptSupport = node.ChildNodes[5].InnerText=="Yes"?true:false;
        //                log.ClientNetRuntime = node.ChildNodes[6].InnerText;
        //                log.ClientOS = node.ChildNodes[7].InnerText;
        //                log.ClientOSLanguage = node.ChildNodes[8].InnerText;
        //                log.ServerOS = node.ChildNodes[9].InnerText;
        //                log.ServerNetRuntime = node.ChildNodes[10].InnerText;
        //                log.ComputerName = node.ChildNodes[11].InnerText;
        //                log.ErrorExecutionTrace = node.ChildNodes[12].InnerText;
        //                log.ErrorMessage = node.ChildNodes[13].InnerText;
        //                log.ErrorStackTrace = node.ChildNodes[14].InnerText;
        //                log.ErrorURL = node.ChildNodes[15].InnerText;
        //                log.LoginUser = node.ChildNodes[16].InnerText;
        //                log.OrganizationID = node.ChildNodes[17].InnerText;
        //                log.CreatedOn = node.ChildNodes[18].InnerText;
        //            }
        //            else
        //            {
        //                continue;
        //            }
        //        }
        //    }
        //    return log;
        //}

        ///// <summary>
        ///// Delete a ErrorLog according to the Errorlog ID
        ///// </summary>
        ///// <param name="strErrorLogID"></param>
        //public void DeleteErrorLog(Guid ErrorLogID)
        //{
        //    string[] files = Directory.GetFiles(LogConfig.Instance.ErrorLogPath, "ErrorLog*.xml", SearchOption.TopDirectoryOnly);
        //    XmlDocument doc = new XmlDocument();

        //    foreach (string file in files)
        //    {
        //        doc.Load(file);
        //        XmlElement root = doc.DocumentElement;
        //        foreach (XmlNode node in root.ChildNodes)
        //        {
        //            if (node.FirstChild.InnerText == ErrorLogID.ToString())
        //            {
        //                root.RemoveChild(node);
        //                doc.Save(file);
        //                return;
        //            }
        //            else
        //            {
        //                continue;
        //            }
        //        }
        //    }
        //}


        private void WriteLog(ErrorLog message, string filepath)
        {
            if (File.Exists(filepath))
            {
                AppendXML(message, filepath);
            }
            else
            {
                CreateNewXML(message, filepath);
            }
        }

        private void CreateNewXML(ErrorLog message, string filepath)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("LOG_ErrorLogs");
            doc.AppendChild(root);
            XmlElement errorlog = doc.CreateElement("LOG_ErrorLog");
            root.AppendChild(errorlog);

            //add detail error log info
            XmlElement element1 = doc.CreateElement("LOG_ErrorLogID");
            element1.InnerText = message.ErrorLogID;
            errorlog.AppendChild(element1);

            XmlElement element2 = doc.CreateElement("LOG_CurrentTheadID");
            element2.InnerText = message.ThreadId;
            errorlog.AppendChild(element2);
            //XmlElement element2 = doc.CreateElement("LOG_AWBVersion");
            //element2.InnerText = message.AWBVersion;
            //errorlog.AppendChild(element2);

            //XmlElement element3 = doc.CreateElement("LOG_AWBAssembliesVersion");
            //element3.InnerText = message.AWBAssembliesVersion;
            //errorlog.AppendChild(element3);

            XmlElement element4 = doc.CreateElement("LOG_ClientBrowser");
            element4.InnerText = message.ClientBrowser;
            errorlog.AppendChild(element4);

            XmlElement element5 = doc.CreateElement("LOG_ClientBrowserLanguage");
            element5.InnerText = message.ClientBrowserLanguage;
            errorlog.AppendChild(element5);

            XmlElement element6 = doc.CreateElement("LOG_ClientJavaScriptSupport");
            element6.InnerText = message.ClientJavaScriptSupport == true ? "Yes" : "No";
            errorlog.AppendChild(element6);

            XmlElement element7 = doc.CreateElement("LOG_ClientNetRuntime");
            element7.InnerText = message.ClientNetRuntime;
            errorlog.AppendChild(element7);

            XmlElement element8 = doc.CreateElement("LOG_ClientOS");
            element8.InnerText = message.ClientOS;
            errorlog.AppendChild(element8);

            XmlElement element9 = doc.CreateElement("LOG_ClientOSLanguage");
            element9.InnerText = message.ClientOSLanguage;
            errorlog.AppendChild(element9);

            XmlElement element10 = doc.CreateElement("LOG_ServerOS");
            element10.InnerText = message.ServerOS;
            errorlog.AppendChild(element10);

            XmlElement element11 = doc.CreateElement("LOG_ServerNetRuntime");
            element11.InnerText = message.ServerNetRuntime;
            errorlog.AppendChild(element11);

            XmlElement element12 = doc.CreateElement("LOG_ComputerName");
            element12.InnerText = message.ComputerName;
            errorlog.AppendChild(element12);

            XmlElement element13 = doc.CreateElement("LOG_ErrorExecutionTrace");
            element13.InnerText = message.ErrorExecutionTrace;
            errorlog.AppendChild(element13);

            XmlElement element14 = doc.CreateElement("LOG_ErrorMessag");
            element14.InnerText = message.ErrorMessage;
            errorlog.AppendChild(element14);

            XmlElement element15 = doc.CreateElement("LOG_ErrorStackTrace");
            element15.InnerText = message.ErrorStackTrace;
            errorlog.AppendChild(element15);

            XmlElement element16 = doc.CreateElement("LOG_ErrorURL");
            element16.InnerText = message.ErrorURL;
            errorlog.AppendChild(element16);

            XmlElement element17 = doc.CreateElement("LOG_LoginUser");
            element17.InnerText = message.LoginUser;
            errorlog.AppendChild(element17);

            XmlElement element18 = doc.CreateElement("LOG_OrganizationID");
            element18.InnerText = message.OrganizationID;
            errorlog.AppendChild(element18);

            XmlElement element19 = doc.CreateElement("LOG_CreatedOn");
            element19.InnerText = message.CreatedOn;
            errorlog.AppendChild(element19);

            //add xml head
            XmlProcessingInstruction xpi = doc.CreateProcessingInstruction("xml", "version='1.0' encoding='utf-8'");
            doc.InsertBefore(xpi, doc.ChildNodes[0]);
            doc.Save(filepath);
        }

        private void AppendXML(ErrorLog message, string filepath)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(filepath);
            XmlElement root = doc.DocumentElement; // 获取根节点

            XmlElement errorlog = doc.CreateElement("LOG_ErrorLog");
            root.AppendChild(errorlog);

            XmlElement eid = doc.CreateElement("LOG_ErrorLogID");
            eid.InnerText = message.ErrorLogID;
            errorlog.AppendChild(eid);

            //XmlElement element2 = doc.CreateElement("LOG_AWBVersion");
            //element2.InnerText = message.AWBVersion;
            //errorlog.AppendChild(element2);

            //XmlElement element3 = doc.CreateElement("LOG_AWBAssembliesVersion");
            //element3.InnerText = message.AWBAssembliesVersion;
            //errorlog.AppendChild(element3);

            XmlElement element2 = doc.CreateElement("ClientHostName");
            element2.InnerText = message.ClientHostName;
            errorlog.AppendChild(element2);

            XmlElement element3 = doc.CreateElement("ClientHostAddress");
            element3.InnerText = message.ClientHostAddress;
            errorlog.AppendChild(element3);


            XmlElement element4 = doc.CreateElement("LOG_ClientBrowser");
            element4.InnerText = message.ClientBrowser;
            errorlog.AppendChild(element4);

            XmlElement element5 = doc.CreateElement("LOG_ClientBrowserLanguage");
            element5.InnerText = message.ClientBrowserLanguage;
            errorlog.AppendChild(element5);

            XmlElement element6 = doc.CreateElement("LOG_ClientJavaScriptSupport");
            element6.InnerText = message.ClientJavaScriptSupport == true ? "Yes" : "No";
            errorlog.AppendChild(element6);

            XmlElement element7 = doc.CreateElement("LOG_ClientNetRuntime");
            element7.InnerText = message.ClientNetRuntime;
            errorlog.AppendChild(element7);

            XmlElement element8 = doc.CreateElement("LOG_ClientOS");
            element8.InnerText = message.ClientOS;
            errorlog.AppendChild(element8);

            XmlElement element9 = doc.CreateElement("LOG_ClientOSLanguage");
            element9.InnerText = message.ClientOSLanguage;
            errorlog.AppendChild(element9);

            XmlElement element10 = doc.CreateElement("LOG_ServerOS");
            element10.InnerText = message.ServerOS;
            errorlog.AppendChild(element10);

            XmlElement element11 = doc.CreateElement("LOG_ServerNetRuntime");
            element11.InnerText = message.ServerNetRuntime;
            errorlog.AppendChild(element11);

            XmlElement element12 = doc.CreateElement("LOG_ComputerName");
            element12.InnerText = message.ComputerName;
            errorlog.AppendChild(element12);

            XmlElement element13 = doc.CreateElement("LOG_ErrorExecutionTrace");
            element13.InnerText = message.ErrorExecutionTrace;
            errorlog.AppendChild(element13);

            XmlElement element14 = doc.CreateElement("LOG_ErrorMessag");
            element14.InnerText = message.ErrorMessage;
            errorlog.AppendChild(element14);

            XmlElement element15 = doc.CreateElement("LOG_ErrorStackTrace");
            element15.InnerText = message.ErrorStackTrace;
            errorlog.AppendChild(element15);

            XmlElement element16 = doc.CreateElement("LOG_ErrorURL");
            element16.InnerText = message.ErrorURL;
            errorlog.AppendChild(element16);

            XmlElement element17 = doc.CreateElement("LOG_LoginUser");
            element17.InnerText = message.LoginUser;
            errorlog.AppendChild(element17);

            XmlElement element18 = doc.CreateElement("LOG_OrganizationID");
            element18.InnerText = message.OrganizationID;
            errorlog.AppendChild(element18);

            XmlElement element19 = doc.CreateElement("LOG_CreatedOn");
            element19.InnerText = message.CreatedOn;
            errorlog.AppendChild(element19);

            root.AppendChild(errorlog);
            doc.Save(filepath);
        }

    }
}
