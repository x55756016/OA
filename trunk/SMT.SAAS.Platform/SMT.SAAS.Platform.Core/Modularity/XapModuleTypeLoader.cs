using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Windows;
using System.Windows.Resources;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 加载解析子系统
    /// 此过程需要对子系统进行版本检测、更新、客户端存储、加密解密、XAP文件解析。
    /// 此类主要是针对XAP类型文件进行解析。不直接解析以rar承载的资源文件。
    /// </summary>
    [System.ComponentModel.Description("负责下载远程模块并将他们加载到当前应用程序域中。 Component responsible for downloading remote modules and load their $LS$$SL$$LE$Type$EL$ into the current application domain.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xap")]
    public class XapModuleTypeLoader : IModuleTypeLoader
    {
        private Dictionary<string, List<ModuleInfo>> downloadingModules = new Dictionary<string, List<ModuleInfo>>();
        private HashSet<string> downloadedUris = new HashSet<string>();
        private const string VERSION = ".Von";
        /// <summary>
        /// 模块数据包下载进度发生变更的时候触发的事件。
        /// </summary>
        public event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        private void RaiseModuleDownloadProgressChanged(ModuleDownloadProgressChangedEventArgs e)
        {
            if (this.ModuleDownloadProgressChanged != null)
            {
                this.ModuleDownloadProgressChanged(this, e);
            }
        }

        /// <summary>
        /// 模块加载完成或加载失败后触发此事件。
        /// </summary>
        public event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;

        private void RaiseLoadModuleCompleted(ModuleInfo moduleInfo, Exception error)
        {
            this.RaiseLoadModuleCompleted(new LoadModuleCompletedEventArgs(moduleInfo, error));
        }

        private void RaiseLoadModuleCompleted(LoadModuleCompletedEventArgs e)
        {
            if (this.LoadModuleCompleted != null)
            {
                this.LoadModuleCompleted(this, e);
            }
        }

        /// <summary>
        /// 验证当前需要加载的 <paramref name="moduleInfo"/>包含的地址是否可以下载。
        /// </summary>
        /// <param name="moduleInfo">
        /// 需要进行判断验证的子系统信息。
        /// </param>
        /// <returns>
        /// 若当前子系统的引用地址可以访问则返回<see langword="true"/>，否则返回 <see langword="false"/>。
        /// </returns>
        public bool CanLoadModuleType(ModuleInfo moduleInfo)
        {
            //if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");
            //if (!string.IsNullOrEmpty(moduleInfo.Ref))
            //{
            //    Uri uri;
            //    return Uri.TryCreate(moduleInfo.Ref, UriKind.RelativeOrAbsolute, out uri);
            //}

            return true;
        }

        /// <summary>
        /// 找到模块。
        /// Retrieves the <paramref name="moduleInfo"/>.
        /// </summary>
        /// <param name="moduleInfo">
        /// 模块需要具有加载类型。
        /// Module that should have it's type loaded.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is sent to completion event")]
        public void LoadModuleType(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new System.ArgumentNullException("moduleInfo");
            }

            try
            {
                //  Uri key = new Uri(moduleInfo.Ref, UriKind.RelativeOrAbsolute);
                string key = string.IsNullOrEmpty(moduleInfo.Ref) ? moduleInfo.FileName : moduleInfo.Ref;

                //检测模块是否包含需要下载文件，没有直接跳过。
                if (string.IsNullOrEmpty(key))
                {
                    this.RaiseLoadModuleCompleted(moduleInfo,null);

                    return;
                }

                #region 检测版本

                var localAppInfo = GetLocalVersion(string.Format("{0}/{1}", moduleInfo.FileName, moduleInfo.FileName + VERSION));
                if (moduleInfo.Version == localAppInfo.Version)
                {
                    XapAnalysis analysis = new XapAnalysis();
                    analysis.Analysis(GetLocalXapFile(moduleInfo.FileName));
                    this.RecordDownloadSuccess(key);
                    this.RaiseLoadModuleCompleted(moduleInfo, null);

                    Debug.WriteLine("Load Xap From Local : " + moduleInfo.FileName);
                }
                else
                {
                    #region 下载XAP文件.
                    //判断将要下载的子系统是否已经被下载过，
                    //若已经被下载，则触发子系统加载完成事件。
                    if (this.IsSuccessfullyDownloaded(key))
                    {
                        this.RaiseLoadModuleCompleted(moduleInfo, null);
                    }
                    else
                    {
                        //判断是否正处在下载中，下载中的URI将不提供下次处理程序。
                        bool needToStartDownload =!this.IsDownloading(key);
 
                        //记录子系统的信息以及其URI信息，标识为下载中。
                        //用于控制重复下载请求。
                        this.RecordDownloading(key, moduleInfo);

                        if (needToStartDownload)
                        {
                            //初始化一个下载器，下载器分为从WCF下载和WEB下载两种方式,其均实现了IFileDownloader接口
                            //若直接通过WEB下载则创建的类型为WebFileDownloader
                            //若需要通过WCF下载则创建类型为FileDownloader
                            //通过下载后对获取的文件流（基本为XAP）进行解析。
                            IFileDownloader downloader = this.CreateDownloader(moduleInfo.IsOnWeb);
                            downloader.DownloadProgressChanged += this.IFileDownloader_DownloadProgressChanged;
                            //重要的事件
                            downloader.DownloadCompleted += this.IFileDownloader_DownloadCompleted;
                            downloader.DownloadAsync(moduleInfo.FileName, moduleInfo.FileName);
                        }
                    }

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.RaiseLoadModuleCompleted(moduleInfo, ex);
            }
        }

        /// <summary>
        /// 创建一个<see cref="IFileDownloader"/>用于下载远程模块。
        /// Creates the <see cref="IFileDownloader"/> used to retrieve the remote modules.
        /// </summary>
        /// <returns>
        /// <see cref="IFileDownloader"/> 用于下载远程模块。
        /// The <see cref="IFileDownloader"/> used to retrieve the remote modules.
        /// </returns>
        protected virtual IFileDownloader CreateDownloader(bool isWeb)
        {
            if (isWeb)
                return new FileDownloader();
            else
                return new WCFFileDownloader();
        }

        void IFileDownloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // I ensure the download completed is on the UI thread so that types can be loaded into the application domain.
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(new Action<DownloadProgressChangedEventArgs>(this.HandleModuleDownloadProgressChanged), e);
            }
            else
            {
                this.HandleModuleDownloadProgressChanged(e);
            }
        }

        private void HandleModuleDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            string uri = (string)e.UserState;
            List<ModuleInfo> moduleInfos = this.GetDownloadingModules(uri);

            foreach (ModuleInfo moduleInfo in moduleInfos)
            {
                this.RaiseModuleDownloadProgressChanged(new ModuleDownloadProgressChangedEventArgs(moduleInfo, e.BytesReceived, e.TotalBytesToReceive));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void IFileDownloader_DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            // A new IFileDownloader instance is created for each download.
            // I unregister the event to allow for garbage collection.
            IFileDownloader fileDownloader = (IFileDownloader)sender;
            fileDownloader.DownloadProgressChanged -= this.IFileDownloader_DownloadProgressChanged;
            fileDownloader.DownloadCompleted -= this.IFileDownloader_DownloadCompleted;

            // I ensure the download completed is on the UI thread so that types can be loaded into the application domain.
            //确保下载完成处理是在UI线程中，这样便于类型加载到应用程序中。
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(new Action<DownloadCompletedEventArgs>(this.HandleModuleDownloaded), e);
            }
            else
            {
                this.HandleModuleDownloaded(e);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception sent to completion event")]
        private void HandleModuleDownloaded(DownloadCompletedEventArgs e)
        {
            string uri = (string)e.UserState;
            List<ModuleInfo> moduleInfos = this.GetDownloadingModules(uri);

            Exception error = e.Error;
            if (error == null)
            {
                try
                {
                    if (e.Result == null)
                        throw new Exception("ModuleDownloaded Failed");

                    //记录已经下载的模块
                    this.RecordDownloadComplete(uri);

                    Debug.Assert(!e.Cancelled, "Download should not be cancelled");

                    XapAnalysis analysis = new XapAnalysis();
                    analysis.Analysis(e.Result);

                    this.RecordDownloadSuccess(uri);

                    SaveLocalXapFile(e.Result, moduleInfos[0]);

                    Debug.WriteLine("Load Xap From WCF : " + moduleInfos[0].FileName);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    if (e.Result != null)
                        e.Result.Close();
                }
            }

            foreach (ModuleInfo moduleInfo in moduleInfos)
            {
                this.RaiseLoadModuleCompleted(moduleInfo, error);
            }
        }

        private bool IsDownloading(string uri)
        {
            lock (this.downloadingModules)
            {
                return this.downloadingModules.ContainsKey(uri);
            }
        }

        private void RecordDownloading(string uri, ModuleInfo moduleInfo)
        {
            lock (this.downloadingModules)
            {
                List<ModuleInfo> moduleInfos;
                if (!this.downloadingModules.TryGetValue(uri, out moduleInfos))
                {
                    moduleInfos = new List<ModuleInfo>();
                    this.downloadingModules.Add(uri, moduleInfos);
                }

                if (!moduleInfos.Contains(moduleInfo))
                {
                    moduleInfos.Add(moduleInfo);
                }
            }
        }

        private List<ModuleInfo> GetDownloadingModules(string uri)
        {
            lock (this.downloadingModules)
            {
                return new List<ModuleInfo>(this.downloadingModules[uri]);
            }
        }

        private void RecordDownloadComplete(string uri)
        {
            lock (this.downloadingModules)
            {
                if (!this.downloadingModules.ContainsKey(uri))
                {
                    this.downloadingModules.Remove(uri);
                }
            }
        }

        private bool IsSuccessfullyDownloaded(string uri)
        {
            lock (this.downloadedUris)
            {
                return this.downloadedUris.Contains(uri);
            }
        }

        private void RecordDownloadSuccess(string uri)
        {
            lock (this.downloadedUris)
            {
                this.downloadedUris.Add(uri);
            }
        }

        private static IEnumerable<AssemblyPart> GetParts(Stream stream)
        {
            List<AssemblyPart> assemblyParts = new List<AssemblyPart>();

            var streamReader = new StreamReader(Application.GetResourceStream(new StreamResourceInfo(stream, null), new Uri("AppManifest.xaml", UriKind.Relative)).Stream);
            using (XmlReader xmlReader = XmlReader.Create(streamReader))
            {
                xmlReader.MoveToContent();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Deployment.Parts")
                    {
                        using (XmlReader xmlReaderAssemblyParts = xmlReader.ReadSubtree())
                        {
                            while (xmlReaderAssemblyParts.Read())
                            {
                                if (xmlReaderAssemblyParts.NodeType == XmlNodeType.Element && xmlReaderAssemblyParts.Name == "AssemblyPart")
                                {
                                    AssemblyPart assemblyPart = new AssemblyPart();
                                    assemblyPart.Source = xmlReaderAssemblyParts.GetAttribute("Source");
                                    assemblyParts.Add(assemblyPart);
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return assemblyParts;
        }

        private static void LoadAssemblyFromStream(Stream sourceStream, AssemblyPart assemblyPart)
        {
            Stream assemblyStream = Application.GetResourceStream(new StreamResourceInfo(sourceStream, null),
                new Uri(assemblyPart.Source, UriKind.Relative)).Stream;

            assemblyPart.Load(assemblyStream);
        }

        /// <summary>
        /// 获取本地配置文件
        /// </summary>
        /// <returns></returns>
        private ModuleInfo GetLocalVersion(string filePath)
        {
            //获取本地的升级文件
            //  string filePath = string.Format("{0}/{1}", xapInfo.FileName, VersionFileName);
            if (IosManager.ExistsFile(filePath))
            {
                using (Stream localfilesm = new MemoryStream(IosManager.GetFileBytes(filePath)))
                {
                    XElement xmlClient = XElement.Load(System.Xml.XmlReader.Create(localfilesm));

                    ModuleInfo modeuleInfo = (from c in xmlClient.DescendantsAndSelf("ModuleInfo")
                                              select new ModuleInfo
                                           {
                                               ModuleName = c.Elements("ModuleName").SingleOrDefault().Value,
                                               Version = c.Elements("Version").SingleOrDefault().Value,
                                               FileName = c.Elements("FileName").SingleOrDefault().Value,
                                               EnterAssembly = c.Elements("EnterAssembly").SingleOrDefault().Value,
                                               IsSave = c.Elements("IsSave").SingleOrDefault().Value,
                                               HostAddress = c.Elements("HostAddress").SingleOrDefault().Value,
                                               ServerID = c.Elements("ServerID").SingleOrDefault().Value,
                                               ClientID = c.Elements("ClientID").SingleOrDefault().Value,
                                               Description = c.Elements("Description").SingleOrDefault().Value,
                                           }).FirstOrDefault();
                    return modeuleInfo;
                }
            }
            else
            {
                return new ModuleInfo() { Version = "1.0.0.0000" };
            }
        }

        ///<summary>
        /// 获取本地的xap文件包
        /// </summary>
        /// <returns>xap文件流</returns>
        private Stream GetLocalXapFile(string fileName)
        {
            string filePath = string.Format("{0}/{1}", fileName, fileName);
            byte[] fileBytes = IosManager.GetFileBytes(filePath);
            if ((fileBytes != null) && (fileBytes.Length > 0))
            {
                return new MemoryStream(fileBytes, 0, fileBytes.Length);
            }
            return Stream.Null;
        }

        /// <summary>
        /// 存储xap文件到本地
        /// </summary>
        /// <param name="xapFileStream">xap文件流</param>
        /// <param name="moduleInfo">子系统信息</param>
        private void SaveLocalXapFile(Stream xapFileStream, ModuleInfo moduleInfo)
        {
            xapFileStream.Position = 0L;
            byte[] buffer = new byte[xapFileStream.Length];
            xapFileStream.Read(buffer, 0, buffer.Length);
            IosManager.CreateFile(moduleInfo.FileName, moduleInfo.FileName, buffer);

            SetLocalVersion(moduleInfo);
        }

        /// <summary>
        /// 存储xap版本配置文件到本地
        /// </summary>
        /// <param name="appInfo">文件信息</param>
        private void SetLocalVersion(ModuleInfo moduleInfo)
        {
            XElement moduleInfoXML = new XElement("ModuleInfo",
                new XElement("ModuleName", moduleInfo.ModuleName),
                new XElement("Version", moduleInfo.Version),
                new XElement("FileName", moduleInfo.FileName),
                new XElement("EnterAssembly", moduleInfo.EnterAssembly),
                new XElement("IsSave", moduleInfo.IsSave),
                new XElement("HostAddress", moduleInfo.HostAddress),
                new XElement("ServerID", moduleInfo.ServerID),
                new XElement("ClientID", moduleInfo.ClientID),
                new XElement("Description", moduleInfo.Description)
                );


            IosManager.CreateFile(moduleInfo.FileName, moduleInfo.FileName + VERSION, moduleInfoXML);
        }
    }
}
