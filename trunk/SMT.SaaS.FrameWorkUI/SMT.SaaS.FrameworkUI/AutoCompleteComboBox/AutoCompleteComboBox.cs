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
using System.Windows.Controls.Primitives;

namespace SMT.SaaS.FrameworkUI.AutoCompleteComboBox
{
    //ToggleButton和TextBox两个控件组成
    [TemplatePart(Name = AutoCompleteComboBox.ElementToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = AutoCompleteComboBox.ElementTextBox, Type = typeof(TextBox))]
    public class AutoCompleteComboBox : AutoCompleteBox
    {
        private const string ElementToggleButton = "ToggleButton";
        private const string ElementTextBox = "Text";//找Themes下样式里面相应名称的控件
         ToggleButton _ToggleButton;
         TextBox _TextBox;
         public TextBox TxtLookUp
         {
             get { return this._TextBox; }
             set { _TextBox = value; }
         }

         public ToggleButton ToggleButton
        {
            get { return _ToggleButton; }
            set
            {
                if (_ToggleButton != null)
                {
                    _ToggleButton.Click -= new RoutedEventHandler(ToggleButton_Click);
                    _ToggleButton.MouseEnter -= new MouseEventHandler(ToggleButton_MouseEnter);
                    _ToggleButton.MouseLeave -= new MouseEventHandler(ToggleButton_MouseLeave);
                }

                _ToggleButton = value;

                if (_ToggleButton != null)
                {
                    _ToggleButton.Click += new RoutedEventHandler(ToggleButton_Click);
                    _ToggleButton.MouseEnter += new MouseEventHandler(ToggleButton_MouseEnter);
                    _ToggleButton.MouseLeave += new MouseEventHandler(ToggleButton_MouseLeave);
                }
            }
        }

        public AutoCompleteComboBox()
        {
            this.DefaultStyleKey = typeof(AutoCompleteComboBox);
            Loaded += new RoutedEventHandler(AutoCompleteComboBox_Loaded);
            this.DropDownClosed += new RoutedPropertyChangedEventHandler<bool>(AutoCompleteComboBox_DropDownClosed);
            this.DropDownOpened += new RoutedPropertyChangedEventHandler<bool>(AutoCompleteComboBox_DropDownOpened);
        }
        /// <summary>
        /// 打开下拉的时候让搜索模式为Contains
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AutoCompleteComboBox_DropDownOpened(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            this.FilterMode = AutoCompleteFilterMode.Contains;
        }

        /// <summary>
        /// 关闭下拉的时候进行判断，如果没有选择（既不合法的字典）时，把文本框清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AutoCompleteComboBox_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (this.SelectedItem == null && this.SearchText == string.Empty)
            {
                this.Text = string.Empty;
            }
        }

        void AutoCompleteComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ToggleButton = GetTemplateChild(ElementToggleButton) as ToggleButton;
            TxtLookUp = GetTemplateChild(ElementTextBox) as TextBox;
        }

        void ToggleButton_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "ToggleButtonOver", true);
        }

        void ToggleButton_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "ToggleButtonOut", true);
        }

        /// <summary>
        /// 下拉的时候显示*符号，然后加载所有字典（没有办法的措施，有好的办法再说）
        /// 然后模式由默认的StartsWith改为Contains
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.Text) || this.Text == "*")
            {
                this.Text = "*";
                this.FilterMode = AutoCompleteFilterMode.None;
            }
            else
            {
                this.FilterMode = AutoCompleteFilterMode.Contains;
            }
            this.IsDropDownOpen = !this.IsDropDownOpen;
        }
    }
}
