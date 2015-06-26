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
using SMT.FileUpLoad.Classes;

namespace SMT.FileUpLoad
{
    public partial class FileUploadWindow2 : ChildWindow
    {
        private SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient _client;
        public delegate void DelegateFileCompleted();
        public event DelegateFileCompleted FileCompleted;
        /// <summary>
        /// 不显示删除按钮
        /// </summary>
        public bool NotShowDeleteButton{get;set;}
        public FileCollection _files;
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
        /// 文件名(原始文件名)
        /// </summary>    
        public string FileName { get; set; }
        /// <summary>
        /// 文件类型(.doc、.xls、.txt、.pdf......)
        /// </summary>    
        public string FileType { get; set; }
        /// <summary>
        /// 文件地址
        /// </summary>  
        public string FileUrl { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>     
        public string FileSize { get; set; }
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
        /// <summary>
        /// 所属公司
        /// </summary>
        public string OWNERCOMPANYID { get; set; }
        /// <summary>
        /// 所属部门
        /// </summary>
        public string OWNERDEPARTMENTID { get; set; }
        /// <summary>
        /// 所属岗位
        /// </summary>
        public string OWNERPOSTID { get; set; }
        /// <summary>
        /// 所属人
        /// </summary>
        public string OWNERERID { get; set; }
        #endregion
        #region 变量定义
        /// <summary>
        ///文件保存位置目录
        /// </summary>
        public string SavePath { get; set; }
        /// <summary>
        /// 最大允许上传（一次最大可上传多少个文件）
        /// </summary>
        public int MaxConcurrentUploads { get; set; }
        /// <summary>
        /// 每次上传块大小
        /// </summary>
        public double UploadChunkSize { get; set; }
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
        /// <summary>
        /// 可上传数(数量)
        /// </summary>
        private double TotalUploadSize { get; set; }
        /// <summary>
        /// 已上传总量(大小)
        /// </summary>
        private double TotalUploaded { get; set; }
        public long MaximumTotalUpload { get; set; }
        public long MaximumUpload = 1073741824000;
        public int MaxNumberToUpload { get; set; }
        private bool IsCancelUpload = false;//是否取消上传；
        /// <summary>
        /// 文件的最大限制
        /// </summary>
        public double MaxSize { get; set; }
        ///// <summary>
        ///// 模块代号
        ///// </summary>
        //public string ModelCode { get; set; }
        ///// <summary>
        ///// 系统代号
        ///// </summary>
        //public string SystemCode { get; set; }
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
        #region 第1步
        public FileUploadWindow2()
        {//第1步

            InitializeComponent();

            this.OverlayOpacity = 0;
            Multiselect = true;
            uploading = false;
            addFilesButton.Click += new RoutedEventHandler(addFilesButton_Click);
            uploadButton.Click += new RoutedEventHandler(uploadButton_Click);
            clearFilesButton.Click += new RoutedEventHandler(clearFilesButton_Click);
            displayThumbailChckBox.Checked += new RoutedEventHandler(displayThumbailChckBox_Checked);
            displayThumbailChckBox.Unchecked += new RoutedEventHandler(displayThumbailChckBox_Checked);
            Loaded += new RoutedEventHandler(FileUploadControl_Loaded);
           
        }
     
        public FileUploadWindow2(UserConfig uc)
        {//第1步(初始化条件)

            InitializeComponent();
            if (uc != null)
            {
                NotShowDeleteButton = uc.NotShowDeleteButton;
            }
            this.OverlayOpacity = 0;
            #region 用户定义设置          
           // this.SmtFileListId = Guid.NewGuid().ToString();// uc.SmtFileListId;//主键ID
            this.CustomFileName = uc.CustomFileName;//文件名(用户自定义)         
            this.FileUrl = uc.FileUrl;//文件地址
            this.CompanyCode = uc.CompanyCode;//公司代号
            this.CompanyName = uc.CompanyName;//公司名字
            this.SystemCode = uc.SystemCode;//系统代号
            this.ModelCode = uc.ModelCode;//模块代号
            this.ApplicationID = uc.ApplicationID;//业务ID
            this.ThumbnailUrl = uc.ThumbnailUrl;//缩略图地址
            this.Indexl =uc.Indexl;//排序
            this.Remark = uc.Remark;//备注
            this.CreateTime =String.IsNullOrEmpty(uc.CreateTime.ToString())?uc.CreateTime: DateTime.Now;//创建时间
            this.CreateName = uc.CreateName;//创建人
            this.UpdateTime = String.IsNullOrEmpty(uc.UpdateTime.ToString())?uc.UpdateTime: DateTime.Now;;//修改时间
            this.UpdateName = uc.UpdateName;//修改人
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo != null)
            {
                this.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                this.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                this.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                this.OWNERERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].EmployeePostID;
            }
            this.Multiselect = uc.Multiselect;
            this.Filter = uc.Filter;
            this.MaxConcurrentUploads = uc.MaxConcurrentUploads;
            this.MaxSize =ByteConverter.GetByte(uc.MaxSize);
            #endregion       
            uploading = false;
            #endregion
            #region 显示按钮控制
            addFilesButton.Visibility = uc.NotShowSelectButton == true ? Visibility.Collapsed : Visibility.Visible;
            uploadButton.Visibility = uc.NotShowUploadButton == true ? Visibility.Collapsed : Visibility.Visible;
            clearFilesButton.Visibility = uc.NotShowClearButton == true ? Visibility.Collapsed : Visibility.Visible;
            displayThumbailChckBox.Visibility = uc.NotShowThumbailChckBox == true ? Visibility.Collapsed : Visibility.Visible;
            #endregion
           
