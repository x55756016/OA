using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.BLL;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Xml;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OrganizationService
    {
        [OperationContract]
        public void DoWork()
        {
            //在此处添加操作实现
            return;
        }

        [OperationContract]
        public void DoDrive()
        {
            //在此处添加操作实现
            return;
        }

        /// <summary>
        /// 新增员工档案
        /// </summary>
        /// <param name="ent">员工档案</param>
        [OperationContract]
        public V_EMPLOYEEPOST GetVEmployeePost()
        {
            return null;
        }

        /// <summary>
        /// 新增员工档案
        /// </summary>
        /// <param name="ent">员工档案</param>
        [OperationContract]
        public V_ATTENDANCEDEDUCTMASTER GetVAttendanceDeductMaster()
        {
            return null;
        }

        #region T_HR_DEPARTMENTDICTIONARY 部门字典服务
        /// <summary>
        /// 获取所有部门字典服务
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_DEPARTMENTDICTIONARY> GetDepartmentDictionaryAll()
        {
            using (DepartmentDictionaryBLL bll = new DepartmentDictionaryBLL())
            {
                IQueryable<T_HR_DEPARTMENTDICTIONARY> q = bll.GetDepartmentDictionaryAll();
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 获取部门字典信息
        /// </summary>
        /// <returns>返回部门字典列表</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENTDICTIONARY> DepartmentDictionaryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string Checkstate)
        {
            using (DepartmentDictionaryBLL bll = new DepartmentDictionaryBLL())
            {
                IQueryable<T_HR_DEPARTMENTDICTIONARY> q = bll.DepartmentDictionaryPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, Checkstate);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 根据部门字典信息添加部门字典
        /// </summary>
        /// <param name="obj">部门字典</param>
        [OperationContract]
        public void DepartmentDictionaryAdd(T_HR_DEPARTMENTDICTIONARY obj, ref string strMsg)
        {
            using (DepartmentDictionaryBLL bll = new DepartmentDictionaryBLL())
            {
                bll.DepartmentDictionaryAdd(obj, ref strMsg);
            }
        }
        /// <summary>
        /// 根据部门字典信息修改部门字典
        /// </summary>
        /// <param name="obj">部门字典</param>
        [OperationContract]
        public void DepartmentDictionaryUpdate(T_HR_DEPARTMENTDICTIONARY obj, ref string strMsg)
        {
            using (DepartmentDictionaryBLL bll = new DepartmentDictionaryBLL())
            {
                bll.DepartmentDictionaryUpdate(obj, ref strMsg);
            }
        }
        /// <summary>
        /// 根据部门字典ID删除部门字典
        /// </summary>
        /// <param name="strid">部门字典ID</param>
        /// <returns>是否成功删除</returns>
        [OperationContract]
        public bool DepartmentDictionaryDelete(string[] strid, ref string strMsg)
        {
            using (DepartmentDictionaryBLL bll = new DepartmentDictionaryBLL())
            {
                int rslt = bll.DepartmentDictionaryDelete(strid, ref strMsg);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据部门字典ID获取部门字典信息
        /// </summary>
        /// <param name="strid">部门字典ID</param>
        /// <returns>部门字典信息</returns>
        [OperationContract]
        public T_HR_DEPARTMENTDICTIONARY GetDepartmentDictionaryById(string strid)
        {
            using (DepartmentDictionaryBLL bll = new DepartmentDictionaryBLL())
            {
                return bll.GetDepartmentDictionaryById(strid);
            }
        }
        #endregion

        #region T_HR_POSTDICTIONARY 岗位字典服务
        /// <summary>
        /// 获取所有岗位字典服务
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_POSTDICTIONARY> GetPostDictionaryAll()
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                IQueryable<T_HR_POSTDICTIONARY> q = bll.GetPostDictionaryAll();
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 获取所有岗位字典列表
        /// </summary>
        /// <returns>返回岗位列表</returns>
        [OperationContract]
        public List<T_HR_POSTDICTIONARY> PostDictionaryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string Checkstate)
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                IQueryable<T_HR_POSTDICTIONARY> q = bll.PostDictionaryPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, Checkstate);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// 根据部门子典ID获取岗位字典
        /// </summary>
        /// <param name="departmentDictioanryID">部门字典ID</param>
        /// <returns>岗位字典实体集合</returns>
        [OperationContract]
        public List<T_HR_POSTDICTIONARY> GetPostDictionaryByDepartmentDictionayID(string departmentDictioanryID)
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                IQueryable<T_HR_POSTDICTIONARY> q = bll.GetPostDictionaryByDepartmentDictionayID(departmentDictioanryID);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// 添加岗位字典信息
        /// </summary>
        /// <param name="obj">岗位字典信息</param>
        [OperationContract]
        public void PostDictionaryAdd(T_HR_POSTDICTIONARY obj, ref string strMsg)
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                bll.PostDictionaryAdd(obj, ref strMsg);
            }
        }
        /// <summary>
        /// 修改岗位字典信息
        /// </summary>
        /// <param name="obj">岗位字典信息</param>
        [OperationContract]
        public void PostDictionaryUpdate(T_HR_POSTDICTIONARY obj, ref string strMsg)
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                bll.PostDictionaryUpdate(obj, ref strMsg);
            }
        }
        /// <summary>
        /// 删除岗位字典信息
        /// </summary>
        /// <param name="strid">岗位字典ID</param>
        /// <returns>返回岗位字典列表</returns>
        [OperationContract]
        public bool PostDictionaryDelete(string[] strid, ref string strMsg)
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                int rslt = bll.PostDictionaryDelete(strid, ref strMsg);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据岗位字典ID获取岗位字典信息
        /// </summary>
        /// <param name="strid">岗位字典ID</param>
        /// <returns>返回岗位字典信息</returns>
        [OperationContract]
        public T_HR_POSTDICTIONARY GetPostDictionaryById(string strid)
        {
            using (PostDictionaryBLL bll = new PostDictionaryBLL())
            {
                return bll.GetPostDictionaryById(strid);
            }
        }
        #endregion

        #region T_HR_COMPANY 公司设置
        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>可用公司信息列表</returns>
        [OperationContract]
        public List<T_HR_COMPANY> GetCompanyActived(string userID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                IQueryable<T_HR_COMPANY> ents = bll.GetCompanyActived(userID);
                //   Tracer.Serializer(ents.ToList(), "ActivedCompany");
                return ents.Count() > 0 ? ents.ToList() : null;
            }

        }
        /// <summary>
        /// 获取全部公司的视图
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_COMPANY> GetALLCompanyView(string userID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                
                IQueryable<V_COMPANY> ents = bll.GetALLCompanyView(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }

        }
        /// <summary>
        /// 根据实体权限获取公司视图
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_COMPANY> GetCompanyView(string userID, string perm, string entity)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                IQueryable<V_COMPANY> ents = bll.GetCompanyView(userID, perm, entity);
                return ents.Count() > 0 ? ents.ToList() : null;
            }

        }
        /// <summary>
        /// 获取指定时间后更新的是公司视图
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_COMPANY> GetCompanyViewByDateAndUser(string startDate, string userID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                IQueryable<V_COMPANY> ents = bll.GetCompanyViewByDateAndUser(startDate, userID);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }
            }

        }
        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>可用公司信息列表</returns>
        [OperationContract]
        public List<T_HR_COMPANY> GetCompanyByEntityPerm(string userID, string perm, string entity)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                IQueryable<T_HR_COMPANY> ents = null;

                if (string.IsNullOrEmpty(perm) || string.IsNullOrEmpty(entity))
                    ents = bll.GetCompanyActived(userID);
                else
                    ents = bll.GetCompanyActived(userID, perm, entity);

                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }

        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>可用公司信息列表</returns>
        [OperationContract]
        public List<T_HR_COMPANY> GetCompanyAll(string userID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                IQueryable<T_HR_COMPANY> ents = bll.GetCompanyAll(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 根据公司ID获取子公司
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetChildCompanyByCompanyID(List<string> companyIDs)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return bll.GetChildCompanyByCompanyID(companyIDs);
            }
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
        [OperationContract]
        public List<T_HR_COMPANY> CompanyPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string checkState)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                try
                {
                    IQueryable<T_HR_COMPANY> ents = bll.CompanyPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, checkState);
                    if (ents != null)
                    {
                        return ents.Count() > 0 ? ents.ToList() : null;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Debug(ex.ToString());
                }
                return null;
            }
            // return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 根据公司ID获取公司信息
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <returns>公司信息</returns>
        [OperationContract]
        public T_HR_COMPANY GetCompanyById(string companyID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return bll.GetCompanyById(companyID);
            }
        }
        /// <summary>
        /// 根据公司ID集合获取公司
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_COMPANY> GetCompanyByIds(string[] ids)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return bll.GetCompanyByIds(ids);
            }
        }
        /// <summary>
        /// 分步获取组织架构
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <param name="flag"></param>
        /// <param name="orgID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetOrgnazationsBystep(string userID, string perm, string entity, string flag, string orgID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                List<string> orgInfo = bll.GetOrgnazationsBystep(userID, perm, entity, flag, orgID);
                return orgInfo;
            }
        }
        /// <summary>
        /// 获取指定时间后更新的公司
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_COMPANY> GetCompanyWithSpecifiedTime(string startDate)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                var q = bll.GetCompanyWithSpecifiedTime(startDate);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 添加公司
        /// </summary>
        /// <param name="Company">公司实例</param>
        [OperationContract]
        public void CompanyAdd(T_HR_COMPANY entity, ref string strMsg)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                bll.CompanyAdd(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 变更公司
        /// </summary>
        /// <param name="Company">公司实例</param>
        [OperationContract]
        public void CompanyUpdate(T_HR_COMPANY entity, ref string strMsg)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                bll.CompanyUpdate(entity, ref  strMsg);
            }
        }
        /// <summary>
        /// 修改公司排序号
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void CompanyIndexUpdate(T_HR_COMPANY entity, ref string strMsg)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                bll.CompanyIndexUpdate(entity, ref strMsg);
            }
        }
        /// <summary>
        ///删除公司
        /// </summary>
        /// <param name="companyID">公司ID</param>
        [OperationContract]
        public void CompanyDelete(string id, ref string strMsg)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                bll.CompanyDelete(id, ref strMsg);
            }
        }

        /// <summary>
        ///撤消公司
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <returns>是否成功撤消公司</returns>
        [OperationContract]
        public bool CompanyCancel(T_HR_COMPANY entity, ref string strMsg)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return bll.CompanyCancel(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 判断选择的父公司，是否为当前添加(修改)的公司的子公司
        /// </summary>
        /// <param name="companyID">当前添加(修改)公司的ID</param>
        /// <param name="parentCompanyID">选择父公司的ID</param>
        /// <returns>是否为当前公司的子公司</returns>
        [OperationContract]
        public bool IsChildCompany(string companyID, string parentCompanyID)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return bll.IsChildCompany(companyID, parentCompanyID);
            }
        }

        /// <summary>
        /// 判断根公司是否是神州通集团
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsTopCompanySmt(string companyid)
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return IsTopCompanySmt(companyid);
            }
        }
        #endregion

        #region T_HR_DEPARTMENT 公司部门设置
        /// <summary>
        /// 获取全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> GetDepartmentActived(string userID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentActived(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 获取部门id，并附带公司信息
        /// </summary>
        /// <param name="strCompanyId">公司ID字符串，以‘，’分隔id</param>
        /// <returns></returns>
        [OperationContract]
        public List<V_DEPARTMENTSWITHCOMPANY> GetDepartmentByCompanyIDs(string strCompanyId)
        {
            using (DepartmentBLL departmentBll = new DepartmentBLL())
            {
                return departmentBll.GetDepartmentByCompanyIDs(strCompanyId);
            }
        }
        /// <summary>
        /// 获取全部部门视图
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_DEPARTMENT> GetAllDepartmentView(string userID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetAllDepartmentView(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 根据实体权限获取部门视图
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_DEPARTMENT> GetDepartmentView(string userID, string perm, string entity)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentView(userID, perm, entity);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 获取指定时间后更新的部门视图
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_DEPARTMENT> GetDepartmentViewByDateAndUser(string startDate, string userID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentViewByDateAndUser(startDate, userID);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 获取指定时间后更新的部门
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> GetDepartmentWithSpecifiedTime(string startDate)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentWithSpecifiedTime(startDate);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>可用公司信息列表</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> GetDepartmentByEntityPerm(string userID, string perm, string entity)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                IQueryable<T_HR_DEPARTMENT> ents = null;

                if (string.IsNullOrEmpty(perm) || string.IsNullOrEmpty(entity))
                    ents = bll.GetDepartmentActived(userID);
                else
                    ents = bll.GetDepartmentActived(userID, perm, entity);


                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }

        /// <summary>
        /// 根据部门ID获取子部门
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetChildDepartmentBydepartmentID(List<string> departmentIDs)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                return bll.GetChildDepartmentBydepartmentID(departmentIDs);
            }
        }


        /// <summary>
        /// 获取全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> GetDepartmentAll(string userID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentAll(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 获取指定公司的全部可用的部门信息
        /// </summary>
        /// <param name="companyID">部门ID</param>
        /// <returns>可用部门信息列表</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> GetDepartmentActivedByCompanyID(string companyID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentActivedByCompanyID(companyID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> GetDepartmentActivedByCompanyIDAndUserID(string companyID, string userID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.GetDepartmentActivedByCompanyIDAndUserID(companyID, userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的部门信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENT> DepartmentPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string checkState)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                var ents = bll.DepartmentPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, checkState);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 根据部门ID获取部门信息
        /// </summary>
        /// <param name="companyID">部门ID</param>
        /// <returns>部门信息</returns>
        [OperationContract]
        public T_HR_DEPARTMENT GetDepartmentById(string depID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                return bll.GetDepartmentById(depID);
            }
        }
        /// <summary>
        /// 根据部门ID获取所有上级机构
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        [OperationContract]
        public Dictionary<string, string> GetFatherByDepartmentID(string departmentID)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                return bll.GetFatherByDepartmentID(departmentID);
            }
        }
        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="entity">部门实例</param>
        [OperationContract]
        public void DepartmentAdd(T_HR_DEPARTMENT entity, ref string strMsg)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                bll.DepartmentAdd(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 变更部门
        /// </summary>
        /// <param name="entity">部门实例</param>
        [OperationContract]
        public void DepartmentUpdate(T_HR_DEPARTMENT entity, ref string strMsg)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                bll.DepartmentUpdate(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 更改部门排序
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void DepartmentIndexUpdate(T_HR_DEPARTMENT depart, ref string strMsg)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                bll.DepartmentIndexUpdate(depart, ref strMsg);
            }
        }
        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="depID">部门ID</param>
        [OperationContract]
        public void DepartmentDelete(string depID, ref string strMsg)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                bll.DepartmentDelete(depID, ref strMsg);
            }
        }

        /// <summary>
        /// 撤消部门
        /// </summary>
        /// <param name="entity">部门ID</param>
        [OperationContract]
        public void DepartmentCancel(T_HR_DEPARTMENT entity, ref string strMsg)
        {
            using (DepartmentBLL bll = new DepartmentBLL())
            {
                bll.DepartmentCancel(entity, ref strMsg);
            }
        }
        #endregion

        #region T_HR_POST 部门岗位设置
        /// <summary>
        /// 获取全部可用的岗位信息
        /// </summary>
        /// <returns>可用岗位信息列表</returns>
        [OperationContract]
        public List<T_HR_POST> GetPostActived(string userID)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.GetPostActived(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 获取全部岗位视图
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_POST> GetAllPostView(string userID)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.GetAllPostView(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 根据实体权限获取岗位
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_POST> GetPostView(string userID, string perm, string entity)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.GetPostView(userID, perm, entity);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 获取指定时间后更新的岗位视图
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_POST> GetPostViewByDateAndUser(string startDate, string userID)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.GetPostViewByDateAndUser(startDate, userID);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>可用公司信息列表</returns>
        [OperationContract]
        public List<T_HR_POST> GetPostByEntityPerm(string userID, string perm, string entity)
        {
            using (PostBLL bll = new PostBLL())
            {

                IQueryable<T_HR_POST> ents = null;

                if (string.IsNullOrEmpty(perm) || string.IsNullOrEmpty(entity))
                    ents = bll.GetPostActived(userID);
                else
                    ents = bll.GetPostActived(userID, perm, entity);


                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }


        /// <summary>
        /// 获取全部可用的岗位信息
        /// </summary>
        /// <returns>可用岗位信息列表</returns>
        [OperationContract]
        public List<T_HR_POST> GetPostAll(string userID)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.GetPostAll(userID);
                return ents.Count() > 0 ? ents.ToList() : null;
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的部门信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_POST> PostPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string checkState)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.PostPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, checkState);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            //  return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 获取指定时间后更新的岗位
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_POST> GetPostWithSpecifiedTime(string startDate)
        {
            using (PostBLL bll = new PostBLL())
            {
                var ents = bll.GetPostWithSpecifiedTime(startDate);
                if (ents != null)
                {
                    return ents.Count() > 0 ? ents.ToList() : null;
                }
                else
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// 根据岗位ID获取岗位信息
        /// </summary>
        /// <param name="companyID">岗位ID</param>
        /// <returns>岗位信息</returns>
        [OperationContract]
        public T_HR_POST GetPostById(string postID)
        {
            using (PostBLL bll = new PostBLL())
            {
                return bll.GetPostById(postID);
            }

        }
        /// <summary>
        /// 添加岗位
        /// </summary>
        /// <param name="Company">岗位实例</param>
        [OperationContract]
        public void PostAdd(T_HR_POST entity, ref string strMsg)
        {
            using (PostBLL bll = new PostBLL())
            {
                bll.PostAdd(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 变更岗位
        /// </summary>
        /// <param name="Company">岗位实例</param>
        [OperationContract]
        public void PostUpdate(T_HR_POST entity, ref string strMsg)
        {
            using (PostBLL bll = new PostBLL())
            {
                bll.PostUpdate(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 删除岗位
        /// </summary>
        /// <param name="postID">岗位ID</param>
        [OperationContract]
        public void PostDelete(string postID, ref string strMsg)
        {
            using (PostBLL bll = new PostBLL())
            {
                bll.PostDelete(postID, ref strMsg);
            }
        }

        /// <summary>
        /// 撤消岗位
        /// </summary>
        /// <param name="postID">岗位ID</param>
        [OperationContract]
        public void PostCancel(T_HR_POST postID, ref string strMsg)
        {
            using (PostBLL bll = new PostBLL())
            {
                bll.PostCancel(postID, ref strMsg);
            }
        }
        /// <summary>
        /// 获取岗位的人数
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        [OperationContract]
        public int GetPostNumber(string postID)
        {
            using (PostBLL bll = new PostBLL())
            {
                return bll.GetPostNumber(postID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsFatherPost(string postID)
        {
            PostBLL bll = new PostBLL();
            return bll.IsFatherPost(postID);
        }

        /// <summary>
        /// 获取部门下的所有岗位信息
        /// </summary>
        /// <param name="DepartmentID">部门ID</param>
        /// <param name="IsAll">是否是所有的岗位：包括了未生效的，为true返回所有岗位，为false返回生效的岗位</param>
        /// <returns>返回部门下的所有岗位集合</returns>
        [OperationContract]
        public List<T_HR_POST> GetAllPostByDepartId(string DepartmentID,bool IsAll)
        {
            using (PostBLL bll = new PostBLL())
            {
                return bll.GetAllPostByDepartId(DepartmentID,IsAll);
            }
        }

        /// <summary>
        /// 返回缺编的人数
        /// </summary>
        /// <param name="PostID">岗位ID</param>
        /// <returns></returns>
        [OperationContract]
        public int GetLackPostEmployeeCount(string PostID)
        {
            using (PostBLL bll = new PostBLL())
            {
                return bll.GetPostNumber(PostID);
            }
        }

        /// <summary>
        /// 返回在岗的人数
        /// </summary>
        /// <param name="PostID">岗位ID</param>
        /// <returns></returns>
        [OperationContract]
        public int GetOnPostEmployeeCount(string PostID)
        {
            using (PostBLL bll = new PostBLL())
            {
                return bll.GetOnPostNumber(PostID);
            }
        }

        #endregion

        #region T_HR_RELATIONPOST 关联岗位
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的部门信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_RELATIONPOST> RelationPostPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            using (RelationPostBLL bll = new RelationPostBLL())
            {
                return bll.RelationPostPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount);
            }
        }
        /// <summary>
        /// 添加关联岗位信息
        /// </summary>
        /// <param name="entity">关联岗位信息实体</param>
        [OperationContract]
        public void RelationPostAdd(T_HR_RELATIONPOST entity)
        {
            using (RelationPostBLL bll = new RelationPostBLL())
            {
                bll.RelationPostAdd(entity);
            }
        }
        /// <summary>
        /// 修改关联岗位信息
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public void RelationPostUpdate(T_HR_RELATIONPOST entity)
        {
            using (RelationPostBLL bll = new RelationPostBLL())
            {
                bll.RelationPostUpdate(entity);
            }
        }
        /// <summary>
        /// 删除关联岗位信息
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public bool RelationPostDelete(string[] relationPostIDs)
        {
            using (RelationPostBLL bll = new RelationPostBLL())
            {
                int rslt = bll.RelationPostDelete(relationPostIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据关联岗位ID查询岗位信息列表
        /// </summary>
        /// <param name="strID">关联岗位ID</param>
        /// <returns>返回关联岗位信息</returns>
        [OperationContract]
        public V_RELATIONPOST GetRelationPostByID(string strID)
        {
            using (RelationPostBLL bll = new RelationPostBLL())
            {
                return bll.GetRelationPostByID(strID);
            }
        }
        #endregion

        #region Lookup查询Entity的方法
        /// <summary>
        /// Lookup控件查询Entity的方法
        /// </summary> 
        /// <param name="userName">用户名称</param>
        /// <returns>Entity记录集Xml</returns>
        [OperationContract]
        public string GetLookupOjbects(EntityNames entityName, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            string objxml = "";
            object ents = Utility.GetLookupData(entityName, pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
            if (ents != null)
            {
                objxml = SerializerHelper.ContractObjectToXml(ents);
            }
            //object other = SerializerHelper.XmlToContractObject(objxml,typeof(T_HR_COMPANY[]));
            return objxml;
        }

        #endregion

        #region T_HR_COMPANYHISTORY 公司历史记录
        /// <summary>
        /// 获取当前日期最新数据的公司数据
        /// </summary>
        /// <param name="currentDate">日期</param>
        /// <returns>返回公司实例</returns>
        [OperationContract]
        public List<T_HR_COMPANYHISTORY> GetCompanyHistory(DateTime currentDate)
        {
            using (CompanyHistoryBLL bll = new CompanyHistoryBLL())
            {
                return bll.GetCompanyHistory(currentDate);
            }
            //var ents = bll.GetCompanyHistory(currentDate);
            //return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 添加公司历史记录
        /// </summary>
        /// <param name="entity">公司实例</param>
        [OperationContract]
        public void CompanyHistoryAdd(T_HR_COMPANYHISTORY entity)
        {
            using (CompanyHistoryBLL bll = new CompanyHistoryBLL())
            {
                bll.CompanyHistoryAdd(entity);
            }
        }
        /// <summary>
        /// 获取所有生效日期
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_COMPANYHISTORY> GetCompanyHistoryDate()
        {
            using (CompanyHistoryBLL bll = new CompanyHistoryBLL())
            {
                return bll.GetCompanyHistoryDate();
            }
        }
        #endregion

        #region T_HR_DEPARTMENTHISTORY 部门历史记录
        /// <summary>
        /// 获取当前日期最新数据的部门历史数据
        /// </summary>
        /// <param name="currentDate">日期</param>
        /// <returns>返回部门实例</returns>
        [OperationContract]
        public List<T_HR_DEPARTMENTHISTORY> GetDepartmentHistory(DateTime currentDate)
        {
            using (DepartmentHistoryBLL bll = new DepartmentHistoryBLL())
            {
                return bll.GetDepartmentHistory(currentDate);
            }
            //var ents = bll.GetDepartmentHistory(currentDate);
            //return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 添加部门历史记录
        /// </summary>
        /// <param name="entity">部门实例</param>
        [OperationContract]
        public void DepartmentHistoryAdd(T_HR_DEPARTMENTHISTORY entity)
        {
            using (DepartmentHistoryBLL bll = new DepartmentHistoryBLL())
            {
                bll.DepartmentHistoryAdd(entity);
            }
        }
        #endregion

        #region T_HR_POSTHISTORY 岗位历史记录
        /// <summary>
        /// 获取当前日期最新数据的岗位历史数据
        /// </summary>
        /// <param name="currentDate">日期</param>
        /// <returns>返回岗位历史实例</returns>
        [OperationContract]
        public List<T_HR_POSTHISTORY> GetPostHistory(DateTime currentDate)
        {
            using (PostHistoryBLL bll = new PostHistoryBLL())
            {
                return bll.GetPostHistory(currentDate);
            }
            //var ents = bll.GetPostHistory(currentDate);
            //return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 添加岗位历史记录
        /// </summary>
        /// <param name="entity">岗位历史实例</param>
        [OperationContract]
        public void PostHistoryAdd(T_HR_POSTHISTORY entity)
        {
            using (PostHistoryBLL bll = new PostHistoryBLL())
            {
                bll.PostHistoryAdd(entity);
            }
        }
        #endregion

        #region 统一更新审核状态
        /// <summary>
        /// 修改实体审核状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键值</param>
        /// <param name="CheckState">审核状态</param>
        [OperationContract]
        public void UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            using (CompanyHistoryBLL bll = new CompanyHistoryBLL())
            {
                bll.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);
            }
        }
        #endregion
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        /// <summary>
        /// 获取组织架构版本
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public string GetOrgVersion()
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                return bll.ReadVersion();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public void OrgChange()
        {
            using (CompanyBLL bll = new CompanyBLL())
            {
                bll.OrgChange();
            }
        }
    }
}
