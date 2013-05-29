using System;
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
using SMT.FB.UI.Common;
using System.Reflection;
using SMT.FB.UI.Form;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.Validator;
using SMT.FB.UI.Common.Controls;


namespace SMT.FB.UI
{
    public partial class FieldForm : UserControl, IControlAction
    {

        public FieldForm()
        {
            // 为初始化变量所必需
            InitializeComponent();

            Items = new List<DataFieldItem>();
            GForm = SubFieldForm;
            // ShowSampleData();

        }

        private OperationTypes OperationType { get; set; }

        private bool IsReadOnly { get; set; }
        /// <summary>
        /// bug: 初始化是，应该把之前加载的控件删除
        /// </summary>
        private void InitForm()
        {

            this.GForm.Children.Clear();
            this.GForm.ColumnDefinitions.Clear();
            ColumnDefinition c1 = new ColumnDefinition();
            ColumnDefinition c2 = new ColumnDefinition();
            ColumnDefinition c3 = new ColumnDefinition();

            c1.Width = new GridLength(75);
            c2.Width = new GridLength(8);
            c3.Width = new GridLength(1, GridUnitType.Star);
            this.GForm.ColumnDefinitions.Add(c1);
            this.GForm.ColumnDefinitions.Add(c2);
            this.GForm.ColumnDefinitions.Add(c3);

            RowDefinitions.Clear();

            for (int i = 0; i < Items.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = GetRowHeight(Items[i]);
                RowDefinitions.Add(r);

                TextBlock tb = new TextBlock();
                tb.Text = Items[i].PropertyDisplayName;
                tb.SetValue(Grid.ColumnProperty, 0);
                tb.SetValue(Grid.RowProperty, i);
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.VerticalAlignment = VerticalAlignment.Top;
                tb.Height = 22;
                tb.Name = "tb_" + Items[i].Name;
                if (Items[i].Requited)
                {
                    tb.Text += "*";
                }
                if (!string.IsNullOrEmpty(Items[i].TipText))
                {
                    ToolTip toopTip = new ToolTip() { Content = Items[i].TipText };

                    ToolTipService.SetToolTip(tb, toopTip);
                }
                
                FrameworkElement tx = CreateControl(Items[i]);

                if (Items[i].Requited)
                {
                    tx.SetRequiredValidation(ValidatorManager, Items[i].PropertyDisplayName);
                }
                tx.SetValue(Grid.ColumnProperty, 2);
                tx.SetValue(Grid.RowProperty, i);

                tx.HorizontalAlignment = HorizontalAlignment.Stretch;
                tx.VerticalAlignment = VerticalAlignment.Bottom;
                tx.Name = "tx_" + Items[i].Name;
                tx.VerticalAlignment = VerticalAlignment.Top;

                SubFieldForm.Children.Add(tb);
                SubFieldForm.Children.Add(tx);
            }

        }
        
        private void InitForm(IList<DataFieldItem> items)
        {
            Items = items;
            InitForm();
        }
        public RowDefinitionCollection RowDefinitions
        {
            get
            {
                return this.SubFieldForm.RowDefinitions;
            }
        }
        public IList<DataFieldItem> Items { set; get; }
        public Grid GForm { set; get; }
        public Object GetFieldValue(string name)
        {
            Control c = FindControl(name);
            TextBox tb = c as TextBox;
            return tb.Text;
        }
       
