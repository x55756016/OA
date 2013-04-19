/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-22

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于合同类型的定义的数据信息录入

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Application;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractTypeDefinitionPages : BaseForm,IClient, IEntityEditor
    {

        #region 全局变量
        private SmtOADocumentAdminClient contractClient;
        private T_OA_CONTRACTTYPE ContractTypeNew;
        private FormTypes actions;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        public T_OA_CONTRACTTYPE InfoObj
        {
            get { return ContractTypeNew; }
            set
            {
                this.DataContext = value;
                ContractTypeNew = value;
            }
        }
        #endregion
       
        #region 构造函数
        public ContractTypeDefinitionPages(FormTypes action, T_OA_CONTRACTTYPE contractObj)
        {
            InitializeComponent();
            actions = action;
            this.Loaded += (sender, agrs) =>
            {
                #region 原来的
                InitEvent();
                InfoObj = contractObj;
                if (action == FormTypes.Browse)
                {
                    this.ContractType.IsReadOnly = true;
                    this.cbContractLevel.IsEnabled = false;
                    this.ContractPresentation.IsReadOnly = true;
                }
                #endregion
            };
        }
        #endregion

        #region MyRegion
        private void InitEvent()
        {
            InfoObj = new T_OA_CONTRACTTYPE();
            contractClient = new SmtOADocumentAdminClient();
            contractClient.ContractTypeAddCompleted += new EventHandler<ContractTypeAddCompletedEventArgs>(contractClient_ContractTypeAddCompleted);//添加
            contractClient.UpdateContraTypeCompleted += new EventHandler<UpdateContraTypeCompletedEventArgs>(contractClient_UpdateContraTypeCompleted);//修改
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条

                   
                    if (cbContractLevel.SelectedIndex <= 0)//合同级别
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CONTRACTLEVEL"));
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return;
                    }
                    if (actions == FormTypes.New)
                    {
                        InfoObj.CONTRACTTYPEID = System.Guid.NewGuid().ToString();
                        InfoObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                        InfoObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        InfoObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                        InfoObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                        InfoObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        InfoObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        InfoObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        InfoObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        InfoObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        InfoObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建岗位ID
                        InfoObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                        contractClient.ContractTypeAddAsync(InfoObj);
                    }
                    else
                    {
                        InfoObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                        InfoObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名

                        contractClient.UpdateContraTypeAsync(InfoObj);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("保存Save函数", "OA", "T_OA_CONTRACTTYPE", "保存合同类型返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 添加
        void contractClient_ContractTypeAddCompleted(object sender, ContractTypeAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CONTRACTTYPE"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CONTRACTTYPE"));
                            this.actions = FormTypes.Edit;
                        }
                    }
                    RefreshUI(refreshType);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.ToString())); 
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("添加合同类型Completed事件", "OA", "T_OA_CONTRACTTYPE", "添加合同类型返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 修改
        void contractClient_UpdateContraTypeCompleted(object sender, UpdateContraTypeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CONTRACTTYPE"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CONTRACTTYPE"));
                        }
                    }
                    RefreshUI(refreshType);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.ToString()));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("修改合同类型Completed事件", "OA", "T_OA_CONTRACTTYPE", "修改合同类型返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
            }
            return true;
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "CONTRACTTYPE");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "CONTRACTTYPE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "CONTRACTTYPE");
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

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (actions != FormTypes.Browse)
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
        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            contractClient.DoClose();
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
