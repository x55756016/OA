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
    public partial class GradeForm : BaseForm,IClient, IEntityEditor
    {
        #region 初始化变量
        private T_OA_GRADED tmpGradeT = new T_OA_GRADED();
        private SmtOACommonOfficeClient GradeClient = new SmtOACommonOfficeClient();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private Action action;
        private SMTLoading loadbar = new SMTLoading(); 
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
        public GradeForm(Action actionenum, T_OA_GRADED TypeObj)
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            GradeClient.GradeAddCompleted += new EventHandler<GradeAddCompletedEventArgs>(GradeClient_GradeAddCompleted);
            GradeClient.GradeInfoUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(GradeClient_GradeInfoUpdateCompleted);
            GradeClient.GetGradeSingleInfoByIdCompleted += new EventHandler<GetGradeSingleInfoByIdCompletedEventArgs>(GradeClient_GetGradeSingleInfoByIdCompleted);
            action = actionenum;
            loadbar.Start();
            switch (actionenum)
            { 
                case Action.Add:
                    this.tblTitle.Text = Utility.GetResourceStr("ADDTITLE", "GRADENAME");
                    break;
                case Action.Edit:
                    this.tblTitle.Text = Utility.GetResourceStr("ADDTITLE", "GRADENAME");
                    GetGradeDetailInfo(TypeObj);
                    break;
                case Action.Read:
                    this.tblTitle.Text = Utility.GetResourceStr("VIEWTITLE", "GRADENAME");
                    this.txtContent.IsEnabled = false;
                    GetGradeDetailInfo(TypeObj);
                    break;
            }
            
            loadbar.Stop();
        }

        void GradeClient_GetGradeSingleInfoByIdCompleted(object sender, GetGradeSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpGradeT = e.Result;
                }
            }
        }

        void GradeClient_GradeInfoUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "GRADENAME"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error,Utility.GetResourceStr("ERROR"),e.Error.ToString());
                    return;
                }
            }
            //throw new NotImplementedException();
        }
        #endregion

        #region 获取级别信息
        private void GetGradeDetailInfo(T_OA_GRADED TypeObj)
        {
            this.txtContent.Text = TypeObj.GRADED;
            this.txtContent.IsEnabled = false;
            tmpGradeT = TypeObj;
        }
        #endregion

        #region 添加按钮事件
        private void SaveGrade()
        {
            string StrTitle = "";
            StrTitle = this.txtContent.Text.Trim().ToString();
            if (CheckGrede())
            {
                if (!string.IsNullOrEmpty(StrTitle))
                {
                    tmpGradeT.GRADED = StrTitle;
                    if (action == Action.Add)
                    {
                        tmpGradeT.GRADEDID = System.Guid.NewGuid().ToString();
                        

                        tmpGradeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpGradeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpGradeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        tmpGradeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpGradeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        tmpGradeT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpGradeT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpGradeT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpGradeT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        tmpGradeT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                        tmpGradeT.UPDATEUSERNAME = "";
                        tmpGradeT.CREATEDATE = System.DateTime.Now;
                        tmpGradeT.UPDATEDATE = null;
                        tmpGradeT.UPDATEUSERID = "";


                        GradeClient.GradeAddAsync(tmpGradeT);
                    }
                    if (action == Action.Edit)
                    {
                        tmpGradeT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;                        
                        tmpGradeT.UPDATEDATE = System.DateTime.Now;
                        tmpGradeT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        GradeClient.GradeInfoUpdateAsync(tmpGradeT);
                    }
                }
                
            }
            
        }

        void GradeClient_GradeAddCompleted(object sender, GradeAddCompletedEventArgs e)
        {
            
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    //MessageBox.Show("添加级别名称成功");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "GRADENAME"));
                    action = Action.Edit;
                    GradeClient.GetGradeSingleInfoByIdAsync(tmpGradeT.GRADEDID);
                    RefreshUI(saveType);
                    
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "GRADENAME"));
                    return;
                }
            }
            loadbar.Stop();
        }

        private bool CheckGrede()
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

        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "GRADENAME");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "GRADENAME");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "GRADENAME");
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
            SaveGrade();             
        }

        
        #endregion


        #region IForm 成员

        public void ClosedWCFClient()
        {
            GradeClient.DoClose();
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
