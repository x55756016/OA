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

using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.FileUploadWS;
using System.IO;
using System.Reflection;
using SMT.SaaS.LocalData;

namespace SMT.SaaS.FrameworkUI.FileUpload
{
    public partial class CtrlFileUploadM : UserControl
    {
        public delegate void FileUpLoad(FileInfo info);
        public event FileUpLoad FileUpEvent;
        public string UpFileType = "All Files(*.*)|*.*";
        public object EntityEditor { get; set; }//定义一对象
        /// <summary>
        /// 附件集合
        /// </summary>
        //private FileCollection _files;
        public FileCollection _files;

        List<T_SYS_FILEUPLOAD> lst;
        /// <summary>
        /// 隐藏 上传控件时使用
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strm"></param>
        /// <param name="folder_configKey"></param>
        /// <param name="childFolder"></param>
        private string _modelName = "default";
        private string _systemName = "default";
        private bool _isMultiselect = true;
        private long _maxSize = 20971520;

        private bool _hasAccessory = false;//传入的表单ID是否有附件
        public bool HasAccessory
        {
            get { return _hasAccessory; }
            set { _hasAccessory = value; }
        }
        /// <summary>
        /// 父亲ID
        /// </summary>
        public string FormID { get; set; }
        /// <summary>
        /// 功能模块名. 默认 default
        /// </summary>
        public string ModelName { get { return _modelName; } set { _modelName = value; } }
        /// <summary>
        /// 系统名. 默认 default
        /// </summary>
        public string SystemName
        {
            get { return _systemName; }
            set { _systemName = value; }
        }
        /// <summary>
        /// 过滤文件类型
        /// </summary>
        public string Filter
        {
            get { return UpFileType; }
            set { UpFileType = value; }
        }

        /// <summary>
        /// 是否支持多选
        /// </summary>
        public bool IsMultiselect
        {
            get { return _isMultiselect; }
            set { _isMultiselect = value; }
        }

