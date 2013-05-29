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
    public class DeptBudgetApplyForm : FBPage
    {
        FBEntityService fbService;

        public DeptBudgetApplyForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);

        }
        

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            
            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name);
            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }
            List<string> msgs = new List<string>();

            details.ToList().ForEach(item =>
                {
                    T_FB_DEPTBUDGETAPPLYDETAIL detail = item.Entity as T_FB_DEPTBUDGETAPPLYDETAIL;

                    // 表示没做过任何操作
                    if (detail.TOTALBUDGETMONEY.Equal(0) && detail.BUDGETMONEY.Equal(0))
                    {
                        return;
                    }

                    if (detail.TOTALBUDGETMONEY < 0)
                    {
                        string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                        msgs.Add(errorMessage);
                       
                    }
                    if (detail.USABLEMONEY.LessThan(detail.TOTALBUDGETMONEY))
                    {
                        if (detail.TOTALBUDGETMONEY > 0)
                        {
                            //不能大于年度结余
                            //msgs.Add(string.Format(ErrorMessage.BudgetMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                            msgs.Add(string.Format(ErrorMessage.BudgetYearMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                        }
                    }
                    if (detail.BUDGETMONEY < 0)
                    {
                        msgs.Add(string.Format(ErrorMessage.PersonBudgetMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                    }

                });
            
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }
            
        }

        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            //beyond

         // 清除预算明细
            this.OrderEntity.GetRelationFBEntities(typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name).Clear();
            this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETAPPLYDETAIL).Name).Clear();
            // 添加预算明细
            e.Result.ToList().ForEach ( item =>
                {
                    item.FBEntityState = FBEntityState.Added;
                    T_FB_DEPTBUDGETAPPLYDETAIL deptDetail = item.Entity as T_FB_DEPTBUDGETAPPLYDETAIL;
                    deptDetail.T_FB_DEPTBUDGETAPPLYMASTER = this.OrderEntity.Entity as T_FB_DEPTBUDGETAPPLYMASTER;
                    
                    #region beyond
                    item.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETAPPLYDETAIL).Name).ToList().ForEach(fbpersondetail =>
                    {

                        T_FB_PERSONBUDGETAPPLYDETAIL persondetail = fbpersondetail.Entity as T_FB_PERSONBUDGETAPPLYDETAIL;
                        persondetail.CREATEUSERID = this.OrderEntity.LoginUser.Value.ToString();
                        persondetail.CREATEUSERNAME = this.OrderEntity.LoginUser.Text;
                        persondetail.UPDATEUSERID = this.OrderEntity.LoginUser.Value.ToString();
                        persondetail.UPDATEUSERNAME = this.OrderEntity.LoginUser.Text;
                        persondetail.T_FB_DEPTBUDGETAPPLYDETAIL = deptDetail;

                        fbpersondetail.FBEntityState = FBEntityState.Added;
                                                
                    });
                    #endregion 

                   
                    deptDetail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                    deptDetail.T_FB_SUBJECT.T_FB_BUDGETCHECK.Clear();
                });
            this.OrderEntity.FBEntity.AddFBEntities < T_FB_DEPTBUDGETAPPLYDETAIL>(e.Result);

            this.OrderEntity.FBEntity.SetObjValue("Entity.BUDGETMONEY", 0);
            SetPropertyChanged();
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

                OrderEntity.SetObjValue("Entity.ISVALID", "0");
                OrderEntity.SetObjValue("Entity.BUDGETARYMONTH", new DateTime(DataCore.SystemDateTime.Year, DataCore.SystemDateTime.Month, 1).AddMonths(1));
                
                GetOrderDetail();
            }
            else
            {
                SetPropertyChanged();
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
            if (e.Result.Contains("Entity.BUDGETARYMONTH"))
            {
                 GetOrderDetail();
            }
            else if (e.Result.Contains(EntityFieldName.OwnerDepartmentID))
            {
                GetOrderDetail();
                ChangeCreator();
                ChangeOwnerID();
            }
            
        }

        private void GetOrderDetail()
        {

            // 预算月份
            DateTime dtBudgetDate = (DateTime)this.OrderEntity.GetObjValue("Entity.BUDGETARYMONTH");
            string dataValue = dtBudgetDate.ToString("yyyy-MM-dd");
            QueryExpression qeMonth = QueryExpressionHelper.Equal("BUDGETARYMONTH", dataValue);
            qeMonth.QueryType = typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name;

            // 预算部门
            QueryExpression qeDept = QueryExpressionHelper.Equal("OWNERDEPARTMENTID", this.OrderEntity.GetObjValue("Entity.OWNERDEPARTMENTID").ToString());
            qeDept.QueryType = typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name;
            qeDept.RelatedExpression = qeMonth;
            
            fbService.QueryFBEntities(qeDept);
        }

        private void SetPropertyChanged()
        {
            // 因后台保存是二层结构，所有需要把 A->B->C这种结构变成 A->B, A->C结构
            ObservableCollection<FBEntity> detailPersonList = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETAPPLYDETAIL).Name);
            ObservableCollection<FBEntity> detailList = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name);
           
            detailList.ToList().ForEach(item =>
            {
                #region 部门变化
                // 部门预算明细变化事件
                item.Entity.PropertyChanged += (senderDept, eDept) =>
                    {
                        T_FB_DEPTBUDGETAPPLYDETAIL DeptDetail = senderDept as T_FB_DEPTBUDGETAPPLYDETAIL;

                        #region
                        // 部门预算明细的总预算变化时，统计所有明细的预算
                        if (eDept.PropertyName == "TOTALBUDGETMONEY")
                        {
                            decimal? newTotal = detailList.Sum(itemDept =>
                                {
                                    return (decimal?)itemDept.GetObjValue("Entity.TOTALBUDGETMONEY");
                                });
                             
                            this.OrderEntity.SetObjValue("Entity.BUDGETMONEY", newTotal);
                        }
                        // 改：先填写预算总额，后填个人预算，而部门的公共预算＝预算总额 - 个人预算 2011-12-28
                        // 个人预算或部门预算变化时，统计个人预算明细　+ 部门预算
                        if (eDept.PropertyName == "TOTALBUDGETMONEY" || eDept.PropertyName == "PERSONBUDGETMONEY")
                        {
                            DeptDetail.BUDGETMONEY = DeptDetail.TOTALBUDGETMONEY.Subtract(DeptDetail.PERSONBUDGETMONEY).Value;
                        }
                        #endregion
                    };
                #endregion

                #region 个人预算明细变化
                // 个人预算明细变化
                ObservableCollection<FBEntity> obPersonDetail = item.GetRelationFBEntities(typeof(T_FB_PERSONBUDGETAPPLYDETAIL).Name);
                obPersonDetail.ToList().ForEach(itemPerson =>
                {
                    T_FB_PERSONBUDGETAPPLYDETAIL persondetail = itemPerson.Entity as T_FB_PERSONBUDGETAPPLYDETAIL;
                    
                    itemPerson.Entity.PropertyChanged += (senderPerson, ePerson) =>
                        {
                            if (ePerson.PropertyName == "BUDGETMONEY")
                            {
                                
                                var totalPerson = obPersonDetail.Sum(itemPersonDetail =>
                                    {
                                        // 在个人预算中将会部门预算记录，所以需要判断是否统计的对象是个人预算
                                        T_FB_PERSONBUDGETAPPLYDETAIL pDetail = itemPersonDetail.Entity as T_FB_PERSONBUDGETAPPLYDETAIL;
                                        if (pDetail != null)
                                        {

                                            return pDetail.BUDGETMONEY;
                                        }
                                        else
                                        {
                                            return 0;
                                        }
                                    });
                                var itemDept = item.Entity as T_FB_DEPTBUDGETAPPLYDETAIL;
                                itemDept.PERSONBUDGETMONEY = totalPerson;
                            }           
                        };

                    detailPersonList.Add(itemPerson);
                });
                // 因为在个人预算中需要体现部门公共费用栏，则加入部门公共费用(T记录到个人预算集合中
                if (obPersonDetail.Count > 0)
                {
                    var sumEntity = new V_DepartmentSum(item.Entity);
                    var curItemEntity = item.Entity as T_FB_DEPTBUDGETAPPLYDETAIL;
                    var sumFBEntity = sumEntity.ToFBEntity();
                    sumFBEntity.ReadOnly = true;
                    sumEntity.LIMITBUDGETMONEY = curItemEntity.BEGINNINGBUDGETBALANCE.Subtract(obPersonDetail.Sum(itemFB =>
                        {
                             var tee = (itemFB.Entity as T_FB_PERSONBUDGETAPPLYDETAIL);
                             if (tee.LIMITBUDGETMONEY == null)
                             {
                                 return 0;
                             }
                             else
                             {
                                 return tee.LIMITBUDGETMONEY;
                             }
                        }));
                    sumFBEntity.FBEntityState = FBEntityState.Unchanged;
                    obPersonDetail.Insert(0, sumFBEntity);
                }
                else
                {
                    item.HideDetails = true;
                }

                #endregion
            });
        }
    }
   
}
