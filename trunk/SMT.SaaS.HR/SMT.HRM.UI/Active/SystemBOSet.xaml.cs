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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.Saas.Tools.FlowDesignerWS;

namespace SMT.HRM.UI.Active
{


    public partial class SystemBOSet : UserControl, IEntityEditor
    {

        ServiceClient WFBOService = new ServiceClient();

        public event EventHandler<Bo_OnClickEventArgs> Bo_Onclick;

        public SystemBOSet()
        {
            InitializeComponent();
            requestBOSystemList();
        }

        public void requestBOSystemList()
        {
            WFBOService.GetSystemListCompleted += new EventHandler<GetSystemListCompletedEventArgs>(LoadBusinessSystem);
            WFBOService.GetSystemListAsync();
        }

        private void cboBusinessSystem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                        
            if (cboBusinessSystem.SelectedItem != null)
            {
                WFBOService.GetSystemBOListCompleted += new EventHandler<GetSystemBOListCompletedEventArgs>(LoadBOList);
                WFBOService.GetSystemBOListAsync((WFBOSystem)cboBusinessSystem.SelectedItem);
            }
        }

        private void cboBusinessObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            //if (cboBusinessSystem.SelectedItem != null && cboBusinessObject.SelectedItem != null)
            //{
            //    WFBOService.GetSystemBOAttributeListCompleted += new EventHandler<SMT.FlowDesigner.WFBOServiceReference.GetSystemBOAttributeListCompletedEventArgs>(LoadAttrbuteList);
            //    WFBOService.GetSystemBOAttributeListAsync((WFBOServiceReference.WFBOSystem)cboBusinessSystem.SelectedItem, (WFBOServiceReference.WFBOObject)cboBusinessObject.SelectedItem);
            //}
        }


        void LoadBusinessSystem(object sender, GetSystemListCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                cboBusinessSystem.ItemsSource = e.Result.ToList();
                cboBusinessSystem.SelectedIndex = 0;
            }
        }

        void LoadBOList(object sender, GetSystemBOListCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                cboBusinessObject.ItemsSource = e.Result.ToList();
                cboBusinessObject.SelectedIndex = 0;
             }
        }


            
        //private void btnClose_Click(object sender, RoutedEventArgs e)
        //{
        //    //this.DialogResult = false;
        //}

        //private void btnOK_Click(object sender, RoutedEventArgs e)
        //{
        //    //this.DialogResult = true;
        //}

        //private void btnDele_Click(object sender, RoutedEventArgs e)
        //{

        //     //this.DialogResult = true;
        //}

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "所属业务系统和业务对象设置";
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
                    Delete();
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

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("DELBUTTON"),
                ImageUrl = "/SMT.SAAS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SAAS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        public void Save()
        {
            if (this.Bo_Onclick != null)
                this.Bo_Onclick(this, new Bo_OnClickEventArgs(true));
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        public void Delete()
        {
            this.Bo_Onclick(this, new Bo_OnClickEventArgs(true));
            RefreshUI(RefreshedTypes.Close);
        }
    }

     public class Bo_OnClickEventArgs : EventArgs
    {
         public Bo_OnClickEventArgs(object Content)
        {
            this.bo_Content = Content;
        }
        public object bo_Content { get; set; }
    }
}

