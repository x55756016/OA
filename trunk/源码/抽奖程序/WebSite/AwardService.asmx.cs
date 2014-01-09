using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Asd.Award.Domain;

namespace Asd.Award
{
    /// <summary>
    /// AwardService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    [System.Web.Script.Services.GenerateScriptType(typeof(AwardEntity))]
    public class AwardService : System.Web.Services.WebService
    {
        [WebMethod]
        public string[] GetTicket()
        {
            var dt = DataContext.GetTicket();

            var ar = new string[dt.Count];
            var index = 0;
            foreach (var item in dt)
            {
                ar[index] = item.TicketNO;
                index++;
            }
            return ar;
        }

        /// <summary>
        /// 为了获取奖号数量
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public int[] GetTicketCount()
        {
            var dt = DataContext.GetTicket();

            var ar = new int[dt.Count];
            var index = 0;
            foreach (var item in dt)
            {
                ar[index] = item.TicketCount;
                index++;
            }
            return ar;
        }


        [WebMethod]
        public void AddAward(string ticketNO, string level, string remark)
        {
            var dst = new AwardDataSet();
            var item = dst.TmpAward.NewTmpAwardRow();
            item.TicketNO = ticketNO;
            item.Level = level;
            item.Remark = remark;
            item.UpdateTime = DateTime.Now;
            dst.TmpAward.AddTmpAwardRow(item);

            DataContext.Update(dst.TmpAward);
        }

        [WebMethod]
        [System.Web.Script.Services.ScriptMethod()]
        public void AddAwardMany(AwardEntity[] awards)
        {
            foreach (var item in awards)
            {
                this.AddAward(item.TicketNO, item.Level, item.Remark);
            }
        }

        [WebMethod]
        public string[] GetAward()
        {
            var dt = DataContext.GetAward();

            var ar = new string[dt.Count];
            var index = 0;
            foreach (var item in dt)
            {
                ar[index] = item.TicketNO;
                index++;
            }
            return ar;
        }

        [WebMethod]
        [System.Web.Script.Services.ScriptMethod()]
        public AwardEntity[] GetAwardObj()
        {
            var dt = DataContext.GetAward();

            var result = DataContext.BuildAwardEntity(dt.ToArray<AwardDataSet.TmpAwardRow>());
            return result;
        }

        [WebMethod]
        public void DiscardAward(string ticketNO)
        {
            DataContext.DiscardAward(ticketNO);
        }

        [WebMethod]
        [System.Web.Script.Services.ScriptMethod()]
        public AwardEntity[] GenerateAward(string belongTo)
        {
            var machine = DataContext.BuildAwardMachine();
            machine.BelongTo = belongTo;
            var ongoing = machine.GenerateAwardByPlan();
            DataContext.Update(machine.Award);

            var result = DataContext.BuildAwardEntity(ongoing);
            return result;
        }

        [WebMethod]
        public void UpdateQuery(int ticketCount, string ticketNO)
        {
            DataContext.UpdateQuery(ticketCount, ticketNO);
        }

    }


}
