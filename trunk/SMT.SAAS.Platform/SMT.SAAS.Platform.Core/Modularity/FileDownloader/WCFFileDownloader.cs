using System;
using System.Net;
using SMT.SAAS.Platform.Client;
using SMT.SAAS.Platform.Client.PlatformWS;
using SMT.SAAS.Platform.Client;


//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 文件下载，封装了通过WCF下载文件的方式。
    /// </summary>
    public class WCFFileDownloader : IFileDownloader
    {
        private readonly PlatformServicesClient wcfClient = BasicServices.PlatformClient;
        private event EventHandler<DownloadProgressChangedEventArgs> _downloadProgressChanged;
        private event EventHandler<DownloadCompletedEventArgs> _downloadCompleted;

        /// <summary>
        /// 文件下载进度发送变更时触发。
        /// </summary>
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged
        {
            add
            {
                if (this._downloadProgressChanged == null)
                {
                }

                this._downloadProgressChanged += value;
            }

            remove
            {
                this._downloadProgressChanged -= value;
                if (this._downloadProgressChanged == null)
                {
                }
            }
        }

        /// <summary>
        /// 文件下载完成后触发。
        /// </summary>
        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted
        {
            add
            {
                if (this._downloadCompleted == null)
                {
                    this.wcfClient.GetModuleFileStreamCompleted += WcfClient_GetModuleFileStreamCompleted;
                }

                this._downloadCompleted += value;
            }

            remove
            {
                this._downloadCompleted -= value;
                if (this._downloadCompleted == null)
                {
                    this.wcfClient.GetModuleFileStreamCompleted -= this.WcfClient_GetModuleFileStreamCompleted;
                }
            }
        }

        /// <summary>
        /// 根据给定的文件地址<paramref name="uri"/>，开始启动异步下载文件。
        /// </summary>
        /// <param name="uri">
        /// 文件地址。
        /// </param>
        /// <param name="userToken">
        /// 提供异步任务的用户指定的标识符。
        /// </param>
        public void DownloadAsync(Uri uri, object userToken)
        {
            this.DownloadAsync(uri.ToString(), userToken);
        }

        /// <summary>
        /// 根据给定的文件名称，开始启动异步下载文件。
        /// </summary>
        /// <param name="fileName">
        /// 文件名称。
        /// </param>
        /// <param name="userToken">
        /// 提供异步任务的用户指定的标识符。
        /// </param>
        public void DownloadAsync(string fileName, object userToken)
        {
            this.wcfClient.GetModuleFileStreamAsync(fileName, userToken);
        }

        private static DownloadCompletedEventArgs ConvertArgs(GetModuleFileStreamCompletedEventArgs args)
        {
            System.IO.Stream stream =null;
            if(args.Result!=null)
                stream = new System.IO.MemoryStream(args.Result);

            return new DownloadCompletedEventArgs(args.Error == null ? stream : null, args.Error, args.Cancelled, args.UserState);
        }

        void WcfClient_GetModuleFileStreamCompleted(object sender, GetModuleFileStreamCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this._downloadCompleted(this, ConvertArgs(e));
            }
        }
    }
}
