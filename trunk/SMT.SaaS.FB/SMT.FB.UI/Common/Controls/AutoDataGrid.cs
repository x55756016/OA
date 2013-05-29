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
using System.Collections.Generic;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml;
using System.Windows.Markup;
using System.Collections;
using System.Windows.Media.Imaging;
using SMT.FB.UI.FBCommonWS;
using System.Windows.Controls.Primitives;
using SMT.FB.UI.Common.Controls.DataTemplates;

namespace SMT.FB.UI.Common.Controls
{
    public class AutoDataGrid : DataGrid, IControlAction
    {
        public AutoDataGrid() : base()
        {
 
            
            GridItems = new List<GridItem>();
            writtableStyle = new Style();
            SolidColorBrush sbEdit = (SolidColorBrush)Application.Current.Resources["Datagridbg_edit"];
            writtableStyle.Setters.Add(new Setter() { Property = BackgroundProperty, Value = sbEdit });
            //writtableStyle.Setters.Add(new Setter() { Property = BackgroundProperty, Value = new SolidColorBrush(GetColor("#FFE9E8D9")) });

            writtableStyle.TargetType = typeof(DataGridCell);
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            
            this.AutoGenerateColumns = false;

            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;

        }


        public IList<GridItem> GridItems { get; set; }

        public Style writtableStyle;

        private bool isShowDetail;

        private Style columnHeaderStyle = null;

        
        public void InitControl(OperationTypes operationType)
        {
            columnHeaderStyle = (Style)Application.Current.Resources["DataGridColumnHeaderStyle"];
            this.Style = (Style)Application.Current.Resources["DataGridStyle"];
            this.RowStyle = (Style)Application.Current.Resources["DataGridRowStyle"];
            //this.ColumnHeaderStyle = 
            this.Columns.Clear();
            for (int i = 0; i < GridItems.Count; i++)
            {
                DataGridColumn dgtc = GetDataGridColumn(GridItems[i]);
                string toolTipText = GridItems[i].TipText;


                dgtc.Header = GridItems[i].PropertyDisplayName;
                dgtc.Width = new DataGridLength(GridItems[i].Width);
                
                dgtc.SortMemberPath = GridItems[i].PropertyName;
                dgtc.CanUserSort = true;
                
                // 可写背景色
                if (!GridItems[i].IsReadOnly && (operationType != OperationTypes.Browse && operationType != OperationTypes.Audit))
                {
                    dgtc.CellStyle = writtableStyle;
                    
                    dgtc.IsReadOnly = false;
                }
                else
                {
                    dgtc.IsReadOnly = true;
                }
                this.Columns.Add(dgtc);

                var styleHeader = this.columnHeaderStyle;
                if (toolTipText != null)
                {
                    //<TextBlock Margin="3 2 0 2"  Foreground="{StaticResource HeaderBackgroundfontColor1}" x:Name="ContentFontColor">
                    //</TextBlock>

                    XElementString gridXCs = new XElementString("TextBlock",
                        new XAttributeString("Margin", "3 2 0 2"),
                        new XAttributeString("Foreground", "{StaticResource HeaderBackgroundfontColor1}"),
                        new XAttributeString("x:Name", "ContentFontColor"),
                        new XAttributeString("Text", "{Binding  Converter={StaticResource GridHeaderConverter}, RelativeSource={RelativeSource TemplatedParent}}"),
                        new XAttributeString("ToolTipService.ToolTip", toolTipText)
                        );

                    var dtHeadTextBlockTemplate = DataTemplateHelper.GetDataTemplate(gridXCs);

                    styleHeader = styleHeader.Copy();
                    styleHeader.SetStyle(DataGridColumnHeader.ContentTemplateProperty, dtHeadTextBlockTemplate);
                }
                dgtc.HeaderStyle = styleHeader;
                
            }
        }

