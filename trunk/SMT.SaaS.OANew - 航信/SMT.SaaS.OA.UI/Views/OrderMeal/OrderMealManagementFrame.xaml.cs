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
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.OrderMeal
{
    public partial class OrderMealManagementFrame : Page
    {
        public OrderMealManagementFrame()
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
}
