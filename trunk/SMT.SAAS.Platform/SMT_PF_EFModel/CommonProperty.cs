using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供基于Oracle数据库的子系统(应用)信息访问实现
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT_Platform_EFModel
{
    /// <summary>
    /// 公共属性
    /// </summary>
    public class CommonProperty
    {
        /// <summary>
        /// 
        /// </summary>
        public string OwnerID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string OwnerName
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string OwnerCompanyID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string OwnerDepartmentID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string OwnerPostID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CREATEUSERID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CREATEUSERNAME
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CREATECOMPANYID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CREATEDEPARTMENTID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CREATEPOSTID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CREATEDATE
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string UPDATEUSERID
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string UpdateUserName
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPDATEDATE
        {
            get;
            set;
        }
    }
}
