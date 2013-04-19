using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_LandStatistic
    {
        /// <summary>
        /// 机构 ID。
        /// </summary>
        public string OrganizationID { get; set; }
        /// <summary>
        /// 机构名称。
        /// </summary>
        public string OrganizationName { get; set; }
        /// <summary>
        /// 登陆年份。
        /// </summary>
        public int LoginYear { get; set; }
        /// <summary>
        /// 登陆次数－1月。
        /// </summary>
        public int JanTimes { get; set; }
        /// <summary>
        /// 登陆次数－2月。
        /// </summary>
        public int FebTimes { get; set; }
        /// <summary>
        /// 登陆次数－3月。
        /// </summary>
        public int MarTimes { get; set; }
        /// <summary>
        /// 登陆次数－4月。
        /// </summary>
        public int AprTimes { get; set; }
        /// <summary>
        /// 登陆次数－5月。
        /// </summary>
        public int MayTimes { get; set; }
        /// <summary>
        /// 登陆次数－6月。
        /// </summary>
        public int JunTimes { get; set; }
        /// <summary>
        /// 登陆次数－7月。
        /// </summary>
        public int JulTimes { get; set; }
        /// <summary>
        /// 登陆次数－8月。
        /// </summary>
        public int AugTimes { get; set; }
        /// <summary>
        /// 登陆次数－9月。
        /// </summary>
        public int SepTimes { get; set; }
        /// <summary>
        /// 登陆次数－10月。
        /// </summary>
        public int OctTimes { get; set; }
        /// <summary>
        /// 登陆次数－11月。
        /// </summary>
        public int NovTimes { get; set; }
        /// <summary>
        /// 登陆次数－12月。
        /// </summary>
        public int DecTimes { get; set; }
        /// <summary>
        /// 登陆次数－小计。
        /// </summary>
        public int Subtotal { get; set; }
    }
}
