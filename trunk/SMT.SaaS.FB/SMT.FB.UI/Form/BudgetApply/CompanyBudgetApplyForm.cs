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
using SMT.SaaS.FrameworkUI.AuditControl;

namespace SMT.FB.UI.Form.BudgetApply
{
    /// <summary>
    /// 新版年度预算 beyond
    /// </summary>
    public class CompanyBudgetApplyForm : FBPage
    {
        FBEntityService fbService;

        public CompanyBudgetApplyForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);

        }
        

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            
            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name);

            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }

            List<string> msgs = new List<string>();

            details.ToList().ForEach(item =>
                {
                    T_FB_COMPANYBUDGETAPPLYDETAIL detail = item.Entity as T_FB_COMPANYBUDGETAPPLYDETAIL;
                    if (detail.BUDGETMONEY < 0)
                    {
                        string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                        msgs.Add(errorMessage);
                       
                    }
                    //if (detail.USABLEMONEY.LessThan(detail.BUDGETMONEY))
                    //{
                    //    msgs.Add(string.Format(ErrorMessage.BudgetMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                    //}
                    
                });
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }
            
        }

        Dictionary<string, decimal?> dictComDetail = new Dictionary<string, decimal?>();
        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
           

            // 清除预算明细
            this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name).Clear();

            dictComDetail.Clear();
            // 添加预算明细
            e.Result.ToList().ForEach ( item =>
                {
                    T_FB_COMPANYBUDGETAPPLYDETAIL ComDetail = (item.Entity as T_FB_COMPANYBUDGETAPPLYDETAIL);
                    ComDetail.T_FB_COMPANYBUDGETAPPLYMASTER = this.OrderEntity.Entity as T_FB_COMPANYBUDGETAPPLYMASTER;
                    ComDetail.CREATEDATE = DateTime.Now;
                    ComDetail.CREATEUSERID = this.OrderEntity.LoginUser.Value.ToString();
                    ComDetail.UPDATEDATE = DateTime.Now;
                    ComDetail.UPDATEUSERID = this.OrderEntity.LoginUser.Value.ToString(); ;
                    item.FBEntityState = FBEntityState.Added;
                    item.Entity.PropertyChanged +=new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                    dictComDetail.Add(ComDetail.COMPANYBUDGETAPPLYDETAILID, ComDetail.BUDGETMONEY);
                    
                });
            this.OrderEntity.FBEntity.AddFBEntities < T_FB_COMPANYBUDGETAPPLYDETAIL>(e.Result);

            this.OrderEntity.FBEntity.Entity.SetObjValue("BUDGETMONEY", 0);
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

                OrderEntity.SetObjValue("Entity.ISVALID", "0");

                // OrderEntity.SetObjValue("Entity.BUDGETYEAR", DataCore.SystemDateTime.Year);
                OrderEntity.ReferencedData["Entity.BUDGETYEAR"] = DataCore.FindReferencedData<YearItem>(DataCore.SystemDateTime.Year);
                GetOrderDetail();
            }
            else
            {

                dictComDetail.Clear();
                ObservableCollection<FBEntity> detailList = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name);
                detailList.ToList().ForEach(item =>
                {
                    item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                    T_FB_COMPANYBUDGETAPPLYDETAIL ComDetail = (item.Entity as T_FB_COMPANYBUDGETAPPLYDETAIL);
                    dictComDetail.Add(ComDetail.COMPANYBUDGETAPPLYDETAILID, ComDetail.BUDGETMONEY);
                    
                });
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

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if ( e.Result.Contains(EntityFieldName.OwnerDepartmentID))
            {
                 GetOrderDetail();
                 ChangeCreator();
                 ChangeOwnerID();
            }
        }

        void DeatilEntity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BUDGETMONEY")
            {
                T_FB_COMPANYBUDGETAPPLYDETAIL ComDetail = sender as T_FB_COMPANYBUDGETAPPLYDETAIL;
                decimal? budgetMoney = sender.GetObjValue(e.PropertyName) as decimal?;
                decimal? oldbudgetMoney = 0;
                if (dictComDetail.ContainsKey(ComDetail.COMPANYBUDGETAPPLYDETAILID))
                {
                    oldbudgetMoney = dictComDetail[ComDetail.COMPANYBUDGETAPPLYDETAILID];
                }
               
                decimal? SumbudgetMoney = this.OrderEntity.FBEntity.Entity.GetObjValue(e.PropertyName) as decimal?;
                if (SumbudgetMoney==null)
                {
                    SumbudgetMoney = 0;
                }
                SumbudgetMoney = SumbudgetMoney.Add(oldbudgetMoney * -1);
                this.OrderEntity.FBEntity.Entity.SetObjValue("BUDGETMONEY", SumbudgetMoney.Add(budgetMoney));
                
                dictComDetail[ComDetail.COMPANYBUDGETAPPLYDETAILID] = budgetMoney;
            }
        }
        
        private void GetOrderDetail()
        {

            T_FB_COMPANYBUDGETAPPLYMASTER master = this.OrderEntity.Entity as T_FB_COMPANYBUDGETAPPLYMASTER;

            QueryExpression qeYear = QueryExpressionHelper.Equal("BUDGETYEAR", master.BUDGETYEAR.Add(-1).ToString());
            qeYear.QueryType = typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name;           

            // 预算部门
            QueryExpression qeDept = QueryExpressionHelper.Equal("OWNERDEPARTMENTID", this.OrderEntity.GetObjValue("Entity.OWNERDEPARTMENTID").ToString());
            qeDept.QueryType = typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name;
            qeDept.RelatedExpression = qeYear;

            fbService.QueryFBEntities(qeDept);
        }

        protected override void OnAuditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (e.Result == AuditEventArgs.AuditResult.Auditing)
            {
                var DeptDetails = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_DEPTBUDGETADDDETAIL).Name);
                DeptDetails.ToList().ForEach(deptDetail =>
                {
                    var pDetails = deptDetail.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETAPPLYDETAIL).Name);
                    pDetails.ToList().ForEach(item =>
                    {
                        item.SetObjValue(typeof(T_FB_COMPANYBUDGETAPPLYMASTER).Name.ToEntityString(), this.OrderEntity.Entity);
                        item.FBEntityState = FBEntityState.Modified;
                    });
                });
            }
        }

    }
}
