using System;
using System.ComponentModel;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供模块下载过程事件的参数信息
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 提供模块下载过程事件的参数信息。
    /// </summary>
    public class ModuleDownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// 初始化一个新的<see cref="ModuleDownloadProgressChangedEventArgs"/>实例。
        /// </summary>
        /// <param name="moduleInfo">模块信息</param>
        /// <param name="bytesReceived">接收到的数据大小.</param>
        /// <param name="totalBytesToReceive">总数据大小.</param>        
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
        /// 获取模块信息
        /// </summary>
        /// <value>模块信息.</value>
        public ModuleInfo ModuleInfo { get; private set; }

        /// <summary>
        /// 获取接收到的数据大小。
        /// </summary>
        /// <value>接收到的数据大小。</value>
        public long BytesReceived { get; private set; }

        /// <summary>
        /// 获取模块总数据大小。
        /// </summary>
        /// <value>模块总数据大小。</value>
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
