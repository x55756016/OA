using System;
using System.ComponentModel;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: �ṩģ�����ع����¼��Ĳ�����Ϣ
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// �ṩģ�����ع����¼��Ĳ�����Ϣ��
    /// </summary>
    public class ModuleDownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// ��ʼ��һ���µ�<see cref="ModuleDownloadProgressChangedEventArgs"/>ʵ����
        /// </summary>
        /// <param name="moduleInfo">ģ����Ϣ</param>
        /// <param name="bytesReceived">���յ������ݴ�С.</param>
        /// <param name="totalBytesToReceive">�����ݴ�С.</param>        
        public ModuleDownloadProgressChangedEventArgs(ModuleInfo moduleInfo, long bytesReceived, long totalBytesToReceive)
            : base(CalculateProgressPercentage(bytesReceived, totalBytesToReceive), null)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }

            this.ModuleInfo = moduleInfo;
            this.BytesReceived = bytesReceived;
            this.TotalBytesToReceive = totalBytesToReceive;
        }

        /// <summary>
        /// ��ȡģ����Ϣ
        /// </summary>
        /// <value>ģ����Ϣ.</value>
        public ModuleInfo ModuleInfo { get; private set; }

        /// <summary>
        /// ��ȡ���յ������ݴ�С��
        /// </summary>
        /// <value>���յ������ݴ�С��</value>
        public long BytesReceived { get; private set; }

        /// <summary>
        /// ��ȡģ�������ݴ�С��
        /// </summary>
        /// <value>ģ�������ݴ�С��</value>
        public long TotalBytesToReceive { get; private set; }
        

        private static int CalculateProgressPercentage(long bytesReceived, long totalBytesToReceive)
        {
            if ((bytesReceived == 0L) || (totalBytesToReceive == 0L) || (totalBytesToReceive == -1L))
            {
                return 0;
            }

            return (int)((bytesReceived * 100L) / totalBytesToReceive);

        }
    }
}
