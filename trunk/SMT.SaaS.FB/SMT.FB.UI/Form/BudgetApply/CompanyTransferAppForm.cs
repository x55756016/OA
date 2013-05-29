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
    public class CompanyTransferAppForm : FBPage
    {
        const string FIELDNAME_TRANSFERFROM = "TRANSFERFROM";
        const string FIELDNAME_TRANSFERTO = "TRANSFERTO";
        const string Msg_SaveTransfer = "调出单位不能与调入单位一样!";
        
        
         FBEntityService fbService;

         public CompanyTransferAppForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);

        }

        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            // 清除预算明细
            this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYTRANSFERDETAIL).Name).Clear();

            // 添加预算明细
            e.Result.ToList().ForEach ( item =>
                {
                    (item.Entity as T_FB_COMPANYTRANSFERDETAIL).T_FB_COMPANYTRANSFERMASTER = this.OrderEntity.Entity as T_FB_COMPANYTRANSFERMASTER;
                    item.FBEntityState = FBEntityState.Added;
                });
            this.OrderEntity.FBEntity.AddFBEntities<T_FB_COMPANYTRANSFERDETAIL>(e.Result);
        }


        private void InitData()
        {
            //string companyID = Convert.ToString(OrderEntity.GetValue("Entity.OWNERCOMPANYID"));
            //curCompany = DataCore.CompanyList.FirstOrDefault(company => { return company.ID == companyID; });
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                EmployeerData create = this.OrderEntity.GetCreateInfo();
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerID, create.Value);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerName, create.Text);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerPostID, create.Post.Value);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerPostName, create.Post.Text);

                OrderEntity.SetObjValue("Entity.BUDGETYEAR", DataCore.SystemDateTime.Year);
                
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerDepartmentID, this.OrderEntity.GetObjValue(FIELDNAME_TRANSFERFROM.ToEntityString()));
                GetOrderDetail();
            }
            else
            {
                
            }
        }

        protected override void OnLoadDataComplete()
        {
            InitData();
        }

        protected override void OnLoadControlComplete()
        {
            this.OrderEntity.OrderPropertyChanged += new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
            
        }

        
        void EditForm_Saving(object sender, SavingEventArgs e)
        {

            List<string> msgs = new List<string>();

            // 调入，调出单位是否相同
            string strFrom = this.OrderEntity.GetObjValue("Entity." + FIELDNAME_TRANSFERFROM).ToString();
            string strTo = this.OrderEntity.GetObjValue("Entity." + FIELDNAME_TRANSFERTO).ToString();
            if (strFrom == strTo)
            {                
                msgs.Add(Msg_SaveTransfer);
            }

            // 调出金额不可大于可用金额

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYTRANSFERDETAIL).Name);

            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }

            details.ToList().ForEach(item =>
            {
                T_FB_COMPANYTRANSFERDETAIL detail = item.Entity as T_FB_COMPANYTRANSFERDETAIL;

                if (detail.TRANSFERMONEY < 0)
                {
                    string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                    msgs.Add(errorMessage);
                }

                if (detail.USABLEMONEY.LessThan(detail.TRANSFERMONEY))
                {
                    if (detail.TRANSFERMONEY > 0)
                    {
                        msgs.Add(string.Format(ErrorMessage.TransMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                    }
                }

            });
            if (msgs.Count > 0)
            {
                CommonFunction.ShowErrorMessage(msgs);
                e.Action = Actions.Cancel;
            }
        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if (e.Result.Contains(FIELDNAME_TRANSFERFROM.ToEntityString()))
            {
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerDepartmentID, this.OrderEntity.GetObjValue(FIELDNAME_TRANSFERFROM.ToEntityString()));
                GetOrderDetail();
            }
        }

        private void GetOrderDetail()
        {

            // 预算年份
            
            QueryExpression qeYear = this.OrderEntity.GetQueryExpression("BUDGETYEAR");
            qeYear.QueryType = typeof(T_FB_COMPANYTRANSFERDETAIL).Name;

            // 预算部门
            QueryExpression qeDepartment = this.OrderEntity.GetQueryExpression(FIELDNAME_TRANSFERFROM);
            qeDepartment.PropertyName = FieldName.OwnerDepartmentID;
            qeDepartment.RelatedExpression = qeYear;

            // 公司
            QueryExpression qeCompany = this.OrderEntity.GetQueryExpression(FieldName.OwnerCompanyID);
            qeCompany.QueryType = typeof(T_FB_COMPANYTRANSFERDETAIL).Name;
            qeCompany.RelatedExpression = qeDepartment;
            
            fbService.QueryFBEntities(qeCompany);
        }
    }
}
