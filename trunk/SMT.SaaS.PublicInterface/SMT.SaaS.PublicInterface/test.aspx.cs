using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SMT.SaaS.PublicInterface
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PublicService sv = new PublicService();
            string str=sv.GetContentFormatImgToUrl("c63e4df4-58ee-435e-84cb-12c983a26c78");
            Response.Write(str);
        }
    }
}