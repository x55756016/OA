using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SMT.SaaS.Permission.Services
{
    public partial class TestPermission : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Permission.Services.PermissionService p = new PermissionService();
            p.GetFlowUserInfoByRoleID("4228fb43-4bb9-4b20-9316-2e7f6b0bde2a");
        }
    }
}