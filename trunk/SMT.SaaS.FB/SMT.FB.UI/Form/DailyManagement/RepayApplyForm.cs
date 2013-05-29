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
using System.Collections.Generic;
using SMT.FB.UI.FBCommonWS;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Markup;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using SMT.SaaS.FrameworkUI.AuditControl;

namespace SMT.FB.UI.Form.DailyManagement
{
    public class RepayApplyForm : FBPage
    {
        public List<FBEntity> listDetail;

        public RepayApplyForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            ObservableCollection<FBEntity> detailsSave = e.SaveFBEntity.GetRelationFBEntities(typeof(T_FB_REPAYAPPLYDETAIL).Name);

            
            #region 去掉无关的关联
            detailsSave.ToList().ForEach(item =>
            {
                T_FB_REPAYAPPLYDETAIL detail = item.Entity as T_FB_REPAYAPPLYDETAIL;
                detail.T_FB_SUBJECT.T_FB_BORROWAPPLYDETAIL.Clear();
                detail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                detail.T_FB_SUBJECT.T_FB_REPAYAPPLYDETAIL.Clear();
            });
            #endregion
            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_REPAYAPPLYDETAIL).Name);
            List<string> msgs = new List<string>();
            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }
            details.ToList().ForEach(item =>
            {
                T_FB_REPAYAPPLYDETAIL detail = item.Entity as T_FB_REPAYAPPLYDETAIL;
                if (detail.REPAYMONEY < 0)
                {
                    string errorMessage = string.Format(ErrorMessage.RepayMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                    msgs.Add(errorMessage);

                }
                if (detail.T_FB_BORROWAPPLYDETAIL.UNREPAYMONEY.LessThan(detail.REPAYMONEY))
                {
                    msgs.Add(string.Format(ErrorMessage.RepayMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                }
            });
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }

        }
        protected override void OnLoadDataComplete()
        {
            listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_REPAYAPPLYDETAIL).Name).ToList();
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                OrderEntity.SetObjValue("Entity.BUDGETARYMONTH", new DateTime(DataCore.SystemDateTime.Year, DataCore.SystemDateTime.Month, 1));
            }
        }
        protected override void OnLoadControlComplete()
        {
            DetailGrid grid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (grid != null)
            {
                grid.CheckSame = CheckSameSubject;
            }
            this.OrderEntity.OrderPropertyChanged += new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
            this.OrderEntity.CollectionEntityChanged += new EventHandler<EntityChangedArgs>(OrderEntity_CollectionEntityChanged);
        }


        protected bool CheckSameSubject(FBEntity entity)
        {
            var entityDetail = listDetail.FirstOrDefault(item =>
            {
                string str1 = item.GetObjValue("Entity.T_FB_SUBJECT.SUBJECTID").ToString();
                string str2 = entity.GetObjValue("Entity.T_FB_SUBJECT.SUBJECTID").ToString();
                return (str1 == str2) && (!string.IsNullOrEmpty(str1));
            });
            return entityDetail != null;
        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if (object.Equals(sender, this.OrderEntity.Entity))
            {
                if (e.Result.Contains(typeof(T_FB_BORROWAPPLYMASTER).Name.ToEntityString()))
                {
                    OnBorrowIDChanged<T_FB_REPAYAPPLYDETAIL>();
                }
                else if (e.Result.Contains(EntityFieldName.OwnerID))
                {
                    ChangeCreator();
                }
            }
        }

        void OrderEntity_CollectionEntityChanged(object sender, EntityChangedArgs e)
        {
            if (sender.GetType().Name == typeof(T_FB_REPAYAPPLYDETAIL).Name)
            {
                OnDetailChanged(sender, e);
            }
        }

        private void OnBorrowIDChanged<TEntity>()
        {

            DetailGrid dgrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (dgrid != null)
            {
                dgrid.ShowToolBar = false;
            }

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(TEntity).Name);
            details.Clear();

            T_FB_BORROWAPPLYMASTER borrowMaster = this.OrderEntity.GetObjValue(typeof(T_FB_BORROWAPPLYMASTER).Name.ToEntityString()) as T_FB_BORROWAPPLYMASTER;
            if (borrowMaster == null)
            {
                return;
            }

            T_FB_REPAYAPPLYMASTER repayMaster = this.OrderEntity.Entity as T_FB_REPAYAPPLYMASTER;
            if (repayMaster.OWNERID == borrowMaster.OWNERID)
            {
                if (repayMaster.OWNERPOSTID != borrowMaster.OWNERPOSTID)
                {
                    repayMaster.OWNERPOSTID = borrowMaster.OWNERPOSTID;
                    repayMaster.OWNERPOSTNAME = borrowMaster.OWNERPOSTNAME;
                }

                if (repayMaster.OWNERDEPARTMENTID != borrowMaster.OWNERDEPARTMENTID)
                {
                    repayMaster.OWNERDEPARTMENTID = borrowMaster.OWNERDEPARTMENTID;
                    repayMaster.OWNERDEPARTMENTNAME = borrowMaster.OWNERDEPARTMENTNAME;
                }

                if (repayMaster.OWNERCOMPANYID != borrowMaster.OWNERCOMPANYID)
                {
                    repayMaster.OWNERCOMPANYID = borrowMaster.OWNERCOMPANYID;
                    repayMaster.OWNERCOMPANYNAME = borrowMaster.OWNERCOMPANYNAME;
                }
            }

            QueryExpression qeBorrow = QueryExpressionHelper.Equal("T_FB_BORROWAPPLYMASTER.BORROWAPPLYMASTERID", borrowMaster.BORROWAPPLYMASTERID);
            QueryExpression qeOwner = this.OrderEntity.GetQueryExpression(FieldName.OwnerID);
            QueryExpression qeOwnerPost = this.OrderEntity.GetQueryExpression(FieldName.OwnerPostID);

            qeBorrow.QueryType = typeof(TEntity).Name;
            qeBorrow.RelatedExpression = qeOwner;
            qeOwner.RelatedExpression = qeOwnerPost;

            FBEntityService service = new FBEntityService();
            service.QueryFBEntitiesCompleted += (o, ea) =>
            {
                ea.Result.ToList().ForEach(item =>
                {
                    details.Add(item);
                    item.FBEntityState = FBEntityState.Added;
                });
            };
            service.QueryFBEntities(qeBorrow);
        }

        private void OnDetailChanged(object sender, EntityChangedArgs e)
        {
            if (e.ChangedEventArgs.PropertyName == "REPAYMONEY" || e.Action == Actions.Delete)
            {
                var totalMoney = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_REPAYAPPLYDETAIL).Name).Sum(item =>
                {
                    return (item.Entity as T_FB_REPAYAPPLYDETAIL).REPAYMONEY;
                });
                this.OrderEntity.SetObjValue("FBEntity.Entity.TOTALMONEY", totalMoney);
            }
        }
        protected override void OnAuditing(object sender, SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (e.Result == AuditEventArgs.AuditResult.Auditing)
            {
               
            }
        }
    }
}
