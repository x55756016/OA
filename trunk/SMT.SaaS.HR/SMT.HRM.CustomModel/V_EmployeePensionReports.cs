using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工薪酬、社保明细
    /// </summary>
    public class V_EmployeePensionReports
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        public string COMPANYNAME { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string ENDDATE { get; set; }
        /// <summary>
        /// 正式员工医疗参保人数
        /// </summary>
        public decimal? FULLMEDICALNUMERS { get; set; }
        /// <summary>
        /// 正式员工失业人数
        /// </summary>
        public decimal? FULLLOSTNUMERS { get; set; }
        /// <summary>
        /// 正式员工住房公积金人数
        /// </summary>
        public decimal? FULLCPFNUMERS { get; set; }
        /// <summary>
        /// 正式员工生育人数
        /// </summary>
        public decimal? FULLBEARNUMERS { get; set; }
        /// <summary>
        /// 正式员工工伤参保人数
        /// </summary>
        public decimal? FULLINJURYNUMERS { get; set; }
        /// <summary>
        /// 正式员工养老参保人数
        /// </summary>
        public decimal? FULLPENSIONNUMERS { get; set; }
        /// <summary>
        /// 单位缴交医疗数
        /// </summary>
        public decimal? FULLCOMPANYMEDICALNUMERS { get; set; }
        /// <summary>
        /// 个人缴交失业费用
        /// </summary>
        public decimal? FULLCOMPANYLOSTNUMERS { get; set; }
        /// <summary>
        /// 单位缴交住房公积金
        /// </summary>
        public decimal? FULLCOMPANYCPFNUMERS { get; set; }
        /// <summary>
        /// 公司缴交生育金
        /// </summary>
        public decimal? FULLCOMPANYBEARNUMERS { get; set; }
        /// <summary>
        /// 公司缴交工伤金额
        /// </summary>
        public decimal? FULLCOMPANYINJURYNUMERS { get; set; }
        /// <summary>
        /// 公司缴交养老金
        /// </summary>
        public decimal? FULLCOMPANYPENSIONNUMERS { get; set; }

        /// <summary>
        /// 个人缴交医疗数
        /// </summary>
        public decimal? FULLOWNERMEDICALNUMERS { get; set; }
        /// <summary>
        /// 个人缴交失业费用
        /// </summary>
        public decimal? FULLOWNERLOSTNUMERS { get; set; }
        /// <summary>
        /// 个人缴交住房公积金
        /// </summary>
        public decimal? FULLOWNERCPFNUMERS { get; set; }
        /// <summary>
        /// 个人缴交生育金
        /// </summary>
        public decimal? FULLOWNERBEARNUMERS { get; set; }
        /// <summary>
        /// 个人缴交工伤金
        /// </summary>
        public decimal? FULLOWNERINJURYNUMERS { get; set; }
        /// <summary>
        /// 个人缴交养老金
        /// </summary>
        public decimal? FULLOWNERPENSIONNUMERS { get; set; }
        /// <summary>
        /// 缴交人数小计
        /// </summary>
        public decimal? FULLNUMERSTOTAL { get; set; }
        /// <summary>
        /// 单位缴费小计
        /// </summary>
        public decimal? FULLCOMPANYTOTAL { get; set; }
        /// <summary>
        /// 个人缴费小计
        /// </summary>
        public decimal? FULLOWNERTOTAL { get; set; }
        /// <summary>
        /// 固定收入合计
        /// </summary>
        public decimal? TOTOALFIXINCOMEMONEY { get; set; }
        /// <summary>
        /// 绩效合计
        /// </summary>
        public decimal? TOTALPERFORMANCEMONEY { get; set; }
        /// <summary>
        /// 非正式员工医疗参保人数
        /// </summary>
        public decimal? TEMPMEDICALNUMERS { get; set; }
        /// <summary>
        /// 非正式员工失业人数
        /// </summary>
        public decimal? TEMPLOSTNUMERS { get; set; }
        /// <summary>
        /// 非正式员工住房公积金人数
        /// </summary>
        public decimal? TEMPCPFNUMERS { get; set; }
        /// <summary>
        /// 非正式员工生育人数
        /// </summary>
        public decimal? TEMPBEARNUMERS { get; set; }        
        /// <summary>
        /// 非正式员工工伤参保人数
        /// </summary>
        public decimal? TEMPINJURYNUMERS { get; set; }
        /// <summary>
        /// 养老参保人数
        /// </summary>
        public decimal? TEMPPENSIONNUMERS { get; set; }
        /// <summary>
        /// 非正式员工单位缴交医疗数
        /// </summary>
        public decimal? TEMPCOMPANYMEDICALNUMERS { get; set; }
        /// <summary>
        /// 非正式员工个人缴交失业费用
        /// </summary>
        public decimal? TEMPCOMPANYLOSTNUMERS { get; set; }
        /// <summary>
        /// 单位缴交住房公积金
        /// </summary>
        public decimal? TEMPCOMPANYCPFNUMERS { get; set; }
        /// <summary>
        /// 非正式员工单位缴交
        /// </summary>
        public decimal? TEMPCOMPANYBEARNUMERS { get; set; }
        /// <summary>
        /// 非正式员工工伤单位缴交
        /// </summary>
        public decimal? TEMPCOMPANYINJURYNUMERS { get; set; }
        /// <summary>
        /// 养老单位缴交
        /// </summary>
        public decimal? TEMPCOMPANYPENSIONNUMERS { get; set; }
        /// <summary>
        /// 非正式员工个人缴交医疗数
        /// </summary>
        public decimal? TEMPOWNERMEDICALNUMERS { get; set; }
        /// <summary>
        /// 非正式员工个人缴交失业费用
        /// </summary>
        public decimal? TEMPOWNERLOSTNUMERS { get; set; }
        /// <summary>
        /// 非正式员工个人缴交住房公积金
        /// </summary>
        public decimal? TEMPOWNERCPFNUMERS { get; set; }
        /// <summary>
        /// 非正式员工个人缴交
        /// </summary>
        public decimal? TEMPOWNERBEARNUMERS { get; set; }
        /// <summary>
        /// 非正式员工工伤个人缴交
        /// </summary>
        public decimal? TEMPOWNERINJURYNUMERS { get; set; }
        /// <summary>
        /// 养老个人缴交
        /// </summary>
        public decimal? TEMPOWNERPENSIONNUMERS { get; set; }
        /// <summary>
        /// 非合同工缴交人数小计
        /// </summary>
        public decimal? TEMPNUMERS { get; set; }
        /// <summary>
        /// 公司缴交小计
        /// </summary>
        public decimal? TEMPCOMPANYTOTAL { get; set; }
        /// <summary>
        /// 非合同工个人缴交小计
        /// </summary>
        public decimal? TEMPOWNERTOTAL { get; set; }
        /// <summary>
        /// 非正式员工固定收入合计
        /// </summary>
        public decimal? TEMPTOTOALFIXINCOMEMONEY { get; set; }
        /// <summary>
        /// 非正式员工绩效合计
        /// </summary>
        public decimal? TEMPTOTALPERFORMANCEMONEY { get; set; }

    }
}
