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

using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using System.Windows.Browser;
using SMT.Saas.Tools.FileUploadWS;
using System.Collections.ObjectModel;

namespace SMT.SaaS.FrameworkUI.FileUpload
{
    /// <summary>
    /// 上传附件通用类
    /// </summary>
    public class UserFile : INotifyPropertyChanged
    {
       
        private string _fileName;
        private string _fileName_path;
        private Stream _fileStream;
        private Constants.FileStates _state = Constants.FileStates.Pending;
        private double _bytesUploaded = 0;
        private double _fileSize = 0;
        private int _percentage = 0;
        private IFileUploader _fileUploader;
        private string _ID;
        public event EventHandler<FileDownloadEventArgs> Ev_Download;
        public bool IsBrowse = false; //是否为浏览
        public bool ISBROWSE { get { return IsBrowse; } set { IsBrowse = value; } }
        public UserFile() { }
        public void Upload(string initParams)
        {
            this.State = Constants.FileStates.Uploading;
            _fileUploader = new WcfFileUploader(this);
            _fileUploader.Event_Upload += new EventHandler<FileUploadEventArgs>(Event_Upload);
            _fileUploader.UploadFinished += new EventHandler(fileUploader_UploadFinished);
            _fileUploader.StartUpload(initParams);
        }

        public void Upload(string initParams, T_SYS_FILEUPLOAD FileUpload)
        {
            this.State = Constants.FileStates.Uploading;
            _fileUploader = new WcfFileUploader(this, FileUpload);
            _fileUploader.Event_Upload += new EventHandler<FileUploadEventArgs>(Event_Upload);
            _fileUploader.UploadFinished += new EventHandler(fileUploader_UploadFinished);
            _fileUploader.StartUpload(initParams);
        }
        /// <summary>
        /// 删除 服务器上的文件。操作完成后 根据_file删除相应的数据库记录
        /// </summary>
        public void CancelUpload()
        {
            if (_fileUploader != null && this.State == Constants.FileStates.Uploading)
                _fileUploader.CancelUpload();
        }
        /// <summary>
        /// 删除已上传的文件
        /// </summary>
        public void Del_FileAndID()
        {
            _fileUploader = new WcfFileUploader(this);
            if (_fileUploader != null)
            {
                _fileUploader.UploadFinished += new EventHandler(fileUploader_DeleteFinished);
                _fileUploader.Del_FileAndID();
            }
        }
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Download()
        {           
                _fileUploader = new WcfFileUploader(this);
                _fileUploader.Event_Download += new EventHandler<FileDownloadEventArgs>(Event_Download);
                _fileUploader.Download();           
        }
        private void Event_Download(object sender, FileDownloadEventArgs e)
        {
             FileDownloadEventArgs ev = new FileDownloadEventArgs();
            ev.byFile = e.byFile;
            Ev_Download(this, ev);
        }
        private void fileUploader_UploadFinished(object sender, EventArgs e)
        {
            _fileUploader = null;
            this.State = Constants.FileStates.Finished;
        }
        /// <summary>
        /// 删除执行操作 完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileUploader_DeleteFinished(object sender, EventArgs e)
        {
            _fileUploader = null;
            this.State = Constants.FileStates.DeleteFinished;
        }
        private void Event_Upload(object sender, FileUploadEventArgs e)
        {
            if (e.FileName != null)
            {
                this.FileName = e.FileName;
                this.FileName_Path = e.FileName_Path;
            }
        }
       
        #region INotifyPropertyChanged Members

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        //事件主要与数据绑定一起使用，以确保源数据的更新传递到绑定的 UI 属性。有关更多信息
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ---------属性--------

        public Dispatcher UIDispatcher { get; set; }
        public bool HttpUploader { get; set; }
        public string UploadHandlerName { get; set; }     
        /// <summary>
        /// 附件记录id
        /// </summary>
        public string ID { get; set; }

        [ScriptableMember()]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }
        [ScriptableMember()]
        public string FileName_Path
        {
            get { return _fileName_path; }
            set { _fileName_path = value; }
        }
        public Constants.FileStates State
        {
            get { return _state; }
            set {
                _state = value;
                NotifyPropertyChanged("State");
            }
        }
        /// <summary>
        /// 状态信息 
        /// </summary>
        [ScriptableMember()]
        public string StateString { get { return _state.ToString(); } }       
        public Stream FileStream
        {
            get { return _fileStream; }
            set
            {
                _fileStream = value;
                if (_fileStream != null)
                    _fileSize = _fileStream.Length;
            }
        }
        public double FileSize
        { get { return _fileSize; } }

        public double BytesUploaded
        {
            get { return _bytesUploaded; }
            set
            {
                _bytesUploaded = value;
                NotifyPropertyChanged("BytesUploaded");
                Percentage = (int)((value * 100) / FileSize);
            }
        }
        [ScriptableMember()]
        public int Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value;
                NotifyPropertyChanged("Percentage");
            }
        }
        public string ErrorMessage { get; set; }
      
        #endregion
    }
}
