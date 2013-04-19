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

using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI.SelectCityControl
{
    public partial class AreaSortCity : System.Windows.Controls.Window
    {
        private const int wpheight = 20;
        private string strCode = string.Empty;
        public Dictionary<string, string> Result { get; set; }
        public event EventHandler SelectedClicked;
        public event EventHandler Close;
        private bool _selectmulti = false;

        public bool SelectMulti 
        { 
            get{ return this._selectmulti;}
            set{this._selectmulti = value;} 
        }

        public AreaSortCity()
        {
            InitializeComponent();
            LoadUIData();
            Result = new Dictionary<string, string>();
            
        }

        public AreaSortCity(string existCitys)
        {
            InitializeComponent();
            if (existCitys[0] != ',')
                strCode = "," + existCitys; //以‘，’隔离
            else
                strCode = existCitys;
            LoadUIData();
            Result = new Dictionary<string, string>();
        }

        void GetExistCities(string cityCode)
        {
            var CitiesArray = cityCode.Split(',');
            foreach (string city in CitiesArray)
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONARYVALUE.ToString() == city && a.DICTIONCATEGORY == "CITY"
                           select a;
                if (ents.Count() > 0)
                {
                    display.Text += ents.FirstOrDefault().DICTIONARYNAME + " ";
                }
            }
        }

        public Border GetSelectedCities
        {
            get { return this.SelectedCities; }
            set { this.SelectedCities = value; }
        }

        private void LoadUIData()
        {
            var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                       where a.DICTIONCATEGORY == "PROVINCE"
                       orderby a.ORDERNUMBER
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

            //by luojie 显示已有城市
            if(!string.IsNullOrEmpty(strCode))
                GetExistCities(strCode);
        }

        

        void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string temp = string.Empty;
            TextBlock tbtemp = sender as TextBlock;
            // tbtemp.Foreground = new SolidColorBrush(Colors.Red);
            wpCity.Children.Clear();
            //wpCity.Height = wpheight;
            if (this._selectmulti == false)
            {
                SelectedCities.Visibility = Visibility.Collapsed;
                display.Visibility = Visibility.Collapsed;
            }
            if (tbtemp.Tag != null)
            {
                temp = tbtemp.Tag.ToString();
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.T_SYS_DICTIONARY2 != null && a.T_SYS_DICTIONARY2.DICTIONARYID == temp
                           orderby a.ORDERNUMBER
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
                        if (this._selectmulti)
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
                        else
                        {
                            RadioButton rd = new RadioButton();
                            rd.Style = Application.Current.Resources["RadioButtonStyle"] as Style;
                            if (!string.IsNullOrEmpty(display.Text) && display.Text.IndexOf(ent.DICTIONARYNAME) != -1)
                                rd.IsChecked = true;
                            else
                                rd.IsChecked = false;
                            rd.Content = ent.DICTIONARYNAME;
                            rd.Tag = ent.DICTIONARYVALUE;
                            rd.Click += new RoutedEventHandler(rb_Checked);
                            rd.Margin = new Thickness(2);
                            //cb.Height = 80;
                            rd.Width = 80;
                            wpCity.Children.Add(rd);
                        }
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
                    display.Text = display.Text.Replace(cb.Content.ToString()+" ", "").Trim() + " ";
                    strCode = strCode.Replace(","+cb.Tag.ToString() + ",", ",");
                }
                else
                {
                    if (display.Text.IndexOf(cb.Content.ToString()) == -1)
                    {
                        display.Text += cb.Content.ToString() + " ";
                        strCode += cb.Tag.ToString() + ",";
                    }
                }
            }
        }

        #region 单选按钮事件        
        
        void rb_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rd = sender as RadioButton;
            if (rd != null)
            {
                if (rd.IsChecked == true)
                {
                    display.Text = rd.Content.ToString() + " ";
                    strCode = rd.Tag.ToString() + ",";
                }
                
            }
        }
        #endregion
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            
            if (SelectedClicked != null)
            {
                if (string.IsNullOrEmpty(strCode))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "请选择相应的城市！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                Result.Add(display.Text.ToString(), strCode);
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
