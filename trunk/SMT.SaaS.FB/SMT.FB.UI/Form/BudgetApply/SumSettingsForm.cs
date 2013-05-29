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
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI;
using System.Collections.Specialized;


namespace SMT.FB.UI.Form.BudgetApply
{
    public class SumSettingsForm : FBPage
    {
        FBEntityService fbService;

        /// <summary>
        /// 预算汇总设置 构造函数
        /// </summary>
        /// <param name="orderEntity"></param>
        public SumSettingsForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        /// <summary>
        /// 保存单据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            List<string> msgs = new List<string>();
            T_FB_SUMSETTINGSMASTER entCurr = this.OrderEntity.Entity as T_FB_SUMSETTINGSMASTER;
            entCurr.CHECKSTATES = 0;
            entCurr.EDITSTATES = 1;
            
            if (string.IsNullOrWhiteSpace(entCurr.OWNERID) || string.IsNullOrWhiteSpace(entCurr.OWNERNAME))
            {
                msgs.Add("汇总人不能为空");
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
                return;
            }

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_SUMSETTINGSDETAIL).Name);

            ObservableCollection<FBEntity> list0 = new ObservableCollection<FBEntity>();
          
            //明细为为0的不能提交
            if (details.ToList().Count <= 1)
            {
                msgs.Add("预算汇总设置中添加的公司至少超过一家");
            }
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }
        }

        private void InitData()
        {
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
               
            }
            else
            {
                SetSortDetails();
            }

            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_SUMSETTINGSMASTER).Name);
            details.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(details_CollectionChanged);

        }
  
        void details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                SetSortDetails();
            }
        }

        private void SetSortDetails()
        {
            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_SUMSETTINGSDETAIL).Name);
                      DetailGrid dgrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (dgrid != null)
            {
                dgrid.ClearValue(DetailGrid.ItemsSourceProperty);
                dgrid.ItemsSource = details;
            }
        }

        protected override void OnLoadDataComplete()
        {
            InitData();

          
            
        }

        protected override void OnLoadControlComplete()
        {
            DetailGrid dGrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;

            if (dGrid != null && !this.EditForm.IsReInitForm)
            {
                dGrid.ToolBars[0].Title = "选择需要汇总的公司";
                dGrid.AddToolBarItems(dGrid.ToolBars);
                double width = dGrid.ADGrid.Columns[dGrid.ADGrid.Columns.Count - 1].Width.Value;
                dGrid.ToolBarItemClick += new EventHandler<ToolBarItemClickEventArgs>(dGrid_ToolBarItemClick);
            }
            Grid grid = this.EditForm["DetailRemark"] as Grid;

            if (grid != null)
            {
                grid.Height = 250;
                StackPanel sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = "汇总提示：", FontSize = 12 });
                sp.Children.Add(new TextBlock { Text = "1.此功能可满足2级预算汇总功能，若未设置，即按单个公司生成月度预算汇总。", FontSize = 12 });
                sp.Children.Add(new TextBlock { Text = "2.设置的汇总人即最后一级汇总提交人", FontSize = 12 });
                grid.Children.Add(sp);
              
            }
        }

        void dGrid_ToolBarItemClick(object sender, ToolBarItemClickEventArgs e)
        {
            if (e.Action != Actions.Add)
            {
                return;
            }
            e.Action = Actions.Cancel;
            string perm = "3";
            string entity = typeof(T_FB_PERSONMONEYASSIGNMASTER).Name;
            if (this.EditForm.OperationType == OperationTypes.Edit)
            {
                perm = ((int)Permissions.Edit).ToString();
            }
            else if (this.EditForm.OperationType == OperationTypes.Add)
            {
                perm = ((int)Permissions.Add).ToString();
            }
            else
            {
                perm = ((int)Permissions.Browse).ToString();
            }

            string userID = DataCore.CurrentUser.Value.ToString();
            OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);

            ogzLookup.SelectedObjType = OrgTreeItemTypes.Company;

            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;

            ogzLookup.SelectedClick += (o, ea) =>
            {
                if (ogzLookup.SelectedObj.Count > 0)
                {
                    var assignDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_SUMSETTINGSDETAIL).Name);
                    var selectedObjects = ogzLookup.SelectedObj;
                    selectedObjects.ForEach(obj =>
                        {
                            ITextValueItem cdata = DataCore.FindReferencedData<CompanyData>(obj.ObjectID);
                                                       
                            T_FB_SUMSETTINGSDETAIL detail = new T_FB_SUMSETTINGSDETAIL();
                            detail.SUMSETTINGSDETAILID = Guid.NewGuid().ToString();
                            detail.T_FB_SUMSETTINGSMASTER = this.OrderEntity.Entity as T_FB_SUMSETTINGSMASTER;
                            detail.EDITSTATES =1;
                            detail.OWNERCOMPANYID = cdata.Value.ToString();
                            detail.OWNERCOMPANYNAME = cdata.Text;
                            detail.CREATEDATE = DateTime.Now;
                            detail.CREATEUSERID = DataCore.CurrentUser.ID.ToString();
                            detail.CREATEUSERNAME = DataCore.CurrentUser.Text.ToString();

                            FBEntity fbEntity = detail.ToFBEntity();
                            fbEntity.FBEntityState = FBEntityState.Added;
                            assignDetail.Add(fbEntity);
                        });

                }
            };
            ogzLookup.Show<string>(DialogMode.ApplicationModal, plRoot, "", (result) => { });
        }
    }
}

