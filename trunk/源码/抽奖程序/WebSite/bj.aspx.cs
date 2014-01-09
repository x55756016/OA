using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Asd.Award.Domain;

namespace Asd.Award
{
    public partial class AwardPrintBJ : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                var dt = DataContext.GetAward();
                this.bindingData(dt);

                this.tableSZ0.Visible = false;
                this.tableSZ1.Visible = false;
                this.tableSZ2.Visible = false;
                this.tableSZ31.Visible = false;
                //this.tableSZ32.Visible = false;
                //this.tableSZ33.Visible = false;
            }
        }

        private void bindingData(AwardDataSet.TmpAwardDataTable dt)
        {
            foreach (var item in dt)
            {
                var city = item.TicketNO.Substring(0, 2);
                if (city == "SZ")
                {
                    switch (item.Level)
                    {
                        case "3":
                            //switch (item.Remark)
                            //{
                            //    case "Batch1":
                            //        BuildTableRow(this.tableSZ31, item);
                            //        break;
                            //    case "Batch2":
                            //        BuildTableRow(this.tableSZ32, item);
                            //        break;
                            //    case "Batch3":
                            //        BuildTableRow(this.tableSZ33, item);
                            //        break;
                            //}
                            BuildTableRow(this.tableSZ31, item);
                            break;
                        case "2":
                            BuildTableRow(this.tableSZ2, item);
                            break;
                        case "1":
                            BuildTableRow(this.tableSZ1, item);
                            break;
                        case "0":
                            BuildTableRow(this.tableSZ0, item);
                            break;
                    }
                }

                if (city == "BJ")
                {
                    switch (item.Level)
                    {
                        case "3":
                            //switch (item.Remark)
                            //{
                            //    case "Batch1":
                            //        BuildTableRow(this.tableBJ31, item);
                            //        break;
                            //    case "Batch2":
                            //        BuildTableRow(this.tableBJ32, item);
                            //        break;
                            //    case "Batch3":
                            //        BuildTableRow(this.tableBJ33, item);
                            //        break;
                            //}
                            BuildTableRow(this.tableBJ31, item);
                            break;
                        case "2":
                            BuildTableRow(this.tableBJ2, item);
                            break;
                        case "1":
                            BuildTableRow(this.tableBJ1, item);
                            break;
                        case "0":
                            BuildTableRow(this.tableBJ0, item);
                            break;
                    }
                }
            }
        }

        public static void BuildTableRow(HtmlTable table, AwardDataSet.TmpAwardRow item)
        {
            var tr = new HtmlTableRow();
            var cell = new HtmlTableCell();
            cell.InnerText = item.TicketNO;
            tr.Cells.Add(cell);
            table.Rows.Add(tr);
        }
    }
}