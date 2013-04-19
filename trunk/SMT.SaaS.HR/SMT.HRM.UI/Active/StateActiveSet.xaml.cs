/// <summary>
/// Log No.： 1
/// Modify Desc： 角色（或节点）使用Remark（自写备注）
/// Modifier： 冉龙军
/// Modify Date： 2010-08-22
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

namespace SMT.HRM.UI.Active
{
    public partial class StateActiveSet : UserControl, IEntityEditor
    {
        public bool optFlag = true; //true:add,false:delete
        // 1s 冉龙军

        public string Remark { get; set; }    //自写备注

        // 1e
        public event EventHandler<OnClickEventArgs> StateActiveSet_Click;
        string _ActiveName;
        public string ActiveName
        {
            get
            {
                return _ActiveName;
            }
            set
            {
                _ActiveName = value;

            }
        }

       public  List<StateType> StateList = new List<StateType> 
                       {
                         new StateType{StateCode="HrState",StateName="人事经理"},
                         new StateType{StateCode="AcountState",StateName="财务经理"},
                         new StateType{StateCode="ManageState",StateName="部门经理"}
                        };
        public StateActiveSet()
        {
            
            InitializeComponent();
            ActiveName = "";
            cboInfo.ItemsSource = StateList;
            cboInfo.SelectedIndex = 0;
            this.GotFocus += new RoutedEventHandler(GotFocused);
          //  GotFocused(this,null);
          //  cboInfo.SelectedItem = ActiveName;
        }

      public   void GotFocused(object sender, RoutedEventArgs e)
        {
            if (optFlag==true )
            {
                cboInfo.IsEnabled = true;
                //btnOK.Content  = "确 定";

            }
            else
            {
                cboInfo.IsEnabled = false;
                //ActiveName = ((StateType)cboInfo.SelectedItem).StateCode;
                cboInfo.SelectedItem = StateList.Where(s => s.StateCode == ActiveName).ToList().First();
                
              //  btnOK.Content = "删 除";
            }
        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "状态设置";
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
                    UpdateAndClose();//修改
                    break;
                case "1":
                    Delete();//添加
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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

             item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("DELETE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
            };

            items.Add(item);
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

        public void UpdateAndClose()
        {
            ActiveName = ((StateType)cboInfo.SelectedItem).StateCode;
            // 1s 冉龙军
            Remark = ((StateType)cboInfo.SelectedItem).StateName;
            // 1e
            optFlag = true;
            if(this.StateActiveSet_Click!=null)
                this.StateActiveSet_Click(this,new OnClickEventArgs(true));

            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        public void Delete()
        {
            if (optFlag)
                return;
            optFlag = false;

            this.StateActiveSet_Click(this, new OnClickEventArgs(false));
            RefreshUI(RefreshedTypes.Close);
        }
    }

    public class OnClickEventArgs : EventArgs
    {
        public OnClickEventArgs(object Content)
        {
            this.Content_StateActiveSet = Content;
        }
        public object Content_StateActiveSet { get; set; }
    }
}

