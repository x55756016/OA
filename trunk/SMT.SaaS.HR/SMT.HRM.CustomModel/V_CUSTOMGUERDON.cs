/*
 * 文件名：V_CUSTOMGUERDON.cs
 * 作  用：自定义薪资实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年3月12日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_CUSTOMGUERDON 
    {
        public V_CUSTOMGUERDON() { }

        /// <summary>
        /// 自定义薪资ID
        /// </summary>
        public string CUSTOMGUERDONID { get; set; }
        /// <summary>
        /// 薪资金额
        /// </summary>
        public decimal? GUERDONSUM { get; set; }
        /// <summary>
        /// 薪资设置名称
        /// </summary>
        public string GUERDONNAME { get; set; }

        /// <summary>
        /// 指定项目的属性
        /// </summary>
        public string GUERDONCATEGORY { get; set; }
        /// <summary>
        /// 计算类型
        /// </summary>
        public string CALCULATORTYPE { get; set; }

        /// <summary>
        /// 薪资标准ID
        /// </summary>
        public string SALARYSTANDARDID { get; set; }

        /// <summary>
        /// 自定义薪资设置ID
        /// </summary>
        public string CUSTOMGUERDONSETID { get; set; }
        /// <summary>
        /// 薪资标准名称
        /// </summary>
        public string SALARYSTANDARDNAME { get; set; }
    }
}
