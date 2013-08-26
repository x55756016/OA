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

namespace SMT.Workflow.Platform.Designer.Views.Engine
{
    public partial class LooKUP : ChildWindow
    {
        public event EventHandler SelectedClick;
        public List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> SelectList = null;
        public LooKUP()
        {
            InitializeComponent();
            //object objs = null;
            //if (Application.Current.Resources["CurrentUserID"] != null)
            //{
            //    objs = Application.Current.Resources["CurrentUserID"];
            //    Application.Current.Resources.Remove("CurrentUserID");
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //if (Application.Current.Resources["CurrentUserID"] == null)
            //{
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
          //  SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup(Utility.CurrentUser.EMPLOYEEID, "3", "");
           SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SelectList = ent;

                }
            };
            (lookup.FindName("OKButton") as Button).Click += new RoutedEventHandler(LooKUP_Click);
            (lookup.FindName("CancelButton") as Button).Click += new RoutedEventHandler(CancelButton_Click);
            lookup.MultiSelected = false;
            LayoutRoot.Children.Add(lookup);
        }
        public LooKUP(SMT.SaaS.FrameworkUI.OrgTreeItemTypes orgTreeItemType,string title)
        {
            InitializeComponent();
            this.Title = title;
            //object objs = null;
            //if (Application.Current.Resources["CurrentUserID"] != null)
            //{
            //    objs = Application.Current.Resources["CurrentUserID"];
            //    Application.Current.Resources.Remove("CurrentUserID");
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}
            //if (Application.Current.Resources["CurrentUserID"] == null)
            //{
            //    Application.Current.Resources.Add("CurrentUserID", "");
            //}

            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = orgTreeItemType;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SelectList = ent;

                }
            };
            (lookup.FindName("OKButton") as Button).Click += new RoutedEventHandler(LooKUP_Click);
            (lookup.FindName("CancelButton") as Button).Click += new RoutedEventHandler(CancelButton_Click);
            lookup.MultiSelected = false;
            LayoutRoot.Children.Add(lookup);
        }
        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        void LooKUP_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedClick != null)
            {
                this.SelectedClick(this, null);
            }
            this.DialogResult = false;
        }
    }
}

