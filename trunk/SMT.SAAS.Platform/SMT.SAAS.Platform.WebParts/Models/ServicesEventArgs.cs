using System;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.WebParts.ClientServices
{
    /// <summary>
    /// 获取实体完成事件参数,参数中包含了获取后的webpart集合
    /// </summary>
    public class GetEntityListEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        public GetEntityListEventArgs(ObservableCollection<T> result, Exception error)
        {
            this.Result = result;
            this.Error = error;
        }

        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        public GetEntityListEventArgs(ObservableCollection<T> result, Exception error,int pageCount):this(result,error)
        {
            PageCount = pageCount;
        }


        /// <summary>
        /// 返回的结果集
        /// </summary>
        public ObservableCollection<T> Result { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
    }


    /// <summary>
    /// 执行数据库操作完成事件
    /// </summary>
    public class ExectNoQueryEventArgs : EventArgs
    {
        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        public ExectNoQueryEventArgs(bool result, Exception error)
        {
            this.Result = result;
            this.Error = error;
        }
        /// <summary>
        /// 返回的结果集
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
    }
}
