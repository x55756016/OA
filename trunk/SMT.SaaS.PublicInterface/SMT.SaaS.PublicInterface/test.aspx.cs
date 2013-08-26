using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.Foundation.Log;

namespace SMT.SaaS.PublicInterface
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
          
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Tracer.Debug("写日志测试");
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            PublicService sv = new PublicService();
            string str = sv.GetContentFormatImgToUrl("128ef005-4006-4d37-acc7-8e2bb05abfa6");
            Response.Write(str);
        }
    }
}