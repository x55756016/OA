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
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class AddOrderMealRemark : BaseForm,IClient, IEntityEditor
    {
        

        SmtOAPersonOfficeClient OrderMealClient = new SmtOAPersonOfficeClient();
        private T_OA_ORDERMEAL tmpOrderMeal;
        public delegate void refreshGridView();
        private string tmpUserid = Common.CurrentLoginUserInfo.EmployeeID;

        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private Action action;
        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        public AddOrderMealRemark(Action act,T_OA_ORDERMEAL OrderObj)
        {
            InitializeComponent();
            action = act;
            this.tblTitle.Text = "确认" + OrderObj.ORDERMEALTITLE + "订餐";
            tmpOrderMeal = OrderObj;
        }

        private void SaveOrder()
        {
            string StrRemark = "";
            StrRemark = this.txtRemark.Text.Trim().ToString();
            if (!string.IsNullOrEmpty(StrRemark))
            {
                tmpOrderMeal.REMARK = StrRemark;
                tmpOrderMeal.ORDERMEALFLAG = "1"; //已经订餐
                tmpOrderMeal.UPDATEUSERID = tmpUserid;
                tmpOrderMeal.UPDATEDATE = System.DateTime.Now;
                OrderMealClient.OrderMealInfoUpdateCompleted += new EventHandler<OrderMealInfoUpdateCompletedEventArgs>(OrderMealClient_OrderMealInfoUpdateCompleted);
                OrderMealClient.OrderMealInfoUpdateAsync(tmpOrderMeal);
            }
            else
            {
                //MessageBox.Show("确认信息不能为空");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("确认信息不能为空"));
                return;
            }
        }

        void OrderMealClient_OrderMealInfoUpdateCompleted(object sender, OrderMealInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //MessageBox.Show("已确认订餐");
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("SUCCESSED"),Utility.GetResourceStr("已订餐"));
                    RefreshUI(saveType);
                    
                }
            }
        }


        #region IEntityEditor
        public string GetTitle()
        {

            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "ORDERMEALINFO");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "ORDERMEALINFO");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "ORDERMEALINFO");
            }
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
                    saveType = RefreshedTypes.LeftMenu;
                    SaveOrder();
                    
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    SaveOrder();                    
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
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

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

            return items;
        }

        private void Close()
        {
            RefreshUI(saveType);
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

        #region 确定、取消
        

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion



        #region IForm 成员

        public void ClosedWCFClient()
        {
            OrderMealClient.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
