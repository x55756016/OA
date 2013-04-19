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
using System.Threading;
namespace SMT.SaaS.FrameworkUI.Validator
{
    public enum ValidationType
    {
        Validator,
        OnDemand
    }


    public abstract class ValidatorBase : DependencyObject
    {
        public ValidatorManager Manager { get; set; }


        public string ManagerName { get; set; }
        public ValidationType ValidationType { get; set; }
        public IIndicator Indicator { get; set; }
        public FrameworkElement ElementToValidate { get; set; }
        public bool IsRequired { get; set; }
        public bool IsValid { get; set; }
        public Brush InvalidBackground { get; set; }
        public Brush InvalidBorder { get; set; }
        public Thickness InvalidBorderThickness { get; set; }


        private Brush OrigBackground = null;
        private Brush OrigBorder = null;
        private Thickness OrigBorderThickness = new Thickness(1);
        private object OrigTooltip = null;

        public ValidatorBase()
        {
            IsRequired = false;
            IsValid = true;
            ManagerName = "";
            this.ValidationType = ValidationType.Validator;


        }
        public DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ValidatorBase), new PropertyMetadata("Error"));
        public DependencyProperty ErrorMessageParameterProperty = DependencyProperty.Register("ErrorMessageParameter", typeof(string), typeof(ValidatorBase), new PropertyMetadata(""));

        /// <summary>
        /// 以,分格的字符串
        /// </summary>
        public string ErrorMessageParameter
        {
            get
            {
                return GetValue(ErrorMessageParameterProperty) as string;
            }
            set
            {
                SetValue(ErrorMessageParameterProperty, value);
                SetErrorMessage(ErrorMessage, value);
            }
        }
        private void SetErrorMessage(string msg, string para)
        {
            string localvalue = ValidatorService.ResourceMgr.GetString(msg, Thread.CurrentThread.CurrentUICulture);
            localvalue = string.IsNullOrEmpty(localvalue) ? msg : localvalue;

            string rslt = localvalue;
            if (!string.IsNullOrEmpty(para))
            {
                string[] paras = para.ToString().Split(',');
                int i = 0;

                foreach (string str in paras)
                {
                    string tmp = ValidatorService.ResourceMgr.GetString(str, Thread.CurrentThread.CurrentUICulture);
                    //SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(str, Localization.UiCulture);
                    paras[i] = string.IsNullOrEmpty(tmp) ? str : tmp;
                    i++;
                }
                rslt = string.Format(localvalue, paras);
            }
            SetValue(ErrorMessageProperty, string.IsNullOrEmpty(rslt) ? msg : rslt);

        }
        public string ErrorMessage
        {
            get
            {
                return GetValue(ErrorMessageProperty) as string;
            }
            set
            {
                SetErrorMessage(value, ErrorMessageParameter);
            }
        }

        public void Initialize(FrameworkElement element)
        {
            ElementToValidate = element;
            element.Loaded += new RoutedEventHandler(element_Loaded);
        }

        private bool loaded = false;

        //public UserControl UserControl { get; set; }
        //Modified reo,may validate for childwindow,page , floatwindow or other controls
        public Control UserControl { get; set; }

        private void element_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                //this.UserControl = FindUserControl(ElementToValidate);
                this.UserControl = FindParentControl<Control>(ElementToValidate);
                //this.UserControl = FindParentControl<Control>(ElementToValidate, "LayoutRoot");

                //no usercontrol.  throw error?
                if (this.UserControl == null) return;

                if (this.Manager == null)
                {
                    this.Manager = FindManager(this.UserControl, ManagerName);
                }
                if (this.Manager == null)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("No ValidatorManager found named '{0}'", ManagerName));
                    throw new Exception(String.Format("No ValidatorManager found named '{0}'", ManagerName));
                }
                this.Manager.Register(ElementToValidate, this);

                if (ValidationType == ValidationType.Validator)
                {
                    ActivateValidationRoutine();
                }

                //Use the properties from the manager if they are not set at the control level
                if (this.InvalidBackground == null)
                {
                    this.InvalidBackground = this.Manager.InvalidBackground;
                }

                if (this.InvalidBorder == null)
                {
                    this.InvalidBorder = this.Manager.InvalidBorder;

                    if (InvalidBorderThickness.Bottom == 0)
                    {
                        this.InvalidBorderThickness = this.Manager.InvalidBorderThickness;
                    }
                }

                if (this.Indicator == null)
                {
                    Type x = this.Manager.Indicator.GetType();
                    this.Indicator = x.GetConstructor(System.Type.EmptyTypes).Invoke(null) as IIndicator;
                    foreach (var param in x.GetProperties())
                    {
                        var val = param.GetValue(this.Manager.Indicator, null);
                        if (param.CanWrite && val != null && val.GetType().IsPrimitive)
                        {
                            param.SetValue(this.Indicator, val, null);
                        }
                    }
                }
                loaded = true;
            }
            ElementToValidate.Loaded -= new RoutedEventHandler(element_Loaded);
        }

        public void SetManagerAndControl(ValidatorManager manager, FrameworkElement element)
        {
            this.Manager = manager;
            this.ElementToValidate = element;
        }

        public bool Validate(bool checkControl)
        {
            bool newIsValid;
            if (checkControl)
            {
                newIsValid = ValidateControl() && ValidateRequired();
            }
            else
            {
                newIsValid = ValidateRequired();
            }

            if (newIsValid && !IsValid)
            {
                ControlValid();
            }
            if (!newIsValid && IsValid)
            {
                ControlNotValid();
            }
            IsValid = newIsValid;
            return IsValid;
        }

        public virtual void ActivateValidationRoutine()
        {
            ElementToValidate.LostFocus += new RoutedEventHandler(ElementToValidate_LostFocus);
            ElementToValidate.KeyUp += new KeyEventHandler(ElementToValidate_KeyUp);
        }

        /// <summary>
        /// Find the nearest UserControl up the control tree for the FrameworkElement passed in
        /// </summary>
        /// <param name="element">Control to validate</param>
        //modified by reo 
        // protected static UserControl FindUserControl(FrameworkElement element)
        protected static Control FindUserControl(FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }
            if (element.Parent != null)
            {
                if (element.Parent is UserControl || element.Parent is ChildWindow || element.Parent is System.Windows.Controls.Window)
                {
                    return element.Parent as Control;
                }
                return FindUserControl(element.Parent as FrameworkElement);
            }
            return null;
        }

        protected static T FindParentControl<T>(DependencyObject item) where T : class
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                T parentGrid = parent as T;
                return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent);
            }
            return null;
        }

        protected static T FindParentControl<T>(DependencyObject item,string ctrName) where T : class
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                T parentGrid = parent as T;
                Control ctr = parent as Control;
                
                if (ctr != null)
                {
                    if (ctr.Name == ctrName)
                    {
                        return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent, ctrName);
                    }
                    else
                    {
                        return FindParentControl<T>(parent, ctrName);
                    }
                }
                //return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent, ctrName);
            }
            return null;
        }

        protected virtual void ElementToValidate_KeyUp(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate() { Validate(false); });
        }

        protected virtual void ElementToValidate_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate() { Validate(true); });
        }

        protected abstract bool ValidateControl();

        protected bool ValidateRequired()
        {
            if (IsRequired && ElementToValidate is TextBox)
            {
                TextBox box = ElementToValidate as TextBox;
                return !String.IsNullOrEmpty(box.Text);
            }
            else if (IsRequired && ElementToValidate is TextBlock)
            {
                TextBlock tb = ElementToValidate as TextBlock;
                return !String.IsNullOrEmpty(tb.Text);
            }
            else if (IsRequired && ElementToValidate is LookUp)
            {
                LookUp lookUp = ElementToValidate as LookUp;
                return !(lookUp.SelectItem == default(object));
            }
            else if (IsRequired && ElementToValidate is ComboBox)
            {
                ComboBox cbb = ElementToValidate as ComboBox;
                return !(cbb.SelectedItem == default(object));
            }
            return true;
        }

        protected void ControlNotValid()
        {
            GoToInvalidStyle();
        }

        protected void ControlValid()
        {
            GoToValidStyle();
        }

        protected virtual void GoToInvalidStyle()
        {
            if (!string.IsNullOrEmpty(this.ErrorMessage))
            {
                object tooltip = ToolTipService.GetToolTip(ElementToValidate);

                if (tooltip != null)
                {
                    OrigTooltip = tooltip;
                }

                //causing a onownermouseleave error currently...
                this.ElementToValidate.ClearValue(ToolTipService.ToolTipProperty);

                SetToolTip(this.ElementToValidate, this.ErrorMessage);
            }

            if (Indicator != null)
            {
                Indicator.ShowIndicator(this);
            }
            SetInvalidStyle();
           
        }

        protected virtual void GoToValidStyle()
        {
            if (!string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ElementToValidate.ClearValue(ToolTipService.ToolTipProperty);

                if (this.OrigTooltip != null)
                {
                    SetToolTip(this.ElementToValidate, this.OrigTooltip);
                }
            }

            if (Indicator != null)
            {
                Indicator.HideIndicator();
            }

            SetValidStyle();
        }

        protected virtual void SetInvalidStyle()
        {
            Control box = ElementToValidate as Control;
            if (InvalidBackground != null)
            {
                if (OrigBackground == null)
                {
                    OrigBackground = box.Background;
                }
                box.Background = InvalidBackground;
            }

            if (InvalidBorder != null)
            {
                if (OrigBorder == null)
                {
                    OrigBorder = box.BorderBrush;
                    OrigBorderThickness = box.BorderThickness;
                }
                box.BorderBrush = InvalidBorder;

                if (InvalidBorderThickness != null)
                {
                    box.BorderThickness = InvalidBorderThickness;
                }
            }
        }

        protected virtual void SetValidStyle()
        {
            Control box = ElementToValidate as Control;
            if (OrigBackground != null)
            {
                box.Background = OrigBackground;
            }

            if (OrigBorder != null)
            {
                box.BorderBrush = OrigBorder;

                if (OrigBorderThickness != null)
                {
                    box.BorderThickness = OrigBorderThickness;
                }
            }
        }

        protected void SetToolTip(FrameworkElement element, object tooltip)
        {
            Dispatcher.BeginInvoke(() =>
                        ToolTipService.SetToolTip(element, tooltip));
        }

        private ValidatorManager FindManager(Control c, string groupName)
        {
            string defaultName = "_DefaultValidatorManager";
            var mgr = this.UserControl.FindName(ManagerName);
            if (mgr == null)
            {
                mgr = this.UserControl.FindName(defaultName);
            }
            if (mgr == null)
            {
                mgr = new ValidatorManager()
                {
                    Name = defaultName
                };
                Panel g = c.FindName("LayoutRoot") as Panel;
                g.Children.Add(mgr as ValidatorManager);
            }
            return mgr as ValidatorManager;
        }

    }
}