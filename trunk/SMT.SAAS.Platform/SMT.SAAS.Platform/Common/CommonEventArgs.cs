using System;
using SMT.SAAS.Platform.ViewModel.Menu;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 执行数据库操作完成事件参数
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform
{
    /// <summary>
    /// 执行数据库操作完成事件
    /// </summary>
    public class OnShortCutClickEventArgs : EventArgs
    {
        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        /// <param name="error">错误信息</param>
        public OnShortCutClickEventArgs(MenuViewModel result, Exception error)
        {
            this.Result = result;
            this.Error = error;
        }
        /// <summary>
        /// 返回的结果集
        /// </summary>
        public MenuViewModel Result { get; set; }

        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
    }
}
