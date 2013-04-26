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
using System.IO.IsolatedStorage;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class Helper : UserControl
    {
        /// <summary>
        /// 独立存储
        /// </summary>
        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        /// <summary>
        /// 存储最后一次访问的用户名KEY
        /// </summary>
        private const string POPUPKEY = "POPUPHELPER";
        public Helper()
        {
            InitializeComponent();
        }

        private void ckbisPopup_Checked(object sender, RoutedEventArgs e)
        {
            SetPopup();
        }

        private void ckbisPopup_Unchecked(object sender, RoutedEventArgs e)
        {
            SetPopup();
        }

        private void SetPopup()
        {
            bool ischecked = (bool)ckbisPopup.IsChecked;


            if (!AppSettings.Contains(POPUPKEY))
                AppSettings.Add(POPUPKEY, ischecked);
            else
                AppSettings[POPUPKEY] = ischecked;
        }

    }
}
