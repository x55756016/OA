using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
//using SMT.SaaS.FrameworkUI.FileUpload;
using SMT.Saas.Tools.FileUploadWS;
using System.IO;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class AddMeetingContentForm : BaseForm,IClient, IEntityEditor
    {
        #region  上传附件自定义变量
        private int _maxUpload = 2;
        private string _customParams = null;
        private string _fileFilter = null;
        private bool _HttpUploader = true;
        private string _uploadHandlerName = null;
        private int _maxFileSize = int.MaxValue;
        //private FileCollection _files;
        //private UserFile[] _filesarr;

        [ScriptableMember()]
        public event EventHandler MaximumFileSizeReached;
        #endregion

        public T_SYS_FILEUPLOAD[] _FileUpload;
        private T_OA_MEETINGCONTENT tmpContentT;
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        public delegate void refreshGridView();
        
        public event refreshGridView ReloadDataEvent;
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;


        //private void Event_AllFilesFinished(object sender, FileCountEventArgs e)
        //{
        //    RefreshUI(RefreshedTypes.ProgressBar);            
        //}
        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        public AddMeetingContentForm(T_OA_MEETINGCONTENT obj)
        {
            InitializeComponent();
            tmpContentT = obj;
            //_files = new FileCollection(_customParams, _maxUpload);
            //FileList.ItemsSource = _files;
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "MeetingApp";
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            //ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            //this.lblTitle.Text = "更新个人会议内容";
            this.txtContent.Text = HttpUtility.HtmlDecode( obj.CONTENT);

            MeetingClient.MeetingContentInfoUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ContentClient_MeetingContentInfoUpdateCompleted);
        }



        private void UpdateContent()
        {
            string StrContent = "";
            StrContent = HttpUtility.HtmlEncode( this.txtContent.Text.Trim().ToString());
            if (!string.IsNullOrEmpty(StrContent))
            {
                tmpContentT.CONTENT = StrContent;
                //tmpContentT.
                RefreshUI(RefreshedTypes.ShowProgressBar);
                MeetingClient.MeetingContentInfoUpdateAsync(tmpContentT);

            }
        }

        void ContentClient_MeetingContentInfoUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                //MessageBox.Show("更新成功！");
                //UploadFiles();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("UPDATESUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED","MEETINGCONTENT"));
                
                //ctrFile.FormID = tmpContentT.MEETINGCONTENTID;
                //ctrFile.Save();
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(saveType);
        }

        #region IEntityEditor
        public string GetTitle()
        {  
           return Utility.GetResourceStr("EDITTITLE", "MEETINGCONTENT");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    saveType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    Save();
                    break;
                case "2":
                    UploadFilesFunction();
                    break;

            }
        }
        void UploadFilesFunction()
        {
            System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
            openFileWindow.Multiselect = true;
            //if (openFileWindow.ShowDialog() == true)
            //    foreach (FileInfo file in openFileWindow.Files)
            //       ctrFile.InitFiles(file.Name, file.OpenRead());
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "2",
                Title = Utility.GetResourceStr("UPLOADACCESSORY"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406go.png"
            };
            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };
            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);

            return items;
        }

        private void Close()
        {
            RefreshUI(saveType);
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 确定、取消
        private void Save()
        {
            UpdateContent();
            
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        
        #region 上传附件
        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            SelectUserFiles();
        }

        //选择文件
        private void SelectUserFiles()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            try
            {
                //Check the file filter (filter is used to filter file extensions to select, for example only .jpg files)
                if (!string.IsNullOrEmpty(_fileFilter))
                    ofd.Filter = _fileFilter;
            }
            catch (ArgumentException ex)
            {
                //User supplied a wrong configuration file
                throw new Exception("Wrong file filter configuration.", ex);
            }
            if (ofd.ShowDialog() == true)
            {
                //初始化数组长度
                //_filesarr = new UserFile[ofd.Files.Count()];
                //_FileUpload = new T_SYS_FILEUPLOAD[ofd.Files.Count()];
                //int i = 0;      //文件个数
                ////_files.Clear();
                //foreach (FileInfo file in ofd.Files)
                //{
                //    string fileName = file.Name;
                //    //Create a new UserFile object
                //    UserFile userFile = new UserFile();
                //    userFile.FileName = file.Name;
                //    userFile.FileStream = file.OpenRead();
                //    userFile.UIDispatcher = this.Dispatcher;
                //    userFile.HttpUploader = _HttpUploader;
                //    userFile.UploadHandlerName = _uploadHandlerName;
                //    //Check for the file size limit (configurable)
                //    if (userFile.FileStream.Length <= _maxFileSize)
                //    {
                //        //Add to the list
                //        _files.Add(userFile);
                //        //Add to FileUpload 
                //        _filesarr[i] = userFile;
                //        i++;
                //    }
                //    else
                //    {
                //        //HtmlPage.Window.Alert("Maximum file size is: " + (_maxFileSize / 1024).ToString() + "KB.");
                //        Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),"最大上传为：" + (_maxFileSize / 1024).ToString() + "KB.");
                //        if (MaximumFileSizeReached != null)
                //        {
                //            MaximumFileSizeReached(this, null);
                //        }
                //    }
                //}
            }
        }

        //上传附件
        private void UploadFiles()
        {
            //Tell the collection to start uploading
            //if (_files.Count > 0)
            //{
            //    //T_SYS_FILEUPLOAD FileUpload = new T_SYS_FILEUPLOAD();
            //    if (_filesarr.Length > 0)
            //    {
            //        for (int i = 0; i < _filesarr.Length; i++)
            //        {
            //            T_SYS_FILEUPLOAD FileUpload = new T_SYS_FILEUPLOAD();
            //            FileUpload.FILEUPLOADID = System.Guid.NewGuid().ToString();
            //            FileUpload.COMPANYID = tmpContentT.OWNERCOMPANYID;
            //            FileUpload.MODELNAME = "MeetingContent";
            //            FileUpload.FORMID = tmpContentT.MEETINGCONTENTID;
            //            FileUpload.FILENAME = _filesarr[i].FileName;
            //            FileUpload.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            //            FileUpload.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            //            FileUpload.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            //            FileUpload.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //            FileUpload.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //            FileUpload.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            //            FileUpload.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            //            FileUpload.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            //            FileUpload.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //            FileUpload.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //            FileUpload.CREATEDATE = System.DateTime.Now;

            //            _FileUpload[i] = FileUpload;
            //        }
            //        _files.UploadFiles(_FileUpload);
            //    }
            //    _files.Clear();
            //}
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //ctrFile.Load_fileData(tmpContentT.MEETINGCONTENTID);
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            MeetingClient.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
