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


using SMT.SaaS.FrameworkUI.Helper;
using SMT.Saas.Tools.OrganizationWS;
using PersonnelWS = SMT.Saas.Tools.PersonnelWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.SaaS.FrameworkUI.OrganizationControl
{
    public partial class OrganizationLookupAll : System.Windows.Controls.Window
    {

        public string ModelCode { get; set; }

        public List<ExtOrgObj> SelectedObj
        {
            get
            {
                return orgTree.SelectedObj;
            }
        }


        public event EventHandler SelectedClick;

        public OrgTreeItemTypes SelectedObjType
        {
            get
            {
                return orgTree.SelectedObjType;
            }
            set
            {
                orgTree.SelectedObjType = value;
            }

        }
        public bool SelectSameGradeOnly
        {
            get
            {
                return orgTree.SelectSameGradeOnly;
            }
            set
            {
                orgTree.SelectSameGradeOnly = value;
            }

        }
        public bool MultiSelected;


        public string CurrentUserID
        {
            get
            {
                return orgTree.CurrentUserID;
            }
            set
            {
                orgTree.CurrentUserID = value;
            }

        }

        public string Perm
        {
            get
            {
                return orgTree.Perm;
            }
            set
            {
                orgTree.Perm = value;
            }

        }
        public string Entity
        {
            get
            {
                return orgTree.Entity;
            }
            set
            {
                orgTree.Entity = value;
            }

        }

        public OrganizationLookupAll()
        {
            InitializeComponent();
        }

        //public OrganizationLookup(string userID, string perm, string entity) : this()
        //{

        //    this.CurrentUserID = userID;
        //    this.Perm = perm;
        //    this.Entity = entity;
        //}

        //public OrganizationLookup(string perm, string entity) : this()
        //{
        //    this.Perm = perm;
        //    this.Entity = entity;
        //}

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = orgTree.ValidSelection();

            if (!string.IsNullOrEmpty(errMsg))
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), errMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), errMsg,
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                return;
            }

            if (this.SelectedClick != null)
            {
                this.SelectedClick(this, null);
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.Remove("SYS_CompanyInfo");
            Application.Current.Resources.Remove("SYS_DepartmentInfo");
            Application.Current.Resources.Remove("SYS_PostInfo");
            orgTree.postIDsCach.Clear();
            orgTree.depIDsCach.Clear();
            orgTree.BindTree();
        }

    }
}
