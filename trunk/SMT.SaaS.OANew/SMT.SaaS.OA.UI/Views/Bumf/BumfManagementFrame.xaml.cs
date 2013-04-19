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
using System.Windows.Navigation;
using System.Globalization;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class BumfManagementFrame : Page
    {
        public BumfManagementFrame()
        {
            InitializeComponent();
            SetStyle(AppConfig._CurrentStyleCode);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        #region 页面样式
        public void SetStyle(int Istyle)
        {
            switch (Istyle)
            {
                case -1:
                    break;
                case 0:
                    AppConfig.SetStyles("Assets/Styles.xaml", LayoutRoot);
                    break;
                case 1:
                    AppConfig.SetStyles("Assets/ShinyBlue.xaml", LayoutRoot);
                    //SearchBtn.Style = (Style)Application.Current.Resources["CommonButtonStyle1"];
                    break;
                default:
                    break;
            }
        }
        #endregion

    }


    #region 会议申请状态
    public class ConverterOPTFlagToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "不归档";
                    break;
                case "1":
                    StrReturn = "归档";
                    break;                
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "不归档":
                    StrReturn = "0";
                    break;
                case "归档":
                    StrReturn = "1";
                    break;                
            }
            return StrReturn;

        }
    }
    #endregion
}
