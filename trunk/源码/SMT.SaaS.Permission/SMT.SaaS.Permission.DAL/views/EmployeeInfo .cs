/*
 *即时通讯员工信息接口
 *创建人：刘建兴
 *创建时间：2011-11-16 15:00
 * 
 * 主要用于修改员工信息时使用
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.Permission.DAL.views
{
    public class EmployeeInfo
    {
        /// <summary>
        /// 登录帐号
        /// </summary>
        public string loginAccount { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 联系地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public string AddCode { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string Nation { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 个人说明
        /// </summary>
        public string Remark { get; set; }
        
    }
}
