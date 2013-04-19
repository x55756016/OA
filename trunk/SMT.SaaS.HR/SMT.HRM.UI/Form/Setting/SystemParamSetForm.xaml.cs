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
using System.Collections.ObjectModel;



namespace SMT.HRM.UI.Form.Setting
{
    public partial class SystemParamSetForm : BaseForm, IEntityEditor
    {
        private static string GETMODELTYPE="0";

        public SystemParamSetForm()
        {
            InitializeComponent();
            spsmAll.LoadData(GETMODELTYPE);
        }//0 全局参数;1组织架构参数,2人事管理参数,3考勤管理参数,4薪资管理参数,5绩效管理参数



        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SYSTEMPARAMSET");
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
                    SaveAndClose();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息A",
                Tooltip = "详细信息A"
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };


            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            items.Clear();

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        public void Save()
        {

        }
        public void SaveAndClose()
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GETMODELTYPE = tabcall.SelectedIndex.ToString();
                switch (Convert.ToInt32(GETMODELTYPE))
                {
                    case 0:
                        spsmAll.LoadData(GETMODELTYPE);
                        break;
                    case 1:
                        spsmOrg.LoadData(GETMODELTYPE);
                        break;
                    case 2:
                        spsmPersonnel.LoadData(GETMODELTYPE);
                        break;
                    case 3:
                        spsmAttendance.LoadData(GETMODELTYPE);
                        break;
                    case 4:
                        spsmSalary.LoadData(GETMODELTYPE);
                        break;
                    case 5:
                        spsmPerformance.LoadData(GETMODELTYPE);
                        break;
                }
            }
            catch { }
        }

    }
}
