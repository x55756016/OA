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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.Common;
using System.Text;
using SMT.SaaS.FrameworkUI;

namespace SMT.FB.UI.Views.Query
{
    public class MultiValuesItem<T> where T : class
    {
        public List<T> Values { get; set; }
        public string Text { get; set; }
    }
    public partial class QueryBudgetAccount : FBBasePage
    {
        FBEntityService fbEntityService;
        public QueryBudgetAccount()
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
                return item.Name == "QueryBudgetAccount";
            });

            this.ADtGrid.GridItems = si.Items;
            this.ADtGrid.InitControl(OperationTypes.Browse);

            InitQueryFilter();
        }

        private void InitQueryFilter()
        {


            //Button btnFind = this.expander.FindChildControl<Button>("btnFind");
            btnFind.IsEnabled = true;

            // 查询对象
            // LookUp lkObject = this.expander.FindChildControl<LookUp>("lkObject");
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
            catch
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
            qe.RightType = "QueryBudgetAccount";


            // 开始日期
            QueryExpression qeStartYear = new QueryExpression
            {
                PropertyName = "BUDGETYEAR",
                PropertyValue = DateTime.Now.Year.ToString(),
                Operation = QueryExpression.Operations.GreaterThanOrEqual
            };

            QueryExpression qeStartMonth = new QueryExpression
            {
                PropertyName = "BUDGETMONTH",
                PropertyValue = DateTime.Now.Month.ToString(),
                Operation = QueryExpression.Operations.GreaterThanOrEqual
            };

            qe.RelatedExpression = qeStartYear;
            qeStartYear.RelatedExpression = qeStartMonth;
            qe = qeStartMonth;

            // 结束日期
            QueryExpression qeEndYear = new QueryExpression
            {
                PropertyName = "BUDGETYEAR",
                PropertyValue = DateTime.Now.Year.ToString(),
                Operation = QueryExpression.Operations.LessThanOrEqual
            };

            QueryExpression qeEndMonth = new QueryExpression
            {
                PropertyName = "BUDGETMONTH",
                PropertyValue = DateTime.Now.Month.ToString(),
                Operation = QueryExpression.Operations.LessThanOrEqual
            };
            qe.RelatedExpression = qeEndYear;
            qeEndYear.RelatedExpression = qeEndMonth;
            qe = qeEndMonth;

            // 结束日期
            QueryExpression qeAccountType = new QueryExpression
            {
                PropertyName = "ACCOUNTOBJECTTYPE",
                PropertyValue = "1",
                Operation = QueryExpression.Operations.NotEqual
            };
            qe.RelatedExpression = qeEndMonth;
            qeEndYear.RelatedExpression = qeAccountType;
            qe = qeAccountType;
            
            QueryExpression qeTemp = null;
            // 选择范围
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
                        qeItem.RelatedExpression = qeTemp;
                        qeTemp = qeItem;
                    });
            }

            if (qeTemp != null)
            {
                qe.RelatedExpression = qeTemp;
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
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lkObject.DataContext = null;
        }

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


    }


}
