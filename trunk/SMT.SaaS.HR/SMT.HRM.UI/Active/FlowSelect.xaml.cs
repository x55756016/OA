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
using FlowDesignerWS=SMT.Saas.Tools.FlowDesignerWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.Saas.Tools.FlowDesignerWS;

namespace SMT.HRM.UI.Active
{
    public partial class FlowSelect : UserControl, IEntityEditor
    {
        ServiceClient FlowDesignerService = new FlowDesignerWS.ServiceClient();
        public FLOW_FLOWDEFINE_T FlowDefine = null;
        CheckBox oldcheckBox =null;

        public event EventHandler<Flow_OnClickEventArgs> Flow_OnClick;
        
        public FlowSelect()
        {
            
            InitializeComponent();
            FlowDesignerService.GetFlowDefineCompleted += new EventHandler<FlowDesignerWS.GetFlowDefineCompletedEventArgs>(GetFlowDefineCompleted);

           // GetData();
        }

        void GetFlowDefineCompleted(object sender, FlowDesignerWS.GetFlowDefineCompletedEventArgs e)
        {
            if (e.Result == null)
                MessageBox.Show("没有数据或取出数据失败!");
            else
              {
                  DgView.ItemsSource = e.Result;
                  dataPager.PageCount = e.pageCount;
              }

        }
        public void GetData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            FlowDesignerService.GetFlowDefineAsync(dataPager.PageIndex, dataPager.PageSize, "", filter, paras, pageCount);
        }

        private void AllowCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            FLOW_FLOWDEFINE_T tmpFlowDefine = checkBox.DataContext as FLOW_FLOWDEFINE_T;


            if (FlowDefine != null && FlowDefine != tmpFlowDefine)
            {
                oldcheckBox.IsChecked = false;
                //System.Windows.FrameworkElement xx = DgView.Columns[0].GetCellContent(FlowDefine);
                //CheckBox cb2 = xx.FindName("AllowCheckbox") as CheckBox;
                //cb2.IsChecked = false;
            }

            oldcheckBox = checkBox;
            FlowDefine = tmpFlowDefine;

            //int i = 0;
            //foreach (object  obj in DgView.ItemsSource)
            //{
            //    i++;

            //    System.Windows.FrameworkElement xx = DgView.Columns[0].GetCellContent(obj);
            //    if (xx == null)
            //        return;
            //    CheckBox cb2 = xx.FindName("AllowCheckbox") as CheckBox;
            //   // CheckBox cb2 = DgView.Columns[0].GetCellContent(obj).FindName("AllowCheckbox") as CheckBox;

            //    if (checkBox != cb2)
            //    cb2.IsChecked = false;

            //    xx = null;
            //    cb2 = null;

            //}  
        }

        private void AllowCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            FlowDefine = null;
        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "打开流程定义";
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
                ImageUrl = "/SMT.SAAS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SAAS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        public void Save()
        {
            if (this.Flow_OnClick != null)
                this.Flow_OnClick(this,new Flow_OnClickEventArgs(true));

            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            RefreshUI(RefreshedTypes.Close);      
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
    }

     public class Flow_OnClickEventArgs : EventArgs
    {
        public Flow_OnClickEventArgs(object Content)
        {
            this.Content_Flow = Content;
        }
        public object Content_Flow { get; set; }
    }
}

