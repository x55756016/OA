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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Browser;
using System.Collections.Generic;
using SMT.Saas.Tools.FileUploadWS;

namespace SMT.SaaS.FrameworkUI.FileUpload
{
    [ScriptableType]
    public class FileCollection : ObservableCollection<UserFile>
    {
       
        private double _bytesUploaded = 0;
        private int _percentage = 0;
        private int _currentUpload = 0;
        private string _customParams;
        private int _maxUpload=8;
        private int _totalUploadedFiles = 0;

        public object EntityEditor { get; set; }//定义一对象

        /// <summary>
        /// 动作  主要用在查看
        /// </summary>
        public Constants.FileStates _state;
        public Constants.FileStates FileState { get { return _state; } set { _state = value; } }
        private WcfFileUploader _WcfFileUploader;


        /// <summary>
        /// 是否逻辑删除  传给 userFile
        /// </summary>
        private bool _hasAccessory = false;//传入的表单ID是否有附件
        public bool HasAccessory
        {
            get { return _hasAccessory; }
            set { _hasAccessory = value; }
        }
        private bool _isDeleting = false;
        public bool IsDeleting { get { return _isDeleting; } set { _isDeleting = value; } }
        public double BytesUploaded
        {
            get { return _bytesUploaded; }
            set
            {
                _bytesUploaded = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("BytesUploaded"));
            }
        }

        [ScriptableMember()]
        public string CustomParams
        {
            get { return _customParams; }
            set
            {
                _customParams = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CustomParams"));
            }
        }
        
        [ScriptableMember()]
        public int TotalFilesSelected
        {
            get { return this.Items.Count; }
        }

        [ScriptableMember()]
        public int Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value;

                this.OnPropertyChanged(new PropertyChangedEventArgs("Percentage"));

