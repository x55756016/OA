using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using SMT.FB.UI.FBCommonWS;
using SMT.FB.UI.Common.Controls.DataTemplates;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.Validator;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;


namespace SMT.FB.UI.Common.Controls
{
   
    public partial class DetailGrid : UserControl, IControlAction
    {
        public static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DetailGrid).ItemsSource = e.NewValue as IEnumerable;
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(DetailGrid), new PropertyMetadata(ItemsSourceChanged));
        public DetailGrid()
        {
            InitializeComponent();
            ADGrid.LoadingRowGroup += new EventHandler<DataGridRowGroupHeaderEventArgs>(ADGrid_LoadingRowGroup);
            

        }

        public OperationTypes operationType;
        private OrderDetailGridInfo orderDetailGridInfo;
        public OrderDetailGridInfo OrderDetailGridInfo
        {
            set
            {
                orderDetailGridInfo = value;
                this.GridItems = orderDetailGridInfo.GridItems;
                this.Parameters = orderDetailGridInfo.Parameters;
                if (orderDetailGridInfo.OrderDetailEntityInfo != null)
                {
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                    binding.Path = new PropertyPath(orderDetailGridInfo.OrderDetailEntityInfo.ReferencedMember);
                    binding.Mode = System.Windows.Data.BindingMode.TwoWay;
                    this.SetBinding(DetailGrid.ItemsSourceProperty, binding);
                    this.Name = orderDetailGridInfo.Name;
                }
                SetRowDetailTemplate();
            }
            get
            {
                return orderDetailGridInfo;
            }
        }
        public IList<GridItem> GridItems
        {
            get
            {
                return ADGrid.GridItems;
            }
            set
            {
                ADGrid.GridItems = value;
            }
        }

        private IEnumerable itemsSource = null;
        public IEnumerable ItemsSource
        {
            set
            {
                base.SetValue(ItemsSourceProperty, value);
                itemsSource = value;
                if (Groups.Count > 0 && itemsSource != null)
                {
                    PagedCollectionView pcv = new PagedCollectionView(itemsSource);
                    Groups.ForEach(item =>
                        {
                            pcv.GroupDescriptions.Add(GetGroupDesc(item));
                        });


                    ADGrid.ItemsSource = pcv;
                    // 收缩所有分类
                    foreach (CollectionViewGroup group in pcv.Groups)
                    {
                       // ADGrid.ScrollIntoView(group, null);

                        ADGrid.CollapseRowGroup(group, true);
                    }

                    
                }
                else
                {
                    this.ADGrid.ItemsSource = itemsSource;
                }
            }
            get
            {
                return base.GetValue(ItemsSourceProperty) as IEnumerable;
            }
        }

        public List<ToolbarItem> ToolBars { get; set; }
        public bool ShowToolBar 
        {
            get
            {
                return this.P1.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    this.P1.Visibility = Visibility.Visible;
                }
                else
                {
                    this.P1.Visibility = Visibility.Collapsed;
                }

            }
        }

        private List<string> groups = new List<string>();
        public List<string> Groups
        {
            get
            {
                return groups;
            }
            set
            {
                groups = value;
                this.ItemsSource = this.itemsSource;

            }
        }

        private PropertyGroupDescription GetGroupDesc(string propertyName)
        {
            PropertyGroupDescription pd = new PropertyGroupDescription();
            //string[] groupStrs = propertyName.Split('.');
            //pd.PropertyName = groupStrs[0];
            //if (groupStrs.Length != 1)
            //{
            //    string subPropertyName = propertyName.Substring(pd.PropertyName.Length + 1);
            //    pd.Converter = new GroupConverter(subPropertyName);
            //}
           //pd.PropertyName = "";
            pd.Converter = new GroupConverter(propertyName);
            return pd;

        }
      
        
        #region IControlAction 成员

        public void InitControl(OperationTypes operationType)
        {
            this.operationType = operationType;
            this.ADGrid.InitControl(operationType);
            InitToolBar();
            SetDataChangeEvent();

            if (!(operationType == OperationTypes.Add || operationType == OperationTypes.Edit))
            {
                this.P1.Visibility = Visibility.Collapsed;
            }
            if (this.orderDetailGridInfo.RowDetailDataPanel != null)
            {
                DataGridIconColumn dc = new DataGridIconColumn();
                dc.Binding = new Binding("HideDetails");
                this.ADGrid.Columns.Insert(0, dc);
            }
        }

        void ADGrid_LoadingRowGroup(object sender, DataGridRowGroupHeaderEventArgs e)
        {
            e.RowGroupHeader.PropertyNameVisibility = System.Windows.Visibility.Collapsed;
        }

        private void InitToolBar()
        {
            
            List<ToolbarItem> listBar = new List<ToolbarItem>();
            ToolbarItem tbItem = ToolBarItems.New;
            tbItem.Title = "选择科目";
            listBar.Add(tbItem);
            listBar.Add(ToolBarItems.Delete);
            AddToolBarItems(listBar);
            deatilGridBar.ShowItem(deatilGridBar.TitleImageName, false);
            deatilGridBar.ShowItem(deatilGridBar.HelpButtonName, false);
            deatilGridBar.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(deatilGridBar_ItemClicked);
        }

        public void AddToolBarItems(List<ToolbarItem> listBar)
        {
            ToolBars = listBar;
            deatilGridBar.InitToolBarItem(listBar);
        }

        void deatilGridBar_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            switch (e.Key)
            {
                case "New" :
                    OnToolBarItemClick(Actions.Add);
                    break;
                case "Delete" :
                    OnToolBarItemClick(Actions.Delete);
                    break;

            }
        }


        public Control FindControl(string name)
        {
            if (this.Name == name)
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public void InitData(OrderEntity entity)
        {
            return;
        }

        public bool SaveData(Action<IControlAction> SavedCompletedAction)
        {
            return true;
        }
        #endregion

        #region 事件
        public event EventHandler<ToolBarItemClickEventArgs> ToolBarItemClick;
        #endregion


        protected virtual void OnToolBarItemClick(Actions action)
        {

            if (ToolBarItemClick != null)
            {
                ToolBarItemClickEventArgs args = new ToolBarItemClickEventArgs(action);
                ToolBarItemClick(this, args);

                if (args.Action == Actions.Cancel)
                {
                    return;
                }
            }

           
            if (action == Actions.Add)
            {
                Add();
            }
            else
            {
                IList list = this.ADGrid.SelectedItems as IList;
                Delete(list);
            }
            
        }

        public void AddTemplete(DataTemplate dt)
        {
            this.ADGrid.RowDetailsTemplate = dt;
        }

        private void SetRowDetailTemplate()
        {
            if (this.orderDetailGridInfo.RowDetailDataPanel != null)
            {
                #region RowDetailTemplate
                DataTemplate dt = DataTemplateHelper.GetEmptyGrid();

                this.ADGrid.RowDetailsTemplate = dt;
                this.ADGrid.LoadingRowDetails += (o, e) =>
                {

                    IDataPanel panel = this.orderDetailGridInfo.RowDetailDataPanel;
                    IControlAction control = panel.GetUIElement();
                    Grid grid = e.DetailsElement as Grid;
                    grid.Background = e.Row.Background;

                    if (grid.Children.Count == 0)
                    {                   
                        Border border = new Border();
                        border.Style = (Style)Application.Current.Resources["DetailShow_1"];

                        grid.Children.Add(border);
                        if (control.GetType() == typeof(DetailGrid))
                        {
                            DetailGrid dgrid = control as DetailGrid;
                            border.Child = dgrid;
               
                            // dgrid.Margin = new Thickness(80, 0, 0, 0);
                            control.InitControl(this.operationType);
                        }
                        else
                        {
                            border.Child = (control as UIElement);
                            OperationTypes cOpType = this.operationType;
                            if (this.operationType == OperationTypes.Audit)
                            {
                                cOpType = OperationTypes.Edit;
                            }
                            control.InitControl(cOpType);
                        }
                    }
                    DependencyObject dObj = VisualTreeHelper.GetChild(e.DetailsElement,0);
                    UIElement curControl = (dObj as Border).Child;
                    if (curControl.GetType() == typeof(DetailGrid))
                    {
                        DetailGrid dgrid = curControl as DetailGrid;
                        string entityType = dgrid.OrderDetailGridInfo.OrderDetailEntityInfo.Entity.Type;
                        FBEntity source = e.Row.DataContext as FBEntity; ;
                        if (source != null)
                        {
                            ObservableCollection<FBEntity> list = source.GetRelationFBEntities(entityType);
                            if (list.Count > 0)
                            {
                                dgrid.ItemsSource = list;
                                
                            }
                            else
                            {
                                
                                dgrid.Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                    if (LoadRowDetailComplete != null)
                    {
                        ActionCompletedEventArgs<UIElement> args = new ActionCompletedEventArgs<UIElement>(curControl);
                        LoadRowDetailComplete(o, args);
                    }
                };
                #endregion
            }
            
        }
        public event EventHandler<ActionCompletedEventArgs<UIElement>> LoadRowDetailComplete;

        #region IControlAction 成员

        public bool Validate()
        {
            return true;
        }
        public ValidatorManager ValidatorManager
        {
            get;
            set;
        }


        #endregion

        private SelectedDataManager selectedDataManager;
        public SelectedDataManager SelectedDataManager
        {
            get
            {
                if (selectedDataManager == null)
                {
                    selectedDataManager = new SelectedDataManager();

                    
                }
                return selectedDataManager;
            }
        }

        public Dictionary<string, string> Parameters { get; set; }

        private void SetDataChangeEvent()
        {
            if (Parameters == null)
            {
                return;
            }
            
            OrderEntity orderEntity = this.DataContext as OrderEntity;

            if (orderEntity == null)
            {
                return;
            }
            OnParameterValueChange();
            orderEntity.OrderPropertyChanged += (o, e) =>
                {
                    List<string> listProperty = e.Result;
                    bool isUnChanged = true;

                    foreach (KeyValuePair<string, string> dict in Parameters)
                    {
                        if (dict.Value.StartsWith("{") && dict.Value.EndsWith("}"))
                        {
                            string itemName = dict.Value.Trim('{', '}');
                            isUnChanged &= !listProperty.Contains(itemName);
                        }
                    }
                    if (!isUnChanged)
                    {
                        OnParameterValueChange();
                    }
                };
        }

        private void OnParameterValueChange()
        {
            OrderEntity orderEntity = this.DataContext as OrderEntity;
            QueryExpression qeTop = null;

            foreach (KeyValuePair<string, string> dict in Parameters)
            {
                object value = orderEntity.GetEntityValue(dict.Value);
                QueryExpression qe = QueryExpressionHelper.Equal(dict.Key, value.ToString());
                qe.RelatedExpression = qeTop;
                qeTop = qe;
            }
            if (qeTop != null)
            {

                qeTop.VisitAction = ((int)this.operationType).ToString();
                qeTop.QueryType = this.OrderDetailGridInfo.OrderDetailEntityInfo.Type;
                qeTop.VisitModuleCode = this.OrderDetailGridInfo.OrderDetailEntityInfo.Type;
                
            }

            SelectedDataManager.QueryExpression = qeTop;
        }

        private Func<FBEntity, bool> _CheckSame = null;
        public Func<FBEntity, bool> CheckSame
        {
            get
            {
                return _CheckSame;
            }
            set
            {
                if (!object.Equals ( _CheckSame, value))
                {
                    _CheckSame = value;
                    if (_CheckSame != null)
                    {
                        SelectedDataManager.GetSameItem = (list, item) =>
                        {
                            return CheckSame(item) ? item : null;
                        };
                    }
                    else
                    {
                        SelectedDataManager.GetSameItem = null;
                    }
                }
            }
        }

        public void Add()
        {
            string entity = OrderDetailGridInfo.OrderDetailEntityInfo.Type;

            SelectedForm sf = new SelectedForm();
            sf.SelectedDataManager = this.SelectedDataManager;
            sf.ReferenceDataType = entity;
            //sf.Query = qeTop;
            sf.SelectedCompleted += (o, e) =>
            {
                sf.SelectedItems.ForEach(item =>
                {
                    item.FBEntityState = FBEntityState.Added;
                    (ItemsSource as ObservableCollection<FBEntity>).Add(item);
                });
            };
            sf.Show();
        }

        public void Delete(IList list)
        {
            if (list == null)
            {
                return;
            }

            IList<FBEntity> listSource = this.ItemsSource as IList<FBEntity>;

            for (int i = 0; i < list.Count; i++)
            {
                FBEntity entity = list[i] as FBEntity;
                entity.Actived = false;
                listSource.Remove(entity);
            }
        }
    }
}
