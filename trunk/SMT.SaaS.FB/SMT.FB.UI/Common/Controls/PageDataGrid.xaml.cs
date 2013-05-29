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
using System.Collections.ObjectModel;
using System.Windows.Data;
using SMT.FB.UI.FBCommonWS;


namespace SMT.FB.UI.Common.Controls
{
    public partial class PageDataGrid : UserControl
    {
        public PageDataGrid()
        {
            InitializeComponent();
            this.dataPager.Click += new SaaS.FrameworkUI.GridPager.PagerButtonClick(dataPager_Click);
            this.dataPager.PageButtonCount = 3;
        }

        public ObservableCollection<OrderEntity> ItemsSource
        {
            set;
            get;
        }

        private PageExpression Pager
        {
            get
            {
                return this.Queryer.Pager;
            }
        }
        private PageQueryer _Queryer = null;
        public PageQueryer Queryer 
        {
            get
            {
                if (_Queryer == null)
                {
                    _Queryer = new PageQueryer();
                    _Queryer.QueryCompleted += new EventHandler<FBCommonWS.QueryCompletedEventArgs>(_Queryer_QueryCompleted);
                }
                return _Queryer;
            }
            set
            {
                if ( object.Equals(_Queryer, value))
                {
                    _Queryer = value;
                    _Queryer.QueryCompleted += new EventHandler<FBCommonWS.QueryCompletedEventArgs>(_Queryer_QueryCompleted);
                    
                }
                
                
            }
        }

        public AutoDataGrid Grid
        {
            get
            {
                return this.ADtGrid;
            }
        }

        public IList SelectedItems
        {
            get
            {
                return this.ADtGrid.SelectedItems;
            }
        }
        void _Queryer_QueryCompleted(object sender, FBCommonWS.QueryCompletedEventArgs e)
        {

            this.ItemsSource = e.Result.Result.ToEntityAdapterList<OrderEntity>();
 
            this.ADtGrid.ItemsSource = ItemsSource.Take(Pager.PageSize);

            this.dataPager.PageSize = this.Pager.PageSize;
            this.dataPager.PageIndex = this.Pager.PageIndex;
            this.dataPager.PageCount = this.Pager.PageCount;
            

        }

        void dataPager_Click(object sender, RoutedEventArgs e)
        {
            PageExpression pager = Queryer.QueryExpression.Pager;
            int minRow = pager.PreRowCount / pager.PageSize -1;
            int subPageCount = this.dataPager.PageIndex - pager.PageIndex;
            if (subPageCount <= minRow && subPageCount >= 0)
            {
                // this.ItemsSource
                var data = this.ItemsSource.Skip(subPageCount * pager.PageSize).Take(pager.PageSize);
                this.ADtGrid.ItemsSource = data;
            }
            else
            {
                this.Pager.PageIndex = this.dataPager.PageIndex;
                Queryer.Query();
            }
        }

    }
}
