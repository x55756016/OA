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

using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Form.Salary
{
    public partial class CalculateItemForm : BaseForm, IEntityEditor, IClient// , IAudit
    {
        //EntityManager.GetAllEntityName();//获取所有实体名
        //EntityManager.GetEntityPropertyByName("T_HR_ADJUSTLEAVE");//获取指定实体的所有字段名

        SalaryServiceClient client;
        PermissionServiceClient permClient;
        private bool flag = false;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private FormTypes formType;
        private CalculateFormula form;
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        public T_HR_SALARYITEM SalaryItemPost { get; set; }

        private T_HR_SALARYITEM salaryItemSet;

        public T_HR_SALARYITEM SalaryItemSet
        {
            get { return salaryItemSet; }
            set
            {
                salaryItemSet = value;
                this.DataContext = salaryItemSet;
            }
        }
        private string setID;

        public CalculateItemForm()
        {
            InitializeComponent();
        }

        public CalculateItemForm(FormTypes type, string CalculateItemSetID)
        {
            InitializeComponent();
            FormType = type;
            //InitParas(CalculateItemSetID);
            setID = CalculateItemSetID;
            this.Loaded += new RoutedEventHandler(CalculateItemForm_Loaded);
        }

        void CalculateItemForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas(setID);
        }

        private void InitParas(string CalculateItemSetID)
        {
            client = new SalaryServiceClient();
            client.GetSalaryItemSetByIDCompleted += new EventHandler<GetSalaryItemSetByIDCompletedEventArgs>(client_GetSalaryItemSetByIDCompleted);
            client.SalaryItemSetAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryItemSetAddCompleted);
            client.SalaryItemSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryItemSetUpdateCompleted);

            if (FormType == FormTypes.New)
            {
                SalaryItemSet = new T_HR_SALARYITEM();
                SalaryItemSet.SALARYITEMID = Guid.NewGuid().ToString();
                //SalaryItemSet.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                SetToolBar();
            }
            else
            {
                client.GetSalaryItemSetByIDAsync(CalculateItemSetID);
            }

        }

        void client_SalaryItemSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYITEM"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
            if (flag)
            {
                flag = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_SalaryItemSetAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                FormType = FormTypes.Edit;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYITEM"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
            if (flag)
            {
                flag = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_GetSalaryItemSetByIDCompleted(object sender, GetSalaryItemSetByIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    salaryItemSet = e.Result;
                    this.DataContext = e.Result;
                    if (salaryItemSet.CALCULATORTYPE != string.Empty)
                    {
                        RBHAND.IsChecked = false;
                        //RBCHIVE.IsChecked = false;
                        RBFORMULA.IsChecked = false;
                        SELECTITEM.IsChecked = false;
                    }
                    if (!string.IsNullOrEmpty(SalaryItemSet.MUSTSELECTED))
                        cbMustSelect.IsChecked = SalaryItemSet.MUSTSELECTED == "1" ? true : false;
                    if (!string.IsNullOrEmpty(SalaryItemSet.ISAUTOGENERATE))
                        cbLaterCal.IsChecked = SalaryItemSet.ISAUTOGENERATE == "1" ? true : false;
                    switch (salaryItemSet.CALCULATORTYPE)
                    {
                        case "1":
                            RBHAND.IsChecked = true;
                            cbLaterCal.IsEnabled = false;
                            cbLaterCal.IsChecked = false;
                            break;
                        //case "2":
                        //    RBCHIVE.IsChecked = true;
                        //    cbLaterCal.IsEnabled = false;
                        //    cbLaterCal.IsChecked = false;
                        //    break;
                        case "3":
                            RBFORMULA.IsChecked = true;
                            cbLaterCal.IsEnabled = true;
                            if (string.IsNullOrEmpty(salaryItemSet.CALCULATEFORMULACODE))
                            {
                                txtCalculateCode.Text = "-1";
                            }
                            LoadEntityList();
                            break;
                        case "4":
                            SELECTITEM.IsChecked = true;
                            cbLaterCal.IsEnabled = false;
                            cbLaterCal.IsEnabled = false;
                            LoadEntityList();
                            break;
                    }

                }
            }
            SetToolBar();
        }

        private void LoadEntityList()
        {
            bool isEmpty = false;

            CheckEntityMenuIsNullOrEmpty(out isEmpty);

            if (isEmpty)
            {
                if (permClient == null)
                {
                    permClient = new PermissionServiceClient();
                }

                permClient.GetSysLeftMenuAsync("0", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                permClient.GetSysLeftMenuCompleted += new EventHandler<GetSysLeftMenuCompletedEventArgs>(permClient_GetSysLeftMenuCompleted);
            }

            InitEntityInfoForComboBox();
        }

        void permClient_GetSysLeftMenuCompleted(object sender, GetSysLeftMenuCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                List<T_SYS_ENTITYMENU> list = new List<T_SYS_ENTITYMENU>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    var q = from ent in list
                            select new SMT.SaaS.LocalData.T_SYS_ENTITYMENU
                            {
                                CHILDSYSTEMNAME = ent.CHILDSYSTEMNAME,
                                ENTITYCODE = ent.CHILDSYSTEMNAME,
                                SUPERIORID = string.Empty,//ent.T_SYS_ENTITYMENU2.ENTITYMENUID,
                                ENTITYMENUID = ent.ENTITYMENUID,
                                ENTITYNAME = ent.ENTITYNAME,
                                HASSYSTEMMENU = ent.HASSYSTEMMENU,
                                //HELPFILEPATH=ent.HELPFILEPATH,
                                //HELPTITLE=ent.HELPTITLE, 
                                ISAUTHORITY = ent.ISAUTHORITY,
                                MENUCODE = ent.MENUCODE,
                                MENUICONPATH = ent.MENUICONPATH,
                                MENUNAME = ent.MENUNAME,
                                ORDERNUMBER = ent.ORDERNUMBER,
                                SYSTEMTYPE = ent.SYSTEMTYPE,
                                URLADDRESS = ent.URLADDRESS,
                            };
                    //SMT.SAAS.Main.CurrentContext.Common.EntityMenu = list;
                    SMT.SAAS.Main.CurrentContext.Common.EntityMenu = q.ToList();
                    InitEntityInfoForComboBox();
                }
            }
        }

        private void CheckEntityMenuIsNullOrEmpty(out bool isEmpty)
        {
            isEmpty = false;
            if (SMT.SAAS.Main.CurrentContext.Common.EntityMenu == null)
            {
                isEmpty = true;
                return;
            }

            if (SMT.SAAS.Main.CurrentContext.Common.EntityMenu.Count() == 0)
            {
                isEmpty = true;
                return;
            }
        }

        private void InitEntityInfoForComboBox()
        {
            listEntityName.IsEnabled = true;
            listEntityProperty.IsEnabled = true;
            listEntityName.ItemsSource = Utility.GetSystemEntity();
            if (!string.IsNullOrEmpty(salaryItemSet.ENTITYCODE))
            {
                SetEntitySelected(salaryItemSet.ENTITYCODE);
            }
            if (!string.IsNullOrEmpty(salaryItemSet.ENTITYCOLUMNCODE))
            {
                SetEntityPropertySelected(salaryItemSet.ENTITYCOLUMNCODE);
            }
        }

        /// <summary>
        /// 设置选择实体的选中项
        /// </summary>
        /// <param name="strEntityCode"></param>
        private void SetEntitySelected(string strEntityCode)
        {
            List<T_SYS_ENTITYMENU> ents = listEntityName.ItemsSource as List<T_SYS_ENTITYMENU>;
            var q = from e in ents
                    where e.ENTITYCODE == strEntityCode
                    select e;

            if (q.Count() == 0)
            {
                return;
            }

            listEntityName.SelectedItem = q.FirstOrDefault();
        }

        /// <summary>
        /// 设置选择实体的属性选择项
        /// </summary>
        private void BindEntityProperty()
        {
            if (listEntityName.SelectedItem == null)
            {
                listEntityProperty.ItemsSource = null;
                return;
            }

            listEntityProperty.ItemsSource = Utility.GetEntityPropertyByName(((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYCODE);
        }

        /// <summary>
        /// 设置选择实体的属性选择项
        /// </summary>
        private void SetEntityPropertySelected(string strEntityPropertyCode)
        {
            if (string.IsNullOrEmpty(strEntityPropertyCode))
            {
                return;
            }

            List<EntityProPerty> ents = listEntityProperty.ItemsSource as List<EntityProPerty>;

            if (ents == null)
            {
                return;
            }

            var q = from e in ents
                    where e.ProPertyCode == strEntityPropertyCode
                    select e;

            if (q.Count() == 0)
            {
                return;
            }

            listEntityProperty.SelectedItem = q.FirstOrDefault();
        }

        void form_RefreshedHandler(object sender, EventArgs e)
        {
            txtCalculateFormula.Text = form.Result;
            txtCalculateCode.Text = form.ResultID;
            txtMoney.Text = ""; //= form.Amount.ToString();
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_SALARYITEM", SalaryItemSet.OWNERID,
            //        SalaryItemSet.OWNERPOSTID, SalaryItemSet.OWNERDEPARTMENTID, SalaryItemSet.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            //else
            //    ToolbarItems = Utility.CreateFormEditButton("T_HR_SALARYITEM", SalaryItemSet.OWNERID,
            //        SalaryItemSet.OWNERPOSTID, SalaryItemSet.OWNERDEPARTMENTID, SalaryItemSet.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                switch (rb.Name)
                {
                    case "RBHAND":
                        SalaryItemSet.CALCULATORTYPE = "1";
                        txtMoney.IsEnabled = true;
                        txtCalculateFormula.Text = "";
                        //btCalculate.Visibility = Visibility.Collapsed;
                        btCalculate.IsEnabled = false;
                        cbLaterCal.IsEnabled = false;
                        cbLaterCal.IsChecked = false;
                        txtCalculateCode.Text = string.Empty;
                        listEntityName.IsEnabled = false;
                        listEntityProperty.IsEnabled = false;
                        break;
                    //case "RBCHIVE":
                    //    SalaryItemSet.CALCULATORTYPE = "2";
                    //    SalaryItemSet.GUERDONSUM = 0;
                    //    txtMoney.Text = "";
                    //    txtMoney.IsEnabled = false;
                    //    txtCalculateFormula.Text = "";
                    //    btCalculate.IsEnabled = false;
                    //    cbLaterCal.IsEnabled = false;
                    //    cbLaterCal.IsChecked = false;
                    //    txtCalculateCode.Text = string.Empty;
                    //    //btCalculate.Visibility = Visibility.Collapsed;
                    //    break;
                    case "RBFORMULA":
                        SalaryItemSet.CALCULATORTYPE = "3";
                        txtMoney.IsEnabled = false;
                        btCalculate.IsEnabled = true;
                        cbLaterCal.IsEnabled = true;
                        if (string.IsNullOrEmpty(salaryItemSet.CALCULATEFORMULACODE))
                        {
                            txtCalculateCode.Text = "-1";
                        }
                        LoadEntityList();
                        //txtCalculateCode.Text = "-1";
                        //btCalculate.Visibility = Visibility.Visible;
                        break;
                    case "SELECTITEM":
                        SalaryItemSet.CALCULATORTYPE = "4";
                        SalaryItemSet.GUERDONSUM = 0;
                        txtMoney.Text = "";
                        txtMoney.IsEnabled = true;
                        txtCalculateFormula.Text = "";
                        btCalculate.IsEnabled = false;
                        cbLaterCal.IsEnabled = false;
                        cbLaterCal.IsChecked = true;
                        cbMustSelect.IsChecked = true;
                        cbMustSelect.IsEnabled = false;
                        txtCalculateCode.Text = string.Empty;
                        LoadEntityList();
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        #region IAudit
        //public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        //{
        //    Utility.SetAuditEntity(entity, "T_HR_SALARYITEM", SalaryItemSet.SALARYITEMID);
        //}
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            Utility.UpdateCheckState("T_HR_SALARYITEM", "SALARYITEMID", SalaryItemSet.SALARYITEMID, args);
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
            // SalaryItemSet.CHECKSTATE = state;
            // SalaryItemSet.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            client.SalaryItemSetUpdateAsync(SalaryItemSet, "Audit");
        }

        public string GetAuditState()
        {
            string state = "-1";
            //if (SalaryItemSet != null)
            //    state = SalaryItemSet.CHECKSTATE; 
            return state;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYITEM");
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
                    Save();
                    break;
                case "1":
                    Cancel();
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
        //public List<ToolbarItem> GetToolBarItems()
        //{
        //    List<ToolbarItem> items = new List<ToolbarItem>();

        ////    ToolbarItem item = new ToolbarItem
        ////    {
        ////        DisplayType = ToolbarItemDisplayTypes.Image,
        ////        Key = "0",
        ////        Title = Utility.GetResourceStr("SAVE"),
        ////        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
        ////    };

        ////    items.Add(item);

        ////    item = new ToolbarItem
        ////    {
        ////        DisplayType = ToolbarItemDisplayTypes.Image,
        ////        Key = "1",
        ////        Title = Utility.GetResourceStr("SAVECLOSE"),
        ////        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
        ////    };

        ////    items.Add(item);

        //   return items;
        //}

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
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

        private void Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ProgressBar);
            if (validators.Count > 0)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
                // MessageBox.Show(validators.Count.ToString() + " invalid validators");
            }
            else if (txtMoney.Text.Trim() == string.Empty && SalaryItemSet.CALCULATORTYPE == "1")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "GUERDONSUM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "GUERDONSUM"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else if (combcal == null || combcal.SelectedIndex < 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEMTYPE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEMTYPE"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else if (txtCalculateCode.Text == "-1")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CALCULATEFORMULA"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CALCULATEFORMULA"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else
            {
                if (SalaryItemSet.CALCULATORTYPE == "4" || SalaryItemSet.CALCULATORTYPE == "3")
                {
                    try
                    {
                        SalaryItemSet.ENTITYCODE = ((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYCODE;
                        SalaryItemSet.ENTITYNAME = ((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYNAME;
                        SalaryItemSet.ENTITYCOLUMNCODE = ((EntityProPerty)listEntityProperty.SelectedItem).ProPertyCode;
                        SalaryItemSet.ENTITYCOLUMNNAME = ((EntityProPerty)listEntityProperty.SelectedItem).ProPertyName;
                    }
                    catch (Exception ex)
                    {
                        ex.Message.ToString();
                    }
                }
                if (FormType == FormTypes.Edit)
                {
                    SalaryItemSet.ISAUTOGENERATE = Convert.ToBoolean(cbLaterCal.IsChecked) ? "1" : "0";
                    SalaryItemSet.MUSTSELECTED = Convert.ToBoolean(cbMustSelect.IsChecked) ? "1" : "0";
                    SalaryItemSet.CALCULATEFORMULA = txtCalculateFormula.Text;
                    SalaryItemSet.CALCULATEFORMULACODE = txtCalculateCode.Text;
                    SalaryItemSet.GUERDONSUM = string.IsNullOrEmpty(txtMoney.Text) ? 0 : Convert.ToDecimal(txtMoney.Text);
                    SalaryItemSet.CALCULATORTYPE = (SalaryItemSet.CALCULATORTYPE == null) ? "1" : SalaryItemSet.CALCULATORTYPE;
                    SalaryItemSet.SALARYITEMTYPE = (combcal.SelectedIndex + 1).ToString();

                    //if (SalaryItemSet.CALCULATORTYPE == "4")
                    //{
                    //    SalaryItemSet.ENTITYCODE = ((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYCODE;
                    //    SalaryItemSet.ENTITYNAME = ((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYNAME;
                    //    SalaryItemSet.ENTITYCOLUMNCODE = ((EntityProPerty)listEntityProperty.SelectedItem).ProPertyCode;
                    //    SalaryItemSet.ENTITYCOLUMNNAME = ((EntityProPerty)listEntityProperty.SelectedItem).ProPertyName;
                    //}

                    SalaryItemSet.UPDATEDATE = System.DateTime.Now;
                    client.SalaryItemSetUpdateAsync(SalaryItemSet);
                }
                else
                {
                    //if (Onlyone)
                    //{
                    //    RefreshUI(RefreshedTypes.ProgressBar);
                    //    //txtSalaryName.BorderBrush = new SolidColorBrush(Colors.Red);
                    //}
                    //else
                    //{
                    //txtSalaryName.BorderBrush = new SolidColorBrush(Colors.White);

                    SalaryItemSet.ISAUTOGENERATE = Convert.ToBoolean(cbLaterCal.IsChecked) ? "1" : "0";
                    SalaryItemSet.MUSTSELECTED = Convert.ToBoolean(cbMustSelect.IsChecked) ? "1" : "0";
                    SalaryItemSet.CALCULATEFORMULA = txtCalculateFormula.Text;
                    SalaryItemSet.CALCULATEFORMULACODE = txtCalculateCode.Text;
                    SalaryItemSet.GUERDONSUM = string.IsNullOrEmpty(txtMoney.Text) ? 0 : Convert.ToDecimal(txtMoney.Text);
                    SalaryItemSet.CREATEDATE = System.DateTime.Now;
                    SalaryItemSet.CALCULATORTYPE = (SalaryItemSet.CALCULATORTYPE == null) ? "1" : SalaryItemSet.CALCULATORTYPE;
                    SalaryItemSet.SALARYITEMTYPE = (combcal.SelectedIndex + 1).ToString();

                    //if (SalaryItemSet.CALCULATORTYPE == "4" || SalaryItemSet.CALCULATORTYPE == "3")
                    //{
                    //    try
                    //    {
                    //        SalaryItemSet.ENTITYCODE = ((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYCODE;
                    //        SalaryItemSet.ENTITYNAME = ((T_SYS_ENTITYMENU)listEntityName.SelectedItem).ENTITYNAME;
                    //        SalaryItemSet.ENTITYCOLUMNCODE = ((EntityProPerty)listEntityProperty.SelectedItem).ProPertyCode;
                    //        SalaryItemSet.ENTITYCOLUMNNAME = ((EntityProPerty)listEntityProperty.SelectedItem).ProPertyName;
                    //    }
                    //    catch(Exception ex) 
                    //    {
                    //        ex.Message.ToString();
                    //    }
                    //}

                    SalaryItemSet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    SalaryItemSet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    SalaryItemSet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SalaryItemSet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    SalaryItemSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SalaryItemSet.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    SalaryItemSet.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    SalaryItemSet.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                    client.SalaryItemSetAddAsync(SalaryItemSet);
                    //    }
                }
            }
        }
        private void Cancel()
        {
            flag = true;
            Save();
        }

        private void btCalculate_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtSalaryItemName.Text))
            {
                form = new CalculateFormula(txtSalaryItemName.Text, txtCalculateFormula.Text, txtCalculateCode.Text, txtMoney.Text);
                form.RefreshedHandler += new EventHandler(form_RefreshedHandler);
                EntityBrowser browser = new EntityBrowser(form);
                //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinHeight = 500;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEMNAME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEMNAME"));
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cbMustSelect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listEntityName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BindEntityProperty();
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

    }
}
