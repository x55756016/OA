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

using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Form
{
    public partial class CompanyHistoryForm : BaseForm
    {
        private T_HR_COMPANYHISTORY company;

        public T_HR_COMPANYHISTORY Company
        {
            get { return company; }
            set
            {
                company = value;
                this.DataContext = company;
            }
        }

        PermissionServiceClient perClient;
        public CompanyHistoryForm(T_HR_COMPANYHISTORY org)
        {
            InitializeComponent();
            Company = org;
            this.IsEnabled = false;
        }
    }
}
