using System;
using SMT.Saas.Tools.PermissionWS;

// 内容摘要: 执行数据库操作完成事件参数
  
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
        public OnShortCutClickEventArgs(V_UserMenuPermission result, Exception error)
        {
            this.Result = result;
            this.Error = error;
        }
        /// <summary>
        /// 返回的结果集
        /// </summary>
        public V_UserMenuPermission Result { get; set; }

        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
    }
}