        public long MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = (long)value; }
        }
        /// <summary>
        /// 动作  主要用在查看
        /// </summary>
        public Constants.FileStates _state;
        public Constants.FileStates FileState { get { return _state; } set { _state = value; } }

        /// <summary>
        /// 调用动画加载
        /// </summary>
        public event EventHandler<FileCountEventArgs> Event_AllFilesFinished;
        public CtrlFileUploadM()
        {
            InitializeComponent();
            _files = new FileCollection();

            _files.AllFilesFinished += new EventHandler<FileCountEventArgs>(AllFilesFinished);
            _files.Event_showPnl += new EventHandler(Event_showPnl);
        }
        /// <summary>
        /// 隐藏 上传、选择控件
        /// </summary>
        public void InitBtn(Visibility btnOpen_visibility, Visibility btnUpload_visibility)
        {
            if (this._state == Constants.FileStates.FileBrowse)
                _files.FileState = Constants.FileStates.FileBrowse;

            this.btnOpen.Visibility = btnOpen_visibility;
            this.btnUpload.Visibility = btnUpload_visibility;
            if (this.btnUpload.Visibility == Visibility.Collapsed) _files.IsDeleting = true;
        }
        /// <summary>
        /// 附件上传加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PnlFiles_Loaded(object sender, RoutedEventArgs e)
        {
            
            FileList.ItemsSource = _files;
        }
        /// <summary>
        /// 加载附件数据
        /// </summary>
        /// <param name="formID"></param>
        public void Load_fileData(string formID)
        {
            //获取附件
            if (formID != null && formID.Trim().Length > 0)
            {
                _files.LoadData(formID);
            }
            //if (_files.Count > 0)
            //    _hasAccessory = true;
            
        }

        /// <summary>
        /// 加载附件数据
        /// </summary>
        /// <param name="formID"></param>
        public void Load_fileData(string formID,object obj)
        {
            //获取附件
            if (formID != null && formID.Trim().Length > 0)
            {
                _files.LoadData(formID,obj);
            }
            //if (_files.Count > 0)
            //    _hasAccessory = true;

        }

        //打开 本地文件
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
            openFileWindow.Multiselect = IsMultiselect;
            openFileWindow.Filter = Filter;
            if (openFileWindow.ShowDialog() == true)
                foreach (FileInfo file in openFileWindow.Files)
                {
                    
                    if (file.Length == 0)
                    {
                        MessageBox.Show("请选择需要上传的文件");
                        return;
                    }

                    if (file.Length > MaxSize)
                    {

                        MessageBox.Show("文件大小不能超过"+(MaxSize/1024)+"KB.");
                        return;
                    }

                    try
                    {
                        if (FileUpEvent != null)
                        {
                            FileUpEvent(file);
                        }
                        InitFiles(file.Name, file.OpenRead());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("请检查文件是否被打开，请先关闭文件再上传.");
                        return;
                    }
                }
        }
        //上传服务器
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        /// <summary>
        /// 上传附件之前，初始化 将要上传的 文件信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strm"></param>
        /// <param name="folderKey"></param>
        /// <param name="childFolder">最底层的文件夹，可以是模块名</param>
        public void InitFiles(string name, FileStream strm)
        {
            UserFile userFile = new UserFile();
            userFile.FileName = name;
            userFile.FileStream = strm;
            string compName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;

            userFile.FileName_Path = compName + "\\" + SystemName + "\\" + ModelName + "\\" + name;
            //向文件列表中添加文件信息
            _files.Add(userFile);
            
        }
        /// <summary>
        /// FormID 为空则不调用上传函数
        /// </summary>
        public void Save()
        {
            if (FormID != null && FormID.Trim().Length > 0)
                _UploadFiles();
        }
        /// <summary>
        /// 开始上传附件，及写入数据库记录
        /// </summary>
        /// <param name="formID"></param>
        private void _UploadFiles()
        {
            lst = new List<T_SYS_FILEUPLOAD>();

            foreach (UserFile file in _files)
            {
                if (file.State == Constants.FileStates.Pending)
                {
                    //添加后 获取附件的id。 考虑特殊情况，例如：添加成功后，又立即删除附件，及数据库记录。
                    file.ID = System.Guid.NewGuid().ToString();
                    T_SYS_FILEUPLOAD info = new T_SYS_FILEUPLOAD();
                    Init_Data(file, info);
                    lst.Add(info);
                }
                else if (file.State == Constants.FileStates.Deleted || file.State == Constants.FileStates.Deleteing) //物理删除 文件及数据库记录
                {
                    T_SYS_FILEUPLOAD info = new T_SYS_FILEUPLOAD();
                    Init_Data(file, info);
                    lst.Add(info);
                }
            }
            if (lst.Count > 0)
                _files.UploadFiles(lst);
            else
            {         //当没有需要执行操作的数据时，也返回一个事件调用，供外面根据调用结果，执行下一步逻辑,例如报批件
                AllFilesFinished(this, new FileCountEventArgs());
            }

        }

        private void Init_Data(UserFile file, T_SYS_FILEUPLOAD info)
        {
            UserPost PostInfo = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0];

            info.COMPANYID = PostInfo.CompanyID;
            info.CREATEDATE = DateTime.Now;
            info.CREATEDEPARTMENTID = PostInfo.DepartmentID;
            info.CREATEPOSTID = PostInfo.PostID;
            info.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            info.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            info.FILENAME = file.FileName_Path;
            info.FILEUPLOADID = file.ID;
            info.CREATECOMPANYID = PostInfo.CompanyID;
            info.FORMID = FormID;
            info.MODELNAME = ModelName;
            info.OWNERCOMPANYID = PostInfo.CompanyID;
            info.OWNERDEPARTMENTID = PostInfo.DepartmentID;
            info.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            info.OWNERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            info.OWNERPOSTID = PostInfo.PostID;
            info.SYSTEMCODE = _systemName;
        }
        private void AllFilesFinished(object sender, FileCountEventArgs e)
        {
            
            if (Event_AllFilesFinished != null)
            {
                FileCountEventArgs ev = new FileCountEventArgs();
                Event_AllFilesFinished(this, ev);
            }
        }
        private void Event_showPnl(object sender, EventArgs e)
        {
            //全部逻辑删除后，容器panel也应隐藏
            bool b = false;
            for (int i = 0; i < this._files.Count; i++)
                if (((UserFile)this._files[i]).State != Constants.FileStates.Deleteing)
                    b = true;
            this.PnlFiles.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
