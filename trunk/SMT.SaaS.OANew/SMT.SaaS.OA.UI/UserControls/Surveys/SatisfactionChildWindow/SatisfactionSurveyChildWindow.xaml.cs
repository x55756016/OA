//****满意度调查方案子页面***
//负责人:lezy
//创建时间:2011-6-7
//完成时间：2011-6-30
//**************************


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Validator;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SatisfactionSurveyChildWindow : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量定义
        FormTypes actionType=0;
        string masterId=string.Empty;
        SmtOAPersonOfficeClient client=null;
        T_OA_SATISFACTIONMASTER masterEntity=null;
        T_OA_SATISFACTIONDETAIL detailEntity=null;
        ObservableCollection<T_OA_SATISFACTIONDETAIL> detailList=null;
        #endregion

        #region 构造函数
        public SatisfactionSurveyChildWindow(FormTypes actionType, string masterId)
        {
            InitializeComponent();
            this.actionType = actionType;
            this.masterId = masterId;
            this.Loaded += new RoutedEventHandler(SatisfactionSurveyChildWindow_Loaded);
        }
        #endregion

        #region 事件注册
        void EventResgister()
        {
            client = new SmtOAPersonOfficeClient();
            client.AddSatisfactionMasterCompleted += new EventHandler<AddSatisfactionMasterCompletedEventArgs>(client_AddSatisfactionMasterCompleted);
            client.UpdSatisfactionMasterCompleted += new EventHandler<UpdSatisfactionMasterCompletedEventArgs>(client_UpdSatisfactionMasterCompleted);
            client.GetSatisfactionMasterChildCompleted += new EventHandler<GetSatisfactionMasterChildCompletedEventArgs>(client_GetSatisfactionMasterChildCompleted);

        }
        #endregion

        #region 事件完成程序
        void SatisfactionSurveyChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventResgister();
            masterEntity = new T_OA_SATISFACTIONMASTER();
            detailEntity = new T_OA_SATISFACTIONDETAIL();
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            if (!string.IsNullOrEmpty(masterId) && actionType != FormTypes.New)
            {
                if (actionType == FormTypes.Browse)
                {
                    this.txtContent.IsEnabled = false;
                    this.txtSubject.IsEnabled = false;
                    this.dgSubject.IsEnabled = false;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetSatisfactionMasterChildAsync(masterId);
            }
            else
            {
                SetMaterData();
                SetDetailList();
            }
            this.contextInfo.DataContext = masterEntity;
            this.dgSubject.ItemsSource = detailList;
            dgSubject.SelectedIndex = 0;
        }

        void client_AddSatisfactionMasterCompleted(object sender, AddSatisfactionMasterCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result && e.Error == null)
            {
                actionType = FormTypes.Edit;
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("ADDSUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"));
                return;
            }
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);     
        }

        void client_UpdSatisfactionMasterCompleted(object sender, UpdSatisfactionMasterCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result && e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("UPDATEFAILED"), Utility.GetResourceStr("UPDATEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
               return;
            }
            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"));
        }

        void client_GetSatisfactionMasterChildCompleted(object sender, GetSatisfactionMasterChildCompletedEventArgs e)
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
            this.contextInfo.DataContext = masterEntity = e.Result;
            this.dgSubject.ItemsSource = masterEntity.T_OA_SATISFACTIONDETAIL;
        }
        #endregion

        #region XAML事件完成程序
        void txtSub_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(detailList.LastOrDefault().CONTENT.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return;
                }
                SetDetailList();
                dgSubject.ItemsSource = null;
                dgSubject.ItemsSource = detailList;
                dgSubject.SelectedIndex = 0;
            }
        }
        void delSubject_Click(object sender, RoutedEventArgs e)
        {
            if (detailList.Count > 1)
            {
                var hand = ((Button)sender).Tag as T_OA_SATISFACTIONDETAIL;
                detailList.Remove(hand);
            }
        }
        void dgSubject_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_SATISFACTIONMASTER tmp = e.Row.DataContext as T_OA_SATISFACTIONMASTER;
            ImageButton delBtn = dgSubject.Columns[3].GetCellContent(e.Row).FindName("delSubject") as ImageButton;
            delBtn.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            delBtn.Tag = tmp;
        }
        #endregion

        #region 其他函数
        void SetMaterData()
        {
            masterEntity.SATISFACTIONMASTERID = Guid.NewGuid().ToString();
            masterEntity.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            masterEntity.CREATEUSERID = masterEntity.UPDATEUSERID = masterEntity.OWNERID = Common.CurrentLoginUserInfo.EmployeeID.ToString();
            masterEntity.CREATEUSERNAME = masterEntity.UPDATEUSERNAME = masterEntity.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName.ToString();
            masterEntity.CREATECOMPANYID = masterEntity.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID.ToString();
            masterEntity.CREATEDEPARTMENTID = masterEntity.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID.ToString();
            masterEntity.CREATEPOSTID = masterEntity.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID.ToString();
            //masterEntity.CREATEDATE = masterEntity.UPDATEDATE.Value = DateTime.Now;
        }
        void SetChildData(ref T_OA_SATISFACTIONDETAIL detailEntity)
        {
            if (detailEntity != null)
            {
                detailEntity.SATISFACTIONDETAILID = Guid.NewGuid().ToString();
                detailEntity.SATISFACTIONMASTERID = masterEntity.SATISFACTIONMASTERID;
                detailEntity.CREATEDATE = DateTime.Now;
                detailEntity.UPDATEDATE = DateTime.Now;
                detailEntity.CREATEUSERID = detailEntity.UPDATEUSERID = detailEntity.OWNERID = Common.CurrentLoginUserInfo.EmployeeID.ToString();
                detailEntity.CREATEUSERNAME = detailEntity.UPDATEUSERNAME = detailEntity.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName.ToString();
                detailEntity.CREATECOMPANYID = detailEntity.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID.ToString();
                detailEntity.CREATEDEPARTMENTID = detailEntity.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID.ToString();
                detailEntity.CREATEPOSTID = detailEntity.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID.ToString();
                detailList.Add(detailEntity);
                return;
            }
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        }
        bool CheckGroup()//验证UI数据正确性
        {
            List<ValidatorBase> validators = checkGroup.ValidateAll();
            if (validators != null && validators.Count() > 0)
            {
                foreach (ValidatorBase validator in validators)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(validator.ErrorMessage), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            if (detailList != null && detailList.Count() > 0)
            {
                foreach (var detail in detailList)
                {
                    if (detail.CONTENT == null && detail.CONTENT.Trim().Length == 0)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        return false;
                    }
                }
            }
            return true;
        }
        void SaveSatisfactionMaster()
        {
            if (CheckGroup())
            {
                if (actionType == FormTypes.New)
                {
                    masterEntity.T_OA_SATISFACTIONDETAIL = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();
                    foreach (T_OA_SATISFACTIONDETAIL temp in detailList)
                    {
                        masterEntity.T_OA_SATISFACTIONDETAIL.Add(temp);
                    }
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    client.AddSatisfactionMasterAsync(masterEntity);
                    return;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.UpdSatisfactionMasterAsync(masterEntity);
            }
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
        void SetDetailList()
        {
            detailList = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();
            if (detailList.Count > 0)
            {
                T_OA_SATISFACTIONDETAIL de = detailList[detailList.Count - 1];
                detailEntity.SUBJECTID = de.SUBJECTID + 1;
                SetChildData(ref detailEntity);
            }
            else
            {
                detailEntity.SUBJECTID = 1;
                SetChildData(ref detailEntity);
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

        #region IEntityEditor控制窗体
        public string GetTitle()
        {

            switch (actionType)
            {
                case FormTypes.New:
                    return Utility.GetResourceStr("ADDBUTTON", "OASatisfaction");
                case FormTypes.Edit:
                    return Utility.GetResourceStr("EDIT", "OASatisfaction");
                case FormTypes.Browse:
                    return Utility.GetResourceStr("VIEWTITLE", "OASatisfaction");
                case FormTypes.Audit:
                    return Utility.GetResourceStr("AUDIT", "OASatisfaction");
                default:
                    return Utility.GetResourceStr("ReSubmit", "OASatisfaction");
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
                    SaveSatisfactionMaster();
                    break;
                case "1":
                    SaveSatisfactionMaster();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
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
            new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
           new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),
               ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
       
            };
            return toolBaritems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region IAudit审核控制
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string xmlSource = string.Empty;
            xmlSource = Utility.ObjListToXml<T_OA_SATISFACTIONMASTER>(masterEntity, "OA");
            Utility.SetAuditEntity(entity, "T_OA_SATISFACTIONMASTER", xmlSource);
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
            SaveSatisfactionMaster();
        }

        public string GetAuditState()
        {
            string state = string.Empty;
            if (masterEntity == null)
            {
                state = "-1";
            }
            else
            {
                state = masterEntity.CHECKSTATE;
            }

            if (actionType == FormTypes.Browse)
            {
                state = "-1";
            }

            return state;
        }
        #endregion
        #endregion
    }
}
