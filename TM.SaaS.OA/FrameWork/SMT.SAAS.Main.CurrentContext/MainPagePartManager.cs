using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Resources;
using System.Xml.Linq;
using SMT.SAAS.Main.CurrentContext;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Threading;

namespace SMT.SAAS.Platform.Xamls.LoginPart
{
    public class MainPagePartManager
    {
        //private static List<string> alreadyLoadedAssemblysList;
        /// <summary>
        /// 加载dll完成事件
        /// </summary>
        public event EventHandler LoadDLLCompleted;
        /// <summary>
        /// 更新完成事件
        /// </summary>
        public event EventHandler UpdateDllCompleted;
        /// <summary>
        /// 更新过程消息通知
        /// </summary>
        public event EventHandler<NotifyUserMessageArgs> NotifyUserMessageEvent;
        /// <summary>
        /// 检测的版本文件名称
        /// </summary>
        public string VertionFileName = string.Empty;
        /// <summary>
        /// 本地保存下载的文件夹名称
        /// </summary>
        public string FilePath ="";
        /// <summary>
        /// xap包名称
        /// </summary>
        public string LoadXapName = "";


        private string dllVersionFilePath = string.Empty;
        /// <summary>
        /// 当前正在下载的xap,zip包名称
        /// </summary>
        private string downloadDllName;
        private WebClient webcDllVersion;
        private WebClient DownloadDllClinet;
        /// <summary>
        /// 需要下载的所有xap,zip包集合
        /// </summary>
        private List<string> needDownDllNames = new List<string>();
        #region 全局时间，用来记录操作时间
        DateTime dtstart = DateTime.Now;
        DateTime dtend = DateTime.Now;
        #endregion

        public bool isFirstUser;
        public bool isAutoloaded;
        public Assembly asmMain = null;
        
        #region backgroundWork
        private BackgroundWorker bkGroundWorker = new BackgroundWorker();
        SynchronizationContext syn;

        public void RunWorkerLoadAssemblyPart()
        {
            syn = SynchronizationContext.Current;
            if (bkGroundWorker.IsBusy != true)
            {
                bkGroundWorker.RunWorkerAsync();
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            syn.Post(LoadAssemblyPart, "");
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.LoadDLLCompleted != null)
            {
                LoadDLLCompleted(this, null);
            }
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }
        #endregion
        public MainPagePartManager()
        {
            //if (alreadyLoadedAssemblysList == null) alreadyLoadedAssemblysList = new List<string>();
            bkGroundWorker.WorkerReportsProgress = true;
            bkGroundWorker.WorkerSupportsCancellation = true;
            bkGroundWorker.DoWork += new DoWorkEventHandler(bw_DoWork);
            bkGroundWorker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bkGroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            if (!string.IsNullOrEmpty(FilePath))
            {
                FilePath = SMT.SAAS.Main.CurrentContext.Common.HostIP.Replace(":", "").Replace(".", "") + @"/" + FilePath;
            }
            else
            {
                FilePath = SMT.SAAS.Main.CurrentContext.Common.HostIP.Replace(":", "").Replace(".", "");
            }

        }

        #region 平台组件下载

        /// <summary>
        /// 组件下载更新检测，下载管理器入口
        /// </summary>
        public void dllVersionUpdataCheck()
        {
            dllVersionFilePath = FilePath + "/" + this.VertionFileName;
            string path = "";
            if (VertionFileName == string.Empty)
            {
                path = @"http://" + SMT.SAAS.Main.CurrentContext.Common.HostIP + @"/ClientBin/DllVersion.xml?dt=" + DateTime.Now.Millisecond;
            }else
            {
                path = @"http://" + SMT.SAAS.Main.CurrentContext.Common.HostIP + @"/ClientBin/" + dllVersionFilePath + "?dt=" + DateTime.Now.Millisecond;
            }
            webcDllVersion = new WebClient();
            webcDllVersion.OpenReadCompleted += new OpenReadCompletedEventHandler(webcDllVersion_OpenReadCompleted);
            webcDllVersion.OpenReadAsync(new Uri(path, UriKind.Absolute));
        }

