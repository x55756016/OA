/*
 * 文件名：SalaryLoginBLL.cs
 * 作  用：薪资内部登录
 * 创建人： 喻建华
 * 创建时间：2010年12月8日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using SMT_HRM_EFModel;

using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class SalaryLoginBLL : BaseBll<T_HR_SYSTEMSETTING>
    {
        /// <summary>
        /// 薪资登录核查
        /// </summary>
        /// <param name="employeid">员工</param>
        /// <param name="pwd">密码</param>
        /// <returns>返回结果</returns>
        public bool LoginCheck(string employeid, string pwd)
        {
            var ents = from a in dal.GetTable()
                       where a.OWNERID == employeid && a.PARAMETERVALUE == pwd && a.MODELTYPE == "4"
                       select a;
            if (ents.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 新增系统参数设置
        /// </summary>
        /// <param name="entity">系统参数设置实体</param>
        public void AddSystemParamSet(T_HR_SYSTEMSETTING entity)
        {
            dal.Add(entity);
        }

        /// <summary>
        /// 入职新增薪资登录密码
        /// </summary>
        /// <param name="employeeid">员工自己的ID</param>
        /// <param name="employeeName">员工姓名</param>
        /// <param name="pwd">薪资登录密码</param>
        public void AddSalaryPassword(string employeeid, string employeeName, string pwd)
        {
            try
            {
                T_HR_SYSTEMSETTING entity = new T_HR_SYSTEMSETTING();
                entity.SYSTEMSETTINGID = Guid.NewGuid().ToString();
                entity.OWNERID = employeeid;
                entity.MODELVALUE = "AUTOSALARYPWD";
                entity.PARAMETERNAME = employeeName;
                entity.MODELTYPE = "4";
                entity.REMARK = "AUTOSALARYPWD";
                entity.PARAMETERVALUE = pwd;
                //entity.PARAMETERVALUE = AES.Encrypt(pwd);
                entity.CREATEDATE = System.DateTime.Now;
                dal.Add(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("AddSalaryPassword:" + ex.Message);
            }
        }

        public string GetSalaryPassword(string employeeName)
        {
            var ents = from a in dal.GetTable()
                       join b in dal.GetTable<T_HR_EMPLOYEE>() on a.OWNERID equals b.EMPLOYEEID
                       where b.EMPLOYEEENAME == employeeName
                       select a;
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault().PARAMETERVALUE;
            }
            return "Null";
        }

        /// <summary>
        /// 根据系统参数ID查询实体
        /// </summary>
        /// <param name="SystemParamSetID">系统参数ID</param>
        /// <returns>返回系统参数实体</returns>
        public T_HR_SYSTEMSETTING GetSystemParamSet(string SystemParamSetID)
        {
            var ents = from a in dal.GetTable()
                       where a.SYSTEMSETTINGID == SystemParamSetID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据系统参数类型查询实体
        /// </summary>
        /// <param name="SystemParamSetID">系统类型</param>
        /// <returns>返回系统参数实体</returns>
        public List<T_HR_SYSTEMSETTING> GetSystemParamSetByType(string modeType)
        {
            var ents = from a in dal.GetTable()
                       where a.MODELTYPE == modeType
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_SYSTEMSETTING> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SYSTEMSETTING");


            IQueryable<T_HR_SYSTEMSETTING> ents = dal.GetObjects(); // dal.GetObjects<T_HR_SYSTEMSETTING>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
                //ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SYSTEMSETTING>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 更新系统参数
        /// </summary>
        /// <param name="entity">系统参数实体</param>
        public void SystemParamSetUpdate(T_HR_SYSTEMSETTING entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.SYSTEMSETTINGID == entity.SYSTEMSETTINGID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_SYSTEMSETTING>(entity, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除系统参数记录，可同时删除多行记录
        /// </summary>
        /// <param name="SystemParamSetID">系统参数记录ID数组</param>
        /// <returns></returns>
        public int SystemParamSetDelete(string[] SystemParamSetID)
        {
            foreach (string id in SystemParamSetID)
            {
                var ents = from e in dal.GetObjects<T_HR_SYSTEMSETTING>()
                           where e.SYSTEMSETTINGID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }
    }
}