        private FrameworkElement CreateControl(DataFieldItem dfItem)
        {
            ControlType cType = dfItem.CType;
            Binding binding = new Binding();
            binding.Path = new PropertyPath(dfItem.PropertyName);
            binding.Mode = BindingMode.TwoWay;
            binding.Converter = new CommonConvert(dfItem);
            
            FrameworkElement c = null;
            switch (cType)
            {
                case ControlType.Label:
                    TextBlock tbx = new TextBlock();
                    
                    if (dfItem.ReferenceDataInfo != null)
                    {
                        binding.Path = new PropertyPath(dfItem.ReferenceDataInfo.ReferencedMember + ".Text");
                    }
                    tbx.SetBinding(TextBlock.TextProperty, binding);
                    
                    c = tbx;
                    break;
                case ControlType.TextBox:
                    TextBox tb = new TextBox();
                    tb.SetBinding(TextBox.TextProperty, binding);
                    tb.IsReadOnly = (IsReadOnly || dfItem.IsReadOnly);
                    tb.MaxLength = dfItem.MaxLength;
                    if ( dfItem.DataType.ToLower() == "decimal" || dfItem.DataType.ToLower() == "datetime")
                    {
                        tb.TextAlignment = TextAlignment.Right;
                    }
                    c = tb;
                    break;
                case ControlType.Combobox:

                    ComboBox cbb = new ComboBox();

                    if (dfItem.ReferenceDataInfo != null)
                    {
                        cbb.ItemsSource = this.GetItemSource(dfItem.ReferenceDataInfo.Type);
                        cbb.DisplayMemberPath = "Text";
                        binding.Path = new PropertyPath(dfItem.ReferenceDataInfo.ReferencedMember);
                    }

                    cbb.SetBinding(ComboBox.SelectedItemProperty, binding);
                    cbb.IsEnabled = !(IsReadOnly || dfItem.IsReadOnly);
                   
                    c = cbb;
                    
                    break;
                case ControlType.LookUp:
                    FBLookUp lookUp = new FBLookUp();
                    lookUp.OperationType = this.OperationType;
                    lookUp.DisplayMemberPath = dfItem.PropertyName;
                    if (dfItem.ReferenceDataInfo != null)
                    {
                        lookUp.DisplayMemberPath = dfItem.ReferenceDataInfo.ReferencedMember + ".Text";
                        if (!string.IsNullOrEmpty(dfItem.ReferenceDataInfo.TextPath))
                        {
                            lookUp.DisplayMemberPath = dfItem.ReferenceDataInfo.TextPath;
                        }
                        lookUp.LookUpType = dfItem.ReferenceDataInfo.Type;
                        binding.Path = new PropertyPath(dfItem.ReferenceDataInfo.ReferencedMember);
                    }
                    lookUp.SetBinding(LookUp.SelectItemProperty, binding);
                    lookUp.ReferencedDataInfo = dfItem.ReferenceDataInfo;
                    lookUp.Parameters = dfItem.ReferenceDataInfo.Parameters;
                    lookUp.IsReadOnly = (IsReadOnly || dfItem.IsReadOnly);
                    c = lookUp;
                    break;
                case ControlType.Remark:
                    TextBox tbRemark = new TextBox();
                    tbRemark.AcceptsReturn = true;
                    tbRemark.TextWrapping = TextWrapping.Wrap;
                    //tbRemark.Height = 66;
                    //tbRemark.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    //tbRemark.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    tbRemark.SetBinding(TextBox.TextProperty, binding);
                    c = tbRemark;

                    tbRemark.IsReadOnly = (IsReadOnly || dfItem.IsReadOnly);

                    tbRemark.MaxLength = dfItem.MaxLength;
                    
                    break;
                case ControlType.DatePicker:
                    DatePicker datePicker = new DatePicker();
                    datePicker.SelectedDateFormat = DatePickerFormat.Short;
                    datePicker.SetBinding(DatePicker.SelectedDateProperty, binding);                    
                    c = datePicker;
                    datePicker.IsEnabled = !(IsReadOnly || dfItem.IsReadOnly);
                    break;
                case ControlType.DateTimePicker:
                    DateTimePicker dateTimePicker = new DateTimePicker();
                    dateTimePicker.SetBinding(DateTimePicker.ValueProperty, binding);
                    c = dateTimePicker;
                    dateTimePicker.IsEnabled = !(IsReadOnly || dfItem.IsReadOnly);
                    break;
                // Add By LVCHAO 2011.01.30 14:46
                case ControlType.HyperlinkButton:
                    HyperlinkButton hb = new HyperlinkButton();
                    if (dfItem.ReferenceDataInfo != null)
                    {
                        binding.Path = new PropertyPath(dfItem.ReferenceDataInfo.ReferencedMember + ".Text");
                    }
                    hb.Click += (o, e) =>
                        {
                            var sourceOjb = hb.DataContext.GetObjValue(dfItem.ReferenceDataInfo.ReferencedMember + ".Value");
                            CommonFunction.ShowExtendForm(sourceOjb);
                        };
                    hb.SetBinding(HyperlinkButton.ContentProperty, binding);
                    c = hb;
                    break;
            }
            return c;

        }

        private IList<ITextValueItem> GetItemSource(string referencedDataType)
        {
            if (string.IsNullOrEmpty(referencedDataType))
            {
                return null;
            }
            Type t = CommonFunction.GetType(referencedDataType, CommonFunction.TypeCategory.ReferencedData);
            Type type = typeof(ReferencedData<>).MakeGenericType(t);
            MethodInfo myMethod = typeof(DataCore).GetMethod("GetReferencedData");
            IList<ITextValueItem> list = (IList<ITextValueItem>)myMethod.MakeGenericMethod(t).Invoke(null, null);
            return list;

        }

        private GridLength GetRowHeight(DataFieldItem dfItem)
        {
            ControlType cType = dfItem.CType;

            switch (cType)
            {
                case ControlType.Label:
                case ControlType.TextBox:
                case ControlType.Combobox:
                case ControlType.LookUp:
                    return new GridLength(30);
                case ControlType.Remark:
                    return new GridLength(30, GridUnitType.Auto);
                default:
                    return new GridLength(30);
            }
        }

        private void ShowSampleData()
        {
            List<string[]> FInfo = new List<string[]>();
            FInfo.Add(new string[] { "字段1", "OrderCode", "OrderCode", "1" });
            FInfo.Add(new string[] { "字段2", "OrderStates", "OrderStates", "1" });
            FInfo.Add(new string[] { "字段3", "CreateUser", "CreateUser", "1" });
            FInfo.Add(new string[] { "字段4", "CreateDate", "CreateDate", "1" });

            for (int i = 0; i < FInfo.Count; i++)
            {
                DataFieldItem df = new DataFieldItem();
                df.PropertyDisplayName = FInfo[i][0];
                df.Name = FInfo[i][1];
                df.PropertyName = FInfo[i][2];
                df.CType = (ControlType)(int.Parse(FInfo[i][3]));
                Items.Add(df);
            }
            this.InitForm();
        }

        #region IControlAction 成员

        public void InitControl(OperationTypes operationType)
        {
            OperationType = operationType;
            IsReadOnly = !(OperationType == OperationTypes.Add || OperationType == OperationTypes.Edit);
            InitForm();
        }

        public Control FindControl(string name)
        {
            Object fieldObject = GForm.FindName("tx_" + name);
            return fieldObject as Control;
        }

        #endregion

        #region IControlAction 成员


        public ValidatorManager ValidatorManager
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


}