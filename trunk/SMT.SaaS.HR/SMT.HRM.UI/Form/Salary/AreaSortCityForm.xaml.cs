using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Media;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class AreaSortCityForm : System.Windows.Controls.Window
    {

        private const int wpheight = 20;
        private string strCode = string.Empty;
        public Dictionary<string,string> Result { get; set; }
        public event EventHandler SelectedClicked;
        public event EventHandler Close;


        public AreaSortCityForm()
        {
            InitializeComponent();
            LoadUIData();
            Result = new Dictionary<string, string>();
        }

        private void LoadUIData()
        {
            var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                       where a.DICTIONCATEGORY == "PROVINCE"
                       select new
                       {
                           DICTIONARYID = a.DICTIONARYID,
                           DICTIONARYNAME = a.DICTIONARYNAME,
                           DICTIONARYVALUE = a.DICTIONARYVALUE
                       };
            if (ents.Count() > 0)
            {
                wpProvince.Children.Clear();
                //wpProvince.Height = wpheight;
                foreach (var ent in ents)
                {
                    TextBlock tb = new TextBlock();
                   // tb.Foreground = new SolidColorBrush(Colors.Black);
                    tb.Cursor = Cursors.Hand;
                    tb.Text = ent.DICTIONARYNAME;
                    tb.Tag = ent.DICTIONARYID;
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown);
                    tb.Margin = new Thickness(3);
                    //tb.Height = 80;
                    tb.Width = 80;
                    wpProvince.Children.Add(tb);
                }
            }
        }

        void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string temp = string.Empty;
            TextBlock tbtemp = sender as TextBlock;
           // tbtemp.Foreground = new SolidColorBrush(Colors.Red);
            wpCity.Children.Clear();
            //wpCity.Height = wpheight;
            if (tbtemp.Tag != null)
            {
                temp = tbtemp.Tag.ToString();
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.T_SYS_DICTIONARY2 != null && a.T_SYS_DICTIONARY2.DICTIONARYID == temp
                           select new
                           {
                               DICTIONARYID = a.DICTIONARYID,
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };

                if (ents.Count() > 0)
                {
                    foreach (var ent in ents)
                    {
                        CheckBox cbx = new CheckBox();
                        cbx.Style = Application.Current.Resources["CheckBoxStyle"] as Style;
                        if (!string.IsNullOrEmpty(display.Text) && display.Text.IndexOf(ent.DICTIONARYNAME) != -1)
                            cbx.IsChecked = true;
                        else
                            cbx.IsChecked = false;
                        cbx.Content = ent.DICTIONARYNAME;
                        cbx.Tag = ent.DICTIONARYVALUE;
                        cbx.Click += new RoutedEventHandler(cbx_Checked);
                        cbx.Margin = new Thickness(2);
                        //cb.Height = 80;
                        cbx.Width = 80;
                        wpCity.Children.Add(cbx);
                    }
                }
            }
            else return;
        }

        void cbx_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                if (cb.IsChecked != true)
                {
                    display.Text = display.Text.Replace(cb.Content.ToString(), "").Trim()+" ";
                    strCode = strCode.Replace(cb.Tag.ToString() + ",", "");
                }
                else
                {
                    if (display.Text.IndexOf(cb.Content.ToString()) == -1)
                    {
                        display.Text += cb.Content.ToString() + " ";
                        strCode += cb.Tag.ToString()+",";
                    }
                }
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            Result.Add(display.Text.ToString(),strCode);
            if (SelectedClicked != null)
            {
                SelectedClicked(this, e);
            }
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            display.Text = strCode = string.Empty;
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }
    }
}
