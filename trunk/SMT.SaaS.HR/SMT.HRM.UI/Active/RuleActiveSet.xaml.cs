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
using SMT.SaaS.FrameworkUI.ChildWidow;
using FlowDesignerWS = SMT.Saas.Tools.FlowDesignerWS;

namespace SMT.HRM.UI.Active
{

    //   # region "系统列表定义"
    //public partial class WFBOSystem : Object
    //   {
    //       private string _Name;
    //       private string _Decription;
    //       private string _ObjectFolder;

    //       public WFBOSystem(string name, string description, string objectFolder)
    //       {
    //           this.Name = name;
    //           this.Decription = description;
    //       }

    //       public string Name
    //       {
    //           set
    //           { _Name = value; }
    //           get
    //           { return _Name; }
    //       }

    //       public string Decription
    //       {
    //           set
    //           { _Decription = value; }
    //           get
    //           { return _Decription; }
    //       }

    //       public string ObjectFolder
    //       {
    //           set
    //           { _ObjectFolder = value; }
    //           get
    //           { return _ObjectFolder; }
    //       }



    //   }
    //   # endregion规则设置



    public partial class RuleActiveSet : UserControl, IEntityEditor
    {

        string _StartStateName;
        string _EndStateName;
        string _RuleName;


        public event EventHandler<Rule_OnClickEventArgs> Rule_OnClick;

        SystemBOSet newSystemBOSet = new SystemBOSet();

        #region "业务系统和业务对象"
        //string _BOSystem;
        //string _BOObject;

        //public string BOSystem
        //{
        //    get
        //    {
        //        return _BOSystem;
        //    }
        //    set
        //    {
        //        _BOSystem = value;

        //    }
        //}
        //public string BOObject
        //{
        //    get
        //    {
        //        return _BOObject;
        //    }
        //    set
        //    {

        //        _BOObject = value;

        //    }
        //}

        public FlowDesignerWS.WFBOSystem BOSystem = new FlowDesignerWS.WFBOSystem();
        public FlowDesignerWS.WFBOObject BOObject = new FlowDesignerWS.WFBOObject();

        #endregion

        RuleConditions ruleConditions;

        FlowDesignerWS.ServiceClient WFBOService = new FlowDesignerWS.ServiceClient();

        public OptState OptState = OptState.Add;
        public string StartStateName
        {
            get
            {
                return _StartStateName;
            }
            set
            {

                _StartStateName = value;

            }
        }

        public string EndStateName
        {
            get
            {
                return _EndStateName;
            }
            set
            {

                _EndStateName = value;

            }
        }

        public string RuleName
        {
            get
            {
                return _RuleName;
            }
            set
            {

                _RuleName = value;

            }
        }
        List<StateType> StateList = new List<StateType>();
        List<SystemItem> SystemList = new List<SystemItem>();

        public List<CompareCondition> CoditionList = new List<CompareCondition>();

        public RuleActiveSet()
        {
            InitializeComponent();
            StartStateName = "";
            EndStateName = "";
            RuleName = "";
            txtCompareValue.Text = "";
            CoditionList.Clear();
            requestSystemBOAttributeList();
            LoadCoditionList();
        }


        public RuleActiveSet(RuleConditions mRuleConditions)
        {
            InitializeComponent();

            this.ruleConditions = mRuleConditions;

            this.dgCodition.ItemsSource = ruleConditions.subCondition;
            requestSystemBOAttributeList();
            LoadCoditionList();
        }

        public RuleActiveSet(RuleLine ruleLine)
        {
            StartStateName = ruleLine.StrStartActive;
            EndStateName = ruleLine.StrEndActive;
            RuleName = ruleLine.Name;
            requestSystemBOAttributeList();
            LoadCoditionList();
        }

        public void LoadCoditionList()
        {
            dgCodition.ItemsSource = CoditionList;
            if (CoditionList.Count > 0)
            {
                chkUseCondition.IsChecked = true;
                conditionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                chkUseCondition.IsChecked = false;
                conditionPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void requestSystemBOAttributeList()
        {
            if (!string.IsNullOrEmpty(BOSystem.Name) && !string.IsNullOrEmpty(BOObject.Name))
            {
                WFBOService.GetSystemBOAttributeListCompleted += new EventHandler<FlowDesignerWS.GetSystemBOAttributeListCompletedEventArgs>(LoadAttrbuteList);
                WFBOService.GetSystemBOAttributeListAsync(BOSystem, BOObject);
            }
        }

        void LoadAttrbuteList(object sender, FlowDesignerWS.GetSystemBOAttributeListCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                cboCoditionAttribute.ItemsSource = e.Result.ToList();
                cboCoditionAttribute.SelectedIndex = 0;
            }
        }

        public void SetRuleList(List<StateActive> list)
        {
            StateList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                StateType tmp = new StateType();
                tmp.StateCode = list[i].Name;
                tmp.StateName = list[i].StateName.Text;
                StateList.Add(tmp);
            }

            cboStartInfo.ItemsSource = StateList.Where(s => s.StateCode != "EndFlow").ToList();
            cboStartInfo.SelectedIndex = 0;

            cboNextInfo.ItemsSource = StateList.Where(s => s.StateCode != "StartFlow").ToList();
            cboNextInfo.SelectedIndex = 0;

            if (OptState != OptState.Add)
            {

                cboStartInfo.SelectedItem = StateList.Where(s => s.StateCode == StartStateName).ToList().First();
                cboNextInfo.SelectedItem = StateList.Where(s => s.StateCode == EndStateName).ToList().First();

            }
            //  cboInfo.SelectedItem = ActiveName;
        }



