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

        private TextBox txtDateTime = new TextBox();
        private ChildWindow windowSelectdateTime = new ChildWindow();

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
                txtDateTime.Text = Value.Value.ToString("yyyy-MM-dd HH:mm");
            }
        }
        #endregion

        public Calendar InnerDatePicker { get; set; }
        public ListTimePickerPopup InnerTimePicker { get; set; }

        public DateTimePicker() : base()
        {
            txtDateTime.MinWidth = 80;
            //txtDateTime.IsReadOnly = true;
            txtDateTime.LostFocus += new RoutedEventHandler(txtDateTime_LostFocus);
            DateTimePickerButton btnSelectDateTime = new DateTimePickerButton();
            string strSource="/SMT.SAAS.Themes;component/Images/CalendarIcon-Blue.png" ;
            btnSelectDateTime.AddButtonAction(strSource, "");

            //btnSelectDateTime.Style= Application.Current.Resources["CommonButtonStyle"] as Style;
            btnSelectDateTime.Click += new RoutedEventHandler(btnSelectDateTime_Click);
            //btnSelectDateTime.AddButtonAction("image/add.png","");
            btnSelectDateTime.MinWidth = 20;
            btnSelectDateTime.MinWidth = 20;
            //btnSelectDateTime.Content = "选择时间";
            btnSelectDateTime.BorderThickness = new Thickness(1);
            //btnSelectDateTime. = "";
            StackPanel pnDateTimeShow = new StackPanel();
            pnDateTimeShow.Orientation = Orientation.Horizontal;
            
            pnDateTimeShow.Children.Add(txtDateTime);
            //pnl.Children.Add(spSpliter);
            pnDateTimeShow.Children.Add(btnSelectDateTime);

            this.Content = pnDateTimeShow;
        }

        void txtDateTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDateTime.Text))
            {
                try
                {
                    DateTime dt = DateTime.Parse(txtDateTime.Text);
                    Value = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入有效的时间");
                }
            }
        
        }

        void btnSelectDateTime_Click(object sender, RoutedEventArgs e)
        {
            StackPanel pnl = new StackPanel();
            pnl.Orientation = Orientation.Horizontal;

            //InnerTimePicker.Width = 50;
            //StackPanel spSpliter = new StackPanel();
            //spSpliter.Width = 5;
            //InnerDatePicker = new SmtDatePicker();
            //InnerDatePicker.Style = Application.Current.Resources["DatePickerStyle"] as Style;
            //InnerDatePicker.Margin = new Thickness(0, 0, 5, 0);
            //InnerDatePicker.MinWidth = 110;
            InnerDatePicker = new Calendar();
            //InnerDatePicker.Style = Application.Current.Resources["DatePickerStyle"] as Style;
            InnerDatePicker.Margin = new Thickness(0, 0, 5, 0);
            InnerDatePicker.SelectedDate = DateTime.Now;
            //InnerDatePicker.MinWidth = 110;

            InnerTimePicker = new ListTimePickerPopup();
            InnerTimePicker.Value = DateTime.Parse("8:30");
            InnerTimePicker.Margin = new Thickness(0, 0, 0, 0);
            InnerTimePicker.MinWidth = 170;
            InnerTimePicker.MinHeight = 155;

            Button btnClose = new Button();
            btnClose.Style = Application.Current.Resources["CommonButtonStyle"] as Style;
            btnClose.BorderThickness =new Thickness(1);
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            btnClose.MinWidth = 30;
            btnClose.Content = "确定";



            pnl.Children.Add(InnerDatePicker);
            //pnl.Children.Add(spSpliter);
            pnl.Children.Add(InnerTimePicker);
            pnl.Children.Add(btnClose);
            //this.Content = pnl;
            EndUpdate();

            windowSelectdateTime.Content = pnl;

            double browserHeight = this.txtDateTime.ActualHeight;
            double browserWidth = this.txtDateTime.ActualWidth;
            windowSelectdateTime.Margin = new Thickness(browserWidth, browserHeight, 0, 0);
            windowSelectdateTime.Show();
            windowSelectdateTime.Loaded += new RoutedEventHandler(windowSelectdateTime_Loaded);
        }

        void windowSelectdateTime_Loaded(object sender, RoutedEventArgs e)
        {
            double height = txtDateTime.ActualHeight;
            double width = txtDateTime.ActualWidth;

            windowSelectdateTime.SetStartLocation(height, width);
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            windowSelectdateTime.Close();
        }      

        public event EventHandler ValueChanged;
        public event EventHandler OnValueChanged;

        void InnerTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            SetDateTime();
        }

        void InnerDatePicker_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDateTime();
        }
        //void InnerDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    SetDateTime();
        //}

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
                //TimePcikerSetting(false);
                Value = null;
            }
            else
            {
                //TimePcikerSetting(true);
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
                //txtDateTime.Text = Value.Value.ToString("yyyy-MM-dd HH:mm");
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
            //InnerDatePicker.SelectedDateChanged -= new EventHandler<SelectionChangedEventArgs>(InnerDatePicker_SelectedDateChanged);
            InnerDatePicker.SelectedDatesChanged -= new EventHandler<SelectionChangedEventArgs>(InnerDatePicker_SelectedDatesChanged);
            InnerTimePicker.ValueChanged -= new RoutedPropertyChangedEventHandler<DateTime?>(InnerTimePicker_ValueChanged);
            this.OnValueChanged -= new EventHandler(InnerDateTimePicker_ValueChanged);
        }

        private void EndUpdate()
        {
            //InnerDatePicker.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(InnerDatePicker_SelectedDateChanged);
            InnerDatePicker.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(InnerDatePicker_SelectedDatesChanged);
            
            InnerTimePicker.ValueChanged += new RoutedPropertyChangedEventHandler<DateTime?>(InnerTimePicker_ValueChanged);
            this.OnValueChanged += new EventHandler(InnerDateTimePicker_ValueChanged);
        }

        //private void TimePcikerSetting(bool isEnable)
        //{
        //    this.InnerTimePicker.IsEnabled = isEnable;

        //    if (!isEnable)
        //    {
        //        this.InnerTimePicker.Value = null; ;
        //    }
        //}
    }
}
