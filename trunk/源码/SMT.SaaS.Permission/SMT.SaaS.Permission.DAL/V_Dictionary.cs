using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.Permission.DAL
{
    public class V_Dictionary
    {
        /// <summary>
        /// 字典ID        
        /// </summary>
        public string DICTIONARYID { get; set; }
        /// <summary>
        /// 字典类型
        /// </summary>
        public string DICTIONCATEGORY { get; set; }
        /// <summary>
        /// 字典名
        /// </summary>
        public string DICTIONARYNAME { get; set; }
        /// <summary>
        /// 字典值
        /// </summary>
        public decimal DICTIONARYVALUE { get; set; }
        /// <summary>
        /// 系统编码
        /// </summary>
        public string SYSTEMCODE { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public string FATHERID { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public decimal? ORDERNUMBER { get; set; }
    }
}
