using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_EmployeeSurveysModel
    {
        /// <summary>
        /// 员工调查方案表
        /// </summary>
        public T_OA_REQUIREMASTER T_OA_REQUIREMASTER
        {
            get;
            set;
        }

        /// <summary>
        /// 员工调查申请表
        /// </summary>
        public T_OA_REQUIRE T_OA_REQUIRE
        {
            get;
            set;
        }

        /// <summary>
        /// 员工调查详表
        /// </summary>
        public List<T_OA_REQUIREDETAIL> T_OA_REQUIREDETAIL
        {
            get;
            set;
        }

        /// <summary>
        /// 员工调查详表(题目定义)
        /// </summary>
        public List<T_OA_REQUIREDETAIL2> T_OA_REQUIREDETAIL2
        {
            get;
            set;
        }

         /// <summary>
        /// 调查结果
        /// </summary>
        public List<T_OA_REQUIRERESULT> T_OA_REQUIRERESULT
        {
            get;
            set;
        } 
    }
}
