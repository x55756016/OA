using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Asd.Award.Domain;

namespace Asd.Award
{
    public partial class Ticket : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 修改深圳票数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnsz_Click(object sender, EventArgs e)
        {
            try
            {
                DataContext.UpdateQuery(Convert.ToInt32(tbsz.Text), "深圳");
            }
            catch(Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message +"')</script>");
            }
        }
        /// <summary>
        /// 修改北京票数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnbj_Click(object sender, EventArgs e)
        {
            try
            {
                DataContext.UpdateQuery(Convert.ToInt32(tbbj.Text), "北京");
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }
    }
}