using System;
using System.Collections.Generic;
using System.Linq;

namespace Asd.Award.Domain
{
    public class AwardMachine
    {
        static Random random = new Random();

        public AwardDataSet.TmpOptionDataTable Option { get; private set; }
        public AwardDataSet.TmpTicketDataTable Ticket { get; private set; }
        public AwardDataSet.TmpAwardDataTable Award { get; private set; }
        public string BelongTo { get; set; }

        public AwardMachine(AwardDataSet.TmpOptionDataTable option, AwardDataSet.TmpTicketDataTable ticket, AwardDataSet.TmpAwardDataTable award)
        {
            this.Option = option;
            this.Ticket = ticket;
            this.Award = award;

            this.updateCandidate();
        }

        public AwardDataSet.TmpAwardRow[] GenerateAwardByPlan()
        {
            var plan = this.getOngoing();
            var award = new AwardDataSet.TmpAwardRow[plan.AwardQty];

            for (int i = 0; i < award.Length; i++)
            {
                award[i] = this.generateOneAwardByPlan(plan);
                this.updateCandidate(award[i].TicketNO);
            }

            return award;
        }

        private AwardDataSet.TmpAwardRow generateOneAwardByPlan(AwardDataSet.TmpOptionRow plan)
        {
            var item = this.Award.NewTmpAwardRow();
            item.TicketNO = this.getLucky();
            item.Level = plan.Level;
            item.UpdateTime = DateTime.Now;
            this.Award.AddTmpAwardRow(item);
            return item;
        }

        private string getLucky()
        {
            var luckyNO = ((AwardDataSet.TmpTicketRow)(this.Ticket.Rows[random.Next(this.Ticket.Count)])).TicketNO;
            return luckyNO;
        }

        private void updateCandidate()
        {
            foreach (var ticket in this.Ticket)
            {
                foreach (var award in this.Award)
                {
                    if (ticket.TicketNO == award.TicketNO)
                    {
                        ticket.Delete();
                        break;
                    }
                }
            }
            this.Ticket.AcceptChanges();
        }

        private void updateCandidate(string ticketNO)
        {
            foreach (var ticket in this.Ticket)
            {
                if (ticket.TicketNO == ticketNO) ticket.Delete();
            }
            this.Ticket.AcceptChanges();
        }

        private AwardDataSet.TmpOptionRow getOngoing()
        {
            var done = this.Award.Count;
            var plan = 0;
            foreach (var item in this.Option.Where(p => p.BelongTo == this.BelongTo))
            {
                plan += item.AwardQty;
                if (plan > done)
                {
                    return item;
                }
            }

            return null;
        }
    }
}