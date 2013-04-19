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

namespace SMT.SaaS.FrameworkUI
{
    public class DateTimePicker : UserControl
    {

        public static void ValueChaning(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker dp = d as DateTimePicker;
           
            if (dp.ValueChanged != null)
            {
                dp.ValueChanged(dp, null);
            }

            if (dp.OnValueChanged != null)
            {
                dp.OnValueChanged(dp, null);
            }
            
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DateTime?), typeof(DateTimePicker), new PropertyMetadata(ValueChaning));

        #region
        public DateTime? Value
        {
            get
            {
                if (base.GetValue(ValueProperty) == null)
                {
                    return null;
                }

                return (DateTime)base.GetValue(ValueProperty);
            }
            set
            {
                base.SetValue(ValueProperty, value);
            }
        }
        #endregion

        public SmtDatePicker InnerDatePicker {get;set;}
        public TimePicker InnerTimePicker {get;set;}

        public DateTimePicker() : base()
        {
            StackPanel pnl = new StackPanel();
            InnerDatePicker  = new SmtDatePicker();
            InnerTimePicker  = new TimePicker();
            //InnerTimePicker.Width = 50;
            //StackPanel spSpliter = new StackPanel();
            //spSpliter.Width = 5;
            pnl.Orientation = Orientation.Horizontal;

            InnerDatePicker.Style = Application.Current.Resources["DatePickerStyle"] as Style;
            InnerDatePicker.Margin = new Thickness(0, 0, 5, 0);
            InnerTimePicker.Style = Application.Current.Resources["TimePickerStyle"] as Style;
            //InnerDatePicker.MaxWidth = 300;
            InnerDatePicker.MinWidth = 110;
            //InnerTimePicker.MinWidth = 270;
            //InnerTimePicker.MaxWidth = 110;
            //InnerTimePicker.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            //InnerTimePicker.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            
            //InnerTimePicker.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            //InnerTimePicker.UseLayoutRounding = true;
            
            pnl.Children.Add(InnerDatePicker);
            //pnl.Children.Add(spSpliter);
            pnl.Children.Add(InnerTimePicker);
            TimePcikerSetting(false);
            this.Content = pnl;

            EndUpdate();
        }      

        public event EventHandler ValueChanged;
        public event EventHandler OnValueChanged;

        void InnerTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            SetDateTime();
        }

        void InnerDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDateTime();
        }

        void InnerDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            this.InitDateTime(this.Value);
        }

        private void SetDateTime()
        {
            BeginUpdate();
            DateTime? dateS = InnerDatePicker.SelectedDate;
            DateTime? timeS = InnerTimePicker.Value;

            if (dateS == null )
            {
                InnerTimePicker.Value = null;
                TimePcikerSetting(false);
                Value = null;
            }
            else
            {
                TimePcikerSetting(true);
                DateTime currentValue = DateTime.Now.Date;

                if (dateS != null)
                {
                    currentValue = new DateTime(dateS.Value.Date.Ticks);
                }
                if (timeS != null)
                {
                    currentValue = currentValue.AddHours(timeS.Value.Hour);
                    currentValue = currentValue.AddMinutes(timeS.Value.Minute);
                }
                Value = currentValue;
            }
            EndUpdate();
        }

        private void InitDateTime(DateTime? initValue)
        {
            BeginUpdate();
            if (initValue != null)
            {
                this.InnerDatePicker.SelectedDate = initValue.Value;
                this.InnerTimePicker.Value = initValue;
            }
            EndUpdate();
        }

        private void BeginUpdate()
        {
            InnerDatePicker.SelectedDateChanged -= new EventHandler<SelectionChangedEventArgs>(InnerDatePicker_SelectedDateChanged);
            InnerTimePicker.ValueChanged -= new RoutedPropertyChangedEventHandler<DateTime?>(InnerTimePicker_ValueChanged);
            this.OnValueChanged -= new EventHandler(InnerDateTimePicker_ValueChanged);
        }
        private void EndUpdate()
        {
            InnerDatePicker.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(InnerDatePicker_SelectedDateChanged);
            InnerTimePicker.ValueChanged += new RoutedPropertyChangedEventHandler<DateTime?>(InnerTimePicker_ValueChanged);
            this.OnValueChanged += new EventHandler(InnerDateTimePicker_ValueChanged);
        }

        private void TimePcikerSetting(bool isEnable)
        {
            this.InnerTimePicker.IsEnabled = isEnable;

            if (!isEnable)
            {
                this.InnerTimePicker.Value = null; ;
            }
        }
    }
}
