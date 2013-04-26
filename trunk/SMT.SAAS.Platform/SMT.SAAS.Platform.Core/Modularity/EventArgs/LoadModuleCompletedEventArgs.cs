using System;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: �ṩģ�������ɻ����ʧ�ܺ���¼�����
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// �ṩģ�������ɻ����ʧ�ܺ����Ϣ��
    /// </summary>
    public class LoadModuleCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// ��ʼ��<see cref="LoadModuleCompletedEventArgs"/>����ʵ����
        /// </summary>
        /// <param name="moduleInfo">ģ����Ϣ��</param>
        /// <param name="error">�������쳣��</param>
        public LoadModuleCompletedEventArgs(ModuleInfo moduleInfo, Exception error)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }

            this.ModuleInfo = moduleInfo;
            this.Error = error;
        }

        /// <summary>
        /// ��ȡģ����Ϣ��
        /// </summary>
        /// <value>The module info.</value>
        public ModuleInfo ModuleInfo { get; private set; }

        /// <summary>
        /// ��ȡ�������쳣��
        /// </summary>
        /// <value>
        /// �κο��ܲ������쳣����������ΪNULL��
        /// </value>
        public Exception Error { get; private set; }

        /// <summary>
        ///  ��ȡ������һ��ֵ����ֵָ�������Ƿ��Ѿ����¼����Ķ�����
        /// Gets or sets a value indicating whether the error has been handled by the event subscriber.
        /// </summary>
        /// <value>���¼��Ѿ�����Ϊ<c>true</c>; ����Ϊ<c>false</c>.</value>
        /// <remarks>
        /// ���¼��д����¼����Ķ���δ����ֵΪTrue����ô�¼��������󽫻�����쳣
        /// </remarks>
        public bool IsErrorHandled { get; set; }
    }
}
