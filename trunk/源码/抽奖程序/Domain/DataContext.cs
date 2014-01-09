using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asd.Award.Domain
{
    [Serializable]
    public class AwardEntity
    {
        public string TicketNO;
        public string Level;
        public string Remark;
    }

    public class DataContext
    {

        public static AwardMachine BuildAwardMachine()
        {
            AwardDataSet.TmpOptionDataTable dtOption = GetOption();

            AwardDataSet.TmpTicketDataTable dtTicket = GetTicket();

            AwardDataSet.TmpAwardDataTable dtAward = GetAward();

            var machine = new AwardMachine(dtOption, dtTicket, dtAward);
            return machine;
        }

        public static AwardEntity[] BuildAwardEntity(AwardDataSet.TmpAwardRow[] ongoing)
        {
            var result = new AwardEntity[ongoing.Length];

            for (var i = 0; i < result.Length; i++)
            {
                var item = new AwardEntity();
                item.TicketNO = ongoing[i].TicketNO;
                item.Level = ongoing[i].Level;
                item.Remark = ongoing[i].Remark;
                result[i] = item;
            }
            return result;
        }

        public static AwardDataSet.TmpAwardDataTable GetAward()
        {
            AwardDataSet.TmpAwardDataTable dtAward = null;
            using (var dx = new AwardDataSetTableAdapters.TmpAwardTableAdapter())
            {
                dtAward = dx.GetData();
            }
            return dtAward;
        }

        public static AwardDataSet.TmpTicketDataTable GetTicket()
        {
            AwardDataSet.TmpTicketDataTable dtTicket = null;
            using (var dx = new AwardDataSetTableAdapters.TmpTicketTableAdapter())
            {
                dtTicket = dx.GetData();
            }
            return dtTicket;
        }

        public static AwardDataSet.TmpOptionDataTable GetOption()
        {
            AwardDataSet.TmpOptionDataTable dtOption = null;
            using (var dx = new AwardDataSetTableAdapters.TmpOptionTableAdapter())
            {
                dtOption = dx.GetData();
            }
            return dtOption;
        }

        public static AwardDataSet.TmpSettingDataTable GetSetting()
        {
            AwardDataSet.TmpSettingDataTable dtSetting = null;
            using (var dx = new AwardDataSetTableAdapters.TmpSettingTableAdapter())
            {
                dtSetting = dx.GetData();
            }
            return dtSetting;
        }
        public static void Update(AwardDataSet dataSet)
        {
            using (var dx = new AwardDataSetTableAdapters.TableAdapterManager())
            {
                dx.UpdateAll(dataSet);
            }
        }

        public static void Update(AwardDataSet.TmpTicketDataTable ticket)
        {
            using (var dx = new AwardDataSetTableAdapters.TmpTicketTableAdapter())
            {
                dx.Update(ticket);
            }
        }

        public static void Update(AwardDataSet.TmpAwardDataTable award)
        {
            using (var dx = new AwardDataSetTableAdapters.TmpAwardTableAdapter())
            {
                dx.Update(award);
            }
        }

        public static void ClearTicket()
        {
            using (var dx = new AwardDataSetTableAdapters.TmpTicketTableAdapter())
            {
                dx.DeleteQuery();
            }
        }

        public static void ClearAward()
        {
            using (var dx = new AwardDataSetTableAdapters.TmpAwardTableAdapter())
            {
                dx.DeleteQuery();
            }
        }

        public static void DiscardAward(string ticketNO)
        {
            using (var dx = new AwardDataSetTableAdapters.TmpAwardTableAdapter())
            {
                dx.DeleteByTicketNO(ticketNO);
            }
        }
        /// <summary>
        /// 更新票数
        /// </summary>
        /// <param name="ticketNO">所属地区</param>
        /// <param name="ticketCount">总票数</param>
        public static void UpdateQuery(int ticketCount,string ticketNO)
        {
            using (var dx = new AwardDataSetTableAdapters.TmpTicketTableAdapter())
            {
                dx.UpdateQuery(ticketCount, ticketNO);
            }
        }
    }
}
