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
using System.ServiceModel;
//using mpost.SilverlightFramework;
//using System.Web.Hosting;
using System.IO;
using SMT.Saas.Tools.FileUploadWS;
using System.Collections.Generic;

namespace SMT.SaaS.FrameworkUI.FileUpload
{
    public class WcfFileUploader : IFileUploader
    {
        private UserFile _file;
        private long _dataLength;
        private long _dataSent;
        private FileUploadManagerClient _client;
        private string _initParams;
        private bool _firstChunk = true;
        private bool _lastChunk = false;
        public T_SYS_FILEUPLOAD _fileupload;
        //加载数据
        public delegate void DGets(List<T_SYS_FILEUPLOAD> lst);
        public event EventHandler UploadFinished;
        public event EventHandler<FileUploadEventArgs> Event_Upload;
        public event EventHandler<FileDownloadEventArgs> Event_Download;
        public DGets DGets_Completed;


        public WcfFileUploader()
        {
            Init_event();
        }
        public WcfFileUploader(UserFile file)
        {
            _file = file;
            if (file.FileStream != null) //删除时用
                _dataLength = file.FileStream.Length;
            Init_event();
        }
        public WcfFileUploader(UserFile file, T_SYS_FILEUPLOAD FileUpload)
        {
            _file = file;
            _fileupload = FileUpload;
            _dataLength = _file.FileStream.Length;
            Init_event();
            _client.AddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_AddCompleted);
        }
        //添加数据库记录
        void _client_AddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ChannelIsClosed();
            // throw new NotImplementedException();
        }
        private void Init_event()
        {
            _dataSent = 0;
            _client = new FileUploadManagerClient();
            _client.AddFileCompleted += new EventHandler<AddFileCompletedEventArgs>(AddFileCompleted);
            _client.CancelUploadCompleted += new EventHandler<CancelUploadCompletedEventArgs>(CancelUploadCompleted);
            _client.ChannelFactory.Closed += new EventHandler(ChannelFactory_Closed);
            _client.Get_ParentIDCompleted += new EventHandler<Get_ParentIDCompletedEventArgs>(Get_ParentIDCompleted);
            _client.DownloadCompleted += new EventHandler<DownloadCompletedEventArgs>(DownloadCompleted);

            _client.Del_FileAndIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(Del_FileAndIDCompleted);
        }

        #region IFileUploader Members

        /// <summary>
        ///开始上传文件
        /// </summary>
        /// <param name="initParams"></param>
        public void StartUpload(string initParams)
        {
            _initParams = initParams;
            StartUpload();
        }
        #endregion

        /// <summary>
        /// 开始上传文件。操作完成后 根据_file添加相应的数据库记录
        /// </summary>
        private void StartUpload()
        {
            int size = _file.FileStream.Length.ToInt32();
            byte[] buffer = new byte[size];
            int bytesRead = _file.FileStream.Read(buffer, 0, buffer.Length);
            //Are there any bytes left?
            if (bytesRead != 0)
            {
                _dataSent += bytesRead;
                if (_dataSent == _dataLength)
                    _lastChunk = true;
                //Upload this chunk
                _client.AddFileAsync(_file.FileName_Path, buffer, bytesRead, _initParams, _firstChunk, _lastChunk);
                //反复传
                //Always false after the first message
                _firstChunk = false;
                //关闭文件流
                _file.FileStream.Dispose();
                _file.FileStream.Close();
            }
            else
            {
                _file.FileStream.Dispose();
                _file.FileStream.Close();
                //Close
                _client.ChannelFactory.Close();
            }
        }
        //上传完成后，比较文件名,然后调用AddAsync添加数据库记录
        private void AddFileCompleted(object sender, AddFileCompletedEventArgs e)
        {
            //获取服务器端上传的文件名是否更改后的最新 文件名
            if (e.Result != null && e.Result.Length > 0)
            {
                if (_fileupload != null)
                {
                    if (e.Result != _fileupload.FILENAME)
                    {
                        _fileupload.FILENAME = e.Result;
                        FileUploadEventArgs ev = new FileUploadEventArgs();
                        int i = e.Result.LastIndexOf('\\');
                        ev.FileName = e.Result.Substring(i + 1);
                        ev.FileName_Path = e.Result;
                        Event_Upload(this, ev);
                    }
                    _client.AddAsync(_fileupload);
                }
            }
            else   //Check for webservice errors 
                _file.State = Constants.FileStates.Error;
            //Notify progress change
            OnProgressChanged();

        }


        /// <summary>
        ///取消正在上传 和删除 服务器上的文件
        /// </summary>
        public void CancelUpload()
        { _client.CancelUploadAsync(_file.FileName_Path); }
        //删除附件完成
        private void CancelUploadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _client.ChannelFactory.Close();
        }
        //删除附件,完成后删除数据库记录
        public void Del_FileAndID()
        {
            _client.Del_FileAndIDAsync(_file.FileName_Path, _file.ID);
        }

        private void Del_FileAndIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ChannelIsClosed();
            _client.ChannelFactory.Close();
        }
        public void Download()
        {
            if (_file.FileName_Path != null)
                _client.DownloadAsync(_file.FileName_Path);
        }
        public void DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            FileDownloadEventArgs ev = new FileDownloadEventArgs();
            ev.byFile = e.Result;
            Event_Download(this, ev);
        }








        private void OnProgressChanged()
        { _file.BytesUploaded = _dataSent; }
        private void ChannelFactory_Closed(object sender, EventArgs e)
        { ChannelIsClosed(); }

        private void ChannelIsClosed()
        {
            /// if (!_file.IsDeleted)
            if (UploadFinished != null)
                UploadFinished(this, null);
        }
        //获取数据库数据完成事件
        public void Get_ParentIDCompleted(object sender, Get_ParentIDCompletedEventArgs e)
        {

            if (e.Result != null && e.Result.Count > 0)
                //this.DGets_Completed(e.Result);
            //调用接口事件
            if (e.UserState != null)
            {
                ((IFileLoadedCompleted)e.UserState).FileLoadedCompleted();
            }
        }
        //获取数据库数据
        public void Get_ParentID(string formID)
        {
            _client.Get_ParentIDAsync(formID);
        }

        //获取数据库数据
        public void Get_ParentID(string formID, object objarg)
        {
            _client.Get_ParentIDAsync(formID, objarg);
        }

    }
}
