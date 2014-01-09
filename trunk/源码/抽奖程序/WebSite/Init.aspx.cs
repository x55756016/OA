using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Asd.Award.Domain;

namespace Asd.Award
{
    public partial class _Init : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ButtonBatch_Click(object sender, EventArgs e)
        {
            var start = Int32.Parse(this.TextBoxStart.Text.Trim());
            var end = Int32.Parse(this.TextBoxEnd.Text.Trim());
            var dst = new AwardDataSet();

            for (int i = start; i <= end; i++)
            {
                var item = dst.TmpTicket.NewTmpTicketRow();
                var ticketNO = i.ToString();
                if (ticketNO.Length < 4) ticketNO = ticketNO.PadLeft(4, '0');
                ticketNO = this.RadioButtonList1.SelectedValue +"-"+ ticketNO;
                item.TicketNO = ticketNO;
                dst.TmpTicket.AddTmpTicketRow(item);
            }

            DataContext.Update(dst.TmpTicket);            
        }

        protected void ButtonSingle_Click(object sender, EventArgs e)
        {
            var ticketNO =this.TextBoxSingle.Text.Trim();
            if (ticketNO.Length <4) ticketNO=ticketNO.PadLeft(4,'0');
            ticketNO = this.RadioButtonList1.SelectedValue + "-" + ticketNO;            

            var dst = new AwardDataSet();
            var item = dst.TmpTicket.NewTmpTicketRow();
            item.TicketNO = ticketNO;
            dst.TmpTicket.AddTmpTicketRow(item);

            DataContext.Update(dst.TmpTicket);
            
        }

        protected void ButtonClearTicket_Click(object sender, EventArgs e)
        {
            DataContext.ClearTicket();
        }

        protected void ButtonClearAward_Click(object sender, EventArgs e)
        {
            DataContext.ClearAward();
        }
    }
}
