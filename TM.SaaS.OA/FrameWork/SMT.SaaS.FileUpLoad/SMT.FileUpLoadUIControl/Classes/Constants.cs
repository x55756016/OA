using System;
namespace SMT.FileUpLoad
{
    public static class Constants
    {
        /// <summary>
        /// Possible States
        /// </summary>
        public enum FileStates
        {
            /// <summary>
            /// 选中
            /// </summary>
            Pending = 0,
            /// <summary>
            /// 上传中
            /// </summary>
            Uploading = 1,
            /// <summary>
            /// 完成
            /// </summary>
            Finished = 2,
            /// <summary>
            /// 删除
            /// </summary>
            Deleted = 3,
            /// <summary>
            /// 错误
            /// </summary>
            Error = 4,
            /// <summary>
            /// 移除
            /// </summary>
            Removed=5,
            /// <summary>
            /// 取消
            /// </summary>
            Cancel=6
        }



    }
    /// <summary>
    /// State Converter to CN
    /// </summary>
    public static class ConstantsCN
    {
        public static string CN(SMT.FileUpLoad.Constants.FileStates _state)
        {
            switch (_state)
            {
                case SMT.FileUpLoad.Constants.FileStates.Pending:
                    return "选中";
                case SMT.FileUpLoad.Constants.FileStates.Uploading:
                    return "上传中..";
                case SMT.FileUpLoad.Constants.FileStates.Finished:
                    return "完成";
                case SMT.FileUpLoad.Constants.FileStates.Error:
                    return "错误";
                case SMT.FileUpLoad.Constants.FileStates.Deleted:
                    return "删除";
                case SMT.FileUpLoad.Constants.FileStates.Removed:
                    return "移除";
                case SMT.FileUpLoad.Constants.FileStates.Cancel:
                    return "取消";
                default:
                    return "";
            }
        }
    }
}
