/*
 * 省、市、县城控件类
 * 
 * 可以根据不同的国家选择相应的省、市城市
 * LiuJX   2011-11-25
 * 
 * 
 * 
 * 
 * */
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
    public partial class AreaSortCityNew : System.Windows.Controls.Window
    {
        #region 参数定义        
        
        PermissionServiceClient client = new PermissionServiceClient();
        private const int wpheight = 20;
        private string strCode = string.Empty;
        public Dictionary<string, string> Result { get; set; }
        public event EventHandler SelectedClicked;
        public event EventHandler Close;
        private bool _selectmulti = false;
        private string _SelectCitys = "";//已选择的城市值
        private string[] Citys;//数组参数，用来存放已经选择的城市值

        /// <summary>
        /// 国家集合
        /// </summary>
        private List<T_SYS_COUNTRY> ListCountry = new List<T_SYS_COUNTRY>();
        //省、市集合
        private List<V_ProvinceCity> ListProvince = new List<V_ProvinceCity>();
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 构造函数
        
        public AreaSortCityNew()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            //RefreshUI(RefreshedTypes.ShowProgressBar);
            Result = new Dictionary<string, string>();
            InitEvent();
            
            client.GetCountryAsync();
        }


        #endregion

        #region 初始化事件

        private void InitEvent()
        {
            client.GetCountryCompleted += new EventHandler<GetCountryCompletedEventArgs>(client_GetCountryCompleted);
            client.GetProvinceCityCompleted += new EventHandler<GetProvinceCityCompletedEventArgs>(client_GetProvinceCityCompleted);

        }
        #endregion

        #region 获取省、城市
        void client_GetProvinceCityCompleted(object sender, GetProvinceCityCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ListProvince.Clear();
                ListProvince = e.Result.ToList();
                LoadUIData();
            }
            if (_SelectCitys != "" && _SelectCitys != string.Empty)
            {
                Citys = _SelectCitys.Split(',');
                string StrDisplay = "";
                for (int i = 0; i < Citys.Length - 1; i++)
                {
                    var ent = ListProvince.Where(p=>p.AREAVALUE.ToString() == Citys[i]);
                    if (ent.Count() > 0)
                    {
                        StrDisplay += ent.FirstOrDefault().AREANAME + " ";                        
                    }
                }
                strCode = _SelectCitys;
                display.Text = StrDisplay;
            }
        }
        #endregion

        #region 获取国家信息

        void client_GetCountryCompleted(object sender, GetCountryCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ListCountry.Clear();
                ListCountry = e.Result.ToList();
                client.GetProvinceCityAsync("");//获取所有的省、市、县
            }
        }

        #endregion

        #region 属性参数
        
        
        /// <summary>
        /// 是否允许多选
        /// </summary>
        public bool SelectMulti
        {
            get { return this._selectmulti; }
            set { this._selectmulti = value; }
        }

        /// <summary>
        /// 获取选择的城市
        /// </summary>
        public Border GetSelectedCities
        {
            get { return this.SelectedCities; }
            set { this.SelectedCities = value; }
        }
        /// <summary>
        /// 已经选择的城市
        /// </summary>
        public string GetCitys
        {
            get { return this._SelectCitys; }
            set { this._SelectCitys = value; }
        }
        #endregion

        #region 加载默认参数
        
        
        private void LoadUIData()
        {
            if (ListCountry.Count() > 0)
            {
                wpcountry.Children.Clear();
                
                foreach (var ent in ListCountry)
                {
                    RadioButton rd = new RadioButton();
                    rd.Style = Application.Current.Resources["RadioButtonStyle"] as Style;

                    rd.Content = ent.COUNTRYNAME;
                    rd.Tag = ent.COUNTRYID;
                    if (rd.Content.ToString() == "中国")
                    {
                        rd.IsChecked = true;
                        ShowProvinceCity(ent.COUNTRYID);
                    }
                    else
                    {
                        rd.IsChecked = false;
                    }
                    rd.Click += new RoutedEventHandler(rbCountry_Checked);
                    rd.Margin = new Thickness(2);
                    
                    rd.Width = 80;
                    wpcountry.Children.Add(rd);

                }
            }
            
            loadbar.Stop();
        }
        #endregion

        #region 省、直辖市单击事件
        
        void tbprovince_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string temp = string.Empty;
            TextBlock tbtemp = sender as TextBlock;            
            wpCity.Children.Clear();
            wptown.Children.Clear();//清空之前显示的城镇信息
            //wpCity.Height = wpheight;
            //先不隐藏，如果需要隐藏则去掉注释
            //if (this._selectmulti == false)
            //{
            //    SelectedCities.Visibility = Visibility.Collapsed;
            //    display.Visibility = Visibility.Collapsed;
            //}
            if (tbtemp.Tag != null)
            {
                temp = tbtemp.Tag.ToString();
                if (ListProvince.Count() > 0)
                {
                    var ents = ListProvince.Where(p => p.FATHERID == temp);
                    if (ents.Count() > 0)
                    {
                        foreach (var ent in ents)
                        {
                            //如果是直辖市，则只显示市，而不显示城镇
                            var entcitys = ListProvince.Where(p=>p.PROVINCEID == temp).Where(p=>p.ISCITY == "1").Where(p=>p.ISPROVINCE =="1");
                            if (entcitys.Count() > 0)
                            {
                                if (this._selectmulti)
                                {
                                    CheckBox cbx = new CheckBox();
                                    cbx.Style = Application.Current.Resources["CheckBoxStyle"] as Style;
                                    CheckBoxIsChecked(ent, cbx);  
                                    cbx.Content = ent.AREANAME;
                                    cbx.Tag = ent.AREAVALUE;
                                    cbx.Click += new RoutedEventHandler(cbxProvinceCity_Checked);
                                    cbx.Margin = new Thickness(2);
                                    cbx.Width = 80;
                                    wpCity.Children.Add(cbx);
                                }
                                else
                                {
                                    RadioButton rd = new RadioButton();
                                    rd.Style = Application.Current.Resources["RadioButtonStyle"] as Style;
                                    RadionIsChecked(ent, rd);                                    
                                    rd.Content = ent.AREANAME;
                                    rd.Tag = ent.AREAVALUE;
                                    rd.Click += new RoutedEventHandler(rbProvinceCity_Checked);
                                    rd.Margin = new Thickness(2);
                                    rd.Width = 80;
                                    wpCity.Children.Add(rd);
                                }
                                HideTown(false);//隐藏城镇
                            }                            
                            else
                            {
                                TextBlock tb = new TextBlock();                                
                                tb.Cursor = Cursors.Hand;
                                tb.Text = ent.AREANAME;
                                tb.Tag = ent.PROVINCEID;
                                tb.MouseLeftButtonDown += new MouseButtonEventHandler(tbCity_MouseLeftButtonDown);
                                tb.Margin = new Thickness(3);                                
                                tb.Width = 80;
                                wpCity.Children.Add(tb);
                            }
                        }


                    }
                }

            }
            else return;
        }

        #region Radion按钮是否别选中        
        
        private void RadionIsChecked(V_ProvinceCity ent, RadioButton rd)
        {
            if (Citys != null)
            {
                if (Citys.Length > 0)
                {
                    if (Array.IndexOf(Citys, ent.AREAVALUE.ToString()) > -1)
                    {
                        rd.IsChecked = true;
                    }
                    else
                    {
                        rd.IsChecked = false;
                    }
                }
            }
        }

        #endregion

        #region CHECKBOX是否被选中

        private void CheckBoxIsChecked(V_ProvinceCity ent, CheckBox cbx)
        {
            if (Citys != null)
            {
                if (Citys.Length > 0)
                {
                    if (Array.IndexOf(Citys, ent.AREAVALUE.ToString()) > -1)
                    {
                        cbx.IsChecked = true;
                    }
                    else
                    {
                        cbx.IsChecked = false;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region 显示、隐藏城镇
        private void HideTown(bool IsShow)
        {
            if (!IsShow)
            {
                wptown.Visibility = Visibility.Collapsed;
                brtown.Visibility = Visibility.Collapsed;
            }
            else
            {
                wptown.Visibility = Visibility.Visible;
                brtown.Visibility = Visibility.Visible;
            }
        }
        
        #endregion

        #region 城市按钮选择

        void tbCity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string temp = string.Empty;
            TextBlock tbtemp = sender as TextBlock;
            wptown.Children.Clear();
            HideTown(true);
            //if (this._selectmulti == false)
            //{
            //    SelectedCities.Visibility = Visibility.Collapsed;
            //    display.Visibility = Visibility.Collapsed;
            //}
            if (tbtemp.Tag != null)
            {
                temp = tbtemp.Tag.ToString();

                if (ListProvince.Count() > 0)
                {
                    var ents = ListProvince.Where(p => p.FATHERID == temp);
                    if (ents.Count() > 0)
                    {
                        foreach (var ent in ents)
                        {
                            if (this._selectmulti)
                            {
                                CheckBox cbx = new CheckBox();
                                cbx.Style = Application.Current.Resources["CheckBoxStyle"] as Style;                                
                                CheckBoxIsChecked(ent,cbx);                                
                                cbx.Content = ent.AREANAME;
                                cbx.Tag = ent.AREAVALUE;
                                cbx.Click += new RoutedEventHandler(cbxTown_Checked);
                                cbx.Margin = new Thickness(2);                                
                                cbx.Width = 80;
                                wptown.Children.Add(cbx);
                            }
                            else
                            {
                                RadioButton rd = new RadioButton();
                                rd.Style = Application.Current.Resources["RadioButtonStyle"] as Style;
                                RadionIsChecked(ent,rd);                               
                                rd.Content = ent.AREANAME;
                                rd.Tag = ent.AREAVALUE;
                                rd.Click += new RoutedEventHandler(rbTown_Checked);
                                rd.Margin = new Thickness(2);                                
                                rd.Width = 80;
                                wptown.Children.Add(rd);
                            }
                        }


                    }
                }

            }
            else return;
        }
        #endregion


        

        #region 直辖市的选择        
        
        void cbxProvinceCity_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                if (cb.IsChecked != true)
                {
                    display.Text = display.Text.Replace(cb.Content.ToString(), "").Trim() + " ";
                    strCode = strCode.Replace(cb.Tag.ToString() + ",", "");
                }
                else
                {
                    if (display.Text.IndexOf(cb.Content.ToString()) == -1)
                    {
                        display.Text += cb.Content.ToString() + " ";
                        strCode += cb.Tag.ToString() + ",";
                    }
                }
                ClearCitys();
            }
        }

        void rbProvinceCity_Checked(object sender, RoutedEventArgs e)
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


        #region 县城选择        
        
        void cbxTown_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                if (cb.IsChecked != true)
                {
                    display.Text = display.Text.Replace(cb.Content.ToString(), "").Trim() + " ";
                    strCode = strCode.Replace(cb.Tag.ToString() + ",", "");
                    
                }
                else
                {
                    if (display.Text.IndexOf(cb.Content.ToString()) == -1)
                    {
                        display.Text += cb.Content.ToString() + " ";
                        strCode += cb.Tag.ToString() + ",";
                    }
                }
                ClearCitys();
            }
        }

        #endregion

        /// <summary>
        /// 清空城市选择值
        /// </summary>
        private void ClearCitys()
        {
            if (Citys != null)
            {
                Array.Clear(Citys, 0, Citys.Length);
                Citys = strCode.Split(',');
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
        /// <summary>
        /// 国家单选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rbCountry_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rd = sender as RadioButton;
            string CountryId = "";
            if (rd != null)
            {
                if (rd.IsChecked == true)
                {
                    //display.Text = rd.Content.ToString() + " ";
                    CountryId = rd.Tag.ToString();
                    ShowProvinceCity(CountryId);
                }

            }
        }
        /// <summary>
        /// 城市单选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rbCity_Checked(object sender, RoutedEventArgs e)
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
        /// <summary>
        /// 县城单选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rbTown_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rd = sender as RadioButton;
            string CountryId = "";
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

        #region 显示省、直辖市

        /// <summary>
        /// 显示省、市
        /// </summary>
        /// <param name="CountryId"></param>
        private void ShowProvinceCity(string CountryId)
        {
            if (ListProvince.Count() > 0)
            {
                var ents = ListProvince.Where(p => p.COUNTRYID == CountryId && p.ISPROVINCE == "1");
                wpProvince.Children.Clear();
                foreach (var ent in ents)
                {
                    TextBlock tb = new TextBlock();
                    // tb.Foreground = new SolidColorBrush(Colors.Black);
                    tb.Cursor = Cursors.Hand;
                    tb.Text = ent.AREANAME;
                    tb.Tag = ent.PROVINCEID;
                    ToolTipService.SetToolTip(tb, ent.AREANAME);
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tbprovince_MouseLeftButtonDown);
                    tb.Margin = new Thickness(3);
                    //tb.Height = 80;
                    tb.Width = 80;
                    wpProvince.Children.Add(tb);
                }
            }
        }

        #endregion

        #region 保存按钮

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

        #endregion

        #region 取消按钮

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            display.Text = strCode = string.Empty;
            if (this.Close != null)
                Close(this, EventArgs.Empty);
        }

        #endregion

        
    }
}
