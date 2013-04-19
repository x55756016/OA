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
    public partial class AreaSortForm : BaseForm, IEntityEditor, IClient
    {
        private bool sign = false;
        SalaryServiceClient client;
        private FormTypes formType;
        private string citycode = string.Empty;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        System.Collections.ObjectModel.ObservableCollection<T_HR_AREACITY> citys = new System.Collections.ObjectModel.ObservableCollection<T_HR_AREACITY>();
        public T_HR_AREADIFFERENCE area { get; set; }
        private string areaID { get; set; }
        public T_HR_AREACITY areacity { get; set; }
        //实体中已选的城市
        private string ExistCity = "";

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        public AreaSortForm()
        {
            InitializeComponent();
        }

        public AreaSortForm(FormTypes type, string areaID)
        {
            InitializeComponent();
            InitParas();
            this.areaID = areaID;
            FormType = type;
            if (string.IsNullOrEmpty(areaID))
            {
                area = new T_HR_AREADIFFERENCE();
                area.AREADIFFERENCEID = Guid.NewGuid().ToString();
                area.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                area.CREATEDATE = System.DateTime.Now; 

                area.UPDATEDATE = System.DateTime.Now;
                area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = area;
            }
            else
            {
                client.GetAreaCategoryByIDAsync(areaID);
            }
        }

        private void InitParas()
        {
            areacity = new T_HR_AREACITY();
            client = new SalaryServiceClient();
            client.GetAreaCategoryByIDCompleted += new EventHandler<GetAreaCategoryByIDCompletedEventArgs>(client_GetAreaCategoryByIDCompleted);
            client.AreaCategoryADDCompleted += new EventHandler<AreaCategoryADDCompletedEventArgs>(client_AreaCategoryADDCompleted);
            client.AreaCategoryUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCategoryUpdateCompleted);

            client.AreaCityCheckCompleted += new EventHandler<AreaCityCheckCompletedEventArgs>(client_AreaCityCheckCompleted);
            client.AreaCityByCategoryDeleteCompleted += new EventHandler<AreaCityByCategoryDeleteCompletedEventArgs>(client_AreaCityByCategoryDeleteCompleted);
            client.AreaCityAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCityAddCompleted);
            client.AreaCityLotsofAddCompleted += new EventHandler<AreaCityLotsofAddCompletedEventArgs>(client_AreaCityLotsofAddCompleted);
            client.GetAreaCityByCategoryCompleted += new EventHandler<GetAreaCityByCategoryCompletedEventArgs>(client_GetAreaCityByCategoryCompleted);
        }

        void client_AreaCityCheckCompleted(object sender, AreaCityCheckCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result.Count > 0)
                {
                    string resultStr = string.Empty;
                    Dictionary<string, string> tmp = e.Result;                    
                    string[] arr = null;//实体的城市
                    if (ExistCity != "")
                    {
                        arr = ExistCity.Split(',');
                    }
                    //遍历已经在使用的城市集合
                    foreach (var a in tmp.Keys)
                    {
                        //如果实体的城市中没有的就加提示
                        if (!arr.Contains(a))
                        {
                            resultStr += tmp[a].ToString() + Utility.GetResourceStr("MIDDLETYPES") + "[" + GetCityName(a) + "]\n";   
                        }                        
                    }
                    //如果存在已选的城市就提示
                    if (resultStr != "")
                    {
                        resultStr += Utility.GetResourceStr("ALREADYEXISTNOTADD");
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(resultStr), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                    else
                    {
                        //citycode = citys;
                        if (e.UserState.ToString() != "Add")
                            client.AreaCityByCategoryDeleteAsync(area.AREADIFFERENCEID);
                        else
                            client.AreaCityLotsofAddAsync(citys, "Add");
                    }
                }
                else
                {
                    if (e.UserState.ToString() != "Add")
                        client.AreaCityByCategoryDeleteAsync(area.AREADIFFERENCEID);
                    else
                        client.AreaCityLotsofAddAsync(citys, "Add");
                }
            }
        }

        void client_AreaCityByCategoryDeleteCompleted(object sender, AreaCityByCategoryDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (string.IsNullOrEmpty(citycode))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CITY"));
                    return;
                }
                client.AreaCityLotsofAddAsync(citys, "Update");
            }
        }

        void client_GetAreaCityByCategoryCompleted(object sender, GetAreaCityByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    scSearchCity.TxtSelectedCity.Text = string.Empty;
                    foreach (var ent in e.Result.ToList())
                    {
                        scSearchCity.TxtSelectedCity.Text += GetCityName(ent.CITY) + " ";
                        citycode += ent.CITY + ",";
                    }
                    ExistCity = citycode;
                }
            }
        }

        private string GetCityName(string cityvalue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.DICTIONARYVALUE == Convert.ToDecimal(cityvalue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        void client_AreaCityLotsofAddCompleted(object sender, AreaCityLotsofAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.UserState.ToString() == "Add")
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "AREADIFFERENCECATEGORY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "AREADIFFERENCECATEGORY"));
                else
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "AREADIFFERENCECATEGORY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "AREADIFFERENCECATEGORY"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (sign)
            {
                sign = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_AreaCityAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            //else
            //{
            //   // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CITY"));
            //}
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_AreaCategoryADDCompleted(object sender, AreaCategoryADDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == "SUCCESSED")
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    client.AreaCityCheckAsync(citys, "Add");
                    //client.AreaCityLotsofAddAsync(citys,"Add");
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }

            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_AreaCategoryUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                client.AreaCityCheckAsync(citys, "Update");
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_GetAreaCategoryByIDCompleted(object sender, GetAreaCategoryByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                area = e.Result;
                this.DataContext = area;
                //加载城市
                client.GetAreaCityByCategoryAsync(area.AREADIFFERENCEID);
            }
        }

        public void Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //   Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else
            {
                citys.Clear();
                string[] codes = citycode.Split(',');
                foreach (string code in codes)
                {
                    if (!string.IsNullOrEmpty(code))
                    {
                        T_HR_AREACITY temp = new T_HR_AREACITY();
                        temp.AREACITYID = Guid.NewGuid().ToString();
                        temp.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
                        temp.T_HR_AREADIFFERENCE.AREADIFFERENCEID = area.AREADIFFERENCEID;
                        temp.CITY = code;
                        citys.Add(temp);
                    }
                }

                if (FormType == FormTypes.Edit)
                {
                    area.UPDATEDATE = System.DateTime.Now;
                    area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.AreaCategoryUpdateAsync(area);
                }
                else
                {
                    client.AreaCategoryADDAsync(area);
                }
            }

        }

        public void Cancel()
        {
            Save();
            sign = true;
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("AREACATEGORY");
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
            //NavigateItem item = new NavigateItem
            //{
            //    Title = "详细信息",
            //    Tooltip = "详细信息"
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

        #region IClient
        public void ClosedWCFClient()
        {
            //client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }


        void browser_ReloadDataEvent()
        {

        }

        private void scSearchCity_SelectClick(object sender, EventArgs e)
        {
            AreaSortCityForm SelectCity = new AreaSortCityForm();
            SelectCity.SelectedClicked += (obj, ea) =>
            {
                scSearchCity.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                citycode = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("SELECTEDCITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is AreaSortCityForm)
            {
                (SelectCity as AreaSortCityForm).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
        }
    }
}