        private DataGridColumn GetDataGridColumn(GridItem gItem)
        {
            Binding bding = new Binding();
            if (!string.IsNullOrEmpty(gItem.PropertyName))
            {
                bding.Mode = BindingMode.TwoWay;
                bding.Path = new PropertyPath(gItem.PropertyName);
                if (gItem.ReferenceDataInfo != null)
                {
                    IValueConverter converter = CommonFunction.GetIValueConverter(gItem.ReferenceDataInfo.Type);
                    bding.Converter = converter;
                }
                else if (gItem.PropertyName.Contains("MONEY"))
                {
                    IValueConverter converter = new CurrencyConverter();
                    bding.Converter = converter;
                }
                else
                {
                    IValueConverter converter = new CommonConvert(gItem);
                    bding.Converter = converter;
                }
                
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("");
            }

            DataGridColumn dgc = null;
            switch (gItem.CType)
            {
                case ControlType.CheckBox :
                    DataGridCheckBoxColumn dgcc = new DataGridCheckBoxColumn();
                    dgcc.IsThreeState = false;
                    dgcc.Binding = bding;
                    dgc = dgcc;
                    break;
                case ControlType.Label :
                    if (gItem.ReferenceDataInfo != null)
                    {
                        DataGridReferenceColumn dgrc = new DataGridReferenceColumn();
                        dgrc.Binding = bding;
                        dgrc.DisplayMemberPath = "Text";
                        
                        dgc = dgrc;

                    }
                    else
                    {
                        DataGridTextColumn dgtc = new MyDataGridTextColumn(gItem);
                        dgtc.Binding = bding;
                        dgc = dgtc;
                    }
                    break;

                case ControlType.Combobox :

                    DataGridComboBoxColumn dgcbc = new DataGridComboBoxColumn();
                    if (gItem.ReferenceDataInfo != null)
                    {
                        IList<ITextValueItem> list = DataCore.GetRefData(gItem.ReferenceDataInfo.Type);
                        dgcbc.ItemsSource = list;
                    }
                    dgcbc.DisplayMemberPath = "Text";
                    dgcbc.Binding = bding;

                    dgc = dgcbc;
                    break;

                case ControlType.TreeViewItem :
                    DataGridIconColumn dgtvic = new DataGridIconColumn();
                    dgtvic.Binding = bding;
                    dgc = dgtvic;
                    break;
            }

            return dgc;
        }

        private void cbb_Load(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("a");
        }

        private Color GetColor(string colorName)
        {
            if (colorName.StartsWith("#"))
                colorName = colorName.Replace("#", string.Empty);
            int v = int.Parse(colorName, System.Globalization.NumberStyles.HexNumber);
            return new Color()
            {
                A = Convert.ToByte((v >> 24) & 255),
                R = Convert.ToByte((v >> 16) & 255),
                G = Convert.ToByte((v >> 8) & 255),
                B = Convert.ToByte((v >> 0) & 255)
            };
            
        }

        public Control FindControl(string name)
        {
            return this.FindName(name) as Control;
        }



        #region IControlAction 成员


        public bool Validate()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IControlAction 成员


        public SMT.SaaS.FrameworkUI.Validator.ValidatorManager ValidatorManager
        {
            get;
            set;
        }

        #endregion


        public void InitData(OrderEntity entity)
        {
            return;
        }

        public bool SaveData(Action<IControlAction> SavedCompletedAction)
        {
            return true;
        }
    }

    public class DataGridReferenceColumn : DataGridTextColumn
    {
        public string DisplayMemberPath
        {
            get;
            set;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {


            TextBlock block = new TextBlock();
            block.Margin = new Thickness(4.0);

            block.VerticalAlignment = VerticalAlignment.Center;
            block.SetBinding(TextBlock.DataContextProperty, this.Binding);
            Binding bding = new Binding();
            bding.Path = new PropertyPath(this.DisplayMemberPath);
            block.SetBinding(TextBlock.TextProperty, bding);

            return block;
        }

    }
    public class DataGridComboBoxColumn : DataGridBoundColumn
    {
        public IEnumerable ItemsSource { get; set; }
        public string DisplayMemberPath { get; set; }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {

            ComboBox cbb = new ComboBox();
            cbb.ItemsSource = this.ItemsSource;
            cbb.DisplayMemberPath = DisplayMemberPath;
            cbb.VerticalAlignment = VerticalAlignment.Stretch;

            cbb.Background = new SolidColorBrush(Colors.Transparent);

            cbb.SetBinding(ComboBox.SelectedItemProperty, this.Binding);
            return cbb;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {

            TextBlock block = new TextBlock();
            block.Margin = new Thickness(4.0);

            block.VerticalAlignment = VerticalAlignment.Center;
            block.SetBinding(TextBlock.DataContextProperty, this.Binding);
            Binding bding = new Binding();
            bding.Path = new PropertyPath(this.DisplayMemberPath);
            block.SetBinding(TextBlock.TextProperty, bding);

            return block;
        }

        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {

            ComboBox ccb = editingElement as ComboBox;

            if (ccb == null)
            {

                return null;

            }

            return ccb.SelectedItem;

        }

    }

    public class MyDataGridTextColumn : DataGridTextColumn
    {
        public MyDataGridTextColumn(GridItem gridItem)
        {
            this.GridItem = gridItem;
        }
        public GridItem GridItem { get; set; }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            
            TextBox tb = (TextBox)base.GenerateEditingElement(cell, dataItem);
            tb.TextWrapping = TextWrapping.Wrap;
            tb.AcceptsReturn = true;
            tb.MaxLength = GridItem.MaxLength;

