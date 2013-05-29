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
using SMT.SaaS.FrameworkUI;

namespace SMT.FB.UI.Form.BudgetApply
{
    public class CompanyBudgetSumForm : FBPage
    {
        /// <summary>
        /// 汇总类型 0科目，1部门
        /// </summary>
        /// 
        private int _SumType=0;
        public int SumType 
        {
            get
            {
                return this._SumType;
            }set
            {
                this._SumType=value;
            }
        }

        public CompanyBudgetSumForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            e.SaveFBEntity.CollectionEntity.Clear();
        }

        protected override void OnLoadControlComplete()
        {
            base.OnLoadControlComplete();
            DetailGrid grid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            
            if (grid != null)
            {
                grid.P1.Visibility = System.Windows.Visibility.Visible;
                List<ToolbarItem> list = new List<ToolbarItem>();
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "S1",
                    Title = "按科目查看",
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png",
                    

                };
                list.Add(item);
                 item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "S2",
                    Title = "按公司部门单据查看",
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };
                 list.Add(item);
                
                 grid.AddToolBarItems(list);
            }
            grid.deatilGridBar.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(deatilGridBar_ItemClicked);
            deatilGridBar_ItemClicked(grid, new ToolBar.ToolBarItemClickArgs("S1"));
        }

        void deatilGridBar_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            if (e.Key == "S1")
            {
                this.SumType = 0;
            }
            else
            {
                this.SumType = 1;
            }
           //this.InitData();
            DetailGrid grid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (grid != null)
            {
                if (this.SumType == 0)
                {
                    int i = 1;
                    if (grid.ADGrid.Columns[0].GetType() == typeof(DataGridIconColumn))
                    {
                        i = 2;
                    }
                    grid.ADGrid.Columns[i-1].Visibility = Visibility.Visible;
                    grid.ADGrid.Columns[i].Visibility = Visibility.Visible;
                    grid.ADGrid.Columns[i+3].Visibility = Visibility.Visible;

                    grid.ADGrid.Columns[i+1].Visibility = Visibility.Collapsed;
                    grid.ADGrid.Columns[i+2].Visibility = Visibility.Collapsed;
                    grid.ADGrid.Columns[i+4].Visibility = Visibility.Collapsed;

                    grid.ItemsSource = this.OrderEntity.GetRelationFBEntities(typeof(V_SubjectCompanySum).Name);
                }
                else
                {
                    int i = 1;
                    if (grid.ADGrid.Columns[0].GetType() == typeof(DataGridIconColumn))
                    {
                        i = 2;
                    }
                    grid.ADGrid.Columns[i-1].Visibility = Visibility.Collapsed;
                    grid.ADGrid.Columns[i].Visibility = Visibility.Collapsed;

                    grid.ADGrid.Columns[i + 2].Visibility = Visibility.Visible;
                    grid.ADGrid.Columns[i + 1].Visibility = Visibility.Visible;
                    grid.ADGrid.Columns[i + 3].Visibility = Visibility.Visible;
                    grid.ADGrid.Columns[i + 4].Visibility = Visibility.Visible;
                 
                    grid.ItemsSource = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETSUMDETAIL).Name);
                }
            }
        }

        protected override void OnLoadDataComplete()
        {
            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETSUMDETAIL).Name);
            
            List<T_FB_COMPANYBUDGETAPPLYDETAIL> listDetail = new List<T_FB_COMPANYBUDGETAPPLYDETAIL>();


            this.OrderEntity.FBEntity.OrderDetailBy<T_FB_COMPANYBUDGETAPPLYDETAIL>(item =>
            {
                return item.T_FB_COMPANYBUDGETAPPLYMASTER.OWNERDEPARTMENTID;
            });

            details.ToList().ForEach(item =>
                {
                    var cDetail = (item.Entity as T_FB_COMPANYBUDGETSUMDETAIL).T_FB_COMPANYBUDGETAPPLYDETAIL;
                    item.AddFBEntities<T_FB_COMPANYBUDGETAPPLYDETAIL>(cDetail.ToFBEntityList());

                    item.OrderDetailBy<T_FB_COMPANYBUDGETAPPLYDETAIL>(detail =>
                    {
                        return detail.T_FB_SUBJECT!=null?detail.T_FB_SUBJECT.SUBJECTCODE:string.Empty ;
                    });
                    listDetail.AddRange(cDetail);
                });

            var detailSum = from item in listDetail
                            group item by item.T_FB_SUBJECT into p
                            select new V_SubjectCompanySum
                            {
                                T_FB_SUBJECT = p.Key,
                                BUDGETMONEY = p.Sum(sumItem => sumItem.BUDGETMONEY),
                                T_FB_COMPANYBUDGETAPPLYDETAIL = listDetail.Where(de => de.T_FB_SUBJECT == p.Key).ToList()
                            };
            var detailsTwo = this.OrderEntity.GetRelationFBEntities(typeof(V_SubjectCompanySum).Name);
            foreach (var ds in detailSum)
            {

                
                FBEntity fbDetail = ds.ToFBEntity();
                var listDetails = fbDetail.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name);

                fbDetail.AddFBEntities<T_FB_COMPANYBUDGETAPPLYDETAIL>(ds.T_FB_COMPANYBUDGETAPPLYDETAIL.ToFBEntityList());

                fbDetail.OrderDetailBy<T_FB_COMPANYBUDGETAPPLYDETAIL>(item =>
                {
                    return item.T_FB_COMPANYBUDGETAPPLYMASTER.OWNERDEPARTMENTID;
                });

                detailsTwo.Add(fbDetail);
            }
            this.OrderEntity.FBEntity.OrderDetailBy<V_SubjectCompanySum>(item =>
            {
                return item.T_FB_SUBJECT.SUBJECTCODE;
            });

        }


        protected override void OnAuditing(object sender, AuditEventArgs e)
        {
            try
            {
                this.OrderEntity.FBEntity.CollectionEntity.Clear();

                var posts = (this.OrderEntity.LoginUser as LoginUserData).PostInfos;
                EmployeerData ownerInfo = this.OrderEntity.GetOwnerInfo();

                var finds = posts.Where(item => item.Company.Value.ToString() == ownerInfo.Company.Value.ToString());
                if (finds.Count() > 0)
                {
                    var user = finds.FirstOrDefault();
                    AuditControl ac = sender as AuditControl;
                    ac.AuditEntity.CreateCompanyID = user.Company.Value.ToString();
                    ac.AuditEntity.CreateDepartmentID = user.Department.Value.ToString();
                    ac.AuditEntity.CreatePostID = user.Post.Value.ToString();
                }



                base.OnAuditing(sender, e);
            }
            catch (Exception ex)
            {
                e.Result = AuditEventArgs.AuditResult.Cancel;
                CommonFunction.ShowErrorMessage("提交或审核异常, " + ex.ToString());
            }
        }

    }
   
}
