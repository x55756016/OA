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
    public class TravelApplyForm : FBPage
    {
        public TravelApplyForm(OrderEntity orderEntity) : base(orderEntity)
        {
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_TRAVELEXPAPPLYDETAIL).Name);

            #region 去掉无关的关联
            details.ToList().ForEach(item =>
            {
                T_FB_TRAVELEXPAPPLYDETAIL detail = item.Entity as T_FB_TRAVELEXPAPPLYDETAIL;
                detail.T_FB_SUBJECT.T_FB_BORROWAPPLYDETAIL.Clear();
                detail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                detail.T_FB_SUBJECT.T_FB_TRAVELEXPAPPLYDETAIL.Clear();
            });
            #endregion

            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }

            List<string> msgs = new List<string>();

            details.ToList().ForEach(item =>
            {
                T_FB_TRAVELEXPAPPLYDETAIL detail = item.Entity as T_FB_TRAVELEXPAPPLYDETAIL;
                if (detail.TOTALCHARGE < 0)
                {
                    string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                    msgs.Add(errorMessage);

                }
                if (detail.USABLEMONEY.LessThan(detail.TOTALCHARGE))
                {
                    msgs.Add(string.Format(ErrorMessage.ChargeMoneyBigger, detail.T_FB_SUBJECT.SUBJECTNAME));
                }
            });
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }

        }

        List<string> ps = new List<string> { "AIRFARE", "CARFARE", "LODGINGEXPENSES", "TRAVELLINGALLOWANCE", "LODGESAVINGEXPENSES", "OTHERCHARGE" };
        List<string> ps2 = new List<string> { "MONTH", "DAY", "TOTALDAY"};
        protected override void OnLoadControlComplete()
        {
            DetailGrid dGrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            
            if (dGrid != null && !this.EditForm.IsReInitForm)
            {
                double width = dGrid.ADGrid.Columns[dGrid.ADGrid.Columns.Count - 1].Width.Value;
                dGrid.ToolBarItemClick += new EventHandler<ToolBarItemClickEventArgs>(dGrid_ToolBarItemClick);
                dGrid.LoadRowDetailComplete += new EventHandler<ActionCompletedEventArgs<UIElement>>(dGrid_LoadRowDetailComplete); 
            }
            this.OrderEntity.OrderPropertyChanged += new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
            this.OrderEntity.CollectionEntityChanged += new EventHandler<EntityChangedArgs>(OrderEntity_CollectionEntityChanged);

        }
        protected override void OnLoadDataComplete()
        {
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                OrderEntity.SetObjValue("Entity.BUDGETARYMONTH", new DateTime(DataCore.SystemDateTime.Year, DataCore.SystemDateTime.Month, 1));
            }
        }
        void dGrid_LoadRowDetailComplete(object sender, ActionCompletedEventArgs<UIElement> e)
        {

            if (this.EditForm.OperationType == OperationTypes.Audit)
            {
                IControlAction form = e.Result as IControlAction;
                form.InitControl(OperationTypes.Edit);
            }
        }



        void dGrid_ToolBarItemClick(object sender, ToolBarItemClickEventArgs e)
        {
            if (e.Action != Actions.Add)
            {
                return;
            }
            e.Action = Actions.Cancel;
            FBEntityService service = new FBEntityService();
            QueryExpression qeOwner = this.OrderEntity.GetQueryExpression(FieldName.OwnerID);
            QueryExpression qeDept = this.OrderEntity.GetQueryExpression(FieldName.OwnerDepartmentID);
            QueryExpression qePost = this.OrderEntity.GetQueryExpression(FieldName.OwnerPostID);
            qeDept.RelatedExpression = qeOwner;
            qeOwner.RelatedExpression = qePost;
            qeDept.QueryType = typeof(T_FB_TRAVELEXPAPPLYDETAIL).Name;
            service.QueryFBEntitiesCompleted += (o, ea) =>
                {
                    ObservableCollection<FBEntity> listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_TRAVELEXPAPPLYDETAIL).Name);
                    ObservableCollection<FBEntity> listDetailItem = ea.Result;
                    listDetailItem.ToList().ForEach(item =>
                        {
                            T_FB_TRAVELEXPAPPLYDETAIL t = item.Entity as T_FB_TRAVELEXPAPPLYDETAIL;
                            t.T_FB_TRAVELEXPAPPLYMASTER = this.OrderEntity.Entity as T_FB_TRAVELEXPAPPLYMASTER;
                            t.SERIALNUMBER = listDetail.Count + 1;

                            ps.ForEach(p => { t.SetObjValue(p, decimal.Parse("0")); });
                            t.TOTALCHARGE = decimal.Parse("0");
                            
                            listDetail.Add(item);
                        });
                };


            service.QueryFBEntities(qeDept);
  

        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            if (object.Equals(sender, this.OrderEntity.Entity))
            {
                if (e.Result.Contains(typeof(T_FB_BORROWAPPLYMASTER).Name.ToEntityString()))
                {
                    OnBorrowIDChanged<T_FB_TRAVELEXPAPPLYDETAIL>();
                }
                else if (e.Result.Contains(EntityFieldName.OwnerID))
                {
                    ChangeCreator();
                }
            }

        }
        void OrderEntity_CollectionEntityChanged(object sender, EntityChangedArgs e)
        {
            if (sender.GetType().Name == typeof(T_FB_TRAVELEXPAPPLYDETAIL).Name)
            {
                OnDetailChanged(sender, e);

                if (e.ChangedEventArgs.PropertyName == "TOTALCHARGE")
                {
                    var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_TRAVELEXPAPPLYDETAIL).Name);
                    decimal? sumtotal = details.ToEntityList<T_FB_TRAVELEXPAPPLYDETAIL>().Sum(item => 
                        { return item.TOTALCHARGE; });

                    this.OrderEntity.SetObjValue("TOTALMONEY", Math.Round(sumtotal.Value, 2));
                    
                }
                if (e.ChangedEventArgs.PropertyName == "AUDITCHARGEMONEY")
                {

                    var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_TRAVELEXPAPPLYDETAIL).Name);
                    decimal? sumtotal = details.ToEntityList<T_FB_TRAVELEXPAPPLYDETAIL>().Sum(item =>
                    { return item.AUDITCHARGEMONEY; });
                    this.OrderEntity.SetObjValue("AUDITCHARGEMONEY", Math.Round(sumtotal.Value, 2));
                }
            }
        }


        private DataTemplate GetDTTemp(double textboxWidh)
        {
            //内存中动态生成一个XAML，描述了一个DataTemplate
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XElement xDataTemplate =
            new XElement(ns + "DataTemplate", new XAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
            new XElement(ns + "Grid", new XAttribute("Background", "White"),
                new XElement(ns + "Grid.ColumnDefinitions",
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "75")),
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "25")),
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", textboxWidh.ToString()))
                    ),
                new XElement(ns + "Grid.RowDefinitions",
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition")
                    ),
                new XElement(ns + "TextBlock", new XAttribute("Text", "机票费"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "0")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "车船费"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "1")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "住宿费"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "2")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "出差补助"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "3")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "住宿节约补助"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "4")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "其他"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "5")),

                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.AIRFARE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "0")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.CARFARE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "1")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.LODGINGEXPENSES}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "2")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.TRAVELLINGALLOWANCE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "3")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.LODGESAVINGEXPENSES}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "4")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.OTHERCHARGE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "5"))
                
            )
            );

            //将内存中的XAML实例化成为DataTemplate对象，并赋值给
            //ListBox的ItemTemplate属性，完成数据绑定
            XmlReader xr = xDataTemplate.CreateReader();
            DataTemplate dataTemplate = XamlReader.Load(xDataTemplate.ToString()) as DataTemplate;

            
            return dataTemplate;
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
            string propertyName = e.ChangedEventArgs.PropertyName;
            if (ps2.Contains(propertyName) || e.Action == Actions.Delete)
            {
                EntityObject ode = sender as EntityObject;
                decimal d = CommonFunction.TryConvertValue<decimal>(ode.GetObjValue(propertyName));
                ode.SetObjValue(propertyName, (int)d);

            }
            if (ps.Contains(propertyName) || e.Action == Actions.Delete)
            {
                EntityObject ode = sender as EntityObject;
                decimal? total = 0;
                ps.ForEach(p =>
                {
                    total += CommonFunction.TryConvertValue<decimal>(ode.GetObjValue(p));
                }
                );

                if (this.IsUnSubmit)
                {
                    ode.SetObjValue("TOTALCHARGE", Math.Round(total.Value, 2));
                }
                ode.SetObjValue("AUDITCHARGEMONEY", Math.Round(total.Value, 2));
            }
        }
    }

    
}
