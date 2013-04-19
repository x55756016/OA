/*
 * 文件名：V_ATTENDANCEDEDUCTMASTER.cs
 * 作  用：T_HR_ATTENDANCEDEDUCTMASTER 实体扩展类
 * 创建人：吴鹏
 * 创建时间：2010-3-18 14:53:08
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;

namespace SMT.HRM.CustomModel
{
    public class V_ATTENDANCEDEDUCTMASTER : EntityObject
    {
        public V_ATTENDANCEDEDUCTMASTER()
        { }

        public SMT_HRM_EFModel.T_HR_ATTENDANCEDEDUCTMASTER T_HR_ATTENDANCEDEDUCTMASTER { get; set; }

        /// <summary>
        /// 0迟到,1 早退,2未刷卡,3旷工(字典值)
        /// </summary>
        public string ATTENDABNORMALTYPENAME { get; set; }

        /// <summary>
        /// 1、每次扣X元；2、按日薪/分钟 * 迟到的分钟数（不足一分按一分算），最低扣 X 元；3分段扣款(字典值)
        /// </summary>
        public string FINETYPENAME { get; set; }

        /// <summary>
        /// 是否累加(字典值)
        /// </summary>
        public string ISACCUMULATINGNAME { get; set; }

        /// <summary>
        /// 是否扣全勤(字典值)
        /// </summary>
        public string ISPERFECTATTENDANCEFACTORNAME { get; set; }
    }
}