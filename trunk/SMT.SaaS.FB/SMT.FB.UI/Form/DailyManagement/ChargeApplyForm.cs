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
using System.ComponentModel;
using System.Collections.Specialized;

namespace SMT.FB.UI.Form.DailyManagement
{
    public class ChargeApplyForm : FBPage
    {

        public ChargeApplyForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            T_FB_CHARGEAPPLYMASTER entCheck = this.OrderEntity.Entity as T_FB_CHARGEAPPLYMASTER;
            if (entCheck.T_FB_BORROWAPPLYMASTER != null && entCheck.PAYTYPE != 2)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage("必须选择冲借款！");
                return;
            }

            if (entCheck.PAYTYPE == 2 && entCheck.T_FB_BORROWAPPLYMASTER == null)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage("请确认已填入借款单！");
                return;
            }

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_CHARGEAPPLYDETAIL).Name);

            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }

            if (details.Count > 5)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.OverDetailInfo);
                return;
            }

            #region 去掉无关的关联
            details.ToList().ForEach(item =>
            {
                T_FB_CHARGEAPPLYDETAIL detail = item.Entity as T_FB_CHARGEAPPLYDETAIL;
                detail.T_FB_SUBJECT.T_FB_BORROWAPPLYDETAIL.Clear();
                detail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                detail.T_FB_SUBJECT.T_FB_CHARGEAPPLYDETAIL.Clear();
            });
            #endregion



            List<string> msgs = new List<string>();

            details.ToList().ForEach(item =>
            {
                T_FB_CHARGEAPPLYDETAIL detail = item.Entity as T_FB_CHARGEAPPLYDETAIL;
                if (detail.CHARGEMONEY <= 0)
                {
                    string errorMessage = string.Format(ErrorMessage.ChargeMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                    msgs.Add(errorMessage);

                }
                //if (detail.USABLEMONEY.LessThan(detail.CHARGEMONEY))
                //{
                //    msgs.Add(string.Format(ErrorMessage.ChargeMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                //}
            });

            // 
            var items = from item in details
                        group item by new
                        {
                            item.GetEntity<T_FB_CHARGEAPPLYDETAIL>().T_FB_SUBJECT.SUBJECTNAME,
                            item.GetEntity<T_FB_CHARGEAPPLYDETAIL>().CHARGETYPE,
                            item.GetEntity<T_FB_CHARGEAPPLYDETAIL>().USABLEMONEY
                        } into g
                        where g.Sum(item => item.GetEntity<T_FB_CHARGEAPPLYDETAIL>().CHARGEMONEY) > g.Key.USABLEMONEY
                        select new { g.Key, totalCharge = g.Sum(item => item.GetEntity<T_FB_CHARGEAPPLYDETAIL>().CHARGEMONEY) };


            foreach (var item in items)
            {
                msgs.Add(string.Format(ErrorMessage.ChargeMoneyBigger, item.Key.SUBJECTNAME));
            }
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }
        }

        protected override void OnLoadDataComplete()
        {

            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                OrderEntity.SetObjValue("Entity.BUDGETARYMONTH", new DateTime(DataCore.SystemDateTime.Year, DataCore.SystemDateTime.Month, 1));
            }

        }
        protected override void OnLoadControlComplete()
        {


            this.OrderEntity.OrderPropertyChanged += new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
            this.OrderEntity.CollectionEntityChanged += new EventHandler<EntityChangedArgs>(OrderEntity_CollectionEntityChanged);
            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_CHARGEAPPLYDETAIL).Name);

            details.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(details_CollectionChanged);

            IsVisibleToolBar();
        }

        /// <summary>
        /// 检查是否显示当前表单的子列表是否显示DataGrid Toolbar
        /// </summary>
        private void IsVisibleToolBar()
        {
            DetailGrid dgrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (dgrid != null)
            {
                if (dgrid.operationType == OperationTypes.Edit || dgrid.operationType == OperationTypes.ReSubmit)
                {
                    OrderEntity orderEntity = dgrid.DataContext as OrderEntity;
                    T_FB_CHARGEAPPLYMASTER entCheck = orderEntity.Entity as T_FB_CHARGEAPPLYMASTER;
                    if (entCheck.T_FB_BORROWAPPLYMASTER != null || entCheck.T_FB_BORROWAPPLYMASTERReference.EntityKey != null)
                    {
                        dgrid.ShowToolBar = false;
                    }
                }
            }
        }

        void details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                ReSortDetails();
            }
        }
        private void ReSortDetails()
        {
            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_CHARGEAPPLYDETAIL).Name);
            for (int i = 0; i < details.Count; i++)
            {
                (details[i].Entity as T_FB_CHARGEAPPLYDETAIL).SERIALNUMBER = i + 1;
            }
        }
        void OrderEntity_CollectionEntityChanged(object sender, EntityChangedArgs e)
        {
            if (sender.GetType().Name == typeof(T_FB_CHARGEAPPLYDETAIL).Name)
            {
                OnDetailChanged(sender, e);
            }
        }


        protected bool CheckSameSubject(FBEntity entity)
        {
            var listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_CHARGEAPPLYDETAIL).Name).ToList();
            var entityDetail = listDetail.FirstOrDefault(item =>
                {
                    string str1 = item.GetObjValue("Entity.T_FB_SUBJECT.SUBJECTID").ToString();
                    string str2 = entity.GetObjValue("Entity.T_FB_SUBJECT.SUBJECTID").ToString();
                    return (str1 == str2) && (!string.IsNullOrEmpty(str1));
                });
            return entityDetail != null;
        }
        private void InitData()
        {

        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if (object.Equals(sender, this.OrderEntity.Entity))
            {
                if (e.Result.Contains(typeof(T_FB_BORROWAPPLYMASTER).Name.ToEntityString()))
                {
                    OnBorrowIDChanged<T_FB_CHARGEAPPLYDETAIL>();
                }

                #region 测试
                else if (e.Result.Contains("Entity.REMARK"))
                {
                    string remark = Convert.ToString(this.OrderEntity.GetObjValue("Entity.REMARK"));
                    if (remark == "#Show Version#")
                    {
                        this.OrderEntity.SetObjValue("Entity.REMARK", "2010.12.21.18.00");
                    }
                }
                #endregion

                else if (e.Result.Contains(EntityFieldName.OwnerID))
                {
                    ChangeCreator();
                }
                else if (e.Result.Contains("Entity.PAYTYPE"))
                {
                    int iPayType = 0;
                    int.TryParse(this.OrderEntity.GetObjValue("Entity.PAYTYPE").ToString(), out iPayType);
                    SMT.FB.UI.FBLookUp fblBorrow = this.EditForm.FindControl("BorrowApply") as SMT.FB.UI.FBLookUp;
                    DetailGrid dgrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;

                    T_FB_BORROWAPPLYMASTER borrowMaster = this.OrderEntity.GetObjValue(typeof(T_FB_BORROWAPPLYMASTER).Name.ToEntityString()) as T_FB_BORROWAPPLYMASTER;

                    if (iPayType != 2 && borrowMaster != null)
                    {
                        if (dgrid != null)
                        {
                            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_CHARGEAPPLYDETAIL).Name);
                            details.Clear();
                        }

                        borrowMaster = null;
                        this.OrderEntity.ReferencedData["Entity.T_FB_BORROWAPPLYMASTER"] = DataCore.FindRefData("BorrowOrderData", borrowMaster);
                        dgrid.ShowToolBar = true;
                    }
                }
            }

        }

        private void OnBorrowIDChanged<TEntity>() where TEntity : EntityObject
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

            int iPayType = 0;
            int.TryParse(this.OrderEntity.GetObjValue("Entity.PAYTYPE").ToString(), out iPayType);
            if (iPayType != 2)
            {
                this.OrderEntity.ReferencedData["Entity.PAYTYPE"] = DataCore.FindRefData("PayTypeData", 2);
            }

            T_FB_CHARGEAPPLYMASTER chargeMaster = this.OrderEntity.Entity as T_FB_CHARGEAPPLYMASTER;
            if (chargeMaster.OWNERID == borrowMaster.OWNERID)
            {
                if (chargeMaster.OWNERPOSTID != borrowMaster.OWNERPOSTID)
                {
                    chargeMaster.OWNERPOSTID = borrowMaster.OWNERPOSTID;
                    chargeMaster.OWNERPOSTNAME = borrowMaster.OWNERPOSTNAME;
                }

                if (chargeMaster.OWNERDEPARTMENTID != borrowMaster.OWNERDEPARTMENTID)
                {
                    chargeMaster.OWNERDEPARTMENTID = borrowMaster.OWNERDEPARTMENTID;
                    chargeMaster.OWNERDEPARTMENTNAME = borrowMaster.OWNERDEPARTMENTNAME;
                }

                if (chargeMaster.OWNERCOMPANYID != borrowMaster.OWNERCOMPANYID)
                {
                    chargeMaster.OWNERCOMPANYID = borrowMaster.OWNERCOMPANYID;
                    chargeMaster.OWNERCOMPANYNAME = borrowMaster.OWNERCOMPANYNAME;
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
            if (e.ChangedEventArgs.PropertyName == "CHARGEMONEY" || e.Action == Actions.Delete)
            {
                var totalMoney = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_CHARGEAPPLYDETAIL).Name).Sum(item =>
                    {
                        return (item.Entity as T_FB_CHARGEAPPLYDETAIL).CHARGEMONEY;
                    });
                this.OrderEntity.SetObjValue("FBEntity.Entity.TOTALMONEY", totalMoney);
            }
        }

    }
}
