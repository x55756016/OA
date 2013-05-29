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
using SMT.FB.UI.FBCommonWS;
using System.Collections;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.FB.UI.Form;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.FB.UI.Views.Query
{
    public partial class QueryFBOrder : FBBasePage
    {

        FBEntityService fbEntityService;
        public QueryFBOrder()
        {
            InitializeComponent();
            this.FBBasePageLoaded += new EventHandler(QueryBudgetAccount_FBBasePageLoaded);
            fbEntityService = new FBEntityService();
            fbEntityService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbEntityService_QueryFBEntitiesCompleted);
        }


        #region 初始化
        void QueryBudgetAccount_FBBasePageLoaded(object sender, EventArgs e)
        {
            SelectorHelper.InitSelectorInfoCompleted += new EventHandler(SelectorHelper_InitSelectorInfoCompleted);
            SelectorHelper.InitSelectorInfo();
        }

        void SelectorHelper_InitSelectorInfoCompleted(object sender, EventArgs e)
        {
            SelectorHelper.InitSelectorInfoCompleted -= new EventHandler(SelectorHelper_InitSelectorInfoCompleted);
            SelectorInfo si = SelectorHelper.Selectors.FirstOrDefault(item =>
            {
                return item.Name == "QueryOrder";
            });

            this.ADtGrid.GridItems = si.Items;
            this.ADtGrid.InitControl(OperationTypes.Browse);

            InitQueryFilter();
        }

        private void InitQueryFilter()
        {


            Button btnFind = this.expander.FindChildControl<Button>("btnFind");
            btnFind.IsEnabled = true;

            // 预算类型
            ComboBox cbbBudgetType = this.expander.FindChildControl<ComboBox>("cbbBudgetType");
            CommonFunction.FillQueryComboBox<BudgetTypeData>(cbbBudgetType);
            ITextValueItem allItem = DataCore.AllSelectdData;

            cbbBudgetType.DisplayMemberPath = "Text";
            cbbBudgetType.SelectedItem = allItem;

            // 查询对象
            LookUp lkObject = this.expander.FindChildControl<LookUp>("lkObject");
            MultiValuesItem<ExtOrgObj> item = new MultiValuesItem<ExtOrgObj>();
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ep = new Saas.Tools.PersonnelWS.T_HR_EMPLOYEE();
            ep.EMPLOYEECNAME = DataCore.CurrentUser.Text;
            ep.EMPLOYEEID = DataCore.CurrentUser.Value.ToString();

            List<ExtOrgObj> list = new List<ExtOrgObj>() { new ExtOrgObj { ObjectInstance = ep } };
            item.Values = list;
            item.Text = ep.EMPLOYEECNAME;
            lkObject.SelectItem = item;
            lkObject.DataContext = item;
            lkObject.DisplayMemberPath = "Text";
        }

        #endregion

        #region 查询数据
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcess();
                QueryEntity();
            }
            catch (Exception ex)
            {
                CloseProcess();
            }

        }

        public void QueryEntity()
        {
            QueryExpression qeBudget = new QueryExpression();
            QueryExpression qe = qeBudget;

            qe.PropertyName = "USABLEMONEY";
            qe.PropertyValue = "0";
            qe.Operation = QueryExpression.Operations.GreaterThan;
            qe.QueryType = "QueryBudgetAccount";

            ComboBox cbbBudgetType = this.expander.FindChildControl<ComboBox>("cbbBudgetType");
            ITextValueItem vitem = cbbBudgetType.SelectedItem as ITextValueItem;
            if (vitem != null && !string.IsNullOrEmpty(vitem.Value.ToString()))
            {
                QueryExpression qeType = QueryExpressionHelper.Equal("AccountObjectType", vitem.Value.ToString());
                qe.RelatedExpression = qeType;
                qe = qeType;
            }
            LookUp lkObject = this.expander.FindChildControl<LookUp>("lkObject");
            MultiValuesItem<ExtOrgObj> mutilValues = lkObject.SelectItem as MultiValuesItem<ExtOrgObj>;
            if (mutilValues != null)
            {
                Dictionary<OrgTreeItemTypes, string> dictTypes = new Dictionary<OrgTreeItemTypes, string>();
                dictTypes.Add(OrgTreeItemTypes.Company, FieldName.OwnerCompanyID);
                dictTypes.Add(OrgTreeItemTypes.Department, FieldName.OwnerDepartmentID);
                dictTypes.Add(OrgTreeItemTypes.Personnel, FieldName.OwnerID);
                dictTypes.Add(OrgTreeItemTypes.Post, FieldName.OwnerPostID);

                mutilValues.Values.ForEach(item =>
                {

                    string propertyName = dictTypes[item.ObjectType];
                    QueryExpression qeItem = QueryExpressionHelper.Equal(propertyName, item.ObjectID);
                    qeItem.RelatedType = QueryExpression.RelationType.Or;
                    qe.RelatedExpression = qeItem;
                    qe = qeItem;
                });
            }
            fbEntityService.QueryFBEntities(qeBudget);
        }

        void fbEntityService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            BindingData(e.Result);
            CloseProcess();
        }
        private void BindingData(IEnumerable orders)
        {

            PagedCollectionView pcv = new PagedCollectionView(orders);
            pcv.PageSize = 25;

            dataPager.DataContext = pcv;
            ADtGrid.ItemsSource = pcv;

        }
        #endregion

        #region 查询条件
        private void lkObject_FindClick(object sender, EventArgs e)
        {
            LookUp lookUp = sender as LookUp;
            string userID = DataCore.CurrentUser.Value.ToString();
            string perm = ((int)Permissions.Browse).ToString();
            string entity = "QueryBudgetAccount";
            OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);
            ogzLookup.MultiSelected = true;
            ogzLookup.SelectedObjType = OrgTreeItemTypes.All;


            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;

            ogzLookup.SelectedClick += (o, ev) =>
            {
                if (ogzLookup.SelectedObj.Count > 0)
                {
                    List<ExtOrgObj> list = new List<ExtOrgObj>();
                    string text = " ";

                    foreach (ExtOrgObj obj in ogzLookup.SelectedObj)
                    {
                        list.Add(obj);

                        text = text.Trim() + ";" + obj.ObjectName;

                    }
                    MultiValuesItem<ExtOrgObj> item = new MultiValuesItem<ExtOrgObj>();
                    item.Values = list;
                    item.Text = text.Substring(1);
                    lookUp.SelectItem = item;
                    lookUp.DataContext = item;
                    //  lookUp.DisplayMemberPath = "Text";
                }
            };
            ogzLookup.Show<string>(DialogMode.ApplicationModal, plRoot, "", (result) => { });
        }
        #endregion

        #region ToolBar

        private void InitToolBar()
        {
            tooBarTop.InitToolBarItem(new List<ToolbarItem>() { ToolBarItems.View });
            tooBarTop.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(tooBarTop_ItemClicked);
        }

        void tooBarTop_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            if (this.ADtGrid.SelectedItems.Count == 0)
            {
                CommonFunction.NotifySelection(null);
                return;
            }

            OrderEntity orderEntity = this.ADtGrid.SelectedItem as OrderEntity;
            ShowEditForm(orderEntity, OperationTypes.Browse);
         }

        private void ShowEditForm(OrderEntity orderEntity, OperationTypes operationType)
        {
           
            FBPage page = FBPage.GetPage(orderEntity);
            page.EditForm.OperationType = operationType;
            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;
            EntityBrowser eb = new EntityBrowser(page);
            eb.Show<string>(DialogMode.Default, plRoot, "", (result) => { });


            eb.MinWidth = 400;
            eb.MinHeight = 300;
                        
            page.PageClosing += (o, e) =>
            {
                eb.Close();
            };
        }

        #endregion
    }
}
