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
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class PriorityForm : BaseForm,IClient, IEntityEditor
    {
        #region 初始化变量
        private T_OA_PRIORITIES tmpProritityT = new T_OA_PRIORITIES();
        T_OA_PRIORITIES typeObj;
        private SmtOACommonOfficeClient PrioritityClient = new SmtOACommonOfficeClient();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private Action action;
        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
        #endregion

        #region 构造函数
        public PriorityForm(Action actionenum, T_OA_PRIORITIES typeObj)
        {
            InitializeComponent();
            action = actionenum;
            this.typeObj = typeObj;
            this.Loaded += new RoutedEventHandler(PriorityForm_Loaded);
        }

        void PriorityForm_Loaded(object sender, RoutedEventArgs e)
        {
            PrioritityClient.DocPriorityAddCompleted += new EventHandler<DocPriorityAddCompletedEventArgs>(PrioritityClient_DocPriorityAddCompleted);
            PrioritityClient.DocPriorityInfoUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(PrioritityClient_DocPriorityInfoUpdateCompleted);
            switch (action)
            {
                case Action.Add:
                    this.tblTitle.Text = Utility.GetResourceStr("ADDTITLE", "PRIORITYNAME");
                    break;
                case Action.Edit:
                    this.tblTitle.Text = Utility.GetResourceStr("EDITTITLE", "PRIORITYNAME");
                    GetDocTypeDetailInfo(typeObj);
                    break;
                case Action.Read:
                    this.tblTitle.Text = Utility.GetResourceStr("VIEWTITLE", "PRIORITYNAME");
                    this.txtcontent.IsEnabled = false;
                    GetDocTypeDetailInfo(typeObj);
                    break;
            }
        }

        void PrioritityClient_DocPriorityInfoUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PRIORITYNAME"));                    
                    RefreshUI(saveType);
                }
            }
        }
        
        

        private void GetDocTypeDetailInfo(T_OA_PRIORITIES TypeObj)
        {
            this.txtcontent.Text = TypeObj.PRIORITIES;
            this.txtcontent.IsEnabled = false;
            tmpProritityT = TypeObj;
        }
        #endregion

        #region 保存缓急信息

        private void SavePriorities()
        {
            string StrTitle = "";
            StrTitle = this.txtcontent.Text.Trim().ToString();
            if (CheckPriority())
            {

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    RefreshUI(RefreshedTypes.ProgressBar);
                    if (action == Action.Add)
                    {
                        tmpProritityT.PRIORITIESID = System.Guid.NewGuid().ToString();
                        tmpProritityT.PRIORITIES = StrTitle;

                        tmpProritityT.CREATEDATE = System.DateTime.Now;

                        tmpProritityT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpProritityT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpProritityT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        tmpProritityT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpProritityT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        tmpProritityT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpProritityT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpProritityT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpProritityT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        tmpProritityT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                        tmpProritityT.UPDATEUSERNAME = "";
                        tmpProritityT.UPDATEDATE = null;
                        tmpProritityT.UPDATEUSERID = "";


                        PrioritityClient.DocPriorityAddAsync(tmpProritityT);
                    }
                    if (action == Action.Edit)
                    {
                        tmpProritityT.PRIORITIES = StrTitle;

                        tmpProritityT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpProritityT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpProritityT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        tmpProritityT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpProritityT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        
                        tmpProritityT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpProritityT.UPDATEDATE = System.DateTime.Now;
                        tmpProritityT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        PrioritityClient.DocPriorityInfoUpdateAsync(tmpProritityT);
                    }
                }
                
            }
        }

        void PrioritityClient_DocPriorityAddCompleted(object sender, DocPriorityAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    action = Action.Edit;
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "PRIORITY"));
                    RefreshUI(saveType);
                    //RefreshUI(RefreshedTypes.CloseAndReloadData);   //不让修改
                    
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "PRIORITYNAME"));
                    return;
                }
            }

        }
        #endregion

        #region IEntityEditor
        private bool CheckPriority()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            return true;
        }
        public string GetTitle()
        {            
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "PRIORITY");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "PRIORITY");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "PRIORITY");
            }
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
                    saveType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
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
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (action != Action.Read && action != Action.Edit)
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

        #region 确定、取消
        private void Save()
        {
             SavePriorities();             
        }

        #endregion



        #region IForm 成员

        public void ClosedWCFClient()
        {
            PrioritityClient.DoClose();
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
