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
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.AuditControl;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class CityForm : BaseForm, IEntityEditor,IClient
    {

        public T_HR_AREACITY areacity { get; set; }
        public FormTypes FormType { get; set; }
        private string areaID { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
        public CityForm(FormTypes type, string areaID, string CITYID)
        {
            InitializeComponent();

            InitParas();
            this.areaID = areaID;
            FormType = type;
            if (string.IsNullOrEmpty(CITYID))
            {
                areacity = new T_HR_AREACITY();
                areacity.AREACITYID = Guid.NewGuid().ToString();
                areacity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                areacity.CREATEDATE = System.DateTime.Now;

                //areacity.UPDATEDATE = System.DateTime.Now;
                //areacity.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                //this.DataContext = areacity;
            }

        }

        private void InitParas()
        {
            client.AreaCityAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCityAddCompleted);
            client.GetAreaCategoryCompleted += new EventHandler<GetAreaCategoryCompletedEventArgs>(client_GetAreaCategoryCompleted);
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(permissionClient_GetSysDictionaryByFatherIDCompleted);
            client.GetAreaCategoryAsync();
        }

        void client_GetAreaCategoryCompleted(object sender, GetAreaCategoryCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                cbArea.ItemsSource = e.Result;
                foreach(var item in e.Result)
                {
                    if(item.AREADIFFERENCEID==areaID)
                    {
                        cbArea.SelectedItem = item;
                    }
                }

            }
        }

        void permissionClient_GetSysDictionaryByFatherIDCompleted(object sender, GetSysDictionaryByFatherIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                cbCity.ItemsSource = e.Result;

            }
        }

        void client_AreaCityAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CITY"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);

        }



        //void client_AreaCategoryUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "AREADIFFERENCE"));
        //    }

        //    RefreshUI(RefreshedTypes.All);
        //}


        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CITY");
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
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("BASEINFO"),
            //    Tooltip = Utility.GetResourceStr("BASEINFO")
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("SALARYARCHIVE"),
            //    Tooltip = Utility.GetResourceStr("SALARYARCHIVE"),
            //    Url = "/Salary/SalaryArchive"
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

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        public bool Save()
        {

            RefreshUI(RefreshedTypes.ProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            //if (validators.Count > 0)
            //{
            //    //could use the content of the list to show an invalid message summary somehow
            //    //MessageBox.Show(validators.Count.ToString() + " invalid validators");
            //}
            //else
            //{



            //if (FormType == FormTypes.Edit)
            //{
            //    area.UPDATEDATE = System.DateTime.Now;
            //    area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //    client.AreaCategoryUpdateAsync(area);
            //}
            //else
            //{
            if (cbProvince.SelectedIndex >=0 && cbCity.SelectedIndex >=0)
            {
                areacity.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
                areacity.T_HR_AREADIFFERENCE.AREADIFFERENCEID = (cbArea.SelectedItem as T_HR_AREADIFFERENCE).AREADIFFERENCEID;
                areacity.CITY = (cbCity.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                client.AreaCityAddAsync(areacity);
            }

            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CITY"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            //}
            return true;
            //}
        }
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }



        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cbProvince_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbProvince.SelectedIndex >=0)
            {
                permissionClient.GetSysDictionaryByFatherIDAsync((cbProvince.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYID);
            }
            else
            {
                cbCity.SelectedIndex = -1;
            }
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
