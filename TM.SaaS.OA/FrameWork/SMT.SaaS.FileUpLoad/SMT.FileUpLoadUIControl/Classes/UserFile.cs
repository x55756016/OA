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


namespace SMT.FileUpLoad
{
  

    public class UserFile : INotifyPropertyChanged
    {
        /// <summary>
        /// 上传进度改变事件
        /// </summary>
        public event ProgressChangedEvent UploadProgressChanged;
        public Dispatcher UIDispatcher { get; set; }
        #region 上传实体接口(WCF上传，WebClient上传)

        /// <summary>
        /// 上传实体接口(WCF上传，WebClient上传)
        /// </summary>
        private IFileUploader _fileUploader;
        #endregion
        #region 是否显示删除控按钮
        /// <summary>
        /// 不显示删除按钮
        /// </summary>
        public bool NotShowDeleteButton { get; set; }
        #endregion
        #region 与数据库对应的变量定义
        /// <summary>
        /// 主键ID(数据库)
        /// </summary>            
        public string SmtFileListId { get; set; }
        /// <summary>
        /// 文件名（用户自定义）
        /// </summary>    
        public string CustomFileName { get; set; }
        /// <summary>
        /// 文件类型(.doc、.xls、.txt、.pdf......)
        /// </summary>    
        public string FileType { get; set; }        
        /// <summary>
        /// 文件在服务器上的路径
        /// </summary>  
        public string FileUrl { get; set; }
        /// <summary>
        /// 公司代号
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 公司名字
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 系统代号
        /// </summary> 
        public string SystemCode { get; set; }
        /// <summary>
        /// 模块代号
        /// </summary>  
        public string ModelCode { get; set; }
        /// <summary>
        /// 业务ID
        /// </summary>
        public string ApplicationID { get; set; }
        /// <summary>
        /// 缩略图地址
        /// </summary>   
        public string ThumbnailUrl { get; set; }
        /// <summary>
        /// 排序
        /// </summary>  
        public decimal Indexl { get; set; }
        /// <summary>
        /// 备注
        /// </summary>    
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>      
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>  
        public string CreateName { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>    
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>    
        public string UpdateName { get; set; }

        #endregion  
        /// <summary>
        ///  文件保存的位置目录
        /// </summary>
        public string SavePath { get; set; }
        #region 业务系统Guid
        /// <summary>
        /// 存储ID(业务ID)
        /// </summary>
        public string ID
        {
            get;
            set;
        }
        #endregion

        #region 用于真实存储ID(些ID动态产生，用于唯一性)
        /// <summary>
        /// 用于真实存储ID(些ID动态产生，用于唯一性)
        /// </summary>
        public string TempID
        {
            get;
            set;
        }
        #endregion
        #region 文件上传名称加密（用于断点续传）
        /// <summary>
        /// 加密后文件名(系统代号，模块代号，用户ID)
        /// </summary>
        public string MD5String
        {
            get;
            set;
        }
        #endregion
        #region 上传文件名称
        private string _fileName;
        /// <summary>
        /// 文件名（原始文件名）
        /// </summary>
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
        #endregion

        #region 上传状态
        private Constants.FileStates _state = Constants.FileStates.Pending;
        public Constants.FileStates State
        {
            get { return _state; }
            set
            {
                _state = value;
                NotifyPropertyChanged("State");
                NotifyPropertyChanged("StateInfo");
                if (FileFinished != null)
                {
                    if (value == Constants.FileStates.Finished)
                    {
                        FileFinished(this);
                    }
                }
            }
        }
        #endregion
        #region 是否显示缩略图
        private bool displayThumbnail;
        public bool DisplayThumbnail
        {
            get { return displayThumbnail; }
            set
            {
                displayThumbnail = value;
                NotifyPropertyChanged("DisplayThumbnail");

            }
        }
        #endregion
        #region 上传状态（转换成中文）
        /// <summary>
        /// 上传状态（转换成中文）
        /// </summary>
        public string StateInfo
        {
            get
            {
                return ConstantsCN.CN(State);

            }
            set
            {
                NotifyPropertyChanged("StateInfo");
            }
        }
        #endregion
        #region 文件是否删除
        private bool _isDeleted = false;
        /// <summary>
        /// 文件是否已删除
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                _isDeleted = value;

                if (_isDeleted)
                    CancelUpload();

                NotifyPropertyChanged("IsDeleted");
            }
        }
        #endregion
        #region 上传文件流
        private Stream _fileStream;
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
        #endregion
        #region 文件大小
        private double _fileSize = 0;
        /// <summary>
        /// 文件大小
        /// </summary>
        public double FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                _fileSize = value;
            }
        }
        #endregion
        #region 已上传Bytes数量
        private double _bytesUploaded = 0;
        /// <summary>
        ///  已上传Bytes数量
        /// </summary>
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
        #endregion
        #region 上传发送bytes
        private double _bytesSended;
        /// <summary>
        /// 上传发送bytes(速度)
        /// </summary>
        public double BytesSended
        {
            get {
                return _bytesSended;
            }
            set {
                _bytesSended = value;
                NotifyPropertyChanged("BytesSended");
                if (UploadProgressChanged != null)
                {
                    UploadProgressChangedEventArgs args = new UploadProgressChangedEventArgs(0, BytesSended, BytesUploaded, this.FileSize, this.FileName);
                    UploadProgressChanged(this, args);

                }
            }

        }
        #endregion
        #region 上传进度
        private int _percentage = 0;
        /// <summary>
        /// 上传进度
        /// </summary>
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
        #endregion
        #region 创建用户
        public string UserID
        {
            get;
            set;
        }
        #endregion

        public delegate void DelegateFinish(UserFile _userfile);
        public event DelegateFinish FileFinished;
        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="initParams"></param>
        public void Upload(string initParams)
        {
            this.State = Constants.FileStates.Uploading;
            _fileUploader = new WcfFileUploader(this);//WCF上传
            _fileUploader.StartUpload(initParams);
            _fileUploader.UploadFinished += new EventHandler(fileUploader_UploadFinished);
        }
     
        /// <summary>
        /// 取消上传
        /// </summary>
        public void CancelUpload()
        {
            if (_fileUploader != null && this.State == Constants.FileStates.Uploading)
            {
                this.State = Constants.FileStates.Removed;
                _fileUploader.CancelUpload();
            }
        }

        /// <summary>
        /// 取消已取消的上传
        /// </summary>
        public void CancelCancelUpload()
        {
            if (_fileUploader != null && this.State == Constants.FileStates.Uploading)
            {
                this.State = Constants.FileStates.Pending;
                //_fileUploader.CancelUpload();
            }
        }

        /// <summary>
        /// 上传完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileUploader_UploadFinished(object sender, EventArgs e)
        {
            _fileUploader = null;

            this.State = Constants.FileStates.Finished;
        }

        public string OWNERCOMPANYID {get;set;}
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERID { get; set; }


        #region INotifyPropertyChanged Members

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
