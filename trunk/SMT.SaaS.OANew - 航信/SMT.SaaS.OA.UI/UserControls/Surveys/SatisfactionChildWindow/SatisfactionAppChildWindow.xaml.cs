//----------------------------------------------------
// 文件名：SatisfactionAppChildWindow.xaml.cs
//作  用：员工满意度调查申请
//创建人：勒中玉
//创建时间：2011-6-15
//修改人：勒中玉
//修改时间：2011-7-22
//---------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Validator;
using System.Collections.ObjectModel;
using FrUi = SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using System.Linq;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SatisfactionAppChildWindow : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量定义
        string appId = string.Empty;
        FormTypes actionType=0;
        SelectScheme scheme=null;
        SmtOAPersonOfficeClient client=null;
        V_Satisfactions appView = null;
        List<ExtOrgObj> lookupObjectList=null;
        PermissionServiceClient permissionClient;
        #endregion

        #region 构造函数
        public SatisfactionAppChildWindow(FormTypes actionType, string appId)
        {
            InitializeComponent();
            this.actionType = actionType;
            this.appId = appId;
            this.Loaded += new RoutedEventHandler(EmployeeSurveyAppChildWindow_Loaded);
          }
        #endregion

        #region 事件注册
        void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            permissionClient = new PermissionServiceClient();
            client.AddSatisfactionAppCompleted += new EventHandler<AddSatisfactionAppCompletedEventArgs>(client_AddSatisfactionAppCompleted);
            client.UpdSatisfactionAppCompleted += new EventHandler<UpdSatisfactionAppCompletedEventArgs>(client_UpdSatisfactionAppCompleted);
            client.GetSatisfactionAppChildCompleted += new EventHandler<GetSatisfactionAppChildCompletedEventArgs>(client_GetSatisfactionAppChildCompleted);
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(permissionClient_GetSysDictionaryByFatherIDCompleted);
        }
        #endregion

        
        #region 事件完成程序
        /// <summary>
        /// 页面载入事件
        /// </summary>
        void EmployeeSurveyAppChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventRegister();
            
 
            if (actionType != FormTypes.New)
            {
                lookupObjectList = new List<ExtOrgObj>();
                if (string.IsNullOrEmpty(appId))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                if (actionType == FormTypes.Browse)
                {
                    this.contextInfo.IsEnabled = false;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetSatisfactionAppChildAsync(appId);
            } 
          
          
            this.startDate.SelectedDate = DateTime.Now;
            this.startDate.SelectedDate = DateTime.Now.AddDays(30);
        }

        void client_AddSatisfactionAppCompleted(object sender, AddSatisfactionAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result)
            {
                actionType = FormTypes.Edit;
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("ADDSUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"));
                RefreshUI(RefreshedTypes.All);
                RefreshUI(RefreshedTypes.AuditInfo);
                return;
             }
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        }

        void client_UpdSatisfactionAppCompleted(object sender, UpdSatisfactionAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result && !(string.IsNullOrEmpty(e.Error.Message)))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("UPDATEFAILED"), Utility.GetResourceStr("UPDATEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"));
          }

        void client_GetSatisfactionAppChildCompleted(object sender, GetSatisfactionAppChildCompletedEventArgs e)
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
            
            
             BindingCheckBox();
            
             DataGridConverter();
             RefreshUI(RefreshedTypes.All);
             RefreshUI(RefreshedTypes.AuditInfo);
           }

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
            ObservableCollection<T_SYS_DICTIONARY> hander = e.Result;
            Answer.ItemsSource = hander;
            Answer.DisplayMemberPath = "DICTIONARYNAME";
            Answer.SelectedIndex = 0;
            Answer.IsEnabled = true;
        }

        #endregion

        #region XAML事件完成程序
        void AnswerGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)//选择答案组时,读取答案并将控件设置为可选
        {
            T_SYS_DICTIONARY sys = ((ComboBox)sender).SelectedItem as T_SYS_DICTIONARY;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            permissionClient.GetSysDictionaryByFatherIDAsync(sys.DICTIONARYID);
        }
        #endregion

        #region 其他函数
        void SetAppData()//设置申请表保存数据
        {
          
          
        }

        void SetDistributeuserData()//设置发布对象保存数据
        {
           
        }

        IssuanceObjectType GetObjectType(ExtOrgObj objExt)//判断过滤LookUp
        {
            switch (objExt.ObjectType)
            {
                case FrUi.OrgTreeItemTypes.Company:
                    return IssuanceObjectType.Company;
                case FrUi.OrgTreeItemTypes.Department:
                    return IssuanceObjectType.Department;
                case FrUi.OrgTreeItemTypes.Post:
                    return IssuanceObjectType.Post;
                default:
                    return IssuanceObjectType.Employee;
            }
        }

        void SaveAppData()//保存或修改调查申请
        {
            if (CheceGroup())
            {
                
            }
        }

        bool CheceGroup()//验证UI数据正确性
        {
            string start = string.Empty;
            string end = string.Empty;
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
            if (DGDistribute.SelectedIndex < 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "DISTRBUTEOBJECT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            if (string.IsNullOrEmpty(this.startDate.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.startDate.Focus();
                return false;
            }
            start = this.startDate.SelectedDate.Value.ToString("d");
            if (string.IsNullOrEmpty(this.endDate.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENDTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.startDate.Focus();
                return false;
            }
            end = this.endDate.SelectedDate.Value.ToString("d");
            if (this.endDate.SelectedDate.Value < this.startDate.SelectedDate.Value)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANENDDATE", "STARTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return false;
            }

            if (AnswerGroup.SelectedIndex < 1)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "OAANSWERGROUP"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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

        void SelectView()//获取发布对象
        {
            OrganizationLookup lookup = new OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, sender) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    lookupObjectList = ent;
                    BindingDataGrid();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }

        void SelectMaster()//获取调查方案
        {
            scheme = new SelectScheme("satisfactionMaster");
            EntityBrowser browser = new EntityBrowser(scheme);
            browser.MinHeight = 450;
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()//选择调查方案时完成事件
        {

            if (scheme.DataView == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANENDDATE", "STARTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
         //requireEntity.T_OA_SATISFACTIONMASTER = new T_OA_SATISFACTIONMASTER();
         //requireEntity.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID= scheme.DataView.Satisfactionmasterid;
         //requireEntity.SATISFACTIONTITLE = scheme.DataView.SurveyTitle;
         //   TxtTitle.Text = requireEntity.SATISFACTIONTITLE;
         //   requireEntity.CONTENT = scheme.DataView.Content;
         //   TxtContent.Text = requireEntity.CONTENT; 
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

        void DataGridConverter()//DataGrid字段转换
        {
            
        }

        void BindingDataGrid()//数据绑定到DataGrid
        {
            if (lookupObjectList == null & lookupObjectList.Count < 1)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DISTRBUTEOBJECT", "CANNOTBEEMPTYSELECT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                DGDistribute.ItemsSource = null;
                return;
            }
            DGDistribute.ItemsSource = null;
            DGDistribute.ItemsSource = lookupObjectList;
            
        }

        void BindingCheckBox()//checkbox绑定
        {
            this.IsImplement.IsChecked = appView.requireEntity.OPTFLAG == "1" ? true : false;
            this.IsFill.IsChecked = appView.requireEntity.WAY == "1" ? true : false;
        }
        #endregion

        #region 接口实现

        #region IClient资源回收
        public void ClosedWCFClient()
        {
            client.DoClose();
            permissionClient.DoClose();
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

        #region IEntityEditor窗体控制
        public string GetTitle()
        {
            switch (actionType)
            {
                case FormTypes.New:
                    return Utility.GetResourceStr("ADDBUTTON", "OASatisfactionApp");
                case FormTypes.Edit:
                    return Utility.GetResourceStr("EDIT", "OASatisfactionApp");
                case FormTypes.Browse:
                    return Utility.GetResourceStr("VIEWTITLE", "OASatisfactionApp");
                case FormTypes.Audit:
                    return Utility.GetResourceStr("AUDIT", "OASatisfactionApp");
                default:
                    return Utility.GetResourceStr("ReSubmit", "OASatisfactionAp");
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
                    SaveAppData();
                    break;
                case "1":
                    SaveAppData();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "2":
                    SelectView();
                    break;
                case "3":
                    SelectMaster();
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
            if (actionType != FormTypes.Browse && actionType != FormTypes.Audit)
            {
                List<ToolbarItem> toolBaritems = new List<ToolbarItem>()
             {
              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="3",Title=Utility.GetResourceStr("CHOOSEEMPLOYEESURVEY"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},

              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="2",Title=Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},

              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),
                 ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
             };
                return toolBaritems;
            }
            return new List<ToolbarItem>();
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region IAudit审核控制
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //string xmlSourcce = string.Empty;
            //xmlSourcce = Utility.ObjListToXml<T_OA_SATISFACTIONREQUIRE>(requireEntity, "OA");
            //Utility.SetAuditEntity(entity, "T_OA_SATISFACTIONREQUIRE", xmlSourcce);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            //string state = string.Empty;
            //switch (args)
            //{
            //    case AuditEventArgs.AuditResult.Auditing:
            //        state = Utility.GetCheckState(CheckStates.Approving);
            //        break;
            //    case AuditEventArgs.AuditResult.Successful:
            //        state = Utility.GetCheckState(CheckStates.Approved);
            //        break;
            //    case AuditEventArgs.AuditResult.Fail:
            //        state = Utility.GetCheckState(CheckStates.UnApproved);
            //        break;
            //}
        }

        public string GetAuditState()
        {
            string state = string.Empty;
            if (actionType == FormTypes.Browse)
            {
                state = "-1";
            }
            //if (requireEntity != null)
            //{
            //    state = requireEntity.CHECKSTATE;
            //}
            return state;
        }
        #endregion
        #endregion

    }
}
