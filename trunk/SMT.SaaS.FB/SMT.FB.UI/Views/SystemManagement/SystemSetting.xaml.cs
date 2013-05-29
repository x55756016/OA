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
using System.Windows.Navigation;
using SMT.FB.UI.FBCommonWS;

using SMT.FB.UI.Common;
using SMT.SaaS.FrameworkUI;

namespace SMT.FB.UI.Views.SystemManagement
{
    public partial class SystemSetting : FBBasePage
    {
        public SystemSetting()
        {
            InitializeComponent();
            this.FBBasePageLoaded += new EventHandler(SystemSetting_FBBasePageLoaded);
        }

        void SystemSetting_FBBasePageLoaded(object sender, EventArgs e)
        {
            InitToolBar();
            FBEntityService fbService = new FBEntityService();
            QueryExpression qe = new QueryExpression();
            qe.QueryType = typeof(T_FB_SYSTEMSETTINGS).Name;
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            fbService.QueryFBEntities(qe);
        }

        private void InitToolBar()
        {
            List<ToolbarItem> list = new List<ToolbarItem>();
            list.Add(ToolBarItems.Save);

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "CheckBudget",
                Title = "预算结算",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
            };
            list.Add(item);
            tooBarTop.InitToolBarItem(list);
            tooBarTop.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(tooBarTop_ItemClicked);

        }

        private void tooBarTop_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            
            switch (e.Key)
            {
                case "Save" :
                    this.editForm.Save();
                    break;
                case "CheckBudget" :
                    CheckBudget();
                    break;

            }
            
        }

        public void CheckBudget()
        {
            SMT.Saas.Tools.FBServiceWS.FBServiceClient service = new SMT.Saas.Tools.FBServiceWS.FBServiceClient();
            service.CloseBudgetAsync();
            service.CloseBudgetCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(service_CloseBudgetCompleted);
        }

        void service_CloseBudgetCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            CommonFunction.ShowMessage("提示", "结算完成", CommonFunction.MessageType.Attention);
        }
        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            OrderEntity orderEntity = null;
            FBEntity entity = e.Result.FirstOrDefault();
            if (entity == null)
            {
                orderEntity = new OrderEntity(typeof(T_FB_SYSTEMSETTINGS));
            }
            else
            {
                orderEntity = new OrderEntity(entity);
            }
            
            this.editForm.OrderEntity = orderEntity;
            this.editForm.InitForm();
        }

    }
}
