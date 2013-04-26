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
    /// ���ؽ�����ϵͳ
    /// �˹�����Ҫ����ϵͳ���а汾��⡢���¡��ͻ��˴洢�����ܽ��ܡ�XAP�ļ�������
    /// ������Ҫ�����XAP�����ļ����н�������ֱ�ӽ�����rar���ص���Դ�ļ���
    /// </summary>
    [System.ComponentModel.Description("��������Զ��ģ�鲢�����Ǽ��ص���ǰӦ�ó������С� Component responsible for downloading remote modules and load their $LS$$SL$$LE$Type$EL$ into the current application domain.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xap")]
    public class XapModuleTypeLoader : IModuleTypeLoader
    {
        private Dictionary<string, List<ModuleInfo>> downloadingModules = new Dictionary<string, List<ModuleInfo>>();
        private HashSet<string> downloadedUris = new HashSet<string>();
        private const string VERSION = ".Von";
        /// <summary>
        /// ģ�����ݰ����ؽ��ȷ��������ʱ�򴥷����¼���
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
        /// ģ�������ɻ����ʧ�ܺ󴥷����¼���
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
        /// ��֤��ǰ��Ҫ���ص� <paramref name="moduleInfo"/>�����ĵ�ַ�Ƿ�������ء�
        /// </summary>
        /// <param name="moduleInfo">
        /// ��Ҫ�����ж���֤����ϵͳ��Ϣ��
        /// </param>
        /// <returns>
        /// ����ǰ��ϵͳ�����õ�ַ���Է����򷵻�<see langword="true"/>�����򷵻� <see langword="false"/>��
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
        /// �ҵ�ģ�顣
        /// Retrieves the <paramref name="moduleInfo"/>.
        /// </summary>
        /// <param name="moduleInfo">
        /// ģ����Ҫ���м������͡�
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

                //���ģ���Ƿ������Ҫ�����ļ���û��ֱ��������
                if (string.IsNullOrEmpty(key))
                {
                    this.RaiseLoadModuleCompleted(moduleInfo,null);

                    return;
                }

                #region ���汾

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
                    #region ����XAP�ļ�.
                    //�жϽ�Ҫ���ص���ϵͳ�Ƿ��Ѿ������ع���
                    //���Ѿ������أ��򴥷���ϵͳ��������¼���
                    if (this.IsSuccessfullyDownloaded(key))
                    {
                        this.RaiseLoadModuleCompleted(moduleInfo, null);
                    }
                    else
                    {
                        //�ж��Ƿ������������У������е�URI�����ṩ�´δ������
                        bool needToStartDownload =!this.IsDownloading(key);
 
                        //��¼��ϵͳ����Ϣ�Լ���URI��Ϣ����ʶΪ�����С�
                        //���ڿ����ظ���������
                        this.RecordDownloading(key, moduleInfo);

                        if (needToStartDownload)
                        {
                            //��ʼ��һ������������������Ϊ��WCF���غ�WEB�������ַ�ʽ,���ʵ����IFileDownloader�ӿ�
                            //��ֱ��ͨ��WEB�����򴴽�������ΪWebFileDownloader
                            //����Ҫͨ��WCF�����򴴽�����ΪFileDownloader
                            //ͨ�����غ�Ի�ȡ���ļ���������ΪXAP�����н�����
                            IFileDownloader downloader = this.CreateDownloader(moduleInfo.IsOnWeb);
                            downloader.DownloadProgressChanged += this.IFileDownloader_DownloadProgressChanged;
                            //��Ҫ���¼�
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
        /// ����һ��<see cref="IFileDownloader"/>��������Զ��ģ�顣
        /// Creates the <see cref="IFileDownloader"/> used to retrieve the remote modules.
        /// </summary>
        /// <returns>
        /// <see cref="IFileDownloader"/> ��������Զ��ģ�顣
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
            //ȷ��������ɴ�������UI�߳��У������������ͼ��ص�Ӧ�ó����С�
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

                    //��¼�Ѿ����ص�ģ��
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
        /// ��ȡ���������ļ�
        /// </summary>
        /// <returns></returns>
        private ModuleInfo GetLocalVersion(string filePath)
        {
            //��ȡ���ص������ļ�
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
        /// ��ȡ���ص�xap�ļ���
        /// </summary>
        /// <returns>xap�ļ���</returns>
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
        /// �洢xap�ļ�������
        /// </summary>
        /// <param name="xapFileStream">xap�ļ���</param>
        /// <param name="moduleInfo">��ϵͳ��Ϣ</param>
        private void SaveLocalXapFile(Stream xapFileStream, ModuleInfo moduleInfo)
        {
            xapFileStream.Position = 0L;
            byte[] buffer = new byte[xapFileStream.Length];
            xapFileStream.Read(buffer, 0, buffer.Length);
            IosManager.CreateFile(moduleInfo.FileName, moduleInfo.FileName, buffer);

            SetLocalVersion(moduleInfo);
        }

        /// <summary>
        /// �洢xap�汾�����ļ�������
        /// </summary>
        /// <param name="appInfo">�ļ���Ϣ</param>
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
