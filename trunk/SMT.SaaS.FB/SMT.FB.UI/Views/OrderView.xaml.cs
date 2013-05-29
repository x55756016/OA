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
using SMT.FB.UI.Common;
using System.Reflection;
using SMT.SaaS.Platform;
using System.ComponentModel;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI;
using SMT.FB.UI.FBCommonWS;

namespace SMT.FB.UI.Views
{
    public partial class OrderView : FBBasePage, IWebPart
    {
        IWebPart currentPart = null;
        public OrderView()
        {
            InitializeComponent();
            this.FBBasePageLoaded += new EventHandler(OrderView_FBBasePageLoaded);
            //SMT.FB.UI.Common.CommonFunction.ShowAuditForm("T_FB_BORROWAPPLYMASTER", "e5e70f20-8a08-4248-b81c-7a5329d3e8db", "Audit");
        }

        void OrderView_FBBasePageLoaded(object sender, EventArgs e)
        {

            string typeName = this.Type;

            if (typeName == "AuditOrder")
            {

                AuditOrder ao = new AuditOrder();
                ao.OrderType = typeName;
                ao.InitForm();
                this.LayoutRoot.Children.Add(ao);
                currentPart = ao as IWebPart;
            }
            else
            {
                CommonView commonView = new CommonView();
                Type type = CommonFunction.GetType(typeName, CommonFunction.TypeCategory.EntityObject);
                OrderEntity orderEntity = new OrderEntity(type);
                commonView.DefaultEntity = orderEntity;
                commonView.InitForm();
                this.LayoutRoot.Children.Add(commonView);
                currentPart = commonView as IWebPart;
            }
        }
        private string _Type = string.Empty;
        public string Type
        {
            set
            {
                _Type = value;
            }
            get
            {
                if ( string.IsNullOrEmpty(_Type))
                {
                    if (NavigationContext != null && NavigationContext.QueryString.ContainsKey("Type"))
                    {
                        _Type = NavigationContext.QueryString["Type"];
                    }
                }
                return _Type;
            }
        }

        #region IWebPart 成员
        [DefaultValue(5)]
        public int RowCount
        {
            get
            {
                return currentPart.RowCount;
            }
            set
            {
                currentPart.RowCount = value;
            }
        }

        public void ShowMaxiWebPart()
        {
            currentPart.ShowMaxiWebPart();
        }

        public void ShowMiniWebPart()
        {
            currentPart.ShowMiniWebPart();
        }

        public void RefreshData()
        {
            currentPart.RefreshData();
        }

        public event EventHandler OnMoreChanged;

        #endregion


        public override bool CheckPermisstion()
        {
            if (this.Type == typeof(AuditOrder).Name)
            {
                return true;
                
            }
            PermissionRange range = PermissionRange.Employee;
            if (FBBasePage.DictLessPermission.Keys.Contains(this.Type))
            {
                range = FBBasePage.DictLessPermission[this.Type];
            }

            int perm = PermissionHelper.GetPermissionValue(this.Type, Permissions.Browse);

            // 需要大于等公司的范围权限
            return !(perm > (int)range || perm < 0);

        }

        #region IWebPart Members


        public int RefreshTime
        {
            get;
            set;
        }

        #endregion
    }
}