            if (GridItem.DataType.ToLower() == "decimal" || GridItem.DataType.ToLower() == "datetime")
            {
                tb.TextAlignment = TextAlignment.Right;
            }
            tb.LostFocus += (oo, ee) =>
                {
                    try
                    {
                        TextBox tbTemp = oo as TextBox;
                        object obj = tbTemp.DataContext;
                        if (obj != null)
                        {
                            obj.SetObjValue(this.Binding.Path.Path, tbTemp.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                };
            

            IDetails d = dataItem as IDetails;
            if (d != null)
            {
                tb.IsReadOnly = d.ReadOnly;
            }

            return tb;
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            
            var tb = base.GenerateElement(cell, dataItem) as TextBlock;
            tb.TextWrapping = TextWrapping.Wrap;
            if (GridItem.DataType.ToLower() == "decimal" || GridItem.DataType.ToLower() == "datetime")
            {
                tb.TextAlignment = TextAlignment.Right;
            }
            return tb;
        }
    }

    public class DataGridIconColumn : DataGridBoundColumn
    {
        public DataGridIconColumn()
        {
            this.IsReadOnly = true;
            this.CanUserResize = false;
            this.CanUserSort = false;
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            
            IconCellControl tb = null;
            tb = new IconCellControl(cell, true);
            if (this.Binding != null)
            {   
                //tb.RightTextBlock.SetBinding(TextBlock.TextProperty, this.Binding);
                //tb.RightTextBlock.TextWrapping = TextWrapping.Wrap;

                tb.SetBinding(IconCellControl.HiddenProperty, this.Binding);
            }
            
            return tb;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            throw new NotImplementedException();
        }
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            throw new NotImplementedException();
        }
    }

    public class IconCellControl : Grid
    {
        private static void OnHiddenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconCellControl cell = d as IconCellControl;

            cell.OnHiddenChanged();
        }

        public static readonly DependencyProperty HiddenProperty = DependencyProperty.Register("Hidden", typeof(bool), typeof(IconCellControl), new PropertyMetadata(OnHiddenPropertyChanged));
        
        public IconCellControl(DataGridCell cell, bool isImageOnly)
        {
            this.Cell = cell;
            InitControl();
            OnHiddenChanged();
        }

        public bool Hidden
        {
            get
            {
                return (bool)this.GetValue(HiddenProperty);
            }
            set
            {
                this.SetValue(HiddenProperty, value);
            }
        }
        public DataGridCell Cell { get; set; }
        public DataGrid MainGrid { get; set; }
        public Image LeftImage { get; set; }
        public TextBlock RightTextBlock { get; set; }
        private bool IsImageOnly { get; set; }

        private BitmapImage cImage;
        private BitmapImage oImage;
        
        private bool isOpen;
        public void InitControl()
        {
            Cell.MouseLeftButtonUp += new MouseButtonEventHandler(LeftImage_MouseLeftButtonUp);
            cImage = new BitmapImage(new Uri("/SMT.FB.UI;component/Images/Collapsed.png", UriKind.RelativeOrAbsolute));
            oImage = new BitmapImage(new Uri("/SMT.FB.UI;component/Images/Expend.png", UriKind.RelativeOrAbsolute));

            //  this.ColumnDefinigtions = new ColumnDefinitionCollection();
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(16);
            this.ColumnDefinitions.Add(c1);
            LeftImage = new Image();
            LeftImage.SetValue(Grid.ColumnProperty, 0);
            this.Children.Add(LeftImage);
            Collapsed();

            if (!IsImageOnly)
            {
                ColumnDefinition c2 = new ColumnDefinition();
                c2.Width = new GridLength(1, GridUnitType.Star);

                this.ColumnDefinitions.Add(c2);
                RightTextBlock = new TextBlock();
                RightTextBlock.SetValue(Grid.ColumnProperty, 1);
                this.Children.Add(RightTextBlock);

            }
        }

        void LeftImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Hidden)
            {
                return;
            }
            if (isOpen)
            {
                Collapsed();
            }
            else
            {
                Expend();
            }
        }

        public void Collapsed()
        {
            LeftImage.Source = cImage;
            isOpen = false;


            var row = DataGridRow.GetRowContainingElement(Cell);
            if (row != null)
            {
                row.DetailsVisibility = System.Windows.Visibility.Collapsed;
            }


        }

        public void Expend()
        {
            LeftImage.Source = oImage;
            isOpen = true;

            var row = DataGridRow.GetRowContainingElement(Cell);
            if (row != null)
            {
                row.DetailsVisibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnHiddenChanged()
        {
            if (this.Hidden)
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }

}
