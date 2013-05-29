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
using System.Windows.Data;
using SMT.FB.UI.Form;

using SMT.FB.UI.FBCommonWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.FB.UI.Common;
using SMT.SaaS.Platform;

namespace SMT.FB.UI.Views
{
    public partial class AuditOrder : FBBasePage, IWebPart
    {
        private ComboBox cbbOrderStatus;
        private ComboBox cbbQueryList;
        public AuditOrder()
        {
            OrderSource = new OrderEntityService();

            InitializeComponent();

        }
        public OrderEntityService OrderSource { get; set; }
        public string OrderType { get; set; }
        private IList<OrderEntity> OrderEntities { get; set; }

        public void InitForm()
        {

            OrderInfo order = OrderHelper.GetOrderInfo(OrderType);

            this.FormTitleName.TextTitle.Text = order.Name;
            this.FormTitleName.Visibility = FBBasePage.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            InitToolBar(order);
            InitData(order);
            InitControl(order);

        }

        private void InitToolBar(OrderInfo order)
        {
            tooBarTop.btnNew.Visibility = Visibility.Collapsed;
            tooBarTop.retNew.Visibility = Visibility.Collapsed;
            //修改
            tooBarTop.btnEdit.Visibility = Visibility.Collapsed;
            tooBarTop.retEdit.Visibility = Visibility.Collapsed;
            //删除
            tooBarTop.btnDelete.Visibility = Visibility.Collapsed;

            tooBarTop.btnAudit.Visibility = Visibility.Visible;
            tooBarTop.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);

            tooBarTop.btnRefresh.Click +=new RoutedEventHandler(btnRefresh_Click);
            cbbOrderStatus = tooBarTop.cbxCheckState;
            cbbQueryList = tooBarTop.cbbQueryList;
            this.tooBarTop.ShowView(true);
            this.tooBarTop.stpCheckState.Visibility = Visibility.Collapsed;
            this.tooBarTop.BtnView.Visibility = Visibility.Collapsed;
            this.tooBarTop.retRead.Visibility = Visibility.Collapsed;
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (this.ADtGrid.SelectedItems.Count == 0)
            {
                CommonFunction.NotifySelection(null);
                return;
            }

            OrderEntity orderEntity = this.ADtGrid.SelectedItem as OrderEntity;
            ShowEditForm(orderEntity, OperationTypes.Audit);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.GetOrders();
        }

        private void InitData(OrderInfo order)
        {
            OrderEntity oe = new OrderEntity();
            

            List<ITextValueItem> queryList = new List<ITextValueItem>();

            QueryData qd = new QueryData();
            qd.Text= "待我审核的单据";
            qd.Value = "AuditedOrder";
            qd.QueryExpression = QueryExpressionHelper.Equal("AuditedBy", DataCore.CurrentUser.Value.ToString());
            qd.QueryExpression.QueryType = order.Type;
            queryList.Add(qd);
            
            CommonFunction.FillComboBox(this.cbbQueryList, queryList);

            cbbQueryList.SelectedIndex = 0;

            GetOrders();
        }

        private void InitControl(OrderInfo order)
        {
            this.ADtGrid.GridItems = order.View.GridItems;
            this.ADtGrid.InitControl(OperationTypes.Browse);
            cbbQueryList.SelectionChanged += new SelectionChangedEventHandler(cbbQueryList_SelectionChanged);
            OrderSource.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(OrderSource_QueryFBEntitiesCompleted);
        }

        void OrderSource_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            List<OrderEntity> list = e.Result.ToEntityAdapterList<OrderEntity>().ToList < OrderEntity>();
           
            //this.OrderEntities = e.Result.ToEntityAdapterList<OrderEntity>();
            //this.OrderEntities = e.Result.ToEntityAdapterList<OrderEntity>();
            
            System.Linq.IOrderedEnumerable<OrderEntity> itest = from item in list
                                 orderby item.Entity.GetObjValue("UPDATEDATE") descending
                                 select item;
            this.OrderEntities = itest.ToList();
            BindingData(this.OrderEntities);
            CloseProcess();
        }


        private void BindingData(IList<OrderEntity> orders)
        {

            if (isWebPart)
            {
                var data = orders.Take<OrderEntity>(this.RowCount);
                ADtGrid.ItemsSource = data;
                return;
            }

            PagedCollectionView pcv = null;

            if (orders != null && orders.Count > 0)
            {
                var q = from ent in orders
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 25;
            }

            dataPager.DataContext = pcv;
            ADtGrid.ItemsSource = pcv;

        }

        private void GetOrders()
        {
            ShowProcess();
            QueryData dataQueryList = cbbQueryList.SelectedItem as QueryData;
            QueryExpression qeTop = new QueryExpression();
            qeTop.QueryType = this.OrderType;
            QueryExpression qe = qeTop;
            qe.RelatedType = QueryExpression.RelationType.And;

            if (dataQueryList != null)
            {
                qe.RelatedExpression = dataQueryList.QueryExpression;
                qe = qe.RelatedExpression;
            }
            if (qeTop.RelatedExpression != null)
            {
                qeTop = qeTop.RelatedExpression;
            }
            OrderSource.QueryFBEntities(qeTop);
        }

        private void cbbQueryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            GetOrders();
        }


        private void ShowEditForm(OrderEntity orderEntity,OperationTypes operationType)
        {
            //OrderForm caForm = CommonFunction.GetOrderForm(orderEntity);
            //caForm.Width = this.ActualWidth;
            //caForm.Height = this.ActualHeight;
            //caForm.Closed += (o, e) =>
            //{
            //    if (caForm.IsNeedToRefresh)
            //    {
            //        this.GetOrders();
            //    }
            //};
            //caForm.Show();
            FBPage page = FBPage.GetPage(orderEntity);
            page.EditForm.OperationType = operationType;
            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;
            EntityBrowser eb = new EntityBrowser(page);
            eb.Show<string>(DialogMode.Default, plRoot, "", (result) => { });


            page.RefreshData += (o, e) =>
            {
                this.GetOrders();
            };
            page.PageClosing += (o, e) =>
            {
                eb.Close();
            };
        }

        private bool isWebPart = false;

        #region IWebPart 成员

        
        public int RowCount
        {
            get;
            set;
        }

        public void ShowMaxiWebPart()
        {
            isWebPart = false;

            for (int i = 2; i < this.ADtGrid.Columns.Count; i++)
            {
                this.ADtGrid.Columns[i].Visibility = Visibility.Visible;
            }
            this.tooBarTop.ShowView(true);
        }

        public void ShowMiniWebPart()
        {
            isWebPart = true;

            for (int i = 2; i < this.ADtGrid.Columns.Count; i++)
            {
                this.ADtGrid.Columns[i].Visibility = Visibility.Collapsed;
            }
            this.tooBarTop.ShowView(false);
        }

        public void RefreshData()
        {
            this.GetOrders();
        }

        public event EventHandler OnMoreChanged;
        #endregion     
    
        #region IWebPart Members


        public int RefreshTime
        {
            get;
            set;
        }

        #endregion
    }
}
