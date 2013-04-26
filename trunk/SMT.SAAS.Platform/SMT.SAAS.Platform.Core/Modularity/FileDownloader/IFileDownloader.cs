using System;
using System.Net;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: Description
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ������Դ�ļ����ؽӿ�.
    /// </summary>
    public interface IFileDownloader
    {
        /// <summary>
        /// �ļ����ؽ��ȷ��ͱ��ʱ������
        /// ��ʹ��WebClientʵ�����ص�ʱ�򣬴˷�����Ҫʹ�á�
        /// </summary>
        event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// �ļ�������ɺ󴥷���
        /// </summary>
        event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        /// <summary>
        /// ���ݸ������ļ���ַ<paramref name="uri"/>����ʼ�����첽�����ļ���
        /// </summary>
        /// <param name="uri">
        /// �ļ���ַ��
        /// </param>
        /// <param name="userToken">
        /// �ṩ�첽������û�ָ���ı�ʶ����
        /// </param>
        void DownloadAsync(Uri uri, object userToken);

        /// <summary>
        /// ���ݸ������ļ����ƣ���ʼ�����첽�����ļ���
        /// </summary>
        /// <param name="fileName">
        /// �ļ����ơ�
        /// </param>
        /// <param name="userToken">
        /// �ṩ�첽������û�ָ���ı�ʶ����
        /// </param>
        void DownloadAsync(string fileName, object userToken);
    }
}