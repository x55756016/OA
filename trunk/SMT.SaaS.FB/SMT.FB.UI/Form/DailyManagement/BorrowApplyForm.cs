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

namespace SMT.FB.UI.Form.DailyManagement
{
    public class BorrowApplyForm : FBPage
    {
        public BorrowApplyForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
     
        }

        void EditForm_Saving(object sender, SavingEventArgs e)
        {

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_BORROWAPPLYDETAIL).Name);

            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }
            #region 去掉无关的关联
            details.ToList().ForEach(item =>
            {
                T_FB_BORROWAPPLYDETAIL detail = item.Entity as T_FB_BORROWAPPLYDETAIL;
                detail.T_FB_SUBJECT.T_FB_BORROWAPPLYDETAIL.Clear();
                detail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });
            #endregion

            List<string> msgs = new List<string>();

            details.ToList().ForEach(item =>
            {
                T_FB_BORROWAPPLYDETAIL detail = item.Entity as T_FB_BORROWAPPLYDETAIL;
                if (detail.BORROWMONEY < 0)
                {
                    string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                    msgs.Add(errorMessage);

                }
                if (detail.USABLEMONEY.LessThan(detail.BORROWMONEY))
                {
                    msgs.Add(string.Format(ErrorMessage.BorrowMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                }
            });
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }

        }

        protected bool CheckSameSubject(FBEntity entity)
        {
            var listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_BORROWAPPLYDETAIL).Name).ToList();
            var entityDetail = listDetail.FirstOrDefault(item =>
            {
                string str1 = item.GetObjValue("Entity.T_FB_SUBJECT.SUBJECTID").ToString();
                string str2 = entity.GetObjValue("Entity.T_FB_SUBJECT.SUBJECTID").ToString();
                return (str1 == str2) && (!string.IsNullOrEmpty(str1));
            });
            return entityDetail != null;
        }

        protected override void OnLoadControlComplete()
        {
            DetailGrid grid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (grid != null)
            {
                grid.CheckSame = CheckSameSubject;
            }

            this.OrderEntity.ReferencedData["Entity.REPAYTYPE"] = DataCore.FindRefData("BorrowTypeData", 1);
        }
        protected override void OnLoadDataComplete()
        {
            base.OnLoadDataComplete();
            this.OrderEntity.CollectionEntityChanged += new EventHandler<EntityChangedArgs>(OrderEntity_CollectionEntityChanged);
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                OrderEntity.SetObjValue("Entity.ISREPAIED", "0");
                OrderEntity.SetObjValue("Entity.BUDGETARYMONTH", new DateTime(DataCore.SystemDateTime.Year, DataCore.SystemDateTime.Month, 1));
                //Added by 安凯航 2011年5月24日 
                //类型默认为普通借款
                OrderEntity.ReferencedData["Entity.REPAYTYPE"] = DataCore.FindReferencedData<BorrowTypeData>(1);
            }

            this.OrderEntity.OrderPropertyChanged +=new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
            
        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if (object.Equals(sender, this.OrderEntity.Entity))
            {
                if (e.Result.Contains(EntityFieldName.OwnerID))
                {
                    ChangeCreator();
                }
            }
            
        }

        void OrderEntity_CollectionEntityChanged(object sender, EntityChangedArgs e)
        {
            if (sender.GetType().Name == typeof(T_FB_BORROWAPPLYDETAIL).Name)
            {
                if (e.ChangedEventArgs.PropertyName =="BORROWMONEY" || e.Action == Actions.Delete)
                {
                    // 未还款数 ＝ 借款数
                    sender.SetObjValue("UNREPAYMONEY", sender.GetObjValue("BORROWMONEY"));

                    // 汇总借款总数
                    var totalMoney = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_BORROWAPPLYDETAIL).Name).Sum(item =>
                    {
                        return (item.Entity as T_FB_BORROWAPPLYDETAIL).BORROWMONEY;
                    });
                    this.OrderEntity.SetObjValue("FBEntity.Entity.TOTALMONEY", totalMoney);
                }
            }
        }
    }
}