        //private void btnClose_Click(object sender, RoutedEventArgs e)
        //{
        //    this.DialogResult = false;
        //}

        //private void btnOK_Click(object sender, RoutedEventArgs e)
        //{
        //    StartStateName = ((StateType)cboStartInfo.SelectedItem).StateCode;
        //    EndStateName = ((StateType)cboNextInfo.SelectedItem).StateCode;
        //    if (chkUseCondition.IsChecked == false)
        //    {
        //        CoditionList.Clear();
        //    }
        //    if (StartStateName == EndStateName)
        //    {
        //        MessageBox.Show("起始状态和下一状态不能相同!");
        //        return;
        //    }

        //    this.DialogResult = true;
        //}

        //private void btnDele_Click(object sender, RoutedEventArgs e)
        //{

        //    this.OptState = OptState.Delete;
        //    this.DialogResult = true;
        //}


        private void btnAddCondition_Click(object sender, RoutedEventArgs e)
        {
            //setConditions.Show();
            CompareCondition newCondition = new CompareCondition();
            newCondition.Name = System.Guid.NewGuid().ToString();
            newCondition.Description = ((FlowDesignerWS.WFBOAttribute)cboCoditionAttribute.SelectedItem).Description;
            newCondition.CompAttr = ((FlowDesignerWS.WFBOAttribute)cboCoditionAttribute.SelectedItem).Name;
            newCondition.DataType = ((FlowDesignerWS.WFBOAttribute)cboCoditionAttribute.SelectedItem).DataType;
            newCondition.Operate = ((System.Windows.Controls.ContentControl)(cboOperate.SelectedItem)).Content.ToString(); // cboOperate.SelectedItem.ToString();
            newCondition.CompareValue = txtCompareValue.Text.ToString();
            CoditionList.Add(newCondition);
            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = CoditionList;
        }

        private void btnRemoveCondition_Click(object sender, RoutedEventArgs e)
        {

            string itemName = ((Button)e.OriginalSource).Tag.ToString();
            //MessageBox.Show(itemID);

            foreach (CompareCondition cpitem in CoditionList)
            {
                if (cpitem.Name == itemName)
                {
                    CoditionList.Remove(cpitem);
                    break;
                }
            }

            dgCodition.ItemsSource = null;
            dgCodition.ItemsSource = CoditionList;

        }

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {


        }

        private void chkUseCondition_Unchecked(object sender, RoutedEventArgs e)
        {
            conditionPanel.Visibility = Visibility.Collapsed;
        }

        private void chkUseCondition_Checked(object sender, RoutedEventArgs e)
        {
            conditionPanel.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(BOObject.Name) || string.IsNullOrEmpty(BOSystem.Name))
            {
                // newSystemBOSet.Closed += new EventHandler(newSystemBOSet_Closed);
                // newSystemBOSet.Show();
            }
        }

        void newSystemBOSet_Closed(object sender, EventArgs e)
        {
            //if (newSystemBOSet.DialogResult == true)
            //{
            //    this.BOSystem = (FlowDesignerWS.WFBOSystem)newSystemBOSet.cboBusinessSystem.SelectedItem;
            //    this.BOObject = (FlowDesignerWS.WFBOObject)newSystemBOSet.cboBusinessObject.SelectedItem;
            //    requestSystemBOAttributeList();
            //}
        }



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
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SAAS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("DELBUTTON"),
                ImageUrl = "/SMT.SAAS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
            };

            items.Add(item);
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        public void Save()
        {
            StartStateName = ((StateType)cboStartInfo.SelectedItem).StateCode;
            EndStateName = ((StateType)cboNextInfo.SelectedItem).StateCode;
            if (chkUseCondition.IsChecked == false)
            {
                CoditionList.Clear();
            }
            if (StartStateName == EndStateName)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("PROMPT"), Utility.GetResourceStr("M00007"), Utility.GetResourceStr("CONFIRMBUTTON"));
              //  MessageBox.Show("起始状态和下一状态不能相同!");
                return;
            }

            if(this.Rule_OnClick!=null)
                this.Rule_OnClick(this,new Rule_OnClickEventArgs(true));
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        public void Delete()
        {
            this.OptState = OptState.Delete;
            this.Rule_OnClick(this, new Rule_OnClickEventArgs(true));
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
    }

    public class Rule_OnClickEventArgs : EventArgs
    {
        public Rule_OnClickEventArgs(object Content)
        {
            this.Content_Rule = Content;
        }
        public object Content_Rule { get; set; }
    }
}