                //Notify Javascript of percentage change
                if (TotalPercentageChanged != null)
                    TotalPercentageChanged(this, null);
            }
        }

        [ScriptableMember()]
        public int TotalUploadedFiles
        {
            get { return _totalUploadedFiles; }
            set
            {
                _totalUploadedFiles = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("TotalUploadedFiles"));
            }
        }

        [ScriptableMember()]
        public IList<UserFile> FileList
        {
            get { return this.Items; }
        }

        [ScriptableMember()]
        public event EventHandler SingleFileUploadFinished;

        [ScriptableMember()]
        public event EventHandler<FileCountEventArgs> AllFilesFinished;

        [ScriptableMember()]
        public event EventHandler ErrorOccurred;

        [ScriptableMember()]
        public event EventHandler FileAdded;

        [ScriptableMember()]
        public event EventHandler FileRemoved;

        [ScriptableMember()]
        public event EventHandler StateChanged;

        [ScriptableMember()]
        public event EventHandler TotalPercentageChanged;
        public event EventHandler Event_showPnl;
        public FileCollection(string customParams, int maxUploads)
        {
            _customParams = customParams;
            this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(FileCollection_CollectionChanged);
        }
       /// <summary>
        /// 
       /// </summary>
       /// <param name="customParams"></param>
        /// <param name="showPnl">显示、隐藏容器</param>      
        public FileCollection()
        {           
            this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(FileCollection_CollectionChanged);
        }     
     
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="formID"></param>
        public void LoadData(string formID)
        {
            _WcfFileUploader = new WcfFileUploader();
            _WcfFileUploader.DGets_Completed = new WcfFileUploader.DGets(DGets_Completed);
            _WcfFileUploader.Get_ParentID(formID);
        }


        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="formID"></param>
        public void LoadData(string formID,object obj)
        {
            _WcfFileUploader = new WcfFileUploader();
            _WcfFileUploader.DGets_Completed = new WcfFileUploader.DGets(DGets_Completed);
            EntityEditor = obj;
            _WcfFileUploader.Get_ParentID(formID,obj);
           // _WcfFileUploader.Get_ParentID(formID);
        }
        /// <summary>
        /// 从数据库中加载记录
        /// </summary>
        /// <param name="lst"></param>
        private void DGets_Completed(List<T_SYS_FILEUPLOAD> lst)
        {
            
            if (lst.Count > 0)
                _hasAccessory = true;
            string userName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName;//用户名
            string userPwd = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPwd;//密码
            string aa = HttpUtility.UrlEncode(userPwd);
            foreach(T_SYS_FILEUPLOAD info in lst)
            {
                UserFile item = new UserFile();
                base.Add(item);
                item.ID = info.FILEUPLOADID;
                //zl
                string filepath = info.FILENAME;
                //string aspxpath = "http://localhost:1604/FileDownload.aspx" + "?username=" + userName + "&" + "password=" + aa + "&";
                string serpath = filepath.Substring(0, filepath.IndexOf("\\"));
                string aspxpath = serpath + "/filedownload.aspx" + "?username=" + userName + "&" + "password=" + aa + "&";

                info.FILENAME = info.FILENAME.Substring(info.FILENAME.IndexOf("\\") + 1);
                aspxpath = aspxpath + "filename=" + HttpUtility.UrlEncode( info.FILENAME);
                item.FileName_Path = aspxpath;
                //
                //item.FileName_Path = info.FILENAME;     // by zl
                int i = info.FILENAME.LastIndexOf('\\');
                item.FileName = info.FILENAME.Substring(i + 1);
                item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);

                if (this._state == Constants.FileStates.FileBrowse)
                    item.ISBROWSE = true;
                item.State = Constants.FileStates.DataLoaded;
                
               
               
            }
        }
        //添加一个新文件到 容器
        public new void Add(UserFile item)
        {
            //Listen to the property changed for each added item
            item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);            
            base.Add(item);

            item.State = Constants.FileStates.Pending;
            if (FileAdded != null)
                FileAdded(this, null);
        }

        // 从容器中移除,不删除服务器 相关数据
        public new void Remove(UserFile item)
        {
            base.Remove(item);
            if (FileRemoved != null)
                FileRemoved(this, null);
        }  
        public new void Clear()
        {
            base.Clear();
            if (FileRemoved != null)
                FileRemoved(this, null);
        }
        /// <summary>
        /// Start uploading files
        /// </summary>
        public void UploadFiles()
        {
            lock (this)
            {
                foreach (UserFile file in this)
                {
                    if (file.State == Constants.FileStates.Pending && _currentUpload < _maxUpload)
                    {
                        file.Upload(_customParams);
                        _currentUpload++;
                    }
                    else if (file.State == Constants.FileStates.Deleted || file.State == Constants.FileStates.Deleteing) //物理删除 文件及数据库记录
                    {
                        _currentUpload++;
                        file.Del_FileAndID();
                    }
                }
            }
        }
        /// <summary>
        /// 上传文件，并添加数据库记录
        /// </summary>
        ///
        public void UploadFiles(List<T_SYS_FILEUPLOAD> FileUpload)
        {
            lock (this)
            {
                int i = 0;
                foreach (UserFile file in this)
                {                   
                        if (file.State == Constants.FileStates.Pending && _currentUpload < _maxUpload)
                        {
                            file.Upload(_customParams, FileUpload[i]);
                            i++;
                            _currentUpload++;
                        }
                        else if (file.State == Constants.FileStates.Deleted || file.State == Constants.FileStates.Deleteing) //物理删除 文件及数据库记录
                        {
                            _currentUpload++;
                            file.Del_FileAndID();
                        }
                }
            }
        }
        //上传文件，并添加数据库记录
        public void UploadFiles(T_SYS_FILEUPLOAD[] FileUpload){
            lock (this)
            {
                int i = 0;
                foreach (UserFile file in this)
                {
                    if (file.State == Constants.FileStates.Pending && _currentUpload < _maxUpload)
                    {
                        file.Upload(_customParams, FileUpload[i]);
                        i++;
                        _currentUpload++;
                    }
                    else if (file.State == Constants.FileStates.Deleted || file.State == Constants.FileStates.Deleteing) //物理删除 文件及数据库记录
                    {
                        _currentUpload++;
                        file.Del_FileAndID();
                    }
                }
            }
        }

        /// <summary>
        /// Recount statistics
        /// </summary>
        private void RecountTotal()
        {
            double totalSize = 0;
            double totalSizeDone = 0;
            foreach (UserFile file in this)
            {
                totalSize += file.FileSize;
                totalSizeDone += file.BytesUploaded;
            }
            double percentage = 0;

            if (totalSize > 0)
                percentage = 100 * totalSizeDone / totalSize;
            BytesUploaded = totalSizeDone;
            Percentage = (int)percentage;
        }

        /// <summary>
        /// Check if all files are finished uploading
        /// </summary>
        private void AreAllFilesFinished()
        {
            if (Percentage == 100)
                if (AllFilesFinished != null)
                    AllFilesFinished(this, null);
        }

        //容器更改事件
        private void FileCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Recount total when the collection changed (items added or deleted)
            RecountTotal();
        }
        //属性更改事件
        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                UserFile file = (UserFile)sender;
                if (file.State == Constants.FileStates.Finished)
                {
                    _currentUpload--;
                    TotalUploadedFiles++;

                    UploadFiles();

                    if (SingleFileUploadFinished != null)
                        SingleFileUploadFinished(this, null);
                    if (_currentUpload == 0)
                        if (AllFilesFinished != null)
                        {
                            FileCountEventArgs ev = new FileCountEventArgs();
                            ev.Counted = TotalUploadedFiles;
                            AllFilesFinished(this, ev);
                        }
                }
                else if (file.State == Constants.FileStates.Deleteing) //逻辑删除
                {
                    if (!_isDeleting)
                        file.State = Constants.FileStates.Deleted;
                }
                else if (file.State == Constants.FileStates.Deleted)
                {
                    file.Del_FileAndID();
                    this.Remove(file);
                    file = null;
                }
                else if (file.State == Constants.FileStates.DeleteFinished)
                {
                    _currentUpload--;
                    TotalUploadedFiles++;
                    if (_currentUpload == 0)
                        if (AllFilesFinished != null)
                        {
                            FileCountEventArgs ev = new FileCountEventArgs();
                            ev.Counted = TotalUploadedFiles;
                            AllFilesFinished(this, ev);
                        }
                }
                else if (file.State == Constants.FileStates.Remove)
                {
                    this.Remove(file);
                    file = null;
                }
                else if (file.State == Constants.FileStates.Error)
                {
                    _currentUpload--;

                    UploadFiles();

                    if (ErrorOccurred != null)
                        ErrorOccurred(this, null);
                }
                if (StateChanged != null)
                    StateChanged(this, null);
               // AreAllFilesFinished();
            }
            else if (e.PropertyName == "BytesUploaded")
                RecountTotal();

            if (Event_showPnl != null)
                Event_showPnl(this, null);
        }
    }
}