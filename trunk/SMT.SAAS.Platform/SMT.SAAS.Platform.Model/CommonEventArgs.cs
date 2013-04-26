using System;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Model
{
    /// <summary>
    /// 实体集合完成事件的参数,参数中包含了获取后的实体集合
    /// </summary>
    public class GetEntityListEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 根据参数，创建一个GetEntityListEventArgs的新实例。
        /// </summary>
        /// <param name="result">参数，实体集合</param>
        /// <param name="error">参数，执行过程中是否存在异常</param>
        public GetEntityListEventArgs(ObservableCollection<T> result, Exception error)
        {
            this.Result = result;
            this.Error = error;
        }

        /// <summary>
        /// 根据参数，创建一个GetEntityListEventArgs的新实例。
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        /// <param name="error">参数，执行过程中是否存在异常</param>
        public GetEntityListEventArgs(ObservableCollection<T> result, Exception error, int pageCount)
            : this(result, error)
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
    /// 获取实体完成事件参数。事件参数中包含了获取或的实体信息。
    /// </summary>
    public class GetEntityEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        public GetEntityEventArgs(T result, Exception error)
        {
            this.Result = result;
            this.Error = error;
        }

        /// <summary>
        /// 返回的结果集
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
    }

    /// <summary>
    /// 执行数据库操作完成事件
    /// </summary>
    public class ExecuteNoQueryEventArgs : EventArgs
    {
        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        public ExecuteNoQueryEventArgs(bool result, Exception error)
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

    /// <summary>
    /// 请求失败的事件参数。
    /// </summary>
    public class ExecuteFailedArgs : EventArgs
    {
        /// <summary>
        /// 获取WEBPART完成事件参数
        /// </summary>
        /// <param name="result">返回的webpart集合</param>
        public ExecuteFailedArgs(string message, Exception error)
        {
            this.Message = message;
            this.Error = error;
        }
        /// <summary>
        /// 返回的结果集
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 异常/错误信息
        /// </summary>
        public Exception Error { get; set; }
    }
}
