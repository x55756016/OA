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
using System.Windows.Data;
using System.Collections;
using System.Collections.Generic;
using SMT.Saas.Tools.PermissionWS;
using System.Linq;

namespace SMT.SaaS.FrameworkUI.FBControls
{
    public class FBDataGrid : DataGrid
    {
        public FBDataGrid()
            : base()
        {

            GridItems = new List<FBGridItem>();
            writtableStyle = new Style();
            SolidColorBrush sbEdit = (SolidColorBrush)Application.Current.Resources["Datagridbg_edit"];
            writtableStyle.Setters.Add(new Setter() { Property = BackgroundProperty, Value = sbEdit });

            writtableStyle.TargetType = typeof(DataGridCell);
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            this.AutoGenerateColumns = false;

            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        void AutoDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        public IList<FBGridItem> GridItems { get; set; }

        public Style writtableStyle;

        public void InitControl()
        {
            this.Style = (Style)Application.Current.Resources["DataGridStyle"];
            this.RowStyle = (Style)Application.Current.Resources["DataGridRowStyle"];
            this.ColumnHeaderStyle = (Style)Application.Current.Resources["DataGridColumnHeaderStyle"];
            for (int i = 0; i < GridItems.Count; i++)
            {
                DataGridColumn dgtc = GetDataGridColumn(GridItems[i]);
                dgtc.Header = GridItems[i].PropertyDisplayName;
                dgtc.Width = new DataGridLength(GridItems[i].Width);
                dgtc.IsReadOnly = GridItems[i].IsReadOnly;
                this.Columns.Add(dgtc);
            }
        }
        public void InitControl(DataGridColumn attachColumn)
        {
            this.InitControl();
            this.Columns.Add(attachColumn);
        }

        private DataGridColumn GetDataGridColumn(FBGridItem gItem)
        {
            Binding bding = new Binding();
            if (!string.IsNullOrEmpty(gItem.PropertyName))
            {
                bding.Mode = BindingMode.TwoWay;
                bding.Path = new PropertyPath(gItem.PropertyName);
                bding.Converter = new FBGridValueConverter(gItem);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("");
            }

            DataGridColumn dgc = null;
            switch (gItem.CType)
            {
                case FBControlType.CheckBox:
                    DataGridCheckBoxColumn dgcc = new DataGridCheckBoxColumn();
                    dgcc.IsThreeState = false;
                    dgcc.Binding = bding;
                    dgc = dgcc;
                    break;
                case FBControlType.Label:
                    DataGridTextColumn dgtc = new DataGridTextColumn();
                    dgtc.Binding = bding;

                    if (!gItem.IsReadOnly)
                    {
                        dgtc.CellStyle = writtableStyle;
                    }
                    dgc = dgtc;
                    break;
                case FBControlType.Combobox:

                    DataGridComboBoxColumn dgcbc = new DataGridComboBoxColumn();
                    //if (gItem.ReferenceDataInfo != null)
                    //{
                    //    IList<ITextValueItem> list = DataCore.GetRefData(gItem.ReferenceDataInfo.Type);
                    //    dgcbc.ItemsSource = list;
                    //}
                    dgcbc.DisplayMemberPath = "Text";
                    dgcbc.Binding = bding;

                    dgc = dgcbc;
                    break;
            }

            return dgc;
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

        #region 其他类
        public class FBGridItem
        {
            public string Name { get; set; }
            private string typeName = null;
            public string Type
            {
                get
                {
                    return typeName;
                }
                set
                {
                    typeName = value;
                    if (string.IsNullOrEmpty(Name))
                    {
                        Name = typeName;
                    }
                }
            }

            public string PropertyName { get; set; }
            public string PropertyDisplayName { set; get; }
            public FBControlType CType { set; get; }
            public bool IsReadOnly { set; get; }
            public float Width { set; get; }
            public int MaxLength { get; set; }

            public string _ReferenceType = string.Empty;
            public string ReferenceType
            {
                get
                {
                    return _ReferenceType;
                }
                set
                {
                    if (!object.Equals(_ReferenceType, value))
                    {
                        _ReferenceType = value;
                        List<T_SYS_DICTIONARY> listDict = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

                        var result = listDict.Where(item =>
                        {
                            return item.DICTIONCATEGORY == _ReferenceType;
                        });
                        ReferenceTypes = result.ToList();
                    }
                }
            }

            public List<T_SYS_DICTIONARY> ReferenceTypes { get; set; }


        }

        public class FBGridValueConverter : IValueConverter
        {
            public FBGridValueConverter(FBGridItem item)
            {
                this.FBGridItem = item;
            }

            public FBGridItem FBGridItem { get; set; }
            #region IValueConverter 成员

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (FBGridItem.ReferenceTypes != null)
                {
                    var findResult = FBGridItem.ReferenceTypes.FirstOrDefault(item => item.DICTIONARYVALUE == System.Convert.ToDecimal(value));
                    return findResult.DICTIONARYNAME;
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }

            #endregion
        }

        public enum FBControlType
        {
            Label, TextBox, Combobox = 2, LookUp, Remark, DatePicker = 5, CheckBox
        }

        public class DataGridComboBoxColumn : DataGridBoundColumn
        {

            public IEnumerable ItemsSource { get; set; }

            public string DisplayMemberPath { get; set; }
            public DataGrid DataGrid { get; set; }

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
            public MyDataGridTextColumn(FBGridItem gridItem)
            {
                this.GridItem = gridItem;
            }
            public FBGridItem GridItem { get; set; }
            protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
            {

                TextBox tb = (TextBox)base.GenerateEditingElement(cell, dataItem);
                tb.MaxLength = GridItem.MaxLength;
                return tb;
            }
        }


        
        #endregion
    }
    public class ActionCompletedEventArgs<T> : EventArgs
    {
        private T result;
        public T Result
        {
            get
            {
                return result;
            }
        }
        public ActionCompletedEventArgs(T t)
        {
            this.result = t;
        }
    }

}
