using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;

using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysDictionaryForm : UserControl, IEntityEditor
    {
        private T_SYS_DICTIONARY dictionary { get; set; }
        private PermissionServiceClient client = new PermissionServiceClient();
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public SysDictionaryForm(FormTypes type, string dictID)
        {

            InitializeComponent();
            InitParas();

            FormType = type;
            if (string.IsNullOrEmpty(dictID))
            {
                dictionary = new T_SYS_DICTIONARY();
                dictionary.DICTIONARYID = Guid.NewGuid().ToString();
                dictionary.CREATEUSER = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                dictionary.CREATEDATE = System.DateTime.Now;

                dictionary.UPDATEDATE = System.DateTime.Now;
                //dictionary.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
                client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
                this.DataContext = dictionary;
            }
            else
            {
                //根据ID获取字典
                client.GetSysDictionaryByIDAsync(dictID);
            }
            lkDictionaryType.TxtLookUp.IsEnabled = true;
            if (type == FormTypes.Browse)
            {
                UnEnableFormControl();
            }
        }
        /// <summary>
        /// 禁用表单控件
        /// </summary>
        private void UnEnableFormControl()
        {
            cbxSystemType.IsEnabled = false;
            txtDictionayName.IsEnabled = false;
            txtOrderNumber.IsEnabled = false;
            //txtRemark.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            txtDictCatogry.IsEnabled = false;
            txtSystemCode.IsEnabled = false;
            lkDictionaryType.IsEnabled = false;
            lkFather.IsEnabled = false;
        }

        ///// <summary>
        ///// 绑定系统类型
        ///// </summary>
        //void BindCombox()
        //{
        //    List<SystemType> sys = new List<SystemType>
        //    {
        //        new SystemType{SysTypeCode="001",SysTypeName="Commom"},
        //        new SystemType{SysTypeCode="002",SysTypeName="HR"},
        //        new SystemType{SysTypeCode="003",SysTypeName="OA"},
        //        new SystemType{SysTypeCode="004",SysTypeName="LM"}
        //    };
        //    cbSystemName.ItemsSource = sys;
        //    cbSystemName.UpdateLayout();
        //    cbSystemName.SelectedIndex = 0;
        //}
        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SYSDICTIONARY");
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
                    closeFormFlag = true;
                    Save();
                    // Close();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
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
        void InitParas()
        {
            client.GetSysDictionaryByIDCompleted += new EventHandler<GetSysDictionaryByIDCompletedEventArgs>(client_GetSysDictionaryByIDCompleted);
            client.SysDictionaryAddCompleted += new EventHandler<SysDictionaryAddCompletedEventArgs>(client_SysDictionaryAddCompleted);
            client.SysDictionaryUpdateCompleted += new EventHandler<SysDictionaryUpdateCompletedEventArgs>(client_SysDictionaryUpdateCompleted);
            client.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(client_GetSysDictionaryByCategoryCompleted);
        }





        void client_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                cbxSystemType.ItemsSource = e.Result;
                foreach (T_SYS_DICTIONARY item in cbxSystemType.Items)
                {
                    if (item.DICTIONARYNAME == dictionary.SYSTEMNAME)
                    {
                        cbxSystemType.SelectedItem = item;
                    }
                }
            }
        }

        void client_SysDictionaryUpdateCompleted(object sender, SysDictionaryUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                // ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("MODIFYSUCCESSED", "SYSDICTIONARY"), Utility.GetResourceStr("CONFIRMBUTTON"));
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITION", "SYSDICTIONARY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //  Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED", "SYSDICTIONARY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                }
                RefreshUI(RefreshedTypes.All);
            }
        }
        void client_SysDictionaryAddCompleted(object sender, SysDictionaryAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ADDSUCCESSED", "SYSDICTIONARY"), Utility.GetResourceStr("CONFIRMBUTTON"));
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITION", "SYSDICTIONARY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SYSDICTIONARY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                }
                RefreshUI(RefreshedTypes.All);
            }
        }

        void client_GetSysDictionaryByIDCompleted(object sender, GetSysDictionaryByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    dictionary = e.Result;
                    this.DataContext = dictionary;
                    client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
                    InitBindData();


                }
            }
        }
        void InitBindData()
        {
            ////初始化页面 不能能自动绑定的控件
            //for (int i = 0; i < cbSystemName.Items.Count; i++)
            //{
            //    if ((cbSystemName.Items[i] as SystemType).SysTypeCode == dictionary.SYSTEMCODE)
            //    {
            //        cbSystemName.SelectedIndex = i;
            //        break;
            //    }

            //}
            lkDictionaryType.DataContext = dictionary;

            lkFather.DataContext = dictionary.T_SYS_DICTIONARY2;
            // lkSysType.DataContext = dictionary;
        }
        public void Save()
        {

            RefreshUI(RefreshedTypes.ShowProgressBar);
            dictionary.DICTIONCATEGORY = txtDictCatogry.Text.Trim();
            dictionary.DICTIONCATEGORYNAME = lkDictionaryType.TxtLookUp.Text.Trim();

            dictionary.SYSTEMCODE = txtSystemCode.Text;
            // dictionary.SYSTEMNAME = lkSysType.TxtLookUp.Text.Trim();
            if (cbxSystemType.SelectedItem != null)
            {
                dictionary.SYSTEMNAME = (cbxSystemType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SYSTEMTYPE") + Utility.GetResourceStr("NOTALLOWNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (string.IsNullOrEmpty(dictionary.DICTIONCATEGORY))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("DICTIONCATEGORY") + Utility.GetResourceStr("NOTALLOWNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;

            }
            if (string.IsNullOrEmpty(dictionary.DICTIONARYNAME))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("DICTIONARYNAME") + Utility.GetResourceStr("NOTALLOWNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;

            }
            else
            {
                dictionary.DICTIONARYNAME = dictionary.DICTIONARYNAME.Trim();
            }
            //if (string.IsNullOrEmpty(dictionary.DICTIONCATEGORYNAME))
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Resource.DICTIONARYNAME + Utility.GetResourceStr("NOTALLOWNULL"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}
            //if (string.IsNullOrEmpty(dictionary.DICTIONARYVALUE.ToString()))
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DICTIONARYVALUE") + Utility.GetResourceStr("NOTALLOWNULL"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}
            string strMsg = string.Empty;
            if (FormType == FormTypes.New)
            {
                client.SysDictionaryAddAsync(dictionary, strMsg);
            }
            else
            {

                dictionary.UPDATEDATE = System.DateTime.Now;
                //dictionary.UPDATEUSER = Common.CurrentLoginUserInfo.EmployeeID;

                client.SysDictionaryUpdateAsync(dictionary, strMsg);
            }


        }
        void Close()
        {

        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lkFather_FindClick(object sender, EventArgs e)
        {
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("DICTIONCATEGORY", "DICTIONCATEGORYNAME");
            ////cols.Add("DICTIONCATEGORY", "DICTIONCATEGORY");
            ////cols.Add("DICTIONARYNAME", "DICTIONARYNAME");
            ////cols.Add("DICTIONARYVALUE", "DICTIONARYVALUE");
            ////cols.Add("DICTIONCATEGORYNAME", "DICTIONCATEGORYNAME");
            ////string[] cols = { "DICTIONCATEGORY", "DICTIONCATEGORYNAME" };

            //LookupForm lookup = new LookupForm(EntityNames.SysDictionary,
            //    typeof(List<Permission.UI.PermissionService.T_SYS_DICTIONARY>), cols);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("SysDictionary", "1");
            //cols.Add("DICTIONCATEGORY", "DICTIONCATEGORY");
            //cols.Add("DICTIONARYNAME", "DICTIONARYNAME");
            //cols.Add("DICTIONARYVALUE", "DICTIONARYVALUE");
            //cols.Add("DICTIONCATEGORYNAME", "DICTIONCATEGORYNAME");
            string[] cols = { "DICTIONCATEGORY", "DICTIONCATEGORYNAME", "DICTIONARYNAME" };

            LookupForm lookup = new LookupForm(EntityNames.SysDictionary,
                typeof(List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>), cols, para);
            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY ent = lookup.SelectedObj as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

                if (ent != null)
                {
                    lkFather.DataContext = ent;
                    dictionary.T_SYS_DICTIONARY2 = new T_SYS_DICTIONARY();
                    dictionary.T_SYS_DICTIONARY2.DICTIONARYID = ent.DICTIONARYID;
                }
            };

            lookup.Show();
        }

        private void lkDictionaryType_FindClick(object sender, EventArgs e)
        {
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("DICTIONCATEGORY", "DICTIONCATEGORYNAME");
            ////string[] cols = { "DICTIONCATEGORY", "DICTIONCATEGORYNAME" };

            //LookupForm lookups = new LookupForm(EntityNames.SysDictionary,
            //    typeof(List<Permission.UI.PermissionService.T_SYS_DICTIONARY>), cols);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("SysDictionary", "2");
            string[] cols = { "DICTIONCATEGORY", "DICTIONCATEGORYNAME" };

            LookupForm lookups = new LookupForm(EntityNames.SysDictionary,
                typeof(List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>), cols, para);
            lookups.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY ent = lookups.SelectedObj as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

                if (ent != null)
                {
                    lkDictionaryType.DataContext = ent;

                    txtDictCatogry.Text = ent.DICTIONCATEGORY.Trim();
                    this.lkDictionaryType.TxtLookUp.IsReadOnly = true;
                    txtDictCatogry.IsReadOnly = true;
                }
            };
            lookups.Show();
        }
        //public class SystemType
        //{
        //    public string SysTypeCode { get; set; }
        //    public string SysTypeName { get; set; }
        //}
        #region
        private void lkSysType_FindClick(object sender, EventArgs e)
        {
            // Dictionary<string, string> cols = new Dictionary<string, string>();
            // cols.Add("SYSTEMCODE", "SYSTEMNAME");
            //// string[] cols = { "SYSTEMCODE", "SYSTEMNAME" };

            // LookupForm lookups = new LookupForm( EntityNames.SysDictionary,
            //     typeof(List<Permission.UI.PermissionService.T_SYS_DICTIONARY>), cols);
            //Dictionary<string, string> para = new Dictionary<string, string>();
            //para.Add("SysDictionary", "2");
            //string[] cols = { "SYSTEMCODE", "SYSTEMNAME" };

            //LookupForm lookups = new LookupForm(EntityNames.SysDictionary,
            //    typeof(List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>), cols, para);

            //lookups.SelectedClick += (o, ev) =>
            //{
            //    SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY ent = lookups.SelectedObj as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

            //    if (ent != null)
            //    {
            //        lkSysType.DataContext = ent;

            //        if (!string.IsNullOrEmpty(ent.SYSTEMCODE))
            //        {
            //            txtSystemCode.Text = ent.SYSTEMCODE.Trim();
            //        }
            //        else
            //        {
            //            txtSystemCode.Text = "";
            //        }

            //        this.lkSysType.TxtLookUp.IsReadOnly = true;
            //        txtSystemCode.IsReadOnly = true;
            //    }
            //};
            //lookups.Show();
        }
        #endregion

        private void cbxSystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dictionary.SYSTEMNAME = (cbxSystemType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME;
        }
    }
}