        /// <summary>
        /// 组件下载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webcDllVersion_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("下载版本文件" + this.VertionFileName + "出错，请联系管理员" + e.Error.ToString());
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                    return;
                }
                if (e.Result.Length < 1)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("获取服务器更新列表为空，请联系管理员");
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                    NotifyMsg("获取服务器更新列表出错，请联系管理员");
                    return;
                }
                NotifyMsg("获取服务器更新列表成功，正在获取更新......");
                //将读取服务器下载的Dll Version XML            
                StreamResourceInfo XapSri = new StreamResourceInfo(e.Result, "text/xml");
                Stream manifestStream = XapSri.Stream;
                string strVersionXmlServer = new StreamReader(manifestStream).ReadToEnd();
                //Linq to xml   manifestStream
                XElement dllVersionXElementServer = XDocument.Parse(strVersionXmlServer).Root;
                var dllVersionXElementlistServer = (from ent in dllVersionXElementServer.Elements().Elements()
                                                    select ent).ToList();

                //本地存在dllversion.xml文件
                if (IosManager.ExistsFile(dllVersionFilePath))
                {
                    //比较版本并下载新版本的dll及zip文件
                    IsolatedStorageFileStream xmlIsoStreamLocal = IosManager.GetFileStream(dllVersionFilePath);
                    StreamResourceInfo XapSrilocal = new StreamResourceInfo(xmlIsoStreamLocal, "text/xml");
                    Stream versionXmlStreamLocal = XapSrilocal.Stream;
                    string versionXmlLocal = new StreamReader(versionXmlStreamLocal).ReadToEnd();
                    xmlIsoStreamLocal.Close();
                    if (string.IsNullOrEmpty(versionXmlLocal))
                    {
                        needDownDllNames = (from ent in dllVersionXElementlistServer
                                            select ent.Attribute("Source").Value).ToList();

                        DownLoadDll(needDownDllNames);
                        return;
                    }
                    XElement deploymentRoot = XDocument.Parse(versionXmlLocal).Root;
                    var dllVersionglistlocal = (from assemblyParts in deploymentRoot.Elements().Elements()
                                                select assemblyParts).ToList();

                    var needUpdataDlls = from a in dllVersionXElementlistServer
                                         join b in dllVersionglistlocal
                                         on a.Attribute("Source").Value equals b.Attribute("Source").Value
                                         where a.Attribute("version").Value != b.Attribute("version").Value
                                         select a;
                    if (needUpdataDlls.Count() > 0)
                    {
                        NotifyMsg("系统检测到更新，正在更新本地程序，请稍等......");
                        needDownDllNames = (from ent in needUpdataDlls
                                            select ent.Attribute("Source").Value).ToList();

                        DownLoadDll(needDownDllNames);
                    }
                    else
                    {
                        //判断是否所有dll都在本地存在，处理那种下载途中断网后的问题
                        List<string> needDownload = new List<string>();
                        foreach (var q in dllVersionglistlocal)
                        {
                            string filepath = FilePath + @"/" + q.Attribute("Source").Value;
                            if (!IosManager.ExistsFile(filepath))
                            {
                                NotifyMsg(@"silverlight本地存储异常，请右键点击silverlight
                                ，选择应用程序存储，然后点击全部删除后刷新页面再试");
                                needDownload.Add(q.Attribute("Source").Value);
                                break;
                            }
                        }
                        if (needDownload.Count() > 0)
                        {
                            //DownLoadDll(needDownload);
                        }
                        else
                        {
                            if (UpdateDllCompleted != null)
                            {
                                UpdateDllCompleted(this, null);
                            }
                        }
                    }
                }
                else
                {
                    //从服务器下载所有包
                    needDownDllNames = (from ent in dllVersionXElementlistServer
                                        select ent.Attribute("Source").Value).ToList();

                    DownLoadDll(needDownDllNames);
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("更新系统出错，请联系管理员：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                NotifyMsg("系统更新错误，请联系管理员");
            }
            finally
            {
                try
                {
                    //存储versxml到本地
                    byte[] streambyte = new byte[e.Result.Length];
                    e.Result.Seek(0, SeekOrigin.Begin);
                    e.Result.Read(streambyte, 0, streambyte.Length);
                    e.Result.Close();
                    IosManager.CreateFile(FilePath, VertionFileName, streambyte);
                }
                catch (Exception ex)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.logAndShow(ex.ToString());
                }
            }


        }

        /// <summary>
        /// 下载组件
        /// </summary>
        /// <param name="dllXElements"></param>
        private void DownLoadDll(List<string> dllXElements)
        {

            if (dllXElements.Count < 1)
            {
                if (UpdateDllCompleted != null)
                {
                    UpdateDllCompleted(this, null);
                }      
                return;
            }
            downloadDllName = dllXElements.FirstOrDefault();
            string path = @"http://" + SMT.SAAS.Main.CurrentContext.Common.HostIP + @"/ClientBin/" +FilePath+@"/"+ downloadDllName + "?dt=" + DateTime.Now.Millisecond;

            //SMT.SAAS.Main.CurrentContext.AppContext.logAndShow("正在下载更新：" + path);
            DownloadDllClinet = new WebClient();
            DownloadDllClinet.OpenReadCompleted += new OpenReadCompletedEventHandler(webcDownloadDll_OpenReadCompleted);
            DownloadDllClinet.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadDllClinet_DownloadProgressChanged);
            DownloadDllClinet.OpenReadAsync(new Uri(path, UriKind.Absolute));
        }


        void DownloadDllClinet_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int ProgressValue = e.ProgressPercentage;
            NotifyMsg("正在下载更新：" + downloadDllName + " " + ProgressValue + "%");
        }

        void webcDownloadDll_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("服务器下载" + downloadDllName + "出错，请联系管理员" + e.Error.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            try
            {
                #region 将下载的dll保存至本地
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("服务器下载" + downloadDllName + "完成");
                byte[] streambyte = new byte[e.Result.Length];
                e.Result.Read(streambyte, 0, streambyte.Length);
                e.Result.Close();
                IosManager.CreateFile(FilePath, downloadDllName, streambyte);

                #endregion

                needDownDllNames.Remove(downloadDllName);
                List<string> newDlllist = needDownDllNames;
                DownLoadDll(newDlllist);
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("下载更新出错：" + downloadDllName + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                NotifyMsg("系统更新错误，请联系管理员");
            }
        }

        #endregion
        
        #region "webpart Xap包加载"

        private void LoadAssemblyPart(Object state)
        {
            string XapName = FilePath + @"/" + LoadXapName;
            string DllSourceName = string.Empty;
            AssemblyPart asmPart = null;
            List<XElement> deploymentParts = new List<XElement>();
            try
            {
                #region "直接加载xap包中的dll"
                dtstart = DateTime.Now;
                using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
                {
                    //打开本地xap包流   

                    IsolatedStorageFileStream XapfileStream = store.OpenFile(XapName, FileMode.Open, FileAccess.Read);
                    if (XapfileStream.Length == 0)
                    {
                        //IosManager.DeletFile(ApplicationPath, "SMT.SAAS.Platform.Main.xap");
                        List<string> dllNames = new List<string>();
                        dllNames.Add(LoadXapName);
                        DownLoadDll(dllNames);
                    }

                    #region Original Code
                    StreamResourceInfo XapSri = new StreamResourceInfo(XapfileStream, "application/binary");
                    Stream manifestStream = Application.GetResourceStream(XapSri, new Uri("AppManifest.xaml", UriKind.Relative)).Stream;

                    string appManifest = new StreamReader(manifestStream).ReadToEnd();
                    manifestStream.Close();
                    //Linq to xml   
                    XElement deploymentRoot = XDocument.Parse(appManifest).Root;
                    deploymentParts = (from assemblyParts in deploymentRoot.Elements().Elements()
                                       select assemblyParts).ToList();
                    //检测所有包是否在本地，不在，就从服务器上下载
                    bool canStart = true;
                    List<string> dllDelete = new List<string>();
                    foreach (XElement xElement in deploymentParts)
                    {
                        if (xElement.Name.LocalName != "AssemblyPart" &&
                            xElement.Name.LocalName != "ExtensionPart") continue;
                        if (xElement.Attribute("Source").Value.Contains("zip")
                            && !IosManager.ExistsFile(FilePath + @"/" + xElement.Attribute("Source").Value))
                        {
                            dllDelete.Add(xElement.Attribute("Source").Value);
                            canStart = false;
                        }
                    }
                    if (!canStart)
                    {
                        DownLoadDll(dllDelete);
                        return;
                    }

                    StreamResourceInfo streamInfo;
                    //Assembly assemblyViewModel = null;
                    foreach (XElement xElement in deploymentParts)
                    {
                        try
                        {
                            if (xElement.Name.LocalName != "AssemblyPart" &&
                            xElement.Name.LocalName != "ExtensionPart") continue;
                            DllSourceName = xElement.Attribute("Source").Value;
                            if (DllSourceName.Contains("SMT.SaaS.FrameworkUI"))
                            {
                            }
                            dtstart = DateTime.Now;
                            //form.setLoadmingMessage( "正在加载：" + DllSourceName);
                            if (!DllSourceName.Contains("zip"))
                            {
                                //if (alreadyLoadedAssemblysList.Contains(DllSourceName)) continue;
                                //直接加载dll
                                asmPart = new AssemblyPart();
                                asmPart.Source = DllSourceName;
                                streamInfo = Application.GetResourceStream(new StreamResourceInfo(XapfileStream, "application/binary"), new Uri(DllSourceName, UriKind.Relative));
                                string xapshotName = LoadXapName.Replace(".xap", "");
                                string dllshotName= DllSourceName.Replace(".dll", "");
                                if (XapName.ToUpper().Contains("XAP") && xapshotName.ToUpper() == dllshotName.ToUpper())
                                {
                                    asmMain = asmPart.Load(streamInfo.Stream);
                                }
                                else
                                {
                                   var a = asmPart.Load(streamInfo.Stream);                                    
                                }
                                //alreadyLoadedAssemblysList.Add(DllSourceName);
                                streamInfo.Stream.Close();
                            }
                            else
                            {
                                //加载zip包                   
                                if (DllSourceName.Contains("zip"))
                                {
                                    string dllName = DllSourceName.Replace("zip", "dll");
                                    //if (alreadyLoadedAssemblysList.Contains(dllName)) continue;
                                    //打开本地zip包流                
                                    IsolatedStorageFileStream zipfileStream = IosManager.GetFileStream(FilePath + @"/" + DllSourceName);
                                    streamInfo = Application.GetResourceStream(new StreamResourceInfo(zipfileStream, "application/binary"), new Uri(DllSourceName.Replace("zip", "dll"), UriKind.Relative));
                                    asmPart = new AssemblyPart();
                                    asmPart.Source = DllSourceName.Replace("zip", "dll");
                                    var a = asmPart.Load(streamInfo.Stream);
                                    streamInfo.Stream.Close();
                                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("从Zip文件中加载程序集： " + a.FullName);
                                    //alreadyLoadedAssemblysList.Add(dllName);
                                }
                            }
                            dtend = DateTime.Now;
                            NotifyMsg("正在加载" + DllSourceName);
                            //string strmsg = "加载成功：" + DllSourceName + " 加载耗时： " + (dtend - dtstart).Milliseconds.ToString() + " 毫秒";
                            //SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
                        }
                        catch (Exception ex)
                        {
                            string strmsg = "加载失败：" + DllSourceName + " 错误信息： " + ex.ToString();
                            SMT.SAAS.Main.CurrentContext.AppContext.logAndShow(strmsg);
                            NotifyMsg("系统加载出错，请联系管理员"+System.Environment.NewLine+ex.ToString());
                            return;
                        }
                    }

                    #endregion
                }
                #endregion

                NotifyMsg("系统检测更新完毕,请您登录系统");
                          
            }
            catch (Exception ex)
            {
                NotifyMsg(@"silverlight本地存储异常，请右键点击silverlight
                                ，选择应用程序存储，然后点击全部删除后刷新页面再试");
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(DllSourceName + " 加载系统出错：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
        }

        #endregion
        
        #region 通知
        private void NotifyMsg(string msg)
        {
            if (NotifyUserMessageEvent != null)
            {
                NotifyUserMessageArgs args = new NotifyUserMessageArgs(msg);
                NotifyUserMessageEvent(this, args);
            }
        }
        #endregion

    }

    public class NotifyUserMessageArgs : EventArgs
    {
        /// <summary>
        /// 初始化<see cref="LoadModuleCompletedEventArgs"/>的新实例。
        /// </summary>
        /// <param name="moduleInfo">模块信息。</param>
        /// <param name="error">产生的异常。</param>
        public NotifyUserMessageArgs(string strMsg)
        {
            this.Message = strMsg;
        }
        public string Message { get; private set; }

    }
}
