using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PersonnelWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class WorkerCordForm : BaseForm, IClient, IEntityEditor
    {
        private string editStates = null;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private PersonnelServiceClient client = new PersonnelServiceClient();
        private V_EMPLOYEEPOST employeepost;

        public WorkerCordForm()
        {
            InitializeComponent();
        }

        public WorkerCordForm(T_OA_WORKRECORD selWorkerCord, string editStatue)
            : this()
        {
            workerCordInfo = selWorkerCord;
            editStates = editStatue;
            this.Loaded += new System.Windows.RoutedEventHandler(WorkerCordForm_Loaded);
        }

        void WorkerCordForm_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (workerCordInfo == null && editStates == "add")
            {
                txtContent.Text = "";
                dtiDateTime.DateTimeValue = System.DateTime.Now;
                client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
            }
            if (workerCordInfo != null && editStates == "update")
            {
                txtContent.Text = workerCordInfo.CONTENT;
                txtTitle.Text = workerCordInfo.TITLE;
                txtCreateUserName.Text = workerCordInfo.CREATEUSERNAME;
                dtiDateTime.DateTimeValue = Convert.ToDateTime(workerCordInfo.PLANTIME);
            }
            if (workerCordInfo != null && editStates == "view")
            {
                txtContent.Text = workerCordInfo.CONTENT;
                txtTitle.Text = workerCordInfo.TITLE;
                txtCreateUserName.Text = workerCordInfo.CREATEUSERNAME;
                dtiDateTime.DateTimeValue = Convert.ToDateTime(workerCordInfo.PLANTIME);
                this.IsEnabled = false;
            }
            workerCordManager.WorkerCordAddCompleted += new EventHandler<WorkerCordAddCompletedEventArgs>(workerCordManager_WorkerCordAddCompleted);
            workerCordManager.WorkerCordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(workerCordManager_WorkerCordUpdateCompleted);
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
        }
        private T_OA_WORKRECORD workerCordInfo = null;
        private SmtOAPersonOfficeClient workerCordManager = new SmtOAPersonOfficeClient();

        //验证输入
        private string Check()
        {
            if (txtContent.Text.Trim() == "" || txtContent.Text.Trim() == string.Empty)
            {
                return Utility.GetResourceStr("REQUIRED", "WorkCordContent");
            }

            if (string.IsNullOrEmpty(dtiDateTime.dpDate.Text) || dtiDateTime.tpTime.Value.IsNull())
            {
                return Utility.GetResourceStr("REQUIRED", "WorkCordDateTime");
            }
            return null;
        }

        #region 获取当前用户信息
        void client_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeepost = e.Result;
                GetAllPost(e.Result);
            }
        }
        private void GetAllPost(V_EMPLOYEEPOST ent)//获取当前员工、公司、岗位、部门、联系电话
        {
            if (ent != null && ent.EMPLOYEEPOSTS != null)
            {
                txtCreateUserName.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
            }
        }
        #endregion

        private int CheckInput()
        {
            if (txtTitle.Text.Trim() == string.Empty)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "WorkCordTitle"));
                return -1;
            }
            if (dtiDateTime.DateTimeValue == new DateTime(1, 1, 1, 0, 0, 0))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "STARTTIME"));
                return -1;
            }
            if (txtContent.Text.Trim() == string.Empty)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "WorkCordContent"));
                return -1;
            }
            return 1;
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("WorkerCord");
        }
        public string GetStatus()
        {
            return "";
        }
        public void DoAction(string actionType)
        {
            string errorString = Check();
            if (errorString != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(errorString));
                return;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            foreach (var h in validators)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                return;
            }

            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1": refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (editStates != "view")
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
            if (editStates == "add")//增加
            {
                workerCordInfo = new T_OA_WORKRECORD();
            }
            workerCordInfo.TITLE = txtTitle.Text;
            workerCordInfo.CONTENT = txtContent.Text.Trim();
            workerCordInfo.PLANTIME = dtiDateTime.DateTimeValue;//new DateTime(dtSelDate.Year, dtSelDate.Month, dtSelDate.Day, Convert.ToInt32(txtTimeHour.Text.Trim()), Convert.ToInt32(txtTimeMin.Text.Trim()), 0);

            if (editStates == "add")//增加
            {
                workerCordInfo.WORKRECORDID = System.Guid.NewGuid().ToString();
                workerCordInfo.CREATEDATE = System.DateTime.Now;
                workerCordInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                workerCordInfo.CREATEDATE = System.DateTime.Now;
                workerCordInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                workerCordInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                workerCordInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                workerCordInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                workerCordInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                workerCordInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                workerCordInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                workerCordInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                workerCordInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                workerCordInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                workerCordInfo.UPDATEDATE = System.DateTime.Now;
                workerCordInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                workerCordInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                workerCordManager.WorkerCordAddAsync(workerCordInfo);
            }
            else//修改
            {
                workerCordInfo.UPDATEDATE = System.DateTime.Now;
                workerCordInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                workerCordInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                workerCordManager.WorkerCordUpdateAsync(workerCordInfo);
            }
        }

        void workerCordManager_WorkerCordAddCompleted(object sender, WorkerCordAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (e.Result > 0)
                {
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        editStates = "update";
                        Utility.ShowMessageBox("ADD", false, true);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("ADD", false, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        private void workerCordManager_WorkerCordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowMessageBox("UPDATE", false, true);
                RefreshUI(refreshType);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", false, false);
            }
        }
        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            workerCordManager.DoClose();
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
