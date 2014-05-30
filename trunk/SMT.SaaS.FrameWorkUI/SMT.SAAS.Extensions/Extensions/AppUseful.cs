using System.Linq;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows;

namespace System.Windows
{
    /// <summary>
    /// 应用程序辅助类
    /// </summary>
    public static class AppUseful
    {
       

        private static Content _Content;
        /// <summary>
        /// 获取或设置当前应用程序主机内容
        /// </summary>
        public static Content Content
        {
            get
            {
                if (AppUseful._Content.IsNull())
                {
                    AppUseful._Content = Application.Current.Host.Content;
                }
                return AppUseful._Content;
            }
            set
            {
                AppUseful._Content = value;
            }
        }

     

        private static UIElement _RootVisual;
        /// <summary>
        /// 获取或设置主容器对象,获取为当前应用程序的RootVisual
        /// </summary>
        public static UIElement RootVisual
        {
            get
            {
                if (AppUseful._RootVisual.IsNull())
                {
                    AppUseful._RootVisual = Application.Current.RootVisual;
                }
                return AppUseful._RootVisual;
            }
            set
            {
                AppUseful._RootVisual = value;
            }
        }
        /// <summary>
        /// 获取主视图容器
        /// </summary>
        /// <typeparam name="T">容器类型</typeparam>
        /// <returns>根据已知类型返回容器(UIElement)</returns>
        public static T GetRootVisual<T>() where T : UIElement
        {
            return (T)AppUseful.RootVisual;
        }

        
    }
}
