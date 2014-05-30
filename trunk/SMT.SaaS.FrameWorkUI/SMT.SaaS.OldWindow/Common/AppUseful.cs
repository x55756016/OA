using System.Linq;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows;

namespace System.Windows
{
    public static class AppUseful
    {
        # region Content

        private static Content _Content;

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

        # endregion

        # region RootVisual

        private static UIElement _RootVisual;

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

        public static T GetRootVisual<T>() where T : UIElement
        {
            return (T)AppUseful.RootVisual;
        }

        # endregion
    }
}
