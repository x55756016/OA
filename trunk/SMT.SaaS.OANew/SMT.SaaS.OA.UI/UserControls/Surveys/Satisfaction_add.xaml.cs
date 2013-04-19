using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class Satisfaction_add : BaseForm, IClient, IEntityEditor, IAudit
    {
        /// <summary>
        /// 方案
        /// </summary>
        private V_Satisfaction _survey;
        public V_Satisfaction _Survey {  get { return _survey; } set { _survey = value; } }
        /// <summary>
        /// 题目
        /// </summary>
        private ObservableCollection<T_OA_SATISFACTIONDETAIL> _osub = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();
     
        private RefreshedTypes saveType;
        private FormTypes types;
        private bool isFlow = false;
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();

        public Satisfaction_add(FormTypes type)
        {
            InitializeComponent();
            this.types = type;
            _VM.Add_SSurveyCompleted += new EventHandler<Add_SSurveyCompletedEventArgs>(Add_SSurveyCompleted);
            _VM.Upd_SSurveyCompleted += new EventHandler<Upd_SSurveyCompletedEventArgs>(Upd_SSurveyCompleted);
            _VM.Del_SSurveySubCompleted += new EventHandler<Del_SSurveySubCompletedEventArgs>(Del_SSurveySubCompleted);
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _Survey = new V_Satisfaction();
            _Survey.RequireMaster = new T_OA_SATISFACTIONMASTER();
            _Survey.RequireMaster.SATISFACTIONMASTERID = System.Guid.NewGuid().ToString();
            _survey.RequireMaster.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();

            _Survey.SubjectViewList = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();
         
            NewSubject();
            dg.ItemsSource = _osub;
            dg.SelectedIndex = 0;

            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }
        #region 题目
      
        /// <summary>
        /// 添加一个题目,及答案
        /// </summary>
        private void NewSubject()
        {
            T_OA_SATISFACTIONDETAIL sub = new T_OA_SATISFACTIONDETAIL();
            sub.T_OA_SATISFACTIONMASTER = _Survey.RequireMaster;
            sub.SATISFACTIONDETAILID = Guid.NewGuid().ToString();
            sub.SATISFACTIONMASTERID = _Survey.RequireMaster.SATISFACTIONMASTERID;
            sub.T_OA_SATISFACTIONMASTER = _survey.RequireMaster;
            if (_osub.Count > 0)
            {
                T_OA_SATISFACTIONDETAIL i = _osub[_osub.Count - 1];
                sub.SUBJECTID = i.SUBJECTID + 1;
            }
            else
                sub.SUBJECTID = 1;// 添加保存后，不能全部删除所有题目。

            SetSubject(ref sub);
            _osub.Add(sub);
        }
        //根据 回车键，判断 是否新增行，保存修改行。行加载后， 重新计算行题号  1
        private void txtSub_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dg.SelectedIndex == _osub.Count - 1)
                {
                    T_OA_SATISFACTIONDETAIL vsub = _osub.LastOrDefault();
                    if (vsub.CONTENT != null && vsub.CONTENT.Trim().Length > 0)
                    {
                        NewSubject();
                        //foreach (object obj in dg.ItemsSource)
                        //{
                        //    if (dg.Columns[2].GetCellContent(obj) != null)
                        //    {
                        //        TextBox ckbSelect = dg.Columns[2].GetCellContent(obj).FindName("txtSub") as TextBox;
                        //        ckbSelect.Focus();
                        //    }
                        //} 
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"));
                    }
                }
            }
        }

        //行加载删除按钮 2
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_SATISFACTIONDETAIL tmp = (T_OA_SATISFACTIONDETAIL)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[3].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;           
        }
        
        //题目更改后保存其值
        private void txtSub_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            _osub[dg.SelectedIndex].CONTENT = txt.Text.ToString();
        }

        //删除题目
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_osub.Count > 1) //必须有派车单，司机才能根据派车单提交费用 单
            {
                T_OA_SATISFACTIONDETAIL i = ((Button)sender).DataContext as T_OA_SATISFACTIONDETAIL;
                _osub.Remove(i);

                if (i.SATISFACTIONDETAILID != null) //删除已经保存到服务器中的数据
                {
                    ObservableCollection<T_OA_SATISFACTIONDETAIL> o = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();
                    o.Add(i);
                    _VM.Del_SSurveySubAsync(o);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
        }
        private void Del_SSurveySubCompleted(object sender, Del_SSurveySubCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        #endregion 题目

      
        #region IEntityEditor

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("OASatisfaction");
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            object[,] arr = new object[,]{            
             {ToolbarItemDisplayTypes.Image,"1","SAVEANDCLOSE", "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
             {ToolbarItemDisplayTypes.Image,"0","SAVE","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
            };
            return VehicleMgt.GetToolBarItems(ref arr);
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
        public void DoAction(string actionType)
        {
            if (!Check()) return;           

            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0":
                    isFlow = false;
                    saveType = RefreshedTypes.HideProgressBar;
                    Save();
                    break;
                case "1":
                    isFlow = false;
                    saveType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
              
            }
        }
        public event UIRefreshedHandler OnUIRefreshed;

        #endregion 

        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

       //检查
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
            }
            foreach (T_OA_SATISFACTIONDETAIL vsub in _osub)//if_osub==null??
                if (vsub.CONTENT == null || vsub.CONTENT.Trim().Length == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"));
                    return false;
                }

            return true;
        }
        /// <summary>
        /// 设置 方案其它信息
        /// </summary>
        /// <param name="i"></param>
        private void SetSurvey()
        {
            _Survey.RequireMaster.CREATEDATE = System.DateTime.Now;
            _Survey.RequireMaster.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.RequireMaster.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.RequireMaster.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.RequireMaster.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.RequireMaster.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            _Survey.RequireMaster.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.RequireMaster.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.RequireMaster.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.RequireMaster.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.RequireMaster.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        /// <summary>
        /// 设置 题目其它信息
        /// </summary>
        /// <param name="i"></param>
        private static void SetSubject(ref T_OA_SATISFACTIONDETAIL i)
        {
            i.CREATEDATE = System.DateTime.Now;
            i.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            i.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
       
        bool _isAdd = true;
        private void Save()
        {
            _Survey.RequireMaster.SATISFACTIONTITLE = txtTitle.Text.Trim();
            _Survey.RequireMaster.CONTENT = txtContent.Text.Trim();
            _Survey.RequireMaster.CHECKSTATE= _survey.RequireMaster.CHECKSTATE;
            if (_isAdd)
            {

                SetSurvey();
                for (int i = 0; i < _osub.Count; i++)
                {
                    if (_osub[i].CONTENT != null && _osub[i].CONTENT.Trim().Length > 0 && _osub[i].CONTENT != "无")
                    {
                        T_OA_SATISFACTIONDETAIL sub = _osub[i];
                        _Survey.SubjectViewList.Add(sub);   //题目id    
                    }
                }
                _VM.Add_SSurveyAsync(_Survey);
            }
            else

            
                _VM.Upd_SSurveyAsync(_Survey);
        }       
        //保存之后
        void Add_SSurveyCompleted(object sender, Add_SSurveyCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                _isAdd = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                Utility.ShowMessageBox("ADD", isFlow, true);
                if (isFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("ADD", isFlow, false);
            }
        }
        // //先更新方案成功后， 添加 和修改题目、答案
        void Upd_SSurveyCompleted(object sender, Upd_SSurveyCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                Utility.ShowMessageBox("UPDATE", isFlow, true);
                if (isFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", isFlow, false);
            }
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_SATISFACTIONMASTER>(_survey.RequireMaster, "OA");
            Utility.SetAuditEntity(entity, "T_OA_SATISFACTIONMASTER", _survey.RequireMaster.SATISFACTIONMASTERID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            _survey.RequireMaster.CHECKSTATE = state;
            isFlow = true;
            Save();
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (_survey != null)
                state = _survey.RequireMaster.CHECKSTATE;
            if (types == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
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
