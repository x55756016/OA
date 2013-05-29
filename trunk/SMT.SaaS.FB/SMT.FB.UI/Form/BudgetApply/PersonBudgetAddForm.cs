using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.Common;
using SMT.FB.UI.Common.Controls;
using SMT.FB.UI.FBCommonWS;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace SMT.FB.UI.Form.BudgetApply
{
    public class PersonBudgetAddForm : FBPage
    {

        FBEntityService fbService;
        public PersonBudgetAddForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            // 清除预算明细
            this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETADDDETAIL).Name).Clear();

            // 添加预算明细
            e.Result.ToList().ForEach(item =>
            {
                //(item.Entity as T_FB_PERSONBUDGETADDDETAIL).T_FB_PERSONBUDGETADDMASTER = this.OrderEntity.Entity as T_FB_PERSONBUDGETADDMASTER;
                //item.FBEntityState = EntityState.Added;
            });
            this.OrderEntity.FBEntity.AddFBEntities<T_FB_PERSONBUDGETADDDETAIL>(e.Result);
        }
       
        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETADDDETAIL).Name);
            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }
            List<string> msgs = new List<string>();

            details.ToList().ForEach(item =>
            {
                T_FB_PERSONBUDGETADDDETAIL detail = item.Entity as T_FB_PERSONBUDGETADDDETAIL;
                if (detail.BUDGETMONEY < 0)
                {
                    string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                    msgs.Add(errorMessage);

                }
                if (detail.USABLEMONEY.LessThan(detail.BUDGETMONEY))
                {
                    msgs.Add(string.Format(ErrorMessage.BudgetMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                }

            });
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }

        }

        private void InitData()
        {
            GetOrderDetail();
        }
        protected override void OnLoadDataComplete()
        {
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                InitData();
            }
        }

        protected override void OnLoadControlComplete()
        {
            this.OrderEntity.OrderPropertyChanged += new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if (e.Result.Contains(EntityFieldName.OwnerID))
            {
                GetOrderDetail();
                ChangeCreator();
    
            }
        }

        private void GetOrderDetail()
        {
            QueryExpression qePost = this.OrderEntity.GetQueryExpression(FieldName.OwnerPostID);
            QueryExpression qeDepartment = this.OrderEntity.GetQueryExpression(FieldName.OwnerDepartmentID);
            QueryExpression qeCompany = this.OrderEntity.GetQueryExpression(FieldName.OwnerCompanyID);
            QueryExpression qeOwner = this.OrderEntity.GetQueryExpression(FieldName.OwnerID);

            qeDepartment.RelatedExpression = qeCompany;
            qeCompany.RelatedExpression = qeOwner;

            qePost.QueryType = typeof(T_FB_PERSONBUDGETADDDETAIL).Name;
            qePost.RelatedExpression = qeDepartment;
            fbService.QueryFBEntities(qePost);
        }
    }
}
