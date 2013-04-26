
using System.IO.IsolatedStorage;
using System.Windows;
using System;
namespace SMT.SAAS.Platform.ViewModel.SplashScreen
{
    public class InitPart
    {
        public static IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        
        /// <summary>
        /// 初始化用户定义的主题文件
        /// </summary>
        private void InitTheme()
        {
            string themeName;
            if (AppSettings.Contains("ThemeName"))
            {
                themeName = AppSettings["ThemeName"].ToString();
                AppSettings.Save();
            }
            else
            {
                themeName = "ShinyBlue";
                AppSettings.Add("ThemeName", themeName);
                AppSettings.Save();
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary newStyle3 = new ResourceDictionary();
            Uri uri = new Uri("/SMT.SAAS.Themes;component/" + themeName + ".xaml", UriKind.Relative);
            newStyle3.Source = uri;
            Application.Current.Resources.MergedDictionaries.Add(newStyle3);

            ResourceDictionary newStyle_toolkit = new ResourceDictionary();
            Uri uri_toolkit = new Uri("/SMT.SAAS.Themes;component/ToolKitResource.xaml", UriKind.Relative);
            newStyle_toolkit.Source = uri_toolkit;
            Application.Current.Resources.MergedDictionaries.Add(newStyle_toolkit);

            ResourceDictionary newStyle_template = new ResourceDictionary();
            Uri uri_template = new Uri("/SMT.SAAS.Themes;component/ControlTemplate.xaml", UriKind.Relative);
            newStyle_template.Source = uri_template;
            Application.Current.Resources.MergedDictionaries.Add(newStyle_template);
        }


    }

}
