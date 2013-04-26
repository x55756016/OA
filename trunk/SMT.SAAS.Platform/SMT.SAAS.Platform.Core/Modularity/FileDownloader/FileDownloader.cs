using System;
using System.Net;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ��Ҫ��ʵ���� IFileDownloader �ӿڣ���װ�˶� WebClient ���ʹ�á� 
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 
    /// ������࣬���ڴ������������ļ���<br/>
    /// </summary>
    /// <remarks>
    /// ��Ҫ��ʵ����<see cref="IFileDownloader"/>�ӿڣ���װ�˶�<see cref="WebClient"/> ���ʹ�á�<br/>
    /// </remarks>
    public class FileDownloader : IFileDownloader
    {
        private readonly WebClient webClient = new WebClient();

        private event EventHandler<DownloadProgressChangedEventArgs> _downloadProgressChanged;
        private event EventHandler<DownloadCompletedEventArgs> _downloadCompleted;

        /// <summary>
        /// �ļ����ؽ��ȷ��ͱ��ʱ������
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
        /// �ļ�������ɺ󴥷���
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
        /// ���ݸ������ļ���ַ<paramref name="uri"/>����ʼ�����첽�����ļ���
        /// </summary>
        /// <param name="uri">
        /// �ļ���ַ��
        /// </param>
        /// <param name="userToken">
        /// �ṩ�첽������û�ָ���ı�ʶ����
        /// </param>
        public void DownloadAsync(Uri uri, object userToken)
        {
            this.webClient.OpenReadAsync(uri, userToken);
        }

        /// <summary>
        /// ���ݸ������ļ����ƣ���ʼ�����첽�����ļ���
        /// </summary>
        /// <param name="fileName">
        /// �ļ����ơ�
        /// </param>
        /// <param name="userToken">
        /// �ṩ�첽������û�ָ���ı�ʶ����
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
