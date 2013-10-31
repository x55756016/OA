using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.Permission.DAL
{
    public class V_ProvinceCity
    {
        /// <summary>
        /// 城市ID
        /// </summary>
        public string PROVINCEID { get; set; }
        /// <summary>
        /// 国家ID
        /// </summary>
        public string COUNTRYID { get; set; }
        /// <summary>
        /// 地区名
        /// </summary>
        public string AREANAME { get; set; }
        /// <summary>
        /// 是否是省
        /// </summary>
        public string ISPROVINCE { get; set; }
        /// <summary>
        /// 是否是市
        /// </summary>
        public string ISCITY { get; set; }
        /// <summary>
        /// 是否是城镇
        /// </summary>
        public string ISCOUNTRYTOWN { get; set; }
        /// <summary>
        /// 城市对应的值
        /// </summary>
        public decimal? AREAVALUE { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public string FATHERID { get; set; }


    }
}
