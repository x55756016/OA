using System;
using System.Net;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 主要是实现了 IFileDownloader 接口，封装了对 WebClient 类的使用。 
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 
    /// 定义该类，用于从网络上下载文件。<br/>
    /// </summary>
    /// <remarks>
    /// 主要是实现了<see cref="IFileDownloader"/>接口，封装了对<see cref="WebClient"/> 类的使用。<br/>
    /// </remarks>
    public class FileDownloader : IFileDownloader
    {
        private readonly WebClient webClient = new WebClient();

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
                    this.webClient.DownloadProgressChanged += this.WebClient_DownloadProgressChanged;
                }

                this._downloadProgressChanged += value;
            }

            remove
            {
                this._downloadProgressChanged -= value;
                if (this._downloadProgressChanged == null)
                {
                    this.webClient.DownloadProgressChanged -= this.WebClient_DownloadProgressChanged;
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
                    this.webClient.OpenReadCompleted += this.WebClient_OpenReadCompleted;
                }

                this._downloadCompleted += value;
            }

            remove
            {
                this._downloadCompleted -= value;
                if (this._downloadCompleted == null)
                {
                    this.webClient.OpenReadCompleted -= this.WebClient_OpenReadCompleted;
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
            this.webClient.OpenReadAsync(uri, userToken);
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
            Uri uri = new Uri(fileName, UriKind.RelativeOrAbsolute);
            this.DownloadAsync(uri, userToken);
        }
        
        private static DownloadCompletedEventArgs ConvertArgs(OpenReadCompletedEventArgs args)
        {
            return new DownloadCompletedEventArgs(args.Error == null ? args.Result : null, args.Error, args.Cancelled, args.UserState);
        }

        void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this._downloadProgressChanged(this, e);
        }

        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            this._downloadCompleted(this, ConvertArgs(e));
        }


    }
}
