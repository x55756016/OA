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

using System.Reflection;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class CalculateFormula : BaseForm, IEntityEditor
    {
        SalaryServiceClient client;
        SalaryServiceClient itemclient;
        private const string bzxzid = "1";
        private bool checkResult = false;
        private bool isSave = false;
        List<T_HR_SALARYITEM> listAll = new List<T_HR_SALARYITEM>();
        private bool coesign = true;
        private bool namesign = true;
        private bool symbolsign = true;
        private bool opersign = true;      //逻辑运算符输入验证
        private string symbol;
        private decimal? amount;
        private int counts;
        private int trace;
        private string resultstr;
        private string resultid;
        private string itemName = string.Empty;
        private const string compareSymbol = "+-*/";
        private bool operationType = true;  //ture 是普通公式类型;false 条件公式类型
        private FormTypes formType;
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        public decimal? Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        public string Result
        {
            get { return resultstr; }
            set { resultstr = value; }
        }
        public string ResultID
        {
            get { return resultid; }
            set { resultid = value; }
        }
        public event EventHandler RefreshedHandler;
        public CalculateFormula()
        {
            InitializeComponent();

            listEntityName.ItemsSource = Utility.GetSystemEntity();
            listEntityProperty.ItemsSource = Utility.GetEntityPropertyByName(Utility.GetAllEntityName().FirstOrDefault());
        }   


        public CalculateFormula(string itemname, string CalculateFormulaStr, string CalculateFormulaCode, string money)
        {
            InitializeComponent();
            this.amount = (string.IsNullOrEmpty(money)) ? 0 : money.ToInt32();
            this.symbol = "";
            itemName = itemname;
            tbItemName.Text = itemname + Utility.GetResourceStr("CALFORMULA");
            if (CalculateFormulaCode == "-1") CalculateFormulaCode = string.Empty;
            txtContentCode.Text = CalculateFormulaCode;
            if (!string.IsNullOrEmpty(CalculateFormulaStr))
            {
                //coesign = false;
                //namesign = false;
                //symbolsign = true;
                txtContent.Text = CalculateFormulaStr;
            }
            else
            {
                //coesign = true;
                //namesign = true;
                //symbolsign = false;
            }
            InitPara();
            LoadTree();

            listEntityName.ItemsSource = Utility.GetSystemEntity();
            listEntityProperty.ItemsSource = Utility.GetEntityPropertyByName(Utility.GetAllEntityName().FirstOrDefault());
       
        }

        public void InitPara()
        {
            itemclient = new SalaryServiceClient();
            itemclient.GetSalaryItemSetsCompleted += new EventHandler<GetSalaryItemSetsCompletedEventArgs>(itemclient_GetSalaryItemSetsCompleted);
            itemclient.GetSalaryItemSetByIDCompleted += new EventHandler<GetSalaryItemSetByIDCompletedEventArgs>(itemclient_GetSalaryItemSetByIDCompleted);
        }

        void itemclient_GetSalaryItemSetByIDCompleted(object sender, GetSalaryItemSetByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                    txtItemcode.Text = e.Result.CALCULATEFORMULA;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void itemclient_GetSalaryItemSetsCompleted(object sender, GetSalaryItemSetsCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    listAll = e.Result.ToList();
                    LoadItemType();
                }
                //else
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("is null"));
                //}
            }
        }

        private void insertItem()
        {
            TreeViewItem selectedItem = itemType.SelectedItem as TreeViewItem;
            if (selectedItem != null && namesign && selectedItem.Tag.ToString().Length > 30)
            {
                string IsTag = selectedItem.Tag.ToString();
                txtContent.Text += selectedItem.Header;
                //coesign = false;
                //namesign = false;
                //symbolsign = true;
                //opersign = true;

                txtContentCode.Text += "{" + IsTag + "}";
                txtContent1.Text += "{" + IsTag + "}";
            }
            else
            {
                return;
            }
        }

        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //Utility.SetAuditEntity(entity, "T_HR_CUSTOMGUERDONSET", CustomGuerdonSet.CUSTOMGUERDONSETID);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //Utility.UpdateCheckState("T_HR_CUSTOMGUERDONSET", "CUSTOMGUERDONSETID", customguerdonsetid, args);
            //string state = "";
            //switch (args)
            //{
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
            //        state = Utility.GetCheckState(CheckStates.Approving);
            //        break;
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
            //        state = Utility.GetCheckState(CheckStates.Approved);
            //        break;
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
            //        state = Utility.GetCheckState(CheckStates.UnApproved);
            //        break;
            //}
            //CustomGuerdonSet.CHECKSTATE = state;
            //CustomGuerdonSet.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            //client.CustomGuerdonSetUpdateAsync(CustomGuerdonSet, "Audit");
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                switch (rb.Name)
                {
                    case "rbadd":
                        //CustomGuerdonSet.GUERDONCATEGORY = "1";
                        break;
                    case "rbsubtract":
                        //CustomGuerdonSet.GUERDONCATEGORY = "2";
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            //if (CustomGuerdonSet != null)
            //    state = CustomGuerdonSet.CHECKSTATE;
            return state;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CALCULATEFORMULA");
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
                //case "0":
                //    Check();
                //    break;
                //case "1":
                //    Clears();
                //    break;
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
            //    Title = "薪资标准",
            //    Tooltip = "薪资标准",
            //    Url = "/Salary/SalaryStandard.xaml"
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
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            items.Clear();

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "1",
            //    Title = Utility.GetResourceStr("CLEAR"),
            //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            //};

            //items.Add(item);

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "0",
            //    Title = Utility.GetResourceStr("CHECK"),
            //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            //};

            //items.Add(item);

            return items;
        }

        //public List<ToolbarItem> GetToolBarItems()
        //{
        //    return ToolbarItems;
        //}

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
            if (!Checks())
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATINPUTERRORS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATINPUTERRORS"));
                return;
            }
            if (symbolsign)
            {
                ExecuteResult(true);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATINPUTERRORS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATINPUTERRORS"));
            }
        }
        private void Cancel()
        {
            isSave = true;
            CheckFormula();
            //if (!checkResult)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATINPUTERRORS"));
            //    return;
            //}
            //ExecuteResult(false);
        }
        private void Clears()
        {
            amount = 0;
            txtContent.Text = "";
            txtContentCode.Text = "";
            //namesign = true;
            //symbolsign = false;
        }

        private void Check()
        {
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OK"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OK"));
        }

        #region 加载树

        public void LoadTree()
        {
            itemclient.GetSalaryItemSetsAsync(string.Empty);
        }
        public List<T_SYS_DICTIONARY> GetItemType()
        {
            var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                       where a.DICTIONCATEGORY == "SALARYITEMTYPE"
                       select a;
            return ents.ToList();
        }

        public void LoadItemType()
        {
            itemType.Items.Clear();
            foreach (var ent in GetItemType())
            {
                TreeViewItem tvitem = new TreeViewItem();
                tvitem.Header = ent.DICTIONARYNAME;
                tvitem.DataContext = ent;
                tvitem.Tag = ent.DICTIONARYVALUE;
                itemType.Items.Add(tvitem);
                LoadChildren(tvitem, ent.DICTIONARYVALUE.ToString());
            }
        }

        public void LoadChildren(TreeViewItem tvi, string parentid)
        {
            if (parentid == bzxzid)
            {
                TreeViewItem tvitemchild = new TreeViewItem();
                tvitemchild.Header = Utility.GetResourceStr("STANDARDSALARY");
                tvitemchild.Tag = GetBzxzid();
                tvi.Items.Add(tvitemchild);
            }
            List<T_HR_SALARYITEM> getlist = GetChildContent(parentid);
            if (getlist != null)
            {
                foreach (T_HR_SALARYITEM ent in getlist)
                {
                    TreeViewItem tvitemchild = new TreeViewItem();
                    tvitemchild.Header = ent.SALARYITEMNAME;
                    tvitemchild.DataContext = ent;
                    tvitemchild.Tag = ent.SALARYITEMID;
                    tvi.Items.Add(tvitemchild);
                }
            }

        }

        public List<T_HR_SALARYITEM> GetChildContent(string pid)
        {
            List<T_HR_SALARYITEM> listchild = new List<T_HR_SALARYITEM>();
            if (listAll != null)
            {
                foreach (T_HR_SALARYITEM list in listAll)
                {
                    if (list.SALARYITEMTYPE == pid && list.SALARYITEMNAME != itemName && list.SALARYITEMNAME != Utility.GetResourceStr("STANDARDSALARY") && list.CREATECOMPANYID == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                        listchild.Add(list);
                }
            }
            return listchild;
        }

        public string GetBzxzid()
        {                              //得到标准薪资ID
            string BZXZ = "bzxz";
            for (int i = 0; i < 3; i++)
            {
                BZXZ += BZXZ;
            }
            return BZXZ;
        }

        #endregion

        private void itemType_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            txtItemcode.Text = "";
            try
            {
                TreeViewItem tv = itemType.SelectedItem as TreeViewItem;
                itemclient.GetSalaryItemSetByIDAsync(tv.Tag.ToString());
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        private void ExecuteResult(bool sign)
        {
            if (RefreshedHandler != null)
            {
                resultstr = txtContent.Text;
                resultid = txtContentCode.Text;
                RefreshedHandler(this, new EventArgs());
            }
            if (!sign)
            {
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        private void btInsertItem_Click(object sender, RoutedEventArgs e)
        {
            insertItem();
        }

        private void btInsertSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (lbsign.SelectedItems.Count > 0)
            {
                ListBoxItem lbi = lbsign.SelectedItem as ListBoxItem;

                //if (!symbolsign)
                //{
                //    if (lbi.Content.ToString() == "(")
                //    {
                //        if (string.IsNullOrEmpty(txtContent.Text) && string.IsNullOrEmpty(txtContentCode.Text))
                //        {
                //            symbolsign = true;
                //        }                        
                //    }
                //}

                if (symbolsign)
                {
                    symbol = lbi.Content.ToString();
                    txtContent.Text += lbi.Content.ToString();
                    txtContentCode.Text += lbi.Content.ToString() + " ";

                    txtContent1.Text += " " + lbi.Content.ToString()+" ";
                    //coesign = true;
                    //namesign = true;
                    //opersign = true;
                    //symbolsign = false;
                }
            }
        }

        private void btInsertCoe_Click(object sender, RoutedEventArgs e)
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count <= 0)
            {
                if (coesign && (!string.IsNullOrEmpty(txtCoefficient.Text.Trim())))
                {
                    txtContent.Text += txtCoefficient.Text;
                    txtContentCode.Text += txtCoefficient.Text + " ";

                    txtContent1.Text += " " + txtCoefficient.Text + " ";
                    //namesign = false;
                    //coesign = false;
                    //opersign = true;
                    //symbolsign = true;
                }
            }
        }      

        private void btInsertLogicOper_Click(object sender, RoutedEventArgs e)
        {
            if (operationType)
            {
                return;
            }
            if (cbxkLogicOper.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkLogicOper.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID))
            {
                return;
            }

            if (opersign)
            {
                txtContent.Text += " " + entDic.DICTIONARYNAME + " ";
                txtContentCode.Text += entDic.DICTIONARYNAME + " ";

                txtContent1.Text += " " + entDic.DICTIONARYNAME + " ";
                //coesign = true;
                //namesign = true;
                //symbolsign = true;
                //opersign = false;
            }
        }

        private void btInsertEntityProperty_Click(object sender, RoutedEventArgs e)
        {
            if (operationType)
            {
                return;
            }
            if (listEntityName.SelectedItem == null)
            {
                return;
            }
            T_SYS_ENTITYMENU enttitymenu = listEntityName.SelectedItem as T_SYS_ENTITYMENU;
            if (string.IsNullOrEmpty(enttitymenu.ENTITYCODE) || string.IsNullOrEmpty(enttitymenu.ENTITYNAME))
            {
                return;
            }

            if (listEntityProperty.SelectedItem == null)
            {
                return;
            }

            EntityProPerty entProperty = listEntityProperty.SelectedItem as EntityProPerty;
            if (string.IsNullOrEmpty(entProperty.ProPertyCode))
            {
                return;
            }

            txtContent.Text += " " + enttitymenu.ENTITYNAME + "." + entProperty.ProPertyName + " ";
            txtContentCode.Text += enttitymenu.ENTITYCODE + "." + entProperty.ProPertyCode +" ";

            txtContent1.Text += enttitymenu.ENTITYCODE + "." + entProperty.ProPertyCode + " ";
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            amount = 0;
            txtContent.Text = txtContent1.Text = string.Empty;
            txtContentCode.Text = string.Empty;
            //coesign = false;
            //namesign = true;
            //symbolsign = false;
        }

        private void btCheck_Click(object sender, RoutedEventArgs e)
        {
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式合法"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("公式合法"));
            isSave = false;
            //CheckFormula();
        }

        private void CheckFormula()
        {
            counts = 0;
            trace = 0;
            checkResult = false;
            try
            {
                client = new SalaryServiceClient();
                string[] codes = txtContentCode.Text.Split(',');
                if (codes.Length > 0 && CheckSymbol(codes[codes.Length - 2]))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("算法不完整"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("算法不完整"));
                    return;
                }
                bool ret = false;
                int i = 0;
                client.CheckCalItemCompleted += new EventHandler<CheckCalItemCompletedEventArgs>(client_CheckCalItemCompleted);
                foreach (string code in codes)
                {
                    if (i < codes.Length - 1 && code.Length > 31)
                    {
                        counts++;
                        trace++;
                        client.CheckCalItemAsync(code, code, ret);
                    }
                    i++;
                }
                if (trace == 0)
                {
                    if (Checks())
                    {
                        checkResult = true;
                        if (isSave) ExecuteResult(false);
                        else
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式合法"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("公式合法"));
                    }
                    else
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式算法有错"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                       // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式算法有错"));
                }
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式算法有错,请核对后处理"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式算法有错,请核对后处理"));
            }
        }

        public bool CheckSymbol(string symbol)
        {
            if (symbol.Length == 1)
            {
                if (compareSymbol.Contains(symbol)) return true;
                else return false;
            }
            return false;
        }

        void client_CheckCalItemCompleted(object sender, CheckCalItemCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!e.ret)
                {
                    counts--;
                }
            }
            trace--;
            if (trace == 0)
            {
                if (counts == 0)
                {
                    checkResult = true;
                    if (isSave) ExecuteResult(false);
                    else
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式合法"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("公式合法"));
                }
                else
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式算法有错"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("公式算法有错"));
            }
        }

        void client_AutoCalItemCompleted(object sender, AutoCalItemCompletedEventArgs e)
        {
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("运算结果:"), Utility.GetResourceStr(e.Result.ToString()));
        }

        private bool Checks()
        {
            try
            {
                bool checkcode = false;
                string[] splitstr = txtContentCode.Text.Split(',');
                foreach (var li in listAll)
                {
                    foreach (string s in splitstr)
                    {
                        if (li.SALARYITEMID == s || s == GetBzxzid())
                        {
                            checkcode = true;
                            break;
                        }
                    }
                }
                if (txtContentCode.Text.IndexOf(GetBzxzid()) > 0 || checkcode)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            ExecuteResult(false);
           // Cancel();
        }

        private void listEntityName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BindEntityProperty();
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
            var q = from e in ents
                    where e.ProPertyCode == strEntityPropertyCode
                    select e;

            if (q.Count() == 0)
            {
                return;
            }

            listEntityProperty.SelectedItem = q.FirstOrDefault();
        }

        private void btInsertEntityTable_Click(object sender, RoutedEventArgs e)
        {
            if (operationType)
            {
                return;
            }
            if (listEntityName.SelectedItem == null)
            {
                return;
            }
            T_SYS_ENTITYMENU enttitymenu = listEntityName.SelectedItem as T_SYS_ENTITYMENU;
            if (string.IsNullOrEmpty(enttitymenu.ENTITYCODE) || string.IsNullOrEmpty(enttitymenu.ENTITYNAME))
            {
                return;
            }
            txtContent.Text += " " + enttitymenu.ENTITYNAME + " ";
            txtContentCode.Text += enttitymenu.ENTITYCODE + " ";

            txtContent1.Text += " " + enttitymenu.ENTITYCODE + " ";
        }

        private void cbkInsertFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(cbkInsertFilter.IsChecked))
            {
                operationType = false;
            }
            else
            {
                operationType = true;
            }
            //txtContent.Text = txtContentCode.Text = txtContent1.Text = string.Empty;
        }                
    }
}
