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
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI
{
    public enum ToolbarItemDisplayTypes
    { 
        Image,
        Text,
        ImageAndText
    }
    public class ToolbarItem
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public ToolbarItemDisplayTypes DisplayType { get; set; }
        public string Tooltip { get; set; }
        public string Key { get; set; }
        public virtual event EventHandler<RoutedEventArgs> ItemClick;
    }

    public class ToolBarItems
    {
        public static ToolbarItem View
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "View",
                    Title = Utility.GetResourceStr("VIEW"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1071_d.png"
                };
                return item;
            }
        }

        public static ToolbarItem New
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "New",
                    Title = Utility.GetResourceStr("NEW"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };
                return item;
            }
        }

        public static ToolbarItem Edit
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "Edit",
                    Title = Utility.GetResourceStr("EDIT"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/edit.png"
                };
                return item;
            }
        }


        public static ToolbarItem Save
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "Save",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
                };
                return item;
            }
        }
        public static ToolbarItem SaveAndClose
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "SaveAndClose",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                return item;
            }
        }

        public static ToolbarItem Delete
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "Delete",
                    Title = Utility.GetResourceStr("DELETE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
                };
                return item;
            }
        }

        public static ToolbarItem Cancel
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "Cancel",
                    Title = Utility.GetResourceStr("CLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_close.png"
                };
                return item;
            }
        }

        public static ToolbarItem Submit
        {
            get
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "Submit",
                    Title = Utility.GetResourceStr("SUBMIT"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
                };
                return item;
            }
        }

    }
}