           // addFilesButton.Visibility = Visibility.Collapsed;
            addFilesButton.Click += new RoutedEventHandler(addFilesButton_Click);
            uploadButton.Click += new RoutedEventHandler(uploadButton_Click);
            clearFilesButton.Click += new RoutedEventHandler(clearFilesButton_Click);
            displayThumbailChckBox.Checked += new RoutedEventHandler(displayThumbailChckBox_Checked);
            displayThumbailChckBox.Unchecked += new RoutedEventHandler(displayThumbailChckBox_Checked);
            Loaded += new RoutedEventHandler(FileUploadControl_Loaded);
         

        }
        //窗体加载
        void FileUploadControl_Loaded(object sender, RoutedEventArgs e)
        {
            //fileList.ItemsSource = _files;
            _client = new SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient();
            _client.GetCompanyFileSetInfoCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.GetCompanyFileSetInfoCompletedEventArgs>(_client_GetCompanyFileSetInfoCompleted);
            _client.DeleteFileCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.DeleteFileCompletedEventArgs>(_client_DeleteFileCompleted);
            GetCompanyFileSetInfo(CompanyCode);//获取公司对上传文件的设置信息
          
        }
        //删除文件完成
        void _client_DeleteFileCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.DeleteFileCompletedEventArgs e)
        {
           
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
                    //UserFile file = new UserFile();
                    //file.SystemCode = e.Result.SystemCode;
                    //file.ModelCode = e.Result.ModelCode;
                    //file.BytesSended = e.Result.UploadSpeed;
                    if (String.IsNullOrEmpty(CompanyCode))
                    {//如果用户没有特别设置，就根据配置文件的设置
                        CompanyCode = e.Result.CompanyCode;// 2;
                    }
                    if (String.IsNullOrEmpty(CompanyName))
                    {//如果用户没有特别设置，就根据配置文件的设置
                        CompanyName = e.Result.CompanyName;// 2;
                    }
                    if (String.IsNullOrEmpty(SystemCode))
                    {//如果用户没有特别设置，就根据配置文件的设置
                        SystemCode = e.Result.SystemCode;// 2;
                    }
                    if (MaxConcurrentUploads == 0)
                    {//如果用户没有特别设置，就根据配置文件的设置
                        MaxConcurrentUploads = e.Result.MaxNumber;// 2;
                    }
                    if (MaxSize == 0.0)
                    {//如果用户没有特别设置，就根据配置文件的设置
                        MaxSize = e.Result.MaxSize;// 1024 * 1024 * 100;//100KB
                    }
                    this.SavePath = e.Result.SavePath;
                    MaxNumberToUpload = -1;
                    MaximumTotalUpload = MaximumUpload = -1;
                 
                    if (string.IsNullOrEmpty(Filter))
                    {
                       // Filter = e.Result.Type;// 文件类型选择
                    }
                    _files = new FileCollection("", MaxConcurrentUploads);

                    UploadChunkSize = e.Result.UploadSpeed;// 102401;//4194304
                    fileList.ItemsSource = _files;
                    _files.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_files_CollectionChanged);
                    _files.StateChanged += new EventHandler(_files_StateChanged);
                }

            }
            else
            {
                MessageBox.Show("网络出现错误请联系管理员");
            }
        }
        #endregion     
        #region 第2步
        #region 增加上传文件
        /// <summary>
        /// 增加上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void addFilesButton_Click(object sender, RoutedEventArgs e)
        {//第2步
            string files1 = ",";
            string TempID = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = Multiselect;//是否可以多选
            try
            {
                if (!string.IsNullOrEmpty(Filter))
                    ofd.Filter = Filter;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("不是正确的上传格式.", ex);
            }
            //string StrSpecial = @"~!@#$%^&*()_+- =`;:'\<>；~！@#￥%……&*（）——+=-·";//特殊字符串
            List<string> StrSpecial = GetSpecialString();
            if (ofd.ShowDialog() == true)
            {
                foreach (FileInfo file in ofd.Files)
                {
                    if (file.Length == 0)
                    {
                        MessageBox.Show(file.Name+":文件内容不能为空!");
                        continue;
                    }
                    string[] StrFrontName = file.Name.Split('.');
                    //控制文件名不为特殊字符
                    if (StrFrontName.Count() > 1)
                    {                        
                        var ents = from ent in StrSpecial
                                   where StrFrontName[0].Contains(ent) || StrFrontName[1].Contains(ent)
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            MessageBox.Show(file.Name + "文件名存在特殊字符(*&^%$#@!~`?=+|><';:.,)请重新命名!");
                            continue;
                        }                        
                    }
                    string fileName = file.Name;
                    UserFile userFile = new UserFile();
                    #region 用户定义设置
                    userFile.SavePath = this.SavePath;//文件保存位置目录
                    userFile.SmtFileListId = Guid.NewGuid().ToString();// this.SmtFileListId;//主键ID
                    userFile.CustomFileName = String.IsNullOrEmpty(this.CustomFileName) ? file.Name : this.CustomFileName;//文件名(用户自定义)
                    userFile.FileName =file.Name;//文件名
                    userFile.FileType = file.Extension;//文件类型(.doc、.xls、.txt、.pdf......)
                    userFile.FileUrl = this.FileUrl;//文件地址
                    userFile.FileSize =Convert.ToDouble(file.Length);//
                    userFile.CompanyCode = this.CompanyCode;//公司代号
                    userFile.CompanyName = this.CompanyName;//公司名字
                    userFile.SystemCode = this.SystemCode;//系统代号
                    userFile.ModelCode = this.ModelCode;//模块代号
                    userFile.ThumbnailUrl = this.ThumbnailUrl;//缩略图地址
                    int fileCount = _files.Count() + 1;
                    userFile.Indexl = fileCount;//排序
                    userFile.Remark = this.Remark;//备注
                    userFile.CreateTime =this.CreateTime;//创建时间
                    userFile.CreateName = this.CreateName;//创建人
                    userFile.UpdateTime = this.UpdateTime;//修改时间
                    userFile.UpdateName = this.UpdateName;//修改人
                    userFile.ApplicationID = this.ApplicationID;//业务ID
                    userFile.NotShowDeleteButton = this.NotShowDeleteButton;//不显示删除按钮
                    userFile.ID = this.ApplicationID;
                    userFile.UserID = this.UserID;
                    #endregion
                    //userFile.FileName = file.Name;
                    //userFile.SystemCode = this.SystemCode;
                    //userFile.ModelCode = this.ModelCode;
                    //userFile.FileType = this.FileType;
                    userFile.FileStream = file.OpenRead();
                    userFile.UIDispatcher = this.Dispatcher;                 
                    userFile.DisplayThumbnail = (bool)displayThumbailChckBox.IsChecked;
                    userFile.MD5String = MD5FileName(file.Name, file.Length.ToString(), UserID, SystemCode, ModelCode,this.ApplicationID) + file.Extension;
                    
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
                        if (_files.Count > 0)
                        {
                            #region 不加重复
                            
                            bool bol = false;
                            for (int i = 0; i < _files.Count; i++)
                            {
                                if (_files[i].FileName == userFile.FileName && _files[i].FileSize == userFile.FileSize)
                                {//不加重复
                                    bol = true;//有重复
                                }                              
                            }
                              if(!bol)
                                {
                                    _files.Add(userFile);
                                }
                        }
                        else
                        {
                            _files.Add(userFile);
                        }
                        #endregion
                        // _files.Add(userFile);                    
                      
                    }
                    else
                    {
                        MessageChildWindow messageWindow = new MessageChildWindow();
                       // messageWindow.Message = "Maximum file size is: " + (MaxSize / 1024).ToString() + "KB.";
                         messageWindow.Message = "文件最大限制为: " + ByteConverter.GetSizeName(MaxSize);                        
                        messageWindow.Show();
                    }
                }
            }
        }

        /// <summary>
        /// 获取特殊字符串
        /// </summary>
        /// <returns></returns>
        private List<string> GetSpecialString()
        {
            List<string> LstSpecialStr = new List<string>();
            LstSpecialStr.Add(".");
            LstSpecialStr.Add(",");
            LstSpecialStr.Add("`");
            LstSpecialStr.Add("!");
            LstSpecialStr.Add("@");
            LstSpecialStr.Add("#");
            LstSpecialStr.Add("$");
            LstSpecialStr.Add("%");
            LstSpecialStr.Add("^");
            LstSpecialStr.Add("&");
            LstSpecialStr.Add("*");
            //LstSpecialStr.Add("(");
            //LstSpecialStr.Add(")");
            //LstSpecialStr.Add("-");
            LstSpecialStr.Add("=");
            LstSpecialStr.Add("+");
            //LstSpecialStr.Add("-");
            LstSpecialStr.Add("~");
            LstSpecialStr.Add("\"");
            LstSpecialStr.Add(";");
            LstSpecialStr.Add(":");
            LstSpecialStr.Add("'");
            LstSpecialStr.Add("<");
            LstSpecialStr.Add(">");
            LstSpecialStr.Add("?");
            LstSpecialStr.Add("/");
            LstSpecialStr.Add(" ");
            LstSpecialStr.Add("\\");
            LstSpecialStr.Add("|");
            return LstSpecialStr;
        }
        //文件集合数改变发生
        void _files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {           
            countTextBlock.Text = "文件数: " + _files.Count.ToString();
            TotalUploadSize = _files.Sum(f => f.FileSize);
            TotalUploaded = _files.Sum(f => f.BytesUploaded);
            if (TotalUploaded >= TotalUploadSize)
            {
                totalSizeTextBlock.Text = string.Format("{0} / {1}",
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString(),
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            }
            else
            {
                totalSizeTextBlock.Text = string.Format("{0} / {1}",
                    new ByteConverter().Convert(TotalUploaded, this.GetType(), null, null).ToString(),
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            }
            progressBar.Maximum = TotalUploadSize;
            progressBar.Value = TotalUploaded;
        }
        #endregion
        #endregion
        #region 第3步
        #region 点击上传按钮事件
        void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)uploadButton.Content == "上传")
            {
                
                start = DateTime.Now;
                int intIndex = 0;
                foreach (UserFile file in _files)
                {
                    intIndex = intIndex + 1;
                    //如果是被取消状态则重新上传
                    if (file.State == Constants.FileStates.Removed)
                    {
                        file.State = Constants.FileStates.Pending;
                    }
                    //file.Indexl = intIndex;
                }
                UploadFiles();
                //clearFilesButton.Visibility = Visibility.Collapsed;//上传过程中不能清空
            }
            else
            {
                IsCancelUpload = true;
                _files.CancelUpload();
                //var q = _files.Where(f => f.State == Constants.FileStates.Uploading);
                //foreach (UserFile fu in q)
                //{

                //    fu.CancelUpload();
                //}
                uploading = false;
                uploadButton.Content = "上传";
                clearFilesButton.Visibility = Visibility.Visible;
            }
        }
        //上传文件
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
                uploadButton.Content = "取消";
                _files.UploadFiles();
                clearFilesButton.Visibility = Visibility.Collapsed;//上传过程中不能清空
            }

        }
        void _files_StateChanged(object sender, EventArgs e)
        {//上击上传后发生

            FileCollection List = sender as FileCollection;
            if (List.Count(c => c.State == Constants.FileStates.Uploading) == 0)
            {
                uploading = false;
                uploadButton.Content = "上传";
                clearFilesButton.Visibility = Visibility.Visible;//全部上传完成显示
            }
            #region
          
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
            #endregion
        }
        #region 单个文件上传完成事件
        void userFile_FileFinished(UserFile _userfile)
        {
            if (FileCompleted != null)
            {
                FileCompleted();
            }
        }
        /// <summary>
        /// 进度条事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void userFile_UploadProgressChanged(object sender, UploadProgressChangedEventArgs args)
        {
            if (args.BytesUploaded == 0.0)
            {
                TotalUploaded += args.TotalBytesUploaded;
            }
            else
            {
                TotalUploaded += args.BytesUploaded;
            }
           // TotalUploaded += args.BytesUploaded;
            progressBar.Value = TotalUploaded;//显示总进度条的值
            if (TotalUploaded >= TotalUploadSize)
            {
                totalSizeTextBlock.Text = string.Format("{0} / {1}",
                     new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString(),
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            }
            else
            {
                totalSizeTextBlock.Text = string.Format("{0} / {1}",
                     new ByteConverter().Convert(TotalUploaded, this.GetType(), null, null).ToString(),
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            }
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
        #endregion
        #endregion      

        void displayThumbailChckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            foreach (UserFile fu in _files)
            {
                if (fu.DisplayThumbnail != chkbox.IsChecked)
                    fu.DisplayThumbnail = (bool)chkbox.IsChecked;
            }
        }
        public FileUploadWindow2(Uri uploadUrl)
            : this()
        {
            UploadUrl = uploadUrl;
        }
        //清空列表
        void clearFilesButton_Click(object sender, RoutedEventArgs e)
        {
            //uploadButton.Content = "上传";
            //Speed.Text = "速度:";
            //var q = _files.Where(f => f.State == Constants.FileStates.Uploading);
            //foreach (UserFile fu in q)
            //{//取消正在上传的文件
            //    fu.CancelUpload();
            //}
            //timeLeftTextBlock.Text = "";
            //_files.Clear();
            uploadButton.Content = "上传";
            Speed.Text = "速度:";
            var q = _files.Where(f => f.State == Constants.FileStates.Uploading);
            foreach (UserFile fu in q)
            {//取消正在上传的文件并删除服务器中的文件
                fu.CancelUpload();
               
                    if (MessageBox.Show(fu.FileName + "还没有上传完成,是否保留下一次续传?", "提示信息!", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        _client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
                    }
               
               // _client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
            }
            var q2 = _files.Where(f => f.State == Constants.FileStates.Error || f.State == Constants.FileStates.Removed);
            foreach (UserFile fu in q2)
            {//对于移除和错误的文件，删除服务器中的文件 
               
                    if (MessageBox.Show(fu.FileName+"还没有上传完成,是否保留下一次续传?", "提示信息!", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        _client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
                    }
               
                //_client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
            }
            //删除已上传的文件
            var q3 = _files.Where(f=>f.State == Constants.FileStates.Finished);
            if (q3.Count() > 0)
            {
                for (int i = q3.Count()-1; i >=0; i--)
                {
                    UserFile fu = q3.ToList()[i];
                    fu.IsDeleted = true;
                    _client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
                }
            }
                

            timeLeftTextBlock.Text = "";
            _files.Clear();

            countTextBlock.Text = "文件数: " + _files.Count.ToString();
            TotalUploadSize = _files.Sum(f => f.FileSize);
            TotalUploaded = _files.Sum(f => f.BytesUploaded);
            if (TotalUploaded >= TotalUploadSize)
            {
                totalSizeTextBlock.Text = string.Format("{0} / {1}",
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString(),
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            }
            else
            {
                totalSizeTextBlock.Text = string.Format("{0} / {1}",
                    new ByteConverter().Convert(TotalUploaded, this.GetType(), null, null).ToString(),
                    new ByteConverter().Convert(TotalUploadSize, this.GetType(), null, null).ToString());
            }
            progressBar.Maximum = TotalUploadSize;
            progressBar.Value = TotalUploaded;
        }
        
        
        
        #region Md5加密
        public string MD5FileName(string _name, string size, string _userid,string strSystemCode,string strModelCode,string FormID)
        {
            return MD5CryptoServiceProvider.GetMd5String(_name + size + _userid + strSystemCode + strModelCode + FormID);
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
      
       
    }
}

