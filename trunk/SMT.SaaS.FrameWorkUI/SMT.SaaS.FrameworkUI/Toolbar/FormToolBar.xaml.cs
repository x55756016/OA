/***********************************
 * 功能：view区域中的功能菜单集合
 *       默认显示通用按钮，隐藏部分按钮。
 *       各个页面需加按钮在stpOtherAction面板中加
 *       添加按钮调用方法ImageButton
 * 编辑者：王玲
 * ***************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace SMT.SaaS.FrameworkUI
{
    public partial class FormToolBar : UserControl
    {
       
        public FormToolBar()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FormToolBar_Loaded);
          
            ////ToolBarItem item = new ToolBarItem() { Model = model };
        }

        void FormToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ShowRect();
        }
        public ToolBarItemGroup AddToolButton(ToolBarItemGroupModel model)
        {
            custom.Orientation = Orientation.Horizontal;
            ToolBarItemGroup item = new ToolBarItemGroup() { Model = model };
            item.Model = model;
            custom.Margin = new Thickness(5, 0, 0, 0);
            custom.Children.Add(item);

            Rectangle _regtoolbaritem = new Rectangle();
            _regtoolbaritem.Height = 18;
            _regtoolbaritem.Width = 1;
            _regtoolbaritem.Fill = new SolidColorBrush(Color.FromArgb(255, 154, 154, 153));
            custom.Children.Add(_regtoolbaritem);
            _regtoolbaritem.Margin = new Thickness(67, 3, 0, 3);
            return item;
        }

        public Grid ToolBarGrid
        {
            get { return this.FormToolGrid; }
        }

        public StackPanel FormToolNew
        {
            get { return this.FormToolContainer1; }
        }

        public StackPanel stpCheckState
        {
            get { return this.ftbCheckState; }
        }

        public StackPanel stpOtherAction
        {
            get { return this.StackPanelOtherAction; }
        }

        
        public StackPanel stpOtherRAction
        {
            get { return this.OtherRSp; }
            set { OtherRSp = value; }
        }

        public Image ToolBarPrint
        {
            get { return this.FormToolPrint; }
        }

        public Image ToolBarOutExcel
        {
            get { return this.FormToolOutExcel; }
        }

        public Image ToolBarOutPDF
        {
            get { return this.FormToolOutPDF; }
        }

        public Image ToolBarDelete
        {
            get { return this.FormToolDelete; }
        }

        public Image ToolBarEdit
        {
            get { return this.FormToolEdit; }
        }

        #region line
        public Rectangle retNew
        {
            get { return this.RectangleNew; }
        }
        public Rectangle retRead
        {
            get { return this.RectangleRead; }
        }
        public Rectangle retPDF
        {
            get { return this.RectanglePDF; }
        }
        public Rectangle retEdit
        {
            get { return this.RectangleEdit; }
        }

        public Rectangle retDelete
        {
            get { return this.RectangleDelete; }
        }
        public Rectangle retRefresh
        {
            get { return this.RectangleRefresh; }
        }
        public Rectangle retAudit
        {
            get { return this.RectangleAudit; }
        }

        public Rectangle retAuditNoPass
        {
            get { return RectangleAudit; }
        }
        #endregion

        #region 按钮
 

        public Button btnNew
        {
            get { return this.ButtonNew; }
        }

        //导入数据
        public Button btnImport
         {
             get { return this.Buttonimport; }
             set
             {
                 //ToolTipService.ToolTip="{Binding Converter={StaticResource ResourceConveter}, Source=IMPORT}"
                 TBImport.Text = this.Buttonimport.Content.ToString();
                 FormToolimport.Tag = this.Buttonimport.Content.ToString();
             }
         }

        public Button btnImports(string _img, string _name)
        {
            Buttonimport.Visibility = Visibility.Visible;
            FormToolimport.Source = new BitmapImage(new Uri(_img, UriKind.Relative));
            TBImport.Text = _name;
            FormToolimport.Tag = _name;
            return this.Buttonimport;
        }

         public Button btnPrint
         {
             get { return this.ButtonPrint; }
         }

         public Button btnOutExcel
         {
             get { return this.ButtonOutExcel; }
         }

         public Button btnOutPDF
         {
             get { return this.ButtonOutPDF; }
         }

         public Button btnDelete
         {
             get { return this.ButtonDelete; }
         }

         public Button btnEdit
         {
             get { return this.ButtonEdit; }
         }
        //审核
         public Button btnAudit
         {
             get { return this.ButtonAudit; }
         }
         //提交审核
         public Button btnSumbitAudit
         {
             get { return this.ButtonSumbitAudit; }
         }
         //重新提交
         public Button btnReSubmit
         {
             get { return this.ButtonReSubmit; }
         }

        //报表
         public Button btnReports
         {
             get { return this.ButtonReports; }
         }

         public ComboBox cbxOtherAction
         {
             get { return this.FormToolCBOtherWork; }
         }

         public TextBlock txtOtherName
         {
             get { return this.txtOtherAction; }
         }

         public TextBlock txtCheckStateName
         {
             get { return this.txtCheckState; }
         }  

         public ComboBox cbxCheckState
         {
             get { return this.FormToolCBCheckState; }
         }

         public ComboBox cbbQueryList
         {
             get { return this.FormToolCBView; }
         }

         public void ShowView(bool isShow)
         {
             if (isShow)
             {
                 ftbView.Visibility = Visibility.Visible;
                 // ftbCheckState.Visibility = Visibility.Visible;
             }
             else
             {
                 ftbView.Visibility = Visibility.Collapsed;
                 // ftbCheckState.Visibility = Visibility.Collapsed;
             }
             
         }
        //刷新数据
         public Button btnRefresh
         {
             get { return this.ButtonRefresh; }
         }

        //审核不通过
         public Button btnAduitNoTPass
         {
             get { return this.ButtonAuditNoTPass; }
         }
         //返回待定按钮
         public Button btnOtherAction(string _img,string _name)
         {
             ButtonOtherAction.Visibility = Visibility.Visible;
             OtherActionImg.Source = new BitmapImage(new Uri(_img, UriKind.Relative));
             TBOtherAction.Text = _name;
             OtherActionImg.Tag = _name;
             return this.ButtonOtherAction;
         }

         //查看
         public Button BtnView
         {
             get { return this.ButtonSelect; }
         }
         //审核状态
         public TextBlock TblCheckState
         {
             get { return this.txtCheckState; }
         }
        #endregion

         Rectangle _rec;
        /// <summary>
        ///  新增分割符
        /// </summary>
         public Rectangle AddPartition()
         {
            _rec = new Rectangle();
            _rec.Width = 1;
            _rec.Height = 18;
            _rec.HorizontalAlignment = HorizontalAlignment.Right;
            _rec.Margin = new Thickness(0,4,0,3);
            _rec.Fill = new SolidColorBrush(Color.FromArgb(255,154,154,153));
            return _rec;
         }

         #region 显示对应按钮分割符
        /// <summary>
        ///  根据当前所在按钮显示对应按钮分割符
        /// </summary>
         public  void ShowRect()
         {
             var _visi = Visibility.Collapsed;
             if (btnNew.Visibility == _visi)
             {
                 RectangleNew.Visibility = _visi;//新增
             }
             if (btnPrint.Visibility == _visi && btnOutExcel.Visibility==_visi)
             {
                 RectanglePDF.Visibility = _visi;//打印+导出Excel
             }
             if (btnEdit.Visibility == _visi)
             {
                 RectangleEdit.Visibility = _visi;//编辑
             }
             if (btnDelete.Visibility == _visi)
             {
                 RectangleDelete.Visibility = _visi;//删除
             }
             if (BtnView.Visibility == _visi)
             {
                 RectangleRead.Visibility = _visi;//查看
             }
             if (btnRefresh.Visibility == _visi)
             {
                 RectangleRefresh.Visibility = _visi;//刷新
             }
             if (btnAudit.Visibility == _visi && btnReSubmit.Visibility == _visi)
             {
                 RectangleAudit.Visibility = _visi;//审核+重新提交审核
             }
             if (this.btnReports.Visibility == _visi)
             {
                 RectangleImport.Visibility = _visi;//刷新
             }
         }
        #endregion
    }
}
