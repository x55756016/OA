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
using System.IO;
using System.Windows.Threading;
using SMT.FileUpLoad.Classes;
namespace SMT.FileUpLoad
{
    public class WcfFileUploader : IFileUploader
    {
       
        private UserFile _file;
        /// <summary>
        /// 文件大小的总量
        /// </summary>
        private long _dataLength;
        /// <summary>
        /// 已经上传的量
        /// </summary>
        private long _dataSent;
        private SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient _client;
        private SMT.Saas.Tools.NewFileUploadWS.UserFile model;
        private string _initParams;
        /// <summary>
        /// 第一块
        /// </summary>
        private bool _firstChunk = true;
        /// <summary>
        /// 最后一块
        /// </summary>
        private bool _lastChunk = false;
        private string SystemCode = string.Empty;
        private string ModelCode = string.Empty;
        private string Md5Name = string.Empty;
        private string ID = string.Empty;
        private string TempID = string.Empty;
        private string UserID = string.Empty;
        private Dispatcher Dispatcher;
        public event ProgressChangedEvent UploadProgressChanged;
        public WcfFileUploader(UserFile file)
        {
            #region 传送到服务器的实体         
            model = new SMT.Saas.Tools.NewFileUploadWS.UserFile();
            model.SmtFileListId = file.SmtFileListId;// this.SmtFileListId;//主键ID
            model.FileName = file.CustomFileName;//文件名
            model.FileType = file.FileType;//文件类型(.doc、.xls、.txt、.pdf......)
            model.FileUrl = file.FileUrl;//文件地址
            model.FileSize = file.FileSize;
            model.CompanyCode = file.CompanyCode;//公司代号
            model.CompanyName = file.CompanyName;//公司名字
            model.SystemCode = file.SystemCode;//系统代号
            model.ModelCode = file.ModelCode;//模块代号
            model.ApplicationID = file.ApplicationID;//业务ID
            model.ThumbnailUrl = file.ThumbnailUrl;//缩略图地址
            model.INDEXL = file.Indexl;//排序
            model.Remark = file.Remark;//备注
            model.CreateTime = file.CreateTime;//创建时间
            model.CreateName = file.CreateName;//创建人
            model.UpdateTime = file.UpdateTime;//修改时间
            model.UpdateName = file.UpdateName;//修改人
            model.SavePath = file.SavePath;//文件保存的目录
            //model.OWNERCOMPANYID = file.ow
            #endregion
            _file = file;
            
            UserID = file.UserID;
            _dataLength = _file.FileStream.Length;
            _dataSent = 0;//已经上传的大小,如果是续传,这里要得到上传文的已经上的大小
            SystemCode = file.SystemCode;
            ModelCode = file.ModelCode;
            Md5Name = file.MD5String;
            ID = file.ID;
            TempID = file.TempID;
            _client = new SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient();
            _client.SaveUpLoadFileCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.SaveUpLoadFileCompletedEventArgs>(_client_SaveUpLoadFileCompleted); //新加
           // _client.SaveUpLoadFileCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_SaveUpLoadFileCompleted);//原来
            _client.CancelUploadCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_CancelUploadCompleted);
            _client.CheckFileExistsCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.CheckFileExistsCompletedEventArgs>(_client_CheckFileExistsCompleted);
            _client.ChannelFactory.Closed += new EventHandler(ChannelFactory_Closed);
            _client.GetCompanyFileSetInfoCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.GetCompanyFileSetInfoCompletedEventArgs>(_client_GetCompanyFileSetInfoCompleted);
            _client.DeleteFileCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.DeleteFileCompletedEventArgs>(_client_DeleteFileCompleted);
            _client.SaveUpLoadFileIsExistCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.SaveUpLoadFileIsExistCompletedEventArgs>(_client_SaveUpLoadFileIsExistCompleted);
            
        }

        void _client_SaveUpLoadFileIsExistCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.SaveUpLoadFileIsExistCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageChildWindow messageWindow = new MessageChildWindow();
                // messageWindow.Message = "Maximum file size is: " + (MaxSize / 1024).ToString() + "KB.";
                messageWindow.Message = "上传已存在的文件时出现错误";
                messageWindow.Show();
            }
            else
            {
                if (e.Result == "EXITST")
                {
                    MessageChildWindow messageWindow = new MessageChildWindow();
                    
                    messageWindow.Message = "上传文件已经存在";
                    messageWindow.Show();
                }
                else
                {
                    _file.FileStream.Dispose();
                    _file.FileStream.Close();
                    _client.ChannelFactory.Close();
                }
            }
        }

        void _client_DeleteFileCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.DeleteFileCompletedEventArgs e)
        {
            
        }
        //WCF上传完成
        void _client_SaveUpLoadFileCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.SaveUpLoadFileCompletedEventArgs e)
        {
            //Notify progress change
            OnProgressChanged();
            if (e.Error != null)
            {
                _file.State = Constants.FileStates.Error;
            }
            else
            {
                _file .FileUrl= e.Result;//文件在服务器上的路径
                if (!_file.IsDeleted || _file.State != Constants.FileStates.Removed)
                    StartUpload();
            }
        }
       
        #region WCF完成事件
        void _client_CheckFileExistsCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.CheckFileExistsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                long lengthtemp = e.Result.BytesUploaded;                
                if (lengthtemp > 0)
                {
                    MessageBoxResult result;
                    if (lengthtemp > 0)
                    {
                        result = MessageBox.Show(e.Result.FileName + "文件部分存在, 如果是相同文件名上传请先做删除处理，是否继续上传?", "续传?", MessageBoxButton.OKCancel);

                        if (result == MessageBoxResult.Cancel)
                        {
                            lengthtemp = 0;
                            this._file.BytesUploaded = 0;
                            _client.DeleteFileAsync(e.Result.SmtFileListId, e.Result.FileUrl);
                            this._file.State = Constants.FileStates.Pending;
                            return;
                        }
                        else
                        {
                            
                            this._file.BytesUploaded = lengthtemp;
                            this._dataSent = lengthtemp;
                            int percent = (int)((this._file.BytesUploaded / this._file.FileSize) * 100);
                            _file.FileStream.Seek(lengthtemp, System.IO.SeekOrigin.Begin);
                            UploadProgressChangedEventArgs args = new UploadProgressChangedEventArgs(percent, lengthtemp, lengthtemp, this._file.FileSize, _file.FileName);
                            //this.Dispatcher.BeginInvoke(delegate()
                            //{
                            //    UploadProgressChanged(this, args);
                            //});
                            OnProgressChanged();

                        }
                    }
                }
                
                StartUpload();
            }
        }
        /// <summary>
        /// 上传完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _client_SaveUpLoadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //Notify progress change
            OnProgressChanged();
            if (e.Error != null)
            {
                _file.State = Constants.FileStates.Error;
            }
            else
            {
                if (!_file.IsDeleted||_file.State!=Constants.FileStates.Removed)
                    StartUpload();
            }
        }
        private void _client_CancelUploadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _client.ChannelFactory.Close();
        }
        #endregion
        #region IFileUploader Members
        /// <summary>
        /// 检查文件
        /// </summary>
        public void CheckFileExists()
        {
            _client.CheckFileExistsAsync(model, Md5Name);
        }
        /// <summary>
        /// 开始上传
        /// </summary>
        /// <param name="initParams"></param>
        public void StartUpload(string initParams)
        {
            _initParams = initParams;

           // StartUpload();
            CheckFileExists();
        }

        /// <summary>
        /// 取消上传
        /// </summary>
        public void CancelUpload()
        {
            try
            {
                _client = null;
                //_client.ChannelFactory.Close();
            }
            catch (Exception ex)
            {
                string sr = ex.Message;
            }
                //_client.CancelUploadAsync(_file.FileName);
        }
        #region 获取公司对上传文件的设置信息
      
        //获取公司对上传文件的设置信息
        public void GetCompanyFileSetInfo(string companycode)
        {
            _client.GetCompanyFileSetInfoAsync(companycode);
        } 
        //获取公司对上传文件的设置信息
        void _client_GetCompanyFileSetInfoCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.GetCompanyFileSetInfoCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    UserFile file = new UserFile();
                    file.SystemCode = e.Result.SystemCode;
                    file.ModelCode = e.Result.ModelCode;
                    file.BytesSended = e.Result.UploadSpeed;
                }
                
            }
        }

        public void GetCompanyFileByCodeandName(string companycode,string CompanyName)
        {
            //_client.GetCompanyFileByCodeandNameAsync(companycode, CompanyName);
        }
        //获取公司对上传文件的设置信息
        //void _client_GetCompanyFileSetInfoCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.GetCompanyFileSetInfoCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            UserFile file = new UserFile();
        //            file.SystemCode = e.Result.SystemCode;
        //            file.ModelCode = e.Result.ModelCode;
        //            file.BytesSended = e.Result.UploadSpeed;
        //        }

        //    }
        //}
        #endregion
        /// <summary>
        /// 每次上传文件的大小[块]
        /// </summary>
        public int bytesRead;
        public event EventHandler UploadFinished;

       
        #endregion
        /// <summary>
        /// 开始WCF上传文件
        /// </summary>
        private void StartUpload()
        {     
            byte[] buffer = new byte[16 * 1024];//分块上传，每一块的大小16KB          
            long Length = _file.FileStream.Length;//文件的大小
            if (Length < buffer.Length)
            {//如果文件的大小小于每块的大小,则一次性读完
                buffer = new byte[Length];
                bytesRead = _file.FileStream.Read(buffer, 0, Convert.ToInt32(Length));
            }
            else
            {
                 bytesRead = _file.FileStream.Read(buffer, 0, buffer.Length);
            }
            
            if (bytesRead != 0)
            {
                _dataSent += bytesRead;

                if (_dataSent == _dataLength)
                    _lastChunk = true;
                if (_client!=null)
                {//WCF上传
                    if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo != null)
                    {
                        if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Count > 0)
                        {
                            model.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            model.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            model.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            model.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        }
                    }
                    else
                    {
                        model.OWNERCOMPANYID = "111111";
                        model.OWNERDEPARTMENTID = "222222";
                        model.OWNERPOSTID = "333333";
                        model.OWNERID = "444444";
                    }
                    _client.SaveUpLoadFileAsync(SystemCode, ModelCode, _file.FileName, Md5Name, TempID, ID, buffer, Convert.ToInt32(_dataSent), bytesRead, _firstChunk, _lastChunk, UserID,model);
                }
                _firstChunk = false;

            }
            else
            {//真正上传完成

                _client.SaveUpLoadFileIsExistAsync(SystemCode, ModelCode, _file.FileName, Md5Name, TempID, ID, buffer, Convert.ToInt32(_dataSent), bytesRead, _firstChunk, _lastChunk, UserID, model);
                
            }

        }
        ///// <summary>
        ///// 读取指定大小文件内容
        ///// </summary>
        ///// <param name="size"></param>
        ///// <param name="offset">流的偏移量, -1表示不进行偏移</param>
        ///// <returns></returns>
        //public System.Byte[] Read(int size, long offset)
        //{
        //    if (fs == null)
        //    {
        //        fs = this.FileInfo.File.OpenRead();//.OpenRead();
        //    }
        //    if (offset > -1)
        //    {
        //        fs.Seek(offset, System.IO.SeekOrigin.Begin);
        //    }

        //    if ((UpLoadedSize + size) > this.FileSize)
        //        size = (int)(this.FileSize - this.UpLoadedSize);

        //    if (size > 0)
        //    {
        //        System.Byte[] bytes = new System.Byte[size];
        //        try
        //        {
        //            fs.Read(bytes, 0, size);
        //        }
        //        catch
        //        {
        //            this.CloseFileStream();
        //            throw;
        //        }
        //        return bytes;
        //    }
        //    else
        //        this.CloseFileStream();

        //    return null;
        //}
        private void OnProgressChanged()
        {
            _file.BytesUploaded = _dataSent;
            _file.BytesSended = bytesRead;
        }


        private void ChannelFactory_Closed(object sender, EventArgs e)
        {
            ChannelIsClosed();
        }

    
        private void ChannelIsClosed()
        {
            if (!_file.IsDeleted)
            {
                if (UploadFinished != null)
                    UploadFinished(this, null);
            }
        }
        public EventHandler<System.ComponentModel.AsyncCompletedEventArgs> _client_StoreUpLoadFileCompleted { get; set; }

       
    }
}
