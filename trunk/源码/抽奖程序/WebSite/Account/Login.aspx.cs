using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Asd.Award.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterHyperLink.NavigateUrl = "Register.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);

            if (this.IsPostBack)
            {
               // this.LoginUser.c
            }
        }

        protected void OnAuthenticate(object sender, AuthenticateEventArgs e)
        {
            var authenticated = false;
            if (this.LoginUser.UserName == "asd" && this.LoginUser.Password == "168") authenticated = true;

            e.Authenticated = authenticated;
        }

    }
}
