using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Browser;
using SMT.FileUpLoad;

namespace SMT.FileUpLoad
{
    public partial class FileUploadWindow : ChildWindow
    {
        public delegate void DelegateFileCompleted();
        public event DelegateFileCompleted FileCompleted;
        private FileCollection _files;
        private bool _HttpUploader = true;
        private string _uploadHandlerName;
        #region 变量定义
    
        /// <summary>
        /// 最大允许上传
        /// </summary>
        public int MaxConcurrentUploads { get; set; }
        /// <summary>
        /// 每次上传块大小
        /// </summary>
        public long UploadChunkSize { get; set; }
        /// <summary>
        /// 是否允许切换图片显示
        /// </summary>
        public bool ResizeImage { get; set; }
        public int ImageSize { get; set; }
        /// <summary>
        /// 开始上传时间
        /// </summary>
        private DateTime start;
        /// <summary>
        /// 文件上传过滤
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// 是否允许选择多个文件
        /// </summary>
        public bool Multiselect { get; set; }
        /// <summary>
        /// 上传指定的Url
        /// </summary>
        public Uri UploadUrl { get; set; }
        /// <summary>
        /// 是否上传中
        /// </summary>
        private bool uploading { get; set; }

        private double TotalUploadSize { get; set; }
        /// <summary>
        /// 已上传总量
        /// </summary>
        private double TotalUploaded { get; set; }
        public long MaximumTotalUpload { get; set; }
        public long MaximumUpload = 1073741824000;
        public int MaxNumberToUpload { get; set; }
        private int count = 0;
        public int  MaxSize{get;set;}
        /// <summary>
        /// 模块代号
        /// </summary>
        public string ModelCode{get;set;}
        /// <summary>
        /// 系统代号
        /// </summary>
        public string SystemCode { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 业务ID
        /// </summary>
        public string ApplicationID { get; set; }
        #endregion
        public bool AllowThumbnail 
        {
            get { return displayThumbailChckBox.Visibility == Visibility.Visible; }
            set
            {
                bool temp = value;
                if (temp)
                {
                    displayThumbailChckBox.Visibility = Visibility.Visible;
                }
                else
                {
                    displayThumbailChckBox.Visibility = Visibility.Collapsed;
                }
            }
        }
  
        public FileUploadWindow()
        {
            
            InitializeComponent();
            
            this.OverlayOpacity = 0;
            MaxConcurrentUploads = 2;
            MaxNumberToUpload = -1;
            MaximumTotalUpload = MaximumUpload = -1;
            MaxSize = 1024 * 1024 * 100;
            if(string.IsNullOrEmpty(Filter))
            {
               Filter = "All Files|*.*";
            }
            Multiselect = true;
            uploading = false;
            _files = new FileCollection("", MaxConcurrentUploads);
            UploadChunkSize = 102401;//4194304

            addFilesButton.Click += new RoutedEventHandler(addFilesButton_Click);
            uploadButton.Click += new RoutedEventHandler(uploadButton_Click);
            clearFilesButton.Click += new RoutedEventHandler(clearFilesButton_Click);

            displayThumbailChckBox.Checked += new RoutedEventHandler(displayThumbailChckBox_Checked);
            displayThumbailChckBox.Unchecked += new RoutedEventHandler(displayThumbailChckBox_Checked);
           
            Loaded += new RoutedEventHandler(FileUploadControl_Loaded);
            _files.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_files_CollectionChanged);
            _files.StateChanged += new EventHandler(_files_StateChanged);
        
        }

        void _files_StateChanged(object sender, EventArgs e)
        {//上击上传后发生

            FileCollection List = sender as FileCollection;
            if(List.Count(c=>c.State==Constants.FileStates.Uploading)==0)
            {
                uploading = false;
                uploadButton.Content = "上传";
            }
            //UserFile fu = sender as UserFile;
            //if (fu.State == Constants.FileStates.Finished)
            //{
            //    if (uploading)
            //    {
            //        UploadFiles();
            //    }
            //    else
            //    {

            //    }
            //}
            //else if (fu.State == Constants.FileStates.Removed)
            //{
            //    //files.Remove(fu);
            //    //if (uploading)
            //    //    UploadFiles();
            //}
            //else if (fu.State == Constants.FileStates.Uploading && _files.Count(f => f.State == Constants.FileStates.Uploading) < MaxConcurrentUploads)
            //{
               
            //}
        }

        void _files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            countTextBlock.Text = "文件数: " + _files.Count.ToString();
            TotalUploadSize = _files.Sum(f => f.FileSize);
            TotalUploaded = _files.Sum(f => f.BytesUploaded);
            totalSizeTextBlock.Text = string.Format("{0} / {1}",
                new ByteConverter().Convert(TotalUploaded, this.GetType(), null, null).ToString(),
                new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            progressBar.Maximum = TotalUploadSize;
            progressBar.Value = TotalUploaded;
        }

        void displayThumbailChckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            foreach (UserFile fu in _files)
            {
                if (fu.DisplayThumbnail != chkbox.IsChecked)
                    fu.DisplayThumbnail = (bool)chkbox.IsChecked;
            }
        }
        public FileUploadWindow(Uri uploadUrl)
            : this()
        {
            UploadUrl = uploadUrl;
        }
        void clearFilesButton_Click(object sender, RoutedEventArgs e)
        {
            uploadButton.Content = "上传";
            Speed.Text = "速度:";

            var q = _files.Where(f => f.State == Constants.FileStates.Uploading);
            foreach (UserFile fu in q)
            {
                fu.CancelUpload();
            }
            timeLeftTextBlock.Text = "";
            _files.Clear();
        }
        #region 点击上传按钮事件
        void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)uploadButton.Content == "上传")
            {
                uploadButton.Content = "取消";
                start = DateTime.Now;
                UploadFiles();
            }
            else
            {

                var q = _files.Where(f => f.State == Constants.FileStates.Uploading);
                foreach (UserFile fu in q)
                {
                   
                    fu.CancelUpload();
                }
                uploading = false;
                uploadButton.Content = "上传";
            }
        }
        #endregion
        #region 增加上传文件
        /// <summary>
        /// 增加上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void addFilesButton_Click(object sender, RoutedEventArgs e)
        {
             string files1=",";
            string TempID = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            try
            {
                if (!string.IsNullOrEmpty(Filter))
                    ofd.Filter = Filter;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("不是正确的上传格式.", ex);
            }
            if (ofd.ShowDialog() == true)
            {
                foreach (FileInfo file in ofd.Files)
                {
                    
                    string fileName = file.Name;
                    UserFile userFile = new UserFile();
                    userFile.ID = ApplicationID;
                    userFile.UserID = UserID;
                    userFile.FileName = file.Name;
                    userFile.FileStream = file.OpenRead();
                    userFile.UIDispatcher = this.Dispatcher;
                    userFile.SystemCode = this.SystemCode;
                    userFile.ModelCode = this.ModelCode;
                    userFile.Indexl = 1;
                    userFile.DisplayThumbnail = (bool)displayThumbailChckBox.IsChecked;
                    userFile.MD5String = MD5FileName(file.Name, file.Length.ToString(), UserID,SystemCode,ModelCode) + file.Extension;
                    userFile.UploadProgressChanged += new ProgressChangedEvent(userFile_UploadProgressChanged);
                    userFile.FileFinished += new UserFile.DelegateFinish(userFile_FileFinished);
                    while (true)
                        {
                            TempID = GetTmpID();
                            if (files1.IndexOf("," + TempID + ",") == -1)
                                break;
                        }
                        files1 = files1 + TempID + ",";
                        userFile.TempID = TempID;
                 
                    if (userFile.FileStream.Length <= MaxSize)
                    {
                      
                        _files.Add(userFile);
                    }
                    else
                    {
                        MessageChildWindow messageWindow = new MessageChildWindow();
                        messageWindow.Message = "Maximum file size is: " + (MaxSize / 1024).ToString() + "KB.";
                        messageWindow.Show();
                    }
                }
            }
        }
        #endregion
        #region 单个文件上传完成事件
        void userFile_FileFinished(UserFile _userfile)
        {
            if (FileCompleted != null)
            {
                FileCompleted();
            }
        }

        void userFile_UploadProgressChanged(object sender, UploadProgressChangedEventArgs args)
        {
         
            TotalUploaded += args.BytesUploaded;
            progressBar.Value = TotalUploaded;
            totalSizeTextBlock.Text = string.Format("{0} / {1}",
                 new ByteConverter().Convert(TotalUploaded, this.GetType(), null, null).ToString(),
                new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());

            double ByteProcessTime = 0;
            double EstimatedTime = 0;
            try
            {
                TimeSpan timeSpan = DateTime.Now - start;
                ByteProcessTime = TotalUploaded / timeSpan.TotalSeconds;
                Speed.Text = "速度:" + ((TotalUploaded / 1024) / timeSpan.TotalSeconds).ToString("F0") + "kb/秒";
                EstimatedTime = (TotalUploadSize - TotalUploaded) / ByteProcessTime;
                timeSpan = TimeSpan.FromSeconds(EstimatedTime);
                timeLeftTextBlock.Text = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
            catch { }
        }
        #endregion
        #region Md5加密
        public string MD5FileName(string _name, string size, string _userid,string strSystemCode,string strModelCode)
        {
            return MD5CryptoServiceProvider.GetMd5String(_name + size + _userid + strSystemCode+strModelCode);
        }
        #endregion
        #region 组合成临时上传ID
        static int i = 0;
        public string GetTmpID()
        {
            i++;
            i = i % 100;
            Random rnd = new Random(unchecked((int)DateTime.Now.Ticks+i));
            string strRnd = DateTime.Now.ToString("yyyMMddhhmmss") + rnd.Next(1, 99999999).ToString();
            return strRnd;
        }
        #endregion
      
        void upload_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            if (e.PropertyName == "FileLength")
            {
                _files_CollectionChanged(null, null);
                
            }
        }

        void upload_StatusChanged(object sender, EventArgs e)
        {
            UserFile fu = sender as UserFile;
            if (fu.State ==Constants.FileStates.Finished)
            {
                if (uploading)
                {
                    ExchaneState();
                }
            }
            else if (fu.State == Constants.FileStates.Removed)
            {

                _files.Remove(fu);
                if (uploading)
                    ExchaneState();
            }
            else if (fu.State == Constants.FileStates.Uploading && _files.Count(f => f.State == Constants.FileStates.Uploading) < MaxConcurrentUploads)
            {
                ExchaneState();
            }
        }
        #region 更改完成状态
        private void ExchaneState()
        {
            uploading = true;
            while (_files.Count(f => f.State == Constants.FileStates.Uploading) < MaxConcurrentUploads && uploading)
            {
                if (_files.Count(f => f.State != Constants.FileStates.Finished && f.State != Constants.FileStates.Uploading) > 0)
                {

                    UserFile fu = _files.First(f => f.State != Constants.FileStates.Finished && f.State != Constants.FileStates.Uploading);
                }
                else if (_files.Count(f => f.State == Constants.FileStates.Uploading) == 0)
                {
                    uploading = false;
                    uploadButton.Content = "上传";

                }
                else
                {
                    break;
                }
            }
        }
        #endregion
        void FileUploadControl_Loaded(object sender, RoutedEventArgs e)
        {
            fileList.ItemsSource = _files;
        }
        private void UploadFiles()
        {

            if (_files.Count == 0)
            {
                MessageChildWindow messageWindow = new MessageChildWindow();
                messageWindow.Message = "没有任何文件可上传，请选择文件";
                messageWindow.Show();
            }
            else
            {
                _files.UploadFiles();
            }
          
        }
    }
}

