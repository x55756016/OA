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

using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ArchivesReturnForm : BaseForm,IClient,IEntityEditor
    {

        private string lendingID; //借阅ID
        private SmtOADocumentAdminClient client;
        //private V_ArchivesLending archiveLending;
        private T_OA_LENDARCHIVES lendingArchives;
        private RefreshedTypes refreshType = RefreshedTypes.All;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        #region 初始化
        public ArchivesReturnForm(string lendingID)
        {
            InitializeComponent();
            this.lendingID = lendingID;
            this.Loaded += new RoutedEventHandler(ArchivesReturnForm_Loaded);
        }

        void ArchivesReturnForm_Loaded(object sender, RoutedEventArgs e)
        {
            sDate.IsEnabled = false;
            txtTitle.IsEnabled = false;
            txtCompany.IsEnabled = false;
            InitEvent();
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetLendingListByLendingIdCompleted += new EventHandler<GetLendingListByLendingIdCompletedEventArgs>(client_GetLendingListByLendingIdCompleted);
            client.AddArchivesReturnCompleted += new EventHandler<AddArchivesReturnCompletedEventArgs>(client_AddArchivesReturnCompleted);
            client.GetLendingListByLendingIdAsync(lendingID);
        }
        #endregion

        #region 完成事件
        private void client_AddArchivesReturnCompleted(object sender, AddArchivesReturnCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result == "")
                    {
                        //HtmlPage.Window.Alert("归还档案成功！");
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("RETRUNSUCCESSED", "ARCHIVE"));
                        }
                        else
                        {
                            client.GetLendingListByLendingIdAsync(lendingID);
                        }
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        //HtmlPage.Window.Alert(e.Result);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetLendingListByLendingIdCompleted(object sender, GetLendingListByLendingIdCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    lendingArchives = e.Result.ToList().First();
                    this.txtTitle.Text = lendingArchives.T_OA_ARCHIVES.ARCHIVESTITLE;
                    this.txtCompany.Text = Utility.GetCompanyName(lendingArchives.T_OA_ARCHIVES.COMPANYID);
                    this.sDate.SelectedDate = lendingArchives.STARTDATE;
                    this.eDate.SelectedDate = lendingArchives.PLANENDDATE;
                    BindAduitInfo();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 自定义函数
        /// <summary>
        /// 绑定审核控件数据
        /// </summary>
        public void BindAduitInfo()
        {
            SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
            entity.ModelCode = "archivesLending";
            //entity.FormID = lendingArchives.LENDARCHIVESID;
            //entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            //entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
            //entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
            //entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
            //entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
            entity.FormID = lendingArchives.LENDARCHIVESID;
            entity.CreateCompanyID = lendingArchives.OWNERCOMPANYID;
            entity.CreateDepartmentID = lendingArchives.OWNERDEPARTMENTID;
            entity.CreatePostID = lendingArchives.OWNERPOSTID;
            entity.CreateUserID = lendingArchives.OWNERID;
            entity.CreateUserName = lendingArchives.OWNERNAME;
            entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
            audit.BindingData();
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            try
            {
                //RefreshUI(RefreshedTypes.ProgressBar);
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ProgressBar);
                    if (lendingArchives != null)
                    {
                        lendingArchives.ENDDATE = Convert.ToDateTime(this.eDate.SelectedDate);
                        lendingArchives.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;                        
                        lendingArchives.UPDATEDATE = DateTime.Now;                        
                        lendingArchives.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        client.AddArchivesReturnAsync(lendingArchives);
                    }
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        public bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    //HtmlPage.Window.Alert(h.ErrorMessage);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            if (Convert.ToDateTime(sDate.SelectedDate.ToString()) > Convert.ToDateTime(eDate.SelectedDate.ToString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEGREATERERROR", "LENDTIME,EXPECTEDRETURNTIME"));
                return false;
            }
            return true;
        }
        #endregion


        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("ARCHIVERETURN");
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
                    //refreshType = RefreshedTypes.All;
                    refreshType = RefreshedTypes.CloseAndReloadData;
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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                //Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                //ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
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
