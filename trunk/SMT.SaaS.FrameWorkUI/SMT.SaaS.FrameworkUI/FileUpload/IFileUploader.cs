using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SaaS.FrameworkUI.FileUpload
{
    /// <summary>
    /// 上传附件接口
    /// </summary>
    public interface IFileUploader
    {
        void StartUpload(string initParams);
        void CancelUpload();
        void Del_FileAndID();
        void Download();
        event EventHandler UploadFinished;
        /// <summary>
        ///  添加文件时 发现同名 则加后缀" _N" (0< n &&n<1000),相应的界面的文件名 也要更新，用此事件
        /// </summary>
        event EventHandler<FileUploadEventArgs> Event_Upload;
        event EventHandler<FileDownloadEventArgs> Event_Download;
    }

    public interface IFileLoadedCompleted
    {
        void FileLoadedCompleted();
    }
    public class FileUploadEventArgs : EventArgs
    {
        public string FileName { get; set; }
        /// <summary>
        /// 相对路径+文件名 (公司名/系统名/模块名/文件名)
        /// </summary>
        public string FileName_Path { get; set; }       
    }
    public class FileDownloadEventArgs : EventArgs
    {
        public byte[] byFile { get; set; }
        public string FileName { get; set; }
        public string FileName_sufix { get; set; }
    }
    /// <summary>
    /// 便于异步统计一次上传、下载的应该数量，和实际数量
    /// </summary>
    public class FileCountEventArgs : EventArgs
    {
        /// <summary>
        /// 将要统计的数量
        /// </summary>
        public int Counting;
        /// <summary>
        /// 实际统计的数量
        /// </summary>
        public int Counted;
    }

    public static class Constants
    {
        /// <summary>
        /// Possible States
        /// </summary>
        public enum FileStates
        {
            Pending = 0,
            Uploading = 1,
            Finished = 2,
            /// <summary>
            /// 客户端已物理删除。用自带的上传控件时用
            /// </summary>
            Deleted = 3, 
            /// <summary>
            /// 客户端逻辑删除 。隐藏 自带的上传控件时用
            /// </summary>
            Deleteing=4,
            /// <summary>
            /// file.ID==null就 清空 客户端
            /// </summary>
            Remove = 5,
            /// <summary>
            /// 数据加载
            /// </summary>
            DataLoaded=6,
            Error = 7,
            /// <summary>
            /// 服务端 已物理删除
            /// </summary>
            DeleteFinished=8,
            /// <summary>
            /// 浏览附件
            /// </summary>
            FileBrowse
           
        }
    }
}
