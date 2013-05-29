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
using System.Collections;
using System.Linq;
namespace SMT.FB.UI.Common.Controls
{
    /// <summary>
    /// 扩展ComboBox,解决当ItemsSource里不存在SelectedItem的值 时，显示的问题
    /// 例如在月度预算中，选择项只有当前月和下一个月的选项，但如果要查看当前以前的单据时，数据显示不出来。
    /// </summary>
    public partial class FBComboBox : ComboBox
    {
        public FBComboBox()
            : base()
        {
            base.SelectionChanged += new SelectionChangedEventHandler(MyComboBox_SelectionChanged);
        }

        void MyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbbItem = base.SelectedItem as ComboBoxItem;
            if (cbbItem != null)
            {
                this.SelectedItem = cbbItem.DataContext;
            }
        }

        private IEnumerable _ItemsSource = null;
        public new IEnumerable ItemsSource
        {
            get
            {
                return _ItemsSource;
            }
            set
            {
                if (object.Equals(_ItemsSource, value))
                {
                    return;
                }
                _ItemsSource = value;
                InitItems();

            }
        }


        private void InitItems()
        {
            this.Items.Clear();
            foreach (var item in _ItemsSource)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.DataContext = item;
                this.Items.Add(cbi);
            }
            RefreshDisplayMemberName();
        }

        public new string DisplayMemberPath
        {
            get
            {
                return base.DisplayMemberPath;
            }
            set
            {
                if (object.Equals(base.DisplayMemberPath, value))
                {
                    return;
                }

                base.DisplayMemberPath = value;
                RefreshDisplayMemberName();
            }
        }

        private void RefreshDisplayMemberName()
        {
            if (string.IsNullOrWhiteSpace(this.DisplayMemberPath))
            {
                return;
            }

            this.Items.ForEach(item =>
            {
                ComboBoxItem cbbItem = item as ComboBoxItem;
                System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                binding.Path = new PropertyPath(this.DisplayMemberPath);
                cbbItem.SetBinding(ComboBoxItem.ContentProperty, binding);
            });
        }

        private static void OnSelectedItemXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FBComboBox cbb = d as FBComboBox;

            cbb.SetSelectedItem();
        }
        public static readonly DependencyProperty SelectedItemXProperty = DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(FBComboBox), new PropertyMetadata(OnSelectedItemXChanged));


        public new object SelectedItem
        {
            get
            {
                return base.GetValue(SelectedItemXProperty);
            }
            set
            {
                base.SetValue(SelectedItemXProperty, value);
            }
        }

        private void SetSelectedItem()
        {
            var selectedcbbItem = this.Items.FirstOrDefault(item =>
            {
                ComboBoxItem cbbItem = item as ComboBoxItem;
                return cbbItem.DataContext == this.SelectedItem;
            });

            if (selectedcbbItem == null)
            {
                ComboBoxItem cbbItemHidden = new ComboBoxItem();
                cbbItemHidden.Visibility = System.Windows.Visibility.Collapsed;

                System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                binding.Path = new PropertyPath(this.DisplayMemberPath);
                cbbItemHidden.SetBinding(ComboBoxItem.ContentProperty, binding);
                cbbItemHidden.DataContext = this.SelectedItem;

                this.Items.Add(cbbItemHidden);
                selectedcbbItem = cbbItemHidden;
            }

            base.SelectedItem = selectedcbbItem;
        }
    }
}
