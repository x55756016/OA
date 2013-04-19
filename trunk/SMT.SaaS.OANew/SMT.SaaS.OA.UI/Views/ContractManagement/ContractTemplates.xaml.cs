/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-22

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于合同模板的信息录入，根据不同的合同类型创建合同模板

*********************************************************************************/
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
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Application;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractTemplates : BaseForm, IClient, IEntityEditor
    {

        #region 全局变量

        private SmtOADocumentAdminClient ContractManagement;
        private T_OA_CONTRACTTEMPLATE ContractTemplateNew;
        private FormTypes actions;//操作类型
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        public T_OA_CONTRACTTEMPLATE InfoObj
        {
            get { return ContractTemplateNew; }
            set
            {
                this.DataContext = value;
                ContractTemplateNew = value;
            }
        }
        #endregion

        #region 构造函数

        public ContractTemplates(FormTypes action, V_ContractTemplate TemplageObj)
        {
            InitializeComponent();
            actions = action;
            this.Loaded += (sender, args) =>
            {
                #region 原来的

                InitEvent();
                if (action != FormTypes.New)
                {
                    InfoObj = TemplageObj.contractTemplate;
                    InfoObj.T_OA_CONTRACTTYPE = new T_OA_CONTRACTTYPE();
                    InfoObj.T_OA_CONTRACTTYPE.CONTRACTTYPEID = TemplageObj.contractType;
                }
                if (action == FormTypes.Browse)
                {
                    tbxContractTypeID.IsEnabled = false;
                    cbContractLevel.IsEnabled = false;
                    tbxTemplateName.IsReadOnly = true;
                    tbxContractTitle.IsReadOnly = true;
                    tbxContractText.IsEnabled = false;
                    this.tbxContractText.GetRichTextbox().IsEnabled = false;
                }
                if (action == FormTypes.Edit || action == FormTypes.Audit || action == FormTypes.Browse)
                {
                    tbxContractText.RichTextBoxContext = InfoObj.CONTENT;
                }
                #endregion
            };
        }
        #endregion

        #region 初始化

        private void InitEvent()
        {
            ContractManagement = new SmtOADocumentAdminClient();
            InfoObj = new T_OA_CONTRACTTEMPLATE();
            ContractManagement.GetContractTypeNameInfosToComboxAsync();
            ContractManagement.ContractTemplateAddCompleted += new EventHandler<ContractTemplateAddCompletedEventArgs>(ctsc_ContractTemplateAddCompleted);//添加
            ContractManagement.UpdateContraTemplateCompleted += new EventHandler<UpdateContraTemplateCompletedEventArgs>(ContractManagement_UpdateContraTemplateCompleted);//修改
            ContractManagement.GetContractTypeNameInfosToComboxCompleted += new EventHandler<GetContractTypeNameInfosToComboxCompletedEventArgs>(cmfc_GetContractTypeNameInfosToComboxCompleted);
        }

        void cmfc_GetContractTypeNameInfosToComboxCompleted(object send, GetContractTypeNameInfosToComboxCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        tbxContractTypeID.ItemsSource = e.Result;
                        if (actions != FormTypes.New)
                        {
                            T_OA_CONTRACTTYPE ent;
                            string TypeID = string.Empty;
                            for (int i = 0; i < tbxContractTypeID.Items.Count; i++)
                            {
                                ent = ((T_OA_CONTRACTTYPE)tbxContractTypeID.Items[i]) as T_OA_CONTRACTTYPE;
                                TypeID = ent.CONTRACTTYPE;

                                if (TypeID == InfoObj.T_OA_CONTRACTTYPE.CONTRACTTYPEID)
                                {
                                    tbxContractTypeID.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (tbxContractTypeID.Items.Count > 0)
                            {
                                tbxContractTypeID.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("获取合同类型名称Completed事件", "OA", "T_OA_CONTRACTTEMPLATE", "在合同模版中获取合同类型名称返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
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

                    if (actions == FormTypes.New)
                    {
                        InfoObj.CONTRACTTEMPLATEID = System.Guid.NewGuid().ToString();//模板编号
                        if (tbxContractTypeID.SelectedItem == null) return;
                        T_OA_CONTRACTTYPE ent = tbxContractTypeID.SelectedItem as T_OA_CONTRACTTYPE;
                        InfoObj.T_OA_CONTRACTTYPE = ent; ;//类型编号                        
                        InfoObj.CONTENT = tbxContractText.RichTextBoxContext;
                        InfoObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                        InfoObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        InfoObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                        InfoObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                        InfoObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        InfoObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        InfoObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        InfoObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        InfoObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        InfoObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                        InfoObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                        ContractManagement.ContractTemplateAddAsync(InfoObj);
                    }
                    else
                    {
                        if (tbxContractTypeID.SelectedItem == null) return;
                        T_OA_CONTRACTTYPE ent = tbxContractTypeID.SelectedItem as T_OA_CONTRACTTYPE;

                        InfoObj.CONTENT = tbxContractText.RichTextBoxContext;
                        InfoObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                        InfoObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名
                        InfoObj.T_OA_CONTRACTTYPE = ent;

                        ContractManagement.UpdateContraTemplateAsync(InfoObj);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("保存Save函数", "OA", "T_OA_CONTRACTTEMPLATE", "保存时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 添加
        void ctsc_ContractTemplateAddCompleted(object sender, ContractTemplateAddCompletedEventArgs e)
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
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CONTRACTTEMPLATE"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CONTRACTTEMPLATE"));
                            this.actions = FormTypes.Edit;
                        }
                    }
                    RefreshUI(refreshType);
                    RefreshUI(RefreshedTypes.HideProgressBar);//数据完成后隐藏进度条
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.ToString()));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("添加合同模版Completed事件", "OA", "T_OA_CONTRACTTEMPLATE", "添加合同模版时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 修改
        void ContractManagement_UpdateContraTemplateCompleted(object sender, UpdateContraTemplateCompletedEventArgs e)
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
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CONTRACTTEMPLATE"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CONTRACTTEMPLATE"));
                        }
                    }
                    RefreshUI(refreshType);
                    RefreshUI(RefreshedTypes.HideProgressBar);//数据完成后隐藏进度条
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.ToString()));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("修改合同模版Completed事件", "OA", "T_OA_CONTRACTTEMPLATE", "修改合同模版时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            string tbxTemplateName = "";  //合同模板

            if (string.IsNullOrEmpty(this.tbxTemplateName.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "OATEMPLATENAME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);//模板名称
                this.tbxTemplateName.Focus();
                return false;
            }
            else
            {
                tbxTemplateName = this.tbxTemplateName.Text.ToString();
            }
            if (this.tbxContractText.GetRichTextbox().Xaml == "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "THECONTRACT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);//合同内容
                this.tbxContractText.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(this.tbxContractTitle.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TEMPLATETITLE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);//标题
                this.tbxContractTitle.Focus();
                return false;
            }
            if (cbContractLevel.SelectedIndex <= 0)//合同级别
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "CONTRACTLEVEL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.cbContractLevel.Focus();
                return false;
            }

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
                return Utility.GetResourceStr("ADDTITLE", "CONTRACTTEMPLATE");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "CONTRACTTEMPLATE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "CONTRACTTEMPLATE");
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
        #endregion

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tbxContractText.Height = ((Grid)sender).ActualHeight * 0.5;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            ContractManagement.DoClose();
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
