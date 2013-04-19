//**满意度调查申请发布子页面***
//负责人:lezy
//创建时间:2011-6-9
//完成时间：2011-6-30
//**************************
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.AgentChannel;
using FrUi = SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Validator;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SatisfactionDistributeChildWindow : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量定义
        SmtOAPersonOfficeClient client=null;
        PermissionServiceClient permissionClient=null;
        SelectScheme scheme=null;
        FormTypes actionType=0;
        string formId = string.Empty;
        string distributeId=string.Empty;
        T_OA_SATISFACTIONDISTRIBUTE distributeEntity;
        List<ExtOrgObj> lookupObjectList=null;
        ObservableCollection<T_OA_DISTRIBUTEUSER> distributeuserList=null;
        V_Satisfactions distributeView = null;
        ObservableCollection<V_Satisfactions> dataViewList=null;
        T_OA_DISTRIBUTEUSER distribute=null;
        #endregion

        #region 构造函数
        public SatisfactionDistributeChildWindow(FormTypes actionType, string distributeId)
        {
            InitializeComponent();
            this.actionType = actionType;
            this.distributeId = distributeId;
            this.Loaded += new RoutedEventHandler(SatisfactionDistributeChildWindow_Loaded);
         }
        #endregion

        #region 事件注册
        void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            permissionClient = new PermissionServiceClient();
            client.AddSatisfactionDistributeCompleted += new EventHandler<AddSatisfactionDistributeCompletedEventArgs>(client_AddSatisfactionDistributeCompleted);
            client.UpdSatisfactionDistributeCompleted += new EventHandler<UpdSatisfactionDistributeCompletedEventArgs>(client_UpdSatisfactionDistributeCompleted);
            client.GetSatisfactionDistributeChildCompleted += new EventHandler<GetSatisfactionDistributeChildCompletedEventArgs>(client_GetSatisfactionDistributeChildCompleted);
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(permissionClient_GetSysDictionaryByFatherIDCompleted);
        }
        #endregion

        #region 事件完成程序
        void SatisfactionDistributeChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventRegister();
            distributeEntity = new T_OA_SATISFACTIONDISTRIBUTE();
            distribute = new T_OA_DISTRIBUTEUSER();
            distributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeEntity.T_OA_SATISFACTIONREQUIRE = new T_OA_SATISFACTIONREQUIRE();
            if (actionType != FormTypes.New)
            {
                if (string.IsNullOrEmpty(distributeId))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                if (actionType == FormTypes.Browse)
                {
                    this.contextInfo.IsEnabled = false;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetSatisfactionDistributeChildAsync(distributeId);
                return;
            }
        }

        void client_AddSatisfactionDistributeCompleted(object sender, AddSatisfactionDistributeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result && e.Error!=null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            actionType = FormTypes.Edit;
            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("ADDSUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"));
             }

        void client_UpdSatisfactionDistributeCompleted(object sender, UpdSatisfactionDistributeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result && !(string.IsNullOrEmpty(e.Error.Message)))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("UPDATEFAILED"), Utility.GetResourceStr("UPDATEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"));
        }

        void client_GetSatisfactionDistributeChildCompleted(object sender, GetSatisfactionDistributeChildCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTFOUNDDATAOFMATCHCON"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            distributeView = e.Result;
            this.contextInfo.DataContext = distributeView.disibuteEntity;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            permissionClient.GetSysDictionaryByFatherIDAsync(distributeView.AnswerGroupid);
            ShowAuditControl();
        }

        private void ShowAuditControl()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            
        }

        /// <summary>
        /// 取出分发范围所需字段值，并放入相应类型集合
        /// </summary>
      
        void permissionClient_GetSysDictionaryByFatherIDCompleted(object sender, GetSysDictionaryByFatherIDCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTFOUNDDATAOFMATCHCON"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            if (actionType != FormTypes.New)
            {
                ComboBoxAnswer.ItemsSource = e.Result as ObservableCollection<T_SYS_DICTIONARY>;
                ComboBoxAnswer.DisplayMemberPath = "DICTIONARYNAME";
                ComboBoxPercent.IsEnabled = true;
                SetPercentData();
                //ComboBoxPercent.SelectedItem=
                return;
            }
            ComboBoxAnswer.ItemsSource = e.Result as ObservableCollection<T_SYS_DICTIONARY>;
            ComboBoxAnswer.DisplayMemberPath = "DICTIONARYNAME";
            ComboBoxAnswer.SelectedIndex = 0;
            ComboBoxAnswer.IsEnabled = true;
            SetPercentData();
            ComboBoxPercent.IsEnabled = true;
        }
        #endregion

        #region XAML事件完成程序
        #endregion

        #region 其他函数
      
        /// <summary>
        /// 调查申请选择按钮回调函数
        /// </summary>
        void SelectRequire()
        {
            scheme = new SelectScheme("satisfactionRequire");
            EntityBrowser browser = new EntityBrowser(scheme);
            browser.MinHeight = 450;
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            if (scheme.DataView== null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            BindingDataByRequire();
            }

        /// <summary>
        /// 获取调查申请数据
        /// </summary>
        void BindingDataByRequire()
        {
            distributeEntity.T_OA_SATISFACTIONREQUIRE = new T_OA_SATISFACTIONREQUIRE();
            distribute = new T_OA_DISTRIBUTEUSER();
            distributeEntity.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID = scheme.DataView.Satisfactionrequireid;
            distributeEntity.DISTRIBUTETITLE = scheme.DataView.SurveyTitle;
            TxtTitle.Text = distributeEntity.DISTRIBUTETITLE;
            distributeEntity.CONTENT = scheme.DataView.Content;
            TxtContent.Text = distributeEntity.CONTENT;
            formId = scheme.DataView.Satisfactionrequireid;
            if (string.IsNullOrEmpty(scheme.DataView.AnswerGroupid))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            permissionClient.GetSysDictionaryByFatherIDAsync(scheme.DataView.AnswerGroupid);
        }

        /// <summary>
        /// 获取发布对象,弹出组织架构窗体
        /// </summary>
        void SelectView()
        {
            OrganizationLookup lookup = new OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
                {
                    List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                    if (ent == null && ent.Count < 1)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        return;
                    }
                    lookupObjectList = ent;
                    BindingDataGrid();
                };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }

        /// <summary>
        /// 发布对象绑定到DataGrid
        /// </summary>
        void BindingDataGrid()
        {
            if (lookupObjectList == null & lookupObjectList.Count < 1)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DISTRBUTEOBJECT", "CANNOTBEEMPTYSELECT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                DGDistribute.ItemsSource = null;
                return;
            }
            DGDistribute.ItemsSource = lookupObjectList;
        }

        /// <summary>
        /// 页面保存及保存前数据验证
        /// </summary>
        void SaveDistributeData()
        {
            if (ChecekGroup())
            {
                //distributeEntity.ANSWERID = (ComboBoxAnswer.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                //distributeEntity.PERCENTAGE = (decimal)ComboBoxPercent.SelectedItem;
                //if (actionType == FormTypes.New)
                //{
                //    SetData();
                //    SetDistributeuserData();
                //    RefreshUI(RefreshedTypes.ShowProgressBar);
                //    client.AddSatisfactionDistributeAsync(distributeEntity, distributeuserList);
                //    return;
                //}
                //RefreshUI(RefreshedTypes.ShowProgressBar);
                //client.UpdSatisfactionDistributeAsync(distributeEntity, distributeuserList);
            }

        }

        /// <summary>
        /// 数据验证方法
        /// </summary>
        /// <returns></returns>
        bool ChecekGroup()
        {
            if (string.IsNullOrEmpty(TxtTitle.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEEMPTYSELECT", "SurveysTITLE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(TxtContent.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEEMPTYSELECT", "SurveysContent"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return false;
            }
            if (ComboBoxAnswer.SelectedIndex < 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEEMPTYSELECT", "OAANSWER"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return false;
            }
            if (ComboBoxPercent.SelectedIndex < 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEEMPTYSELECT", "OASURVEYPERCENTAGE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return false;
            }
            List<ValidatorBase> validators = checkGroup.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (ValidatorBase validator in validators)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(validator.ErrorMessage), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 数据绑定强进行字典转换
        /// </summary>
       
     

        /// <summary>
        /// 设置满意度比率的数值
        /// </summary>
        void SetPercentData()
        {
            ObservableCollection<decimal> bindingData = new ObservableCollection<decimal>()
            {
                100,90,80,70,60,50,40,30,20,10
            };
            this.ComboBoxPercent.ItemsSource = bindingData;
            this.ComboBoxAnswer.SelectedIndex = 1;
        }

       

        void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 接口实现

        #region IClient资源回收
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

        #region IEntityEditor菜单控制
        public string GetTitle()
        {
            switch (actionType)
            {
                case FormTypes.New:
                    return Utility.GetResourceStr("ADDBUTTON", "OASatisfactionDistribute");
                case FormTypes.Edit:
                    return Utility.GetResourceStr("EDIT", "OASatisfactionDistribute");
                case FormTypes.Browse:
                    return Utility.GetResourceStr("VIEWTITLE", "OASatisfactionApp");
                case FormTypes.Audit:
                    return Utility.GetResourceStr("AUDIT", "OASatisfactionDistribute");
                default:
                    return Utility.GetResourceStr("ReSubmit", "OASatisfactionDistribute");
            }
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    SaveDistributeData();
                    break;
                case "1":
                    SaveDistributeData();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "2":
                    SelectView();
                    break;
                case "3":
                    SelectRequire();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> navigateItems = new List<NavigateItem>() { new NavigateItem { Title = Utility.GetResourceStr("InfoDetail"), Tooltip = Utility.GetResourceStr("InfoDetail") } };
            return navigateItems;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> toolBaritems = new List<ToolbarItem>()
           {
            new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"},
            new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
           new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="2",Title=Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},
           new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="3",Title=Utility.GetResourceStr("CHOOSEEMPLOYEESURVEYAPP"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"}
           };
            return toolBaritems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region IAudit审核控制
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string XmlSourcce = string.Empty;
            XmlSourcce = Utility.ObjListToXml<T_OA_SATISFACTIONDISTRIBUTE>(distributeEntity, "OA");
            Utility.SetAuditEntity(entity, "T_OA_SATISFACTIONDISTRIBUTE", XmlSourcce);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = string.Empty;
            switch (args)
            {
                case AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            SaveDistributeData();
        }

        public string GetAuditState()
        {
            string state = string.Empty;
            if (actionType == FormTypes.Browse)
            {
                state = "-1";
            }
            if (distributeEntity != null)
            {
                state = distributeEntity.CHECKSTATE;
            }
            return state;
        }
        #endregion
        #endregion

    }
}
