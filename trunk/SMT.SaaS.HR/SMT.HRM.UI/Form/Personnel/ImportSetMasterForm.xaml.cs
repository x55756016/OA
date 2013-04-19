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

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using System.Reflection;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class ImportSetMasterForm : BaseForm, IEntityEditor, IClient
    {
        public FormTypes FormType { get; set; }
        public T_HR_IMPORTSETMASTER Master { get; set; }
        ObservableCollection<T_HR_IMPORTSETDETAIL> detailList = new ObservableCollection<T_HR_IMPORTSETDETAIL>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private string importSetID;
        PersonnelServiceClient client;
        SMT.Saas.Tools.HrCommonServiceWS.HrCommonServiceClient commonClient;
        public ImportSetMasterForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            importSetID = strID;
            InitEnvent(strID);
        }

        private void InitEnvent(string strID)
        {
            client = new PersonnelServiceClient();
            client.GetImportSetMasterByIDCompleted += new EventHandler<GetImportSetMasterByIDCompletedEventArgs>(client_GetImportSetMasterByIDCompleted);
            client.GetImportSetDetailByMasterIDCompleted += new EventHandler<GetImportSetDetailByMasterIDCompletedEventArgs>(client_GetImportSetDetailByMasterIDCompleted);
            client.ImportSetMasterUpdateCompleted += new EventHandler<ImportSetMasterUpdateCompletedEventArgs>(client_ImportSetMasterUpdateCompleted);
            client.ImportSetMasterAddCompleted += new EventHandler<ImportSetMasterAddCompletedEventArgs>(client_ImportSetMasterAddCompleted);
            commonClient = new Saas.Tools.HrCommonServiceWS.HrCommonServiceClient();
            commonClient.GetEntityProNameByEnityNameCompleted += new EventHandler<Saas.Tools.HrCommonServiceWS.GetEntityProNameByEnityNameCompletedEventArgs>(commonClient_GetEntityProNameByEnityNameCompleted);
            this.Loaded += new RoutedEventHandler(ImportSetMasterForm_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse)
            {
                this.IsEnabled = false;

            }
            */
            #endregion
        }

        void ImportSetMasterForm_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            if (FormType == FormTypes.Browse)
            {
                this.IsEnabled = false;

            }
            #endregion

            if (FormType == FormTypes.New)
            {
                Master = new T_HR_IMPORTSETMASTER();
                Master.MASTERID = Guid.NewGuid().ToString();
                ///TODO:EDIT,现在是固定的T_HR_PENSIONDETAIL,可修改成LOOKUP再调用
                Master.ENTITYNAME = "员工社保记录";
                Master.ENTITYCODE = "T_HR_PENSIONDETAIL";
                BindData();
                RefreshUI(RefreshedTypes.ShowProgressBar);
                commonClient.GetEntityProNameByEnityNameAsync("T_HR_PENSIONDETAIL", "PENSIONDETAILID");
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetImportSetMasterByIDAsync(importSetID);
            }
        }

        void commonClient_GetEntityProNameByEnityNameCompleted(object sender, Saas.Tools.HrCommonServiceWS.GetEntityProNameByEnityNameCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    foreach (var str in e.Result)
                    {
                        T_HR_IMPORTSETDETAIL ent = new T_HR_IMPORTSETDETAIL();
                        ent.DETAILID = Guid.NewGuid().ToString();
                        ent.ENTITYCOLUMNCODE = str;
                        ent.ENTITYCOLUMNNAME = Utility.GetResourceStr(str);
                        ent.T_HR_IMPORTSETMASTER = Master;
                        ent.CREATEDATE = DateTime.Now;
                        ent.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        detailList.Add(ent);
                    }
                }
                DtGrid.ItemsSource = detailList;
                DtGrid.DataContext = detailList;
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }




        void client_GetImportSetMasterByIDCompleted(object sender, GetImportSetMasterByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    Master = e.Result;
                    BindData();
                    client.GetImportSetDetailByMasterIDAsync(Master.MASTERID);
                }
            }
        }

        void client_GetImportSetDetailByMasterIDCompleted(object sender, GetImportSetDetailByMasterIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                detailList = e.Result;
                DtGrid.ItemsSource = e.Result;
                DtGrid.DataContext = detailList;
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        private void BindData()
        {
            this.DataContext = Master;
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("IMPORTSETMASTER");
        }
        public string GetStatus()
        {
            //return BlackList != null ? BlackList.BLACKLISTID : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    closeFormFlag = true;
                    Save();
                    // Cancel();
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
            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = Utility.CreateFormSaveButton();
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

        #region IClient
        public void ClosedWCFClient()
        {
            //  throw new NotImplementedException();
            client.DoClose();
            commonClient.DoClose();
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
        private bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (cbxCity.SelectedItem == null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "CITY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            else
            {
                string strMsg = string.Empty;
                if (FormType == FormTypes.New)
                {
                    //所属
                    Master.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    Master.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Master.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Master.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    Master.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Master.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Master.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    Master.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Master.CREATEDATE = DateTime.Now;
                    client.ImportSetMasterAddAsync(Master, detailList, strMsg);
                }
                else
                {
                    Master.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Master.UPDATEDATE = DateTime.Now;
                    client.ImportSetMasterUpdateAsync(Master, detailList, strMsg);
                }
            }
            return true;
        }

        void client_ImportSetMasterUpdateCompleted(object sender, ImportSetMasterUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EXISTIMPORTSETCITY"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
        }

        void client_ImportSetMasterAddCompleted(object sender, ImportSetMasterAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "EXISTIMPORTSETCITY")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EXISTIMPORTSETCITY"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                }
                RefreshUI(RefreshedTypes.All);
            }
        }


        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
    }
}
