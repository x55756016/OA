using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
//using SMT.SaaS.FrameworkUI.FileUpload;
using SMT.Saas.Tools.FileUploadWS;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ArchivesAddForm : BaseForm,IClient, IEntityEditor
    {
        private Action actions;
        private string archiveID;
        private SmtOADocumentAdminClient client;
        public T_SYS_FILEUPLOAD[] _FileUpload;
        private T_OA_ARCHIVES archives;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        FileUploadManagerClient fumClient = new FileUploadManagerClient();
        public T_OA_ARCHIVES Archives
        {
            get { return archives; }
            set
            {
                archives = value;
                this.DataContext = value;
            }
        }


        

        private Items[] comboBoxItem;

        public Items[] ComboBoxItem
        {
            get { return comboBoxItem; }
            set { comboBoxItem = value; }
        }

        #region 初始化

        public ArchivesAddForm(Action action, string archivesID)
        {
            InitializeComponent();
            actions = action;
            archiveID = archivesID;
            this.Loaded+=new RoutedEventHandler(ArchivesAddForm_Loaded);
        }

        void ArchivesAddForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "Archives";
            //ctrFile.InitBtn(Visibility.Visible, Visibility.Collapsed);
            //ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            Utility.CbxItemBinder(comboxType, "RECORDTYPE", "0");
            //_files = new FileCollection(_customParams, _maxUpload);
            //FileList.ItemsSource = _files;
            if (actions == Action.Edit || actions == Action.Read)
            {
                InitData();
                //this.Loaded += new RoutedEventHandler(ArchivesAddForm_Loaded);
                //ctrFile.Load_fileData(archiveID);
                if (actions == Action.Read)
                {
                    this.IsEnabled = false;
                }
            }
            else
            {
                Archives = new T_OA_ARCHIVES();
            }
        }
        //private void Event_AllFilesFinished(object sender, FileCountEventArgs e)
        //{            
            
        //}
        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.AddArchivesCompleted += new EventHandler<AddArchivesCompletedEventArgs>(client_AddArchivesCompleted);
            client.UpdateArchivesCompleted += new EventHandler<UpdateArchivesCompletedEventArgs>(client_UpdateArchivesCompleted);
            client.GetArchivesByIdCompleted += new EventHandler<GetArchivesByIdCompletedEventArgs>(client_GetArchivesByIdCompleted);
        }

        private void BindComboBox()
        {
            Items[] myItem = { new Items("1"), new Items("SENDDOC") };
            comboBoxItem = myItem;
            comboxType.ItemsSource = myItem;
            comboxType.DisplayMemberPath = "recordType";
            comboxType.SelectedIndex = 0;
        }

        private void InitData()
        {
            if (!string.IsNullOrEmpty(archiveID))
            {
                client.GetArchivesByIdAsync(archiveID);
            }
        }
        #endregion

        #region ComboBox定义

        public class Items
        {
            public string recordType { get; set; }
            public Items(string recordTypeInput)
            {
                this.recordType = recordTypeInput;
            }
        }

        private void SetComboBoxSelectIndex(string recordType, ComboBox cbx)
        {
            if (!string.IsNullOrEmpty(recordType))
            {
                foreach (var item in cbx.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYNAME.ToString() == recordType)
                        {
                            cbx.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region  查询、新增、修改完成
        private void client_GetArchivesByIdCompleted(object sender, GetArchivesByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {                    
                    Archives = e.Result;                    
                    SetComboBoxSelectIndex(e.Result.RECORDTYPE, this.comboxType);
                    
                    txtContent.RichTextBoxContext = Archives.CONTENT;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        private void client_AddArchivesCompleted(object sender, AddArchivesCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    //上传附件
                    if (e.Result != "")
                    {                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //ctrFile.FormID = archives.ARCHIVESID;
                        //ctrFile.Save();
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                            RefreshUI(refreshType);
                        }
                        else     //如果是保存则重新取一次数据，并将新增状态改为修改状态
                        {
                            //RefreshUI(RefreshedTypes.ProgressBar);
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                            this.actions = Action.Edit;
                            archiveID = archives.ARCHIVESID;
                            
                            InitData();
                        }
                        //HtmlPage.Window.Alert("档案信息新增成功！");
                        //RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                }
                else
                {
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }


        private void client_UpdateArchivesCompleted(object sender, UpdateArchivesCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    //e.Error
                    //上传附件
                    if (e.Result != "")
                    {
                        //HtmlPage.Window.Alert(e.Result);
                        //RefreshUI(RefreshedTypes.ProgressBar);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //UploadFiles();
                        //HtmlPage.Window.Alert("档案信息修改成功！"); 
                        //ctrFile.FormID = archives.ARCHIVESID;
                        //ctrFile.Save();
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {                            
                            RefreshUI(refreshType);
                        }
                        else
                        {                            
                            InitData();
                        }
                        
                    }
                }
                else
                {
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        #endregion

        
        #region IEntityEditor
        public string GetTitle()
        {
            string StrReturn = "";
            switch (actions)
            { 
                case Action.Add:
                    StrReturn = Utility.GetResourceStr("ADDTITLE", "ARCHIVE");
                    break;
                case Action.Edit:
                    StrReturn= Utility.GetResourceStr("EDITTITLE", "ARCHIVE");
                    break;
                case Action.Read:
                    StrReturn= Utility.GetResourceStr("VIEWTITLE", "ARCHIVE");
                    break;
            }
            return StrReturn;
            
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (actions != Action.Read)
            {

                ToolbarItem item = new ToolbarItem
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
            }
            return items;
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

        #region 保存
        private void Save()
        {
            try
            {                
                if (archives.ARCHIVESTITLE == null || string.IsNullOrEmpty(archives.ARCHIVESTITLE.Trim()))
                {
                    //HtmlPage.Window.Alert("档案标题不能为空!");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ARCHIVESTITLE"));
                    //this.txtTitle.Focus();
                    return;
                }
                if (txtContent.GetRichTextbox().Xaml == "") //if (archives.CONTENT == null || txtContent.GetRichTextbox().Xaml == "")
                {
                    //HtmlPage.Window.Alert("档案内容不能为空!");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ARCHIVESCONTENT"));
                    //this.txtContent.Focus();
                    return;
                }

                //archives.ARCHIVESTITLE = txtTitle.Text.Trim();
                //Items cbxItems = this.comboxType.SelectedItem as Items;
                archives.RECORDTYPE = Utility.GetCbxSelectItemText(comboxType);
                //archives.CONTENT = txtContent.Text.Trim();
                archives.SOURCEFLAG = "1";                
                archives.CONTENT = txtContent.RichTextBoxContext;
                RefreshUI(RefreshedTypes.ProgressBar);
                if (actions == Action.Add) //新增操作
                {
                    //应考虑新增时先点保存再点保存并关闭的情况 此时还是执行新增操作

                    archives.ARCHIVESID = System.Guid.NewGuid().ToString();
                    archives.COMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    archives.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    archives.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    archives.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    archives.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    archives.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    archives.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    archives.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    archives.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    archives.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    archives.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    archives.CREATEDATE = DateTime.Now;
                    client.AddArchivesAsync(archives);
                }
                else                       //更新操作
                {
                    archives.UPDATEDATE = DateTime.Now;
                    archives.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    archives.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    client.UpdateArchivesAsync(archives);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());   
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        //private void SaveAndClose()
        //{                    
        //    Save();
        //}

       
        #endregion

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtContent.Height = ((Grid)sender).ActualHeight * 0.5;
        }



        #region IForm 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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


