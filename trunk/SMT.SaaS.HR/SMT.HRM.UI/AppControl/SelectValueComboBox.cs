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

using SMT.Saas.Tools.PersonnelWS;

namespace SMT.HRM.UI.AppControl
{
    public class SelectValueComboBox : ComboBox
    {
        public DependencyProperty SelectedValueProperty;
        public string SelectedValue
        {
            get
            {
                return GetValue(SelectedValueProperty) as string;

            }
            set
            {
                SetValue(SelectedValueProperty, value);

            }
        }
        public SelectValueComboBox()
        {
            SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(string), typeof(SelectValueComboBox)
              , new PropertyMetadata("", new PropertyChangedCallback(SelectValueComboBox.OnSelectedValuePropertyChanged)));

        }
        public static void OnSelectedValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SelectValueComboBox obj = sender as SelectValueComboBox;
            if (obj != null)
            {
                obj.OnSelectedValuePropertyChanged(e);
            }
        }

        private void OnSelectedValuePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            SetSelectedItem(e.NewValue == null ? "" : e.NewValue.ToString());
        }

        private void SetSelectedItem(string value)
        {
            foreach (var item in Items)
            {
                T_HR_CHECKPOINTLEVELSET obj = item as T_HR_CHECKPOINTLEVELSET;
                if (obj != null)
                {
                    if (obj.POINTSCORE.ToString() == value)
                    {
                        SelectedItem = item;
                        break;
                    }
                }
            }
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            T_HR_CHECKPOINTLEVELSET dict = this.SelectedItem as T_HR_CHECKPOINTLEVELSET;
            if (dict != null)
            {
                SelectedValue = dict.POINTSCORE.ToString();
            }

        }
    }
}
