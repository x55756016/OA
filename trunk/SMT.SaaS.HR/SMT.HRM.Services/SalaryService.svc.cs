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
using System.Collections.ObjectModel;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.HRM.CustomModel;
using System.Configuration;
using System.Web;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SalaryService
    {
        #region T_HR_SalarySolution 薪资方案

        /// <summary>
        /// 新增薪资方案
        /// </summary>
        /// <param name="ent">薪资方案</param>
        [OperationContract]
        public void SalarySolutionAdd(T_HR_SALARYSOLUTION entity)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                bll.SalarySolutionAdd(entity);
            }
        }

        /// <summary>
        /// 搜索相同的薪资方案名
        /// </summary>
        /// <param name="solutionName">薪资方案名</param>
        [OperationContract]
        public bool SalarySolutionSameSearch(string solutionName)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                return bll.SalarySolutionSameSearch(solutionName);
            }
        }

        /// <summary>
        /// 更新薪资方案
        /// </summary>
        /// <param name="entity">薪资方案</param>
        [OperationContract]
        public void SalarySolutionUpdate(T_HR_SALARYSOLUTION entity)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                bll.SalarySolutionUpdate(entity);
            }
        }

        /// <summary>
        /// 根据薪资方案查询薪资方案
        /// </summary>
        /// <param name="employeeID">薪资方案ID</param>
        /// <returns>薪资方案</returns>
        [OperationContract]
        public T_HR_SALARYSOLUTION GetSalarySolutionByID(string ID)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                return bll.GetSalarySolutionByID(ID);
            }
        }

        /// <summary>
        /// 删除薪资方案
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool SalarySolutionDelete(string[] IDs)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                int rslt = bll.SalarySolutionDelete(IDs);

                return (rslt > 0);
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
        public List<T_HR_SALARYSOLUTION> GetSalarySolutionPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, string CheckState)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                IQueryable<T_HR_SALARYSOLUTION> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        #region ---
        /// <summary>
        /// 启动发薪日期定时提醒功能
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        [OperationContract]
        public void GetSalarySolutionEngineXml(T_HR_SALARYSOLUTION entTemp)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                bll.GetSalarySolutionEngineXml(entTemp);
            }
        }

        /// <summary>
        /// 对指定薪资发放日期定时提醒
        /// </summary>
        /// <param name="CreateUserID"></param>
        /// <returns></returns>
        [OperationContract]
        public void TimingPay(T_HR_SALARYSOLUTION salarysolution)
        {
            using (SalarySolutionBLL bll = new SalarySolutionBLL())
            {
                bll.TimingPay(salarysolution);
            }
        }
        #endregion

        #endregion


        #region T_HR_SALARYSTANDARD 薪资标准

        /// <summary>
        /// 批量生成薪资标准
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="postlevel"></param>
        /// <param name="standModel"></param>
        /// <returns></returns>
        [OperationContract]
        public string CreateSalaryStandBatch(T_HR_SALARYSOLUTION solution, Dictionary<string, string> postlevel, T_HR_SALARYSTANDARD standModel)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                return bll.CreateSalaryStandBatch(solution, postlevel, standModel);
            }
        }
        /// <summary>
        /// 根据员工的ID和方案ID获取员工的 薪资标准
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="solutionID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SALARYSTANDARD GetStandardByEmployeeIDAndSolutionID(string employeeID, string solutionID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetStandardByEmployeeIDAndSolutionID(employeeID, solutionID);
            }
        }
        /// <summary>
        /// 根据员工的ID和方案ID 薪资级别 获取员工的 薪资标准
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="solutionID"></param>
        /// <param name="salaryLevel"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SALARYSTANDARD GetStandardByEmployeeIDAndSolutionIDAndSalarylevel(string employeeID, string solutionID, int salaryLevel)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetStandardByEmployeeIDAndSolutionIDAndSalarylevel(employeeID, solutionID, salaryLevel);
            }
        }

        /// <summary>
        /// 新增薪资标准
        /// </summary>
        /// <param name="ent">薪资标准</param>
        [OperationContract]
        public void SalaryStandardAdd(T_HR_SALARYSTANDARD entity)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                bll.SalaryStandardAdd(entity);
            }
        }

        [OperationContract]
        public string AddSalaryStanderAndItems(T_HR_SALARYSTANDARD entity, List<T_HR_SALARYSTANDARDITEM> salaryItems)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                return bll.AddSalaryStanderAndItems(entity, salaryItems);
            }
        }
        /// <summary>
        /// 更新薪资标准
        /// </summary>
        /// <param name="entity">薪资标准</param>
        [OperationContract]
        public void SalaryStandardUpdate(T_HR_SALARYSTANDARD entity)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                bll.SalaryStandardUpdate(entity);
            }
        }

        /// <summary>
        /// 根据薪资方案查询薪资标准
        /// </summary>
        /// <param name="employeeID">薪资标准ID</param>
        /// <returns>薪资标准</returns>
        [OperationContract]
        public T_HR_SALARYSTANDARD GetSalaryStandardByID(string ID)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                return bll.GetSalaryStandardByID(ID);
            }
        }

        /// <summary>
        /// 删除薪资标准
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool SalaryStandardDelete(string[] IDs)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                int rslt = bll.SalaryStandardDelete(IDs);
                return (rslt > 0);
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
        public List<T_HR_SALARYSTANDARD> GetSalaryStandardPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, int sType, string sValue, string CheckState)
        {
            using (SalaryStandardBLL bll = new SalaryStandardBLL())
            {
                IQueryable<T_HR_SALARYSTANDARD> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, sType, sValue, CheckState);
                if (q == null)
                {
                    return null;
                }
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region T_HR_POSTLEVELDISTINCTION 岗位等级薪资
        [OperationContract]
        public List<T_HR_POSTLEVELDISTINCTION> GetAllPostLevelDistinction()
        {
            using (PostLevelDistinctionBLL bll = new PostLevelDistinctionBLL())
            {
                return bll.GetAllPostLevelDistinction();
            }
        }
        [OperationContract]
        public List<T_HR_POSTLEVELDISTINCTION> GetPostLevelDistinctionBySystemID(string SalarySystemID)
        {
            using (PostLevelDistinctionBLL bll = new PostLevelDistinctionBLL())
            {
                return bll.GetPostLevelDistinctionBySystemID(SalarySystemID);
            }
        }
        [OperationContract]
        public T_HR_POSTLEVELDISTINCTION GetPostLevelDistinctionByID(string id)
        {
            using (PostLevelDistinctionBLL bll = new PostLevelDistinctionBLL())
            {
                return bll.GetPostLevelDistinctionByID(id);
            }
        }

        [OperationContract]
        public void PostLevelDistinctionUpdate(List<T_HR_POSTLEVELDISTINCTION> ents)
        {
            using (PostLevelDistinctionBLL bll = new PostLevelDistinctionBLL())
            {
                bll.PostLevelDistinctionUpdate(ents);
            }
        }
        [OperationContract]
        public string PostLevelDistinctionADD(T_HR_POSTLEVELDISTINCTION ent)
        {
            using (PostLevelDistinctionBLL bll = new PostLevelDistinctionBLL())
            {
                return bll.PostLevelDistinctionADD(ent);
            }
        }
        #endregion

        #region T_HR_SALARYLEVEL 薪资级别
        [OperationContract]
        public List<T_HR_SALARYLEVEL> GetAllSalaryLevel()
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                return bll.GetAllSalaryLevel();
            }
        }
        [OperationContract]
        public List<T_HR_SALARYLEVEL> GetSalaryLevelBySystemID(string systemID)
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                return bll.GetSalaryLevelBySystemID(systemID);
            }
        }
        [OperationContract]
        public T_HR_SALARYLEVEL GetSalaryLevelByID(string ID)
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                return bll.GetSalaryLevelByID(ID);
            }
        }
        [OperationContract]
        public void SalaryLevelUpdate(T_HR_SALARYLEVEL obj)
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                bll.SalaryLevelUpdate(obj);
            }
        }
        [OperationContract]
        public void SalaryLevelADD(T_HR_SALARYLEVEL obj)
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                bll.SalaryLevelADD(obj);
            }
        }
        [OperationContract]
        public void GenerateSalaryLevel(int lowSalaryLevel, int highSalaryLevel, string systemID, string userid)
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                bll.GenerateSalaryLevel(lowSalaryLevel, highSalaryLevel, systemID, userid);
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
        public List<T_HR_SALARYLEVEL> GetSalaryLevelPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (SalaryLevelBLL bll = new SalaryLevelBLL())
            {
                IQueryable<T_HR_SALARYLEVEL> q = bll.GetSalaryLevelPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region T_HR_SALARYSOLUTIONASSIGN 薪资方案分配
        [OperationContract]
        public List<V_SALARYSOLUTIONASSIGN> GetSalarySolutionAssignPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL())
            {
                List<V_SALARYSOLUTIONASSIGN> q = bll.SalarySolutionAssignPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q;
            }
        }

        /// <summary>
        /// 新增薪资方案分配
        /// </summary>
        /// <param name="ent">薪资方案分配</param>
        [OperationContract]
        public void SalarySolutionAssignAdd(T_HR_SALARYSOLUTIONASSIGN entity)
        {
            using (SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL())
            {
                bll.AddSalarySolutionAssign(entity);
            }
        }

        /// <summary>
        /// 更新薪资方案分配
        /// </summary>
        /// <param name="entity">薪资方案分配</param>
        [OperationContract]
        public void SalarySolutionAssignUpdate(T_HR_SALARYSOLUTIONASSIGN entity)
        {
            using (SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL())
            {
                bll.SalarySolutionAssignUpdate(entity);
            }
        }

        /// <summary>
        /// 获取薪资方案分配
        /// </summary>
        /// <param name="employeeID">薪资方案分配ID</param>
        /// <returns>薪资方案分配</returns>
        [OperationContract]
        public T_HR_SALARYSOLUTIONASSIGN GetSalarySolutionAssignByID(string ID)
        {
            using (SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL())
            {
                return bll.GetSalarySolutionAssignByID(ID);
            }
        }

        /// <summary>
        /// 获取薪资方案分配视图
        /// </summary>
        /// <param name="employeeID">薪资方案分配视图ID</param>
        /// <returns>薪资方案分配视图</returns>
        [OperationContract]
        public V_SALARYSOLUTIONASSIGN GetSalarySolutionAssignViewByID(string ID)
        {
            using (SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL())
            {
                return bll.GetSalarySolutionAssignViewByID(ID);
            }
        }

        /// <summary>
        /// 删除薪资方案分配
        /// </summary>
        /// <param name="IDs">薪资方案分配IDs</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool SalarySolutionAssignDelete(string[] IDs)
        {
            using (SalarySolutionAssignBLL bll = new SalarySolutionAssignBLL())
            {
                int rslt = bll.SalarySolutionAssignDelete(IDs);

                return (rslt > 0);
            }
        }


        #endregion

        #region T_HR_SALARYARCHIVE 薪资档案
        /// <summary>
        /// 新增薪资档案
        /// </summary>
        /// <param name="ent">薪资档案</param>
        [OperationContract]
        public void SalaryArchiveAdd(T_HR_SALARYARCHIVE entity)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                bll.AddSalaryArchive(entity);
            }
        }
        /// <summary>
        /// 新增薪资档案项目
        /// </summary>
        /// <param name="ent">薪资档案</param>
        [OperationContract]
        public int SalaryArchiveItemAdd(T_HR_SALARYARCHIVEITEM entity)
        {
            using (SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL())
            {
                return bll.SalaryArchiveItemAdd(entity);
            }
        }
        /// <summary>
        /// 根据分配类型生成薪资档案
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectID"></param>
        /// <param name="stander"></param>
        /// <param name="createType">档案创建类型:异动和非异动(false异动)</param>
        [OperationContract]
        public void CreateSalaryArchive(int objectType, string objectID, T_HR_SALARYARCHIVE archive, bool createType)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                bll.CreateSalaryArchive(objectType, objectID, archive, createType);
            }
        }
        /// <summary>
        /// 获取固定薪资
        /// </summary>
        /// <param name="postlevel">岗位级别</param>
        /// <param name="salarylevel">薪资级别</param>
        /// <param name="solutionID">薪资方案ID</param>
        /// <returns></returns>
        [OperationContract]
        public decimal GetFixSalary(decimal postlevel, string salarylevel, string solutionID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetFixSalary(postlevel, salarylevel, solutionID);
            }
        }

        [OperationContract]
        public List<string> GetOldFixSalary(string employeeid)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetOldFixSalary(employeeid);
            }
        }
        /// <summary>
        /// 更新薪资档案
        /// </summary>
        /// <param name="entity">薪资档案</param>
        [OperationContract]
        public void SalaryArchiveUpdate(T_HR_SALARYARCHIVE entity)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                bll.SalaryArchiveUpdate(entity);
            }
        }

        /// <summary>
        /// 根据员工ID更新薪资档案
        /// </summary>
        /// <param name="noid"></param>
        /// <param name="employeeid">员工ID</param>
        /// <param name="year">终止年份</param>
        /// <param name="month">终止月份</param>
        [OperationContract]
        public bool SalaryArchiveUpdateByEmployee(string noid, string employeeid, int year, int month)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.SalaryArchiveUpdateByEmployee(noid, employeeid, year, month);
            }
        }

        /// <summary>
        /// 查询薪资档案
        /// </summary>
        /// <param name="employeeID">薪资档案ID</param>
        /// <returns>薪资档案</returns>
        [OperationContract]
        public T_HR_SALARYARCHIVE GetSalaryArchiveByID(string ID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetSalaryArchiveByID(ID);
            }
        }

        /// <summary>
        /// 查询薪资档案
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public V_SALARYARCHIVEMASTER GetSalaryArchiveMasterByID(string ID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetSalaryArchiveMasterByID(ID);
            }
        }

        [OperationContract]
        public T_HR_SALARYARCHIVE GetSalaryArchiveByEmployeeID(string employeeID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.GetSalaryArchiveByEmployeeID(employeeID);
            }
        }
        /// <summary>
        /// 删除薪资档案
        /// </summary>
        /// <param name="resumeID">档案ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool SalaryArchiveDelete(string[] IDs)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                int rslt = bll.SalaryArchiveDelete(IDs);
                return (rslt > 0);
            }
        }

        /// <summary>
        /// 薪资档案删除(当月删除)
        /// </summary>
        /// <param name="noID"></param>
        /// <param name="employeeid">员工ID</param>
        /// <returns></returns>
        [OperationContract]
        public int SalaryArchiveDeleteByMonth(string noID, string employeeid)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.SalaryArchiveDelete(noID, employeeid);
            }
        }
        /// <summary>
        /// 薪资档案删除(过期删除)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns></returns>
        [OperationContract]
        public int SalaryArchiveDeleteByExpired(string employeeid)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                return bll.SalaryArchiveDelete(employeeid);
            }
        }

        /// <summary>
        /// 删除薪资档案薪资项目
        /// </summary>
        /// <param name="resumeID">档案ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool SalaryArchiveItemDelete(string[] IDs)
        {
            using (SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL())
            {
                int rslt = bll.SalaryArchiveItemDelete(IDs);

                return (rslt > 0);
            }
        }
        /// <summary>
        ///更新档案薪资项目
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void SalaryArchiveItemUpdate(T_HR_SALARYARCHIVEITEM obj)
        {
            using (SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL())
            {
                bll.SalaryArchiveItemUpdate(obj);
            }

        }

        ///// <summary>
        ///// 用于实体Grid中显示数据的分页查询
        ///// </summary>
        ///// <param name="pageIndex">当前页</param>
        ///// <param name="pageSize">每页显示条数</param>
        ///// <param name="sort">排序字段</param>
        ///// <param name="filterString">过滤条件</param>
        ///// <param name="paras">过滤条件中的参数值</param>
        ///// <param name="pageCount">返回总页数</param>
        ///// <returns>查询结果集</returns>
        //[OperationContract]
        //public List<T_HR_SALARYARCHIVE> GetSalaryArchivePaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        //{
        //    SalaryArchiveBLL bll = new SalaryArchiveBLL();

        //    IQueryable<T_HR_SALARYARCHIVE> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
        //    return q.Count() > 0 ? q.ToList() : null;
        //}

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询（带权限）
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_SALARYARCHIVE> GetSalaryArchivePaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, string checkstate, int orgtype, string orgid,int queryCode,string companyID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                IQueryable<T_HR_SALARYARCHIVE> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, checkstate, orgtype, orgid,queryCode,companyID);
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
        /// 用于实体Grid中显示数据的分页查询（带权限）
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        //[OperationContract]
        //public List<T_HR_SALARYARCHIVEITEM> GetSalaryArchiveItemPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        //{
        //    SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL();

        //    IQueryable<T_HR_SALARYARCHIVEITEM> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
        //    return q.Count() > 0 ? q.ToList() : null;
        //}
        [OperationContract]
        public List<V_SALARYARCHIVEITEM> GetSalaryArchiveItemPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL())
            {
                IQueryable<V_SALARYARCHIVEITEM> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        [OperationContract]
        public List<V_SALARYARCHIVEITEM> GetSalaryArchiveItemsByArchiveIDs(List<string> archiveIDs)
        {
            using (SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL())
            {
                return bll.GetSalaryArchiveItemsByArchiveIDs(archiveIDs);
            }
        }
        /// <summary>
        /// 根据ID获取薪资档案项
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        [OperationContract]
        public V_SALARYARCHIVEITEM GetSalaryArchiveItemViewByID(string itemID)
        {
            using (SalaryArchiveItemBLL bll = new SalaryArchiveItemBLL())
            {
                return bll.GetSalaryArchiveItemViewByID(itemID);
            }
        }


        #endregion

        #region T_HR_SALARYARCHIVEHIS 薪资档案历史
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询（带权限）
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_SALARYARCHIVES> GetSalaryArchivehisWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string userID)
        {
            //using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            //{
            //    IQueryable<V_SALARYARCHIVES> q = bll.VQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, userID);
            //    return q.Count() > 0 ? q.ToList() : null;
            //}
            using (SalaryArchiveHisBLL bll = new SalaryArchiveHisBLL())
            {
                IQueryable<V_SALARYARCHIVES> q = bll.VQueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 添加薪资档案历史
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void SalaryArchiveHisAdd(T_HR_SALARYARCHIVEHIS obj)
        {
            using (SalaryArchiveHisBLL bll = new SalaryArchiveHisBLL())
            {
                bll.SalaryArchiveHisAdd(obj);
            }
        }

        # endregion

        #region  T_HR_SALARYARCHIVEHISITEM 薪资档案历史项
        /// <summary>
        /// 添加薪资档案历史项
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [OperationContract]
        public void SalaryArchiveHisItemAdd(T_HR_SALARYARCHIVEHISITEM obj)
        {
            using (SalaryArchiveHisItemBLL bll = new SalaryArchiveHisItemBLL())
            {
                bll.SalaryArchiveHisItemAdd(obj);
            }
        }

        /// <summary>
        /// 添加薪资档案历史项
        /// </summary>
        /// <param name="salaryarchiveID">薪资档案ID</param>
        /// <param name="salarystandardID">薪资标准ID</param>
        /// <param name="createuserID">创建用户ID</param>
        /// <returns></returns>
        [OperationContract]
        public void SalaryArchiveHisItemsAdd(string salaryarchiveID, string salarystandardID, string createuserID)
        {
            using (SalaryArchiveHisItemBLL bll = new SalaryArchiveHisItemBLL())
            {
                bll.SalaryArchiveHisItemsAdd(salaryarchiveID, salarystandardID, createuserID);
            }
        }

        /// <summary>
        /// 根据ID获取薪资档案历史项实体集合
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_SALARYARCHIVEITEM> GetSalaryArchiveHisItemByID(string ID)
        {
            using (SalaryArchiveHisItemBLL bll = new SalaryArchiveHisItemBLL())
            {
                return bll.GetSalaryArchiveHisItemByID(ID).Count() > 0 ? bll.GetSalaryArchiveHisItemByID(ID).ToList() : null;
            }
        }
        #endregion

        #region 自定义薪资档案历史
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询（带权限）
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_CUSTOMGUERDONARCHIVEHIS> GetCustomGuerdonArchiveHisWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (CustomGuerdonArchiveHISBLL bll = new CustomGuerdonArchiveHISBLL())
            {
                IQueryable<V_CUSTOMGUERDONARCHIVEHIS> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region T_HR_EMPLOYEESALARYRECORD 员工薪资记录
        /// <summary>
        /// 新增员工薪资记录
        /// </summary>
        /// <param name="entity">员工薪资记录实体</param>
        [OperationContract]
        public void EmployeeSalaryRecordAdd(T_HR_EMPLOYEESALARYRECORD entity)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                bll.EmployeeSalaryRecordAdd(entity);
            }
        }

        /// <summary>
        /// 获取薪资对比记录(仅获取实发)
        /// </summary>
        /// <param name="SentEmployeeSalaryRecordID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEESALARYRECORD> SalaryContrast(string SentEmployeeSalaryRecordID)
        {
            List<T_HR_EMPLOYEESALARYRECORD> temp = new List<T_HR_EMPLOYEESALARYRECORD>();
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                temp = bll.SalaryContrast(SentEmployeeSalaryRecordID);
                if (temp.Count > 0)
                {
                    return temp;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取薪资对比记录(获取所有)
        /// </summary>
        /// <param name="SentEmployeeSalaryRecordID"></param>
        /// <param name="nowData"></param>
        /// <param name="lastData"></param>
        /// <param name="titleData"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> SalaryContrastAll(string SentEmployeeSalaryRecordID, out List<string> nowData, out List<string> lastData)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                List<string> titleData = new List<string>();
                nowData = new List<string>();
                lastData = new List<string>();
                bll.SalaryContrast(SentEmployeeSalaryRecordID, out nowData, out lastData, out titleData);
                return titleData;
            }
        }

        /// <summary>
        /// 更新员工薪资记录
        /// </summary>
        /// <param name="entity">员工薪资记录实体</param>
        [OperationContract]
        public void EmployeeSalaryRecordUpdate(T_HR_EMPLOYEESALARYRECORD entity)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                bll.EmployeeSalaryRecordUpdate(entity);
            }
        }

        /// <summary>
        /// 查询员工薪资记录
        /// </summary>
        /// <param name="employeeID">员工薪资记录ID</param>
        /// <returns>员工薪资记录</returns>
        [OperationContract]
        public T_HR_EMPLOYEESALARYRECORD GetEmployeeSalaryRecordByID(string ID)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.GetEmployeeSalaryRecordByID(ID);
            }
        }

        /// <summary>
        /// 薪资记录批量删除
        /// </summary>
        /// <param name="EmployeeSalaryRecordIDs">薪资记录IDS</param>
        /// <returns></returns>
        [OperationContract]
        public int EmployeeSalaryRecordOrItemDelete(string[] EmployeeSalaryRecordIDs)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.EmployeeSalaryRecordOrItemDelete(EmployeeSalaryRecordIDs);
            }
        }

        /// <summary>
        /// 删除员工薪资记录
        /// </summary>
        /// <param name="resumeID">员工薪资记录ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool EmployeeSalaryRecordDelete(string[] IDs)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                int rslt = bll.EmployeeSalaryRecordDelete(IDs);
                return (rslt > 0);
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
        public List<T_HR_EMPLOYEESALARYRECORD> GetEmployeeSalaryRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, string strCheckState, string userID)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                IQueryable<T_HR_EMPLOYEESALARYRECORD> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, starttime, endtime, strCheckState, userID);
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

        ///// <summary>
        ///// 结算薪资
        ///// </summary>
        ///// <param name="objectType">结算对象类型</param>
        ///// <param name="objectID">结算对象ID</param>
        ///// <param name="year">结算年份</param>
        ///// <param name="month">结算月份</param>
        ///// <returns></returns>
        //[OperationContract]
        //public List<V_RETURNFBI> GenerateSalaryRecord(int objectType,string GenerateEmployeePostid, string objectID, int year, int month)
        //{
        //    using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
        //    {
        //        string strBalanceEmployeeID = string.Empty;
        //        return bll.GenerateSalaryRecord(0,GenerateEmployeePostid,strBalanceEmployeeID, objectType, objectID, year, month, false);
        //    }
        //}

        /// <summary>
        /// 经过预算的薪资结算
        /// </summary>
        /// <param name="objectType">结算类型 结算类型（0：发薪机构；1：公司；2：离职薪资：3指定结算岗位薪资）</param>
        /// <param name="objectType">结算对象类型 0公司，1部门，2岗位，3员工</param>
        /// <param name="calType">运算类型</param>
        /// <returns>查询结果</returns>
        [OperationContract]
        public Dictionary<object, object> SalaryRecordAccountCheck(Dictionary<string,string> GeneratePrameter,int objectType, string objectID, int year, int month, string construes)
        {
            int GernerateType=int.Parse(GeneratePrameter["GernerateType"]);
            string GenerateEmployeePostid = GeneratePrameter["GenerateEmployeePostid"];
            string GenerateCompanyid = GeneratePrameter["GenerateCompanyid"];

            if (string.IsNullOrEmpty(GernerateType.ToString())
                      || string.IsNullOrEmpty(GenerateEmployeePostid)
                      || string.IsNullOrEmpty(GenerateCompanyid))
            {
                string message = "结算薪资参数异常，请联系管理员，GernerateType" + GeneratePrameter["GernerateType"]
                    + "GenerateEmployeePostid" + GeneratePrameter["GenerateEmployeePostid"]
                + "GenerateCompanyid" + GeneratePrameter["GenerateCompanyid"];
                Dictionary<object, object> GetInfor = new Dictionary<object, object>();
                GetInfor.Add("结算薪资错误", message);
                Tracer.Debug(message);

                return GetInfor;
            }

            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.SalaryRecordAccount(GernerateType, GenerateEmployeePostid, GenerateCompanyid,objectType, objectID, year, month, construes, false);
            }
        }

        //[OperationContract]
        //public Dictionary<object, object> SalaryRecordAccount(int GernerateType,string GenerateEmployeePostid, int objectType,string objectID, int year, int month, string construes)
        //{
        //    using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
        //    {
        //        return bll.SalaryRecordAccount(GernerateType, GenerateEmployeePostid, GenerateCompanyid,objectType, objectID, year, month, construes, true);
        //    }
        //}

        /// <summary>
        /// 经过预算的验证
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回验证结果</returns>
        [OperationContract]
        public List<string> FBStatistics(string employeeID, int year, int month)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.FBStatistics(employeeID, year, month);
            }
        }

        /// <summary>
        /// 按发放对象合计薪资总额进行预算的验证(批量验证)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回验证结果</returns>
        [OperationContract]
        public List<string> FBStatisticsMass(int objectType, string objectID, int year, int month, string userID, string departmentID)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.FBStatistics(objectType, objectID, year, month, userID, departmentID);
            }
        }
        
        /// <summary>
        /// 按发放对象下选择的人员，合计薪资总额进行预算的验证(批量验证)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回验证结果</returns>
        [OperationContract]
        public List<string> FBStatisticsMassByChoose(int objectType, string objectID, decimal dSum, int year, int month, string userID, string departmentID)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.FBStatisticsByChoose(objectType, objectID, dSum, year, month, userID, departmentID);
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询-新查询NEW
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEESALARYRECORD> GetAutoEmployeeSalaryRecordPagings(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                var q = bll.GetAutoEmployeeSalaryRecordPagings(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID);
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
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="MenuSign">菜单标识</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEESALARYRECORD> GetMenuSignAutoEmployeeSalaryRecordPagings(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID, string MenuSign)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                List<T_HR_EMPLOYEESALARYRECORD> q = bll.GetAutoEmployeeSalaryRecordPagings(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID, MenuSign).ToList();
                return q.Count() > 0 ? q : null;
            }
        }

        /// <summary>
        /// 计算项的值
        /// </summary>
        /// <param name="itemid">薪资项的ID</param>
        /// <returns>返回计算项的值</returns>
        [OperationContract]
        public decimal AutoCalItem(string itemid, string employeeID)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {

                return bll.AutoCalItem(itemid, employeeID);
            }
        }


        /// <summary>
        /// 过滤薪资标准
        /// </summary>
        /// <returns>返回过滤的薪资标准</returns>
        [OperationContract]
        public List<string> FilterStandard(string year, string month)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.FilterStandard(year, month);
            }
        }

        /// <summary>
        /// 撤消还款(审核不通过时候撤消还款)
        /// </summary>
        /// <returns>返回结果</returns>
        [OperationContract]
        public bool UndoRepayment(string employeeid, string year, string month)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.UndoRepayment(employeeid, year, month);
            }
        }

        /// <summary>
        /// 批量撤消还款(审核不通过时候批量撤消还款)
        /// </summary>
        /// <returns>返回结果</returns>
        [OperationContract]
        public bool UndoRepaymentMass(int objectType, string objectID, string year, string month)
        {
            using (EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL())
            {
                return bll.UndoRepayment(objectType, objectID, year, month);
            }
        }

        #endregion

        #region T_HR_SALARYRECORDBATCH  薪资记录批量审核

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
        public List<T_HR_EMPLOYEESALARYRECORD> GetSalaryRecordAuditPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                IQueryable<T_HR_EMPLOYEESALARYRECORD> q = bll.GetSalaryRecordAuditPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        [OperationContract]
        public List<V_EMPLOYEESALARYRECORD> GetMassAuditSalaryRecordPagings(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                IQueryable<V_EMPLOYEESALARYRECORD> q = bll.GetMassAuditSalaryRecordPagings(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID);
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
        /// 批量审核 获取数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="orgtype"></param>
        /// <param name="orgid"></param>
        /// <param name="strCheckState"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_salaryRecordDetailView> GetAuditSalaryRecordsPagings(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                var q = bll.GetAuditSalaryRecordsPagings(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID);
                return q;
            }
        }
        /// <summary>
        /// 动态生成薪资批量审核
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="orgtype"></param>
        /// <param name="orgid"></param>
        /// <param name="strCheckState"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public DataSetData GetAuditSalaryRecords(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                var q = bll.GetAuditSalaryRecords(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, orgtype, orgid, strCheckState, userID);
                return q;
            }
        }
        /// <summary>
        /// 获取薪资记录审核统计后数据信息
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="orgtype"></param>
        /// <param name="orgid"></param>
        /// <param name="strCheckState"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetSalaryRecordAuditSum(string sort, string filterString, string[] paras, DateTime starttime, DateTime endtime, int orgtype, string orgid, string strCheckState, string userID, out List<string> nameData)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                return bll.GetSalaryRecordAuditSum(sort, filterString, paras, starttime, endtime, orgtype, orgid, strCheckState, userID, out nameData);
            }
        }

        /// <summary>
        /// 查询薪资记录批量审核实体
        /// </summary>
        /// <param name="SalaryRecordBatchID">薪资记录批量审核ID</param>
        /// <returns>返回薪资记录批量审核实体</returns>
        [OperationContract]
        public T_HR_SALARYRECORDBATCH GetSalaryRecordBatchByID(string SalaryRecordBatchID)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                return bll.GetSalaryRecordBatchByID(SalaryRecordBatchID);
            }
        }

        /// <summary>
        /// 检测员工指定年月的薪资记录是否已经提交审核(用于删除社保缴交记录判断时使用
        /// </summary>
        /// <param name="strMsg">查询返回的详细消息(无提交记录时，返回消息为空)</param>
        /// <param name="filterString">查询条件</param>
        /// <param name="paras">查询参数</param>
        /// <returns>true/false</returns>
        [OperationContract]
        public bool CheckSalaryAuditState(ref string strMsg, string filterString, string[] paras)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                return bll.GetSalaryBatchAuditState(ref strMsg, filterString, paras);
            }
        }

        /// <summary>
        /// 新增薪资记录批量审核
        /// </summary>
        /// <param name="entity">薪资记录批量审核实体</param>
        /// <returns></returns>
        [OperationContract]
        public bool SalaryRecordBatchAdd(T_HR_SALARYRECORDBATCH entity, string[] salaryrecordids)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                return bll.SalaryRecordBatchAdd(entity, salaryrecordids);
            }
        }

        /// <summary>
        /// 更新薪资记录批量审核
        /// </summary>
        /// <param name="entity">薪资记录批量审核实体</param>
        /// <returns></returns>
        [OperationContract]
        public void SalaryRecordBatchUpdate(T_HR_SALARYRECORDBATCH entity)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                bll.SalaryRecordBatchUpdate(entity);
            }
        }

        /// <summary>
        /// 删除薪资记录批量审核，可同时删除多行记录
        /// </summary>
        /// <param name="SalaryRecordBatchIDs">薪资记录批量审核ID数组</param>
        /// <returns></returns>
        [OperationContract]
        public int SalaryRecordBatchDelete(string[] SalaryRecordBatchIDs)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                return bll.SalaryRecordBatchDelete(SalaryRecordBatchIDs);
            }
        }

        #endregion

        #region T_HR_EMPLOYEEADDSUM 员工加扣款
        /// <summary>
        /// 添加员工加扣款
        /// </summary>
        /// <param name="obj">加扣款实例</param>
        [OperationContract]
        public void EmployeeAddSumADD(T_HR_EMPLOYEEADDSUM obj)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                bll.EmployeeAddSumADD(obj);
            }
        }

        /// <summary>
        /// 获取加扣款离职人员信息
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_RESIGN> GetResign(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, string userID, int orgtype, string orgid)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                IQueryable<V_RESIGN> q = bll.GetResign(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, userID, orgtype, orgid);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 批量添加员工加扣款
        /// </summary>
        /// <param name="obj"> 加扣款实例集合</param>
        [OperationContract]
        public bool EmployeeAddSumLotsofADD(List<T_HR_EMPLOYEEADDSUM> objs)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                return bll.EmployeeAddSumLotsofADD(objs);
            }
        }

        /// <summary>
        /// 更新员工加扣款
        /// </summary>
        /// <param name="obj">加扣款实例</param>
        [OperationContract]
        public void EmployeeAddSumUpdate(T_HR_EMPLOYEEADDSUM obj)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                bll.EmployeeAddSumUpdate(obj);
            }
        }
        /// <summary>
        /// 删除员工加扣款      
        /// </summary>
        /// <param name="IDs">加扣款ID</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeAddSumDelete(string[] IDs)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                int rslt = bll.EmployeeAddSumDelete(IDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 通过员工ID集合删除员工加扣款
        /// </summary>
        /// <param name="employeeIDs">员工ID集合</param>
        /// <returns></returns>
        [OperationContract]
        public int EmployeeAddSumByEmployeeIDDelete(string[] employeeIDs, string year, string month)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                return bll.EmployeeAddSumByEmployeeIDDelete(employeeIDs, year, month);
            }
        }
        /// <summary>
        /// 根据ID获取加扣款
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEADDSUM GetEmployeeAddSumByID(string ID)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                return bll.GetEmployeeAddSumByID(ID);
            }
        }
        [OperationContract]
        public V_EmployeeAddsumView GetEmployeeAddSumViewByID(string ID)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                return bll.GetEmployeeAddSumViewByID(ID);
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
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEADDSUM> GetEmployeeAddSumWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, string userID, string CheckState, int orgtype, string orgid)
        {
            using (EmployeeAddSumBLL bll = new EmployeeAddSumBLL())
            {
                IQueryable<T_HR_EMPLOYEEADDSUM> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, starttime, endtime, userID, CheckState, orgtype, orgid);
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
        #endregion

        #region T_HR_EMPLOYEEADDSUMBATCH  员工加扣款批量审核
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
        public List<T_HR_EMPLOYEEADDSUM> GetEmployeeAddSumAuditPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, string userID, string CheckState, int orgtype, string orgid)
        {
            using (EmployeeAddSumBatchBLL bll = new EmployeeAddSumBatchBLL())
            {
                return bll.GetEmployeeAddSumAuditPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, starttime, endtime, userID, CheckState, orgtype, orgid).ToList();
            }
        }

        /// <summary>
        /// 查询员工加扣款批量审核实体
        /// </summary>
        /// <param name="EmployeeAddSumBatchID">员工加扣款批量审核ID</param>
        /// <returns>返回员工加扣款批量审核实体</returns>
        [OperationContract]
        public T_HR_EMPLOYEEADDSUMBATCH GetEmployeeAddSumBatchByID(string EmployeeAddSumBatchID)
        {
            using (EmployeeAddSumBatchBLL bll = new EmployeeAddSumBatchBLL())
            {
                return bll.GetEmployeeAddSumBatchByID(EmployeeAddSumBatchID);
            }
        }

        /// <summary>
        /// 新增员工加扣款批量审核
        /// </summary>
        /// <param name="entity">员工加扣款批量审核实体</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeAddSumBatchAdd(T_HR_EMPLOYEEADDSUMBATCH entity, string[] addsumids)
        {
            using (EmployeeAddSumBatchBLL bll = new EmployeeAddSumBatchBLL())
            {
                return bll.EmployeeAddSumBatchAdd(entity, addsumids);
            }
        }

        /// <summary>
        /// 更新员工加扣款批量审核
        /// </summary>
        /// <param name="entity">员工加扣款批量审核实体</param>
        /// <returns></returns>
        [OperationContract]
        public void EmployeeAddSumBatchUpdate(T_HR_EMPLOYEEADDSUMBATCH entity)
        {
            using (EmployeeAddSumBatchBLL bll = new EmployeeAddSumBatchBLL())
            {
                bll.EmployeeAddSumBatchUpdate(entity);
            }
        }

        /// <summary>
        /// 删除员工加扣款批量审核，可同时删除多行记录
        /// </summary>
        /// <param name="AddSumBatchIDs">员工加扣款批量审核ID数组</param>
        /// <returns></returns>
        [OperationContract]
        public int EmployeeAddSumBatchDelete(string[] AddSumBatchIDs)
        {
            using (EmployeeAddSumBatchBLL bll = new EmployeeAddSumBatchBLL())
            {
                return bll.EmployeeAddSumBatchDelete(AddSumBatchIDs);
            }
        }
        #endregion

        #region T_HR_CUSTOMGUERDONSET 自定义薪资设置

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
        public List<T_HR_CUSTOMGUERDONSET> GetCustomGuerdonSetPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string strCheckState, string userid)
        {
            using (CustomGuerdonSetBLL bll = new CustomGuerdonSetBLL())
            {
                IQueryable<T_HR_CUSTOMGUERDONSET> q = bll.GetCustomGuerdonSetPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, userid);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 添加自定义薪资设置
        /// </summary>
        /// <param name="entity">自定义薪资设置实体</param>
        [OperationContract]
        public void CustomGuerdonSetAdd(T_HR_CUSTOMGUERDONSET entity)
        {
            using (CustomGuerdonSetBLL bll = new CustomGuerdonSetBLL())
            {
                bll.Add(entity);
            }
        }
        /// <summary>
        /// 更新自定义薪资设置记录
        /// </summary>
        /// <param name="entity">自定义薪资设置实体</param>
        [OperationContract]
        public void CustomGuerdonSetUpdate(T_HR_CUSTOMGUERDONSET entity)
        {
            using (CustomGuerdonSetBLL bll = new CustomGuerdonSetBLL())
            {
                bll.CustomGuerdonSetUpdate(entity);
            }
        }
        /// <summary>
        /// 删除自定义薪资设置实体
        /// </summary>
        /// <param name="CustomGuerdonSets">自定义薪资设置ID组</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool CustomGuerdonSetDelete(string[] CustomGuerdonSets)
        {
            using (CustomGuerdonSetBLL bll = new CustomGuerdonSetBLL())
            {
                int rslt = bll.CustomGuerdonSetDelete(CustomGuerdonSets);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据自定义薪资设置ID查询实体
        /// </summary>
        /// <param name="CustomGuerdonSetID">自定义薪资设置ID</param>
        /// <returns>返回自定义薪资设置实体</returns>
        [OperationContract]
        public T_HR_CUSTOMGUERDONSET GetCustomGuerdonSetByID(string CustomGuerdonSetID)
        {
            using (CustomGuerdonSetBLL bll = new CustomGuerdonSetBLL())
            {
                return bll.GetCustomGuerdonSetByID(CustomGuerdonSetID);
            }
        }

        /// <summary>
        /// 根据自定义薪资名称查询
        /// </summary>
        /// <param name="CustomGuerdonSetName">自定义薪资设置名</param>
        /// <returns>返回bool类型</returns>
        [OperationContract]
        public bool GetCustomGuerdonSetName(string CustomGuerdonSetName)
        {
            using (CustomGuerdonSetBLL bll = new CustomGuerdonSetBLL())
            {
                return bll.GetCustomGuerdonSetName(CustomGuerdonSetName);
            }
        }

        #endregion

        #region T_HR_CUSTOMGUERDONSET 自定义薪资

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
        public List<T_HR_CUSTOMGUERDON> GetCustomGuerdonPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (CustomGuerdonBLL bll = new CustomGuerdonBLL())
            {
                IQueryable<T_HR_CUSTOMGUERDON> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 添加自定义薪资
        /// </summary>
        /// <param name="entity">自定义薪资实体</param>
        [OperationContract]
        public string CustomGuerdonAdd(T_HR_CUSTOMGUERDON entity)
        {
            using (CustomGuerdonBLL bll = new CustomGuerdonBLL())
            {
                return bll.CreateCustomGuerdon(entity);
            }
        }

        /// <summary>
        /// 删除自定义薪资实体
        /// </summary>
        /// <param name="CustomGuerdonSets">自定义薪资ID组</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool CustomGuerdonDelete(string[] CustomGuerdons)
        {
            using (CustomGuerdonBLL bll = new CustomGuerdonBLL())
            {
                int rslt = bll.CustomGuerdonDelete(CustomGuerdons);
                return (rslt > 0);
            }
        }

        /// <summary>
        /// 根据薪资标准ID查询实体
        /// </summary>
        /// <param name="SalaryStandardID">薪资标准ID</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_CUSTOMGUERDON> GetCustomGuerdon(string SalaryStandardID)
        {
            using (CustomGuerdonBLL bll = new CustomGuerdonBLL())
            {
                return bll.GetCustomGuerdon(SalaryStandardID);
            }
        }

        /// <summary>
        /// 根据自定义薪资ID查询实体
        /// </summary>
        /// <param name="CustomGuerdonSetID">自定义薪资ID</param>
        /// <returns>返回自定义薪资实体</returns>
        [OperationContract]
        public T_HR_CUSTOMGUERDON GetCustomGuerdonByID(string CustomGuerdonID)
        {
            using (CustomGuerdonBLL bll = new CustomGuerdonBLL())
            {
                return bll.GetCustomGuerdonByID(CustomGuerdonID);
            }
        }

        #endregion

        #region T_HR_CUSTOMGUERDONRECORD 自定义薪资记录

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
        public List<T_HR_CUSTOMGUERDONRECORD> GetCustomGuerdonRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime starttime, DateTime endtime, string strCheckState, string userID)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                IQueryable<T_HR_CUSTOMGUERDONRECORD> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, starttime, endtime, strCheckState, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 根据员工ID查询自定义薪资记录实体
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回自定义薪资记录实体</returns>
        [OperationContract]
        public T_HR_CUSTOMGUERDONRECORD GetEmployeeCustomRecordOne(string employeeID, string year, string month)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                return bll.GetEmployeeCustomRecordOne(employeeID, year, month);
            }
        }

        [OperationContract]
        public List<V_RETURNFBI> CustomGuerdonRecord(int objectType, string objectID, int year, int month)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                return bll.CustomGuerdonRecord(objectType, objectID, year, month, false);
            }
        }

        /// <summary>
        /// 经过预算的自定义薪资结算
        /// </summary>
        /// <param name="objectType">类型</param>
        /// <returns>查询结果</returns>
        [OperationContract]
        public string CustomGuerdonRecordAccount(int objectType, string objectID, int year, int month, string construes)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                return bll.CustomGuerdonRecordAccount(objectType, objectID, year, month, construes);
            }
        }

        /// <summary>
        /// 添加自定义薪资记录
        /// </summary>
        /// <param name="entity">自定义薪资记录实体</param>
        [OperationContract]
        public void CustomGuerdonRecordAdd(T_HR_CUSTOMGUERDONRECORD entity)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                bll.Add(entity);
            }
        }

        /// <summary>
        /// 更新自定义薪资记录
        /// </summary>
        /// <param name="entity">自定义薪资记录实体</param>
        [OperationContract]
        public void CustomGuerdonRecordUpdate(T_HR_CUSTOMGUERDONRECORD entity)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                bll.CustomGuerdonRecordUpdate(entity);
            }
        }

        /// <summary>
        /// 删除自定义薪资记录实体
        /// </summary>
        /// <param name="CustomGuerdonRecords">自定义薪资记录ID组</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool CustomGuerdonRecordDelete(string[] CustomGuerdonRecords)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                int rslt = bll.CustomGuerdonRecordDelete(CustomGuerdonRecords);
                return (rslt > 0);
            }
        }

        /// <summary>
        /// 根据自定义薪资记录ID查询实体
        /// </summary>
        /// <param name="CustomGuerdonRecords">自定义薪资记录ID</param>
        /// <returns>返回自定义薪资记录实体</returns>
        [OperationContract]
        public T_HR_CUSTOMGUERDONRECORD GetCustomGuerdonRecordByID(string CustomGuerdonRecordID)
        {
            using (CustomGuerdonRecordBLL bll = new CustomGuerdonRecordBLL())
            {
                return bll.GetCustomGuerdonRecordByID(CustomGuerdonRecordID);
            }
        }

        #endregion

        #region T_HR_PERFORMANCEREWARDSET 绩效奖金设置
        /// <summary>
        /// 添加绩效奖金设置
        /// </summary>
        /// <param name="obj">绩效奖金设置实例</param>
        [OperationContract]
        public void PerformanceRewardSetAdd(T_HR_PERFORMANCEREWARDSET obj)
        {
            using (PerformanceRewardSetBLL bll = new PerformanceRewardSetBLL())
            {
                bll.PerformanceRewardSetAdd(obj);
            }
        }
        /// <summary>
        /// 更新绩效奖金设置
        /// </summary>
        /// <param name="obj">绩效奖金设置实例</param>
        [OperationContract]
        public void PerformanceRewardSetUpdate(T_HR_PERFORMANCEREWARDSET obj)
        {
            using (PerformanceRewardSetBLL bll = new PerformanceRewardSetBLL())
            {
                bll.PerformanceRewardSetUpdate(obj);
            }
        }
        /// <summary>
        /// 删除绩效奖金设置      
        /// </summary>
        /// <param name="IDs">绩效奖金设置ID</param>
        /// <returns></returns>
        [OperationContract]
        public bool PerformanceRewardSetDelete(string[] IDs)
        {
            using (PerformanceRewardSetBLL bll = new PerformanceRewardSetBLL())
            {
                int rslt = bll.PerformanceRewardSetDelete(IDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据ID获取绩效奖金设置
        /// </summary>
        /// <param name="ID">绩效奖金设置ID</param>
        /// <returns>是否删除标记</returns>
        [OperationContract]
        public T_HR_PERFORMANCEREWARDSET GetPerformanceRewardSetByID(string ID)
        {
            using (PerformanceRewardSetBLL bll = new PerformanceRewardSetBLL())
            {
                return bll.PerformanceRewardSetByID(ID);
            }
        }
        ///// <summary>
        ///// 用于实体Grid中显示数据的分页查询
        ///// </summary>
        ///// <param name="pageIndex">当前页</param>
        ///// <param name="pageSize">每页显示条数</param>
        ///// <param name="sort">排序字段</param>
        ///// <param name="filterString">过滤条件</param>
        ///// <param name="paras">过滤条件中的参数值</param>
        ///// <param name="pageCount">返回总页数</param>
        ///// <returns>查询结果集</returns>
        //[OperationContract]
        //public List<T_HR_PERFORMANCEREWARDSET> GetPerformanceRewardSetWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        //{
        //    PerformanceRewardSetBLL bll = new PerformanceRewardSetBLL();

        //    IQueryable<T_HR_PERFORMANCEREWARDSET> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
        //    return q.Count() > 0 ? q.ToList() : null;
        //}
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询(带有权限)
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_PERFORMANCEREWARDSET> GetPerformanceRewardSetWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, string CheckState)
        {
            using (PerformanceRewardSetBLL bll = new PerformanceRewardSetBLL())
            {
                IQueryable<T_HR_PERFORMANCEREWARDSET> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion
        #region T_HR_CUSTOMGUERDONARCHIVE 自定义薪资档案

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
        public List<V_CUSTOMGUERDONARCHIVE> GetCustomGuerdonArchiveWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (CustomGuerdonArchiveBLL bll = new CustomGuerdonArchiveBLL())
            {

                IQueryable<V_CUSTOMGUERDONARCHIVE> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        ///根据薪资档案ID获取自定义薪资档案
        /// </summary>
        /// <param name="ArchiveID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_CUSTOMGUERDONARCHIVE> GetCustomGuerdonArchiveByArchiveID(string ArchiveID)
        {
            using (CustomGuerdonArchiveBLL bll = new CustomGuerdonArchiveBLL())
            {
                return bll.GetCustomGuerdonArchiveByArchiveID(ArchiveID);
            }
        }
        /// <summary>
        /// 添加自定义薪资档案
        /// </summary>
        /// <param name="obj">自定义薪资档案实例</param>
        [OperationContract]
        public void CustomGuerdonArchiveAdd(T_HR_CUSTOMGUERDONARCHIVE obj)
        {
            using (CustomGuerdonArchiveBLL bll = new CustomGuerdonArchiveBLL())
            {
                bll.CustomGuerdonArchiveAdd(obj);
            }
        }
        /// <summary>
        /// 更新自定义薪资档案
        /// </summary>
        /// <param name="entity">自定义薪资档案实例</param>
        [OperationContract]
        public void CustomGuerdonArchiveUpdate(T_HR_CUSTOMGUERDONARCHIVE obj)
        {
            using (CustomGuerdonArchiveBLL bll = new CustomGuerdonArchiveBLL())
            {
                bll.CustomGuerdonArchiveUpdate(obj);
            }
        }
        #endregion

        #region  薪资发放

        /// <summary>
        /// 更新薪资发放实体 
        /// </summary>
        /// <param name="entity">薪资发放实体</param>
        /// <returns></returns>
        [OperationContract]
        public void PaymentUpdate(T_HR_EMPLOYEESALARYRECORD entity)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                bll.PaymentUpdate(entity);
            }
        }

        /// <summary>
        /// 薪资发放确认
        /// </summary>
        /// <param name="entitys">薪资发放实体集</param>
        /// <returns></returns>
        [OperationContract]
        public void PaymentConfirmUpdate(List<T_HR_EMPLOYEESALARYRECORD> entitys)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                bll.PaymentConfirmUpdate(entitys);
            }
        }

        /// <summary>
        /// 获取薪资发放实体(一条)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资发放实体(一条)</returns>
        [OperationContract]
        public T_HR_EMPLOYEESALARYRECORD GetSalaryRecordOne(string employeeid, string year, string month)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                return bll.GetSalaryRecordOne(employeeid, year, month);
            }
        }

        /// <summary>
        /// 文件上传服务
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="strFilePath">上传文件存储的相对路径</param>
        [OperationContract]
        public void SaveFile(UploadFileModel UploadFile, out string strFilePath)
        {
            string strVirtualPath = ConfigurationManager.AppSettings["FileUploadLocation"].ToString();
            string strPath = HttpContext.Current.Server.MapPath(strVirtualPath) + UploadFile.FileName;
            if (Directory.Exists(HttpContext.Current.Server.MapPath(strVirtualPath)) == false)
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(strVirtualPath));
            }

            FileStream FileStream = new FileStream(strPath, FileMode.Create);
            FileStream.Write(UploadFile.File, 0, UploadFile.File.Length);

            FileStream.Close();
            FileStream.Dispose();

            strFilePath = strVirtualPath + UploadFile.FileName;
        }

        /// <summary>
        /// 导入EXCEL文件
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public List<T_HR_EMPLOYEESALARYRECORD> ImportExcel(UploadFileModel UploadFile, out int failcount, out int successcount, string year, string month, string paySign)
        {
            string strPath = string.Empty;    //, int pageIndex, int pageSize, ref int pageCount, ref string strMsg
            SaveFile(UploadFile, out strPath);
            string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);
            using (PaymentBLL bll = new PaymentBLL())
            {
                return bll.ReadExcel(strPhysicalPath, out failcount, out successcount, year, month, paySign);
            }
        }

        /// <summary>
        /// 读取EXCEL文件
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEESALARYRECORD> ReadExcel(string filepath, out int failcount, out int successcount, string year, string month, string paySign)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                return bll.ReadExcel(filepath, out failcount, out successcount, year, month, paySign);
            }
        }

        /// <summary>
        /// 导出EXCEL(完整数据)
        /// </summary>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        public byte[] ExportExcelAll(string sort, string filterString, string[] paras, string year, string month, int orgtype, string orgid)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                return bll.ExportExcelAll(sort, filterString, paras, year, month, orgtype, orgid);
            }
        }

        /// <summary>
        /// 导出EXCEL
        /// </summary>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        public byte[] ExportExcel(string sort, string filterString, string[] paras, string year, string month, int orgtype, string orgid)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                return bll.ExportExcel(sort, filterString, paras, year, month, orgtype, orgid);
            }
        }

        /// <summary>
        /// 根据薪资批量审核ID导出数据
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="orgtype"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportSalaryExcel(string sort, string filterString, string[] paras, string userID)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                return bll.ExportSalaryExcel(sort, filterString, paras, userID);
            }
        }

        [OperationContract]
        public byte[] ExportSalarySummary(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.ExportSalarySummary(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, year, month, IsPageing);
            }
        }
        [OperationContract]
        public byte[] ExportEmployeeDeductionMoney(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.ExportEmployeeDeductionMoney(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, year, month, IsPageing);
            }
        }
        [OperationContract]
        public byte[] ExportMonthDeductionTaxs(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, bool IsPageing)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.ExportMonthDeductionTaxs(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, IsPageing);
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
        public List<T_HR_EMPLOYEESALARYRECORD> GetPaymentPagings(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string year, string month, int orgtype, string orgid, string userid)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                IQueryable<T_HR_EMPLOYEESALARYRECORD> q = bll.GetPaymentPagings(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, year, month, orgtype, orgid, userid);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        [OperationContract]
        public List<V_PAYMENT> GetPaymentPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string year, string month)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                IQueryable<V_PAYMENT> q = bll.GetPaymentPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, year, month);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        #endregion

        #region 数据加密解密

        /// <summary>
        /// DES进行加密
        /// </summary>
        /// <param name="originalData">被加密的原始数据</param>
        /// <returns>返回字符串类型结果</returns>
        public string DESEncrypt(string originalData)
        {
            return DES.DESEncrypt(originalData);
        }

        /// <summary>
        /// DES进行解密
        /// </summary>
        /// <param name="hashedData">被解密的数据</param>
        /// <returns>返回字符串类型结果</returns>
        public string DESDecrypt(string hashedData)
        {
            return DES.DESDecrypt(hashedData);
        }

        /// <summary>   
        /// AES加密算法   
        /// </summary>   
        /// <param name="plainText">明文字符串</param>     
        /// <returns>返回加密后的密文字节数组</returns>   
        public string AESEncrypt(string plainText)
        {
            return AES.AESEncrypt(plainText);
        }

        /// <summary>   
        /// AES解密算法   
        /// </summary>   
        /// <param name="hashedData">密文字节</param>     
        /// <returns>返回解密后的字符串</returns>    
        public static string AESDecrypt(string hashedData)
        {
            return AES.AESDecrypt(hashedData);
        }

        #endregion

        #region  地区差异补贴
        /// <summary>
        /// 添加地区差异补贴
        /// </summary>
        /// <param name="obj">地区差异补贴实例</param>
        [OperationContract]
        public void AreaAllowanceADD(T_HR_AREAALLOWANCE obj)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                bll.AreaAllowanceAdd(obj);
            }
        }
        /// <summary>
        /// 更新地区差异补贴
        /// </summary>
        /// <param name="obj">地区差异补贴实例</param>
        [OperationContract]
        public void AreaAllowanceUpdate(T_HR_AREAALLOWANCE obj)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                bll.AreaAllowanceUpdate(obj);
            }
        }
        /// <summary>
        /// 地区差异补贴
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void AreaAllowance(List<T_HR_AREAALLOWANCE> objs)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                bll.AreaAllowance(objs);
            }
        }
        /// <summary>
        /// 删除地区差异补贴      
        /// </summary>
        /// <param name="IDs">地区差异补贴ID</param>
        /// <returns></returns>
        [OperationContract]
        public bool AreaAllowanceDelete(string[] IDs)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                int rslt = bll.AreaAllowanceDelete(IDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据地区差异ID获取地区差异补贴
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_AREAALLOWANCE> GetAreaAllowanceByAreaID(string ID)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                return bll.GetAreaAllowanceByAreaID(ID);
            }
        }
        /// <summary>
        /// 根据ID获取地区差异补贴
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_AREAALLOWANCE GetAreaAllowanceByID(string ID)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                return bll.GetAreaAllowanceByID(ID);
            }
        }
        /// <summary>
        /// 根据获取地区差异分类
        /// </summary>
        /// <returns>地区差异分类集</returns>
        [OperationContract]
        public List<T_HR_AREADIFFERENCE> GetAreaCategory()
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {
                return bll.GetAreaCategory();
            }
        }
        /// <summary>
        /// 根据地区分类获取城市
        /// </summary>
        /// <param name="categoryID">地区分类ID</param>
        /// <returns>城市列表</returns>
        [OperationContract]
        public List<T_HR_AREACITY> GetAreaCityByCategory(string categoryID)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                return bll.GetAreaCityByCategory(categoryID);
            }
        }
        /// <summary>
        /// 增加城市
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void AreaCityAdd(T_HR_AREACITY obj)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                bll.AreaCityAdd(obj);

            }
        }

        /// <summary>
        /// 核查城市是否已存在
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        [OperationContract]
        public Dictionary<string, string> AreaCityCheck(List<T_HR_AREACITY> objs)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                return bll.AreaCityCheck(objs);
            }
        }

        /// <summary>
        /// 根据地区类型删除城市
        /// </summary>
        /// <param name="CategoryID">地区类型ID</param>
        /// <returns>返回布尔类型结果</returns>
        [OperationContract]
        public bool AreaCityByCategoryDelete(string CategoryID)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                return bll.AreaCityByCategoryDelete(CategoryID);
            }
        }

        /// <summary>
        /// 多个城市增加
        /// </summary>
        /// <param name="objs"></param>
        [OperationContract]
        public string AreaCityLotsofAdd(List<T_HR_AREACITY> objs)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                return bll.AreaCityLotsofAdd(objs);
            }
        }

        /// <summary>
        /// 删除城市
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        [OperationContract]
        public void AreaCityDelete(string[] IDs)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {
                bll.AreaCityDelete(IDs);
            }
        }
        /// <summary>
        /// 根据ID获取地区
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_AREADIFFERENCE GetAreaCategoryByID(string ID)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {
                return bll.GetAreaCategoryByID(ID);
            }
        }
        /// <summary>
        /// 修改地区
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void AreaCategoryUpdate(T_HR_AREADIFFERENCE obj)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {
                bll.AreaCategoryUpdate(obj);
            }
        }
        /// <summary>
        /// 增加地区
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public string AreaCategoryADD(T_HR_AREADIFFERENCE obj)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {
                return bll.AreaCategoryADD(obj);
            }
        }

        /// <summary>
        /// 删除地区分类,可同时删除多行记录
        /// </summary>
        /// <param name="AreaCategoryIDs">AreaCategoryIDs</param>
        /// <returns></returns>
        [OperationContract]
        public int AreaCategoryDelete(string[] AreaCategoryIDs)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {
                return bll.AreaCategoryDelete(AreaCategoryIDs);
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
        public List<T_HR_AREAALLOWANCE> GetAreaAllowanceWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {

                IQueryable<T_HR_AREAALLOWANCE> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 获取城市分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_AREACITY> GetAreaCityWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (AreaAllowanceBLL bll = new AreaAllowanceBLL())
            {

                IQueryable<T_HR_AREACITY> q = bll.QueryCityWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 用于地区分类Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_AREADIFFERENCE> GetAreaWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {

                IQueryable<T_HR_AREADIFFERENCE> q = bll.QueryAreaWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region 绩效奖金记录
        /// <summary>
        /// 新增绩效奖金记录
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void PerformanceRewardRecordAdd(int orgtype, string orgid, string year, string month, DateTime startTime, DateTime endTime, string construes)
        {
            using (PerformanceRewardRecordBLL bll = new PerformanceRewardRecordBLL())
            {
                bll.PerformanceRewardRecordAdd(orgtype, orgid, year, month, startTime, endTime, construes);
            }
        }

        /// <summary>
        /// 更新绩效奖金记录
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void PerformanceRewardRecordUpdate(T_HR_PERFORMANCEREWARDRECORD obj)
        {
            using (PerformanceRewardRecordBLL bll = new PerformanceRewardRecordBLL())
            {
                bll.PerformanceRewardRecordUpdate(obj);
            }
        }
        /// <summary>
        /// 删除绩效奖金记录
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void PerformanceRewardRecordDelete(string[] IDs)
        {
            using (PerformanceRewardRecordBLL bll = new PerformanceRewardRecordBLL())
            {
                bll.PerformanceRewardRecordDelete(IDs);
            }
        }

        //[OperationContract]
        //public void CreatePerformanceRewardRecord(int objectType, string objectID, T_HR_PERFORMANCEREWARDRECORD record)
        //{
        //    PerformanceRewardRecordBLL bll = new PerformanceRewardRecordBLL();
        //    bll.createPerformanceRewardRecord(objectType, objectID, record);
        //}
        /// <summary>
        /// 根据ID获取绩效奖金
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_PERFORMANCEREWARDRECORD GetPerformaceRewardByID(string ID)
        {
            using (PerformanceRewardRecordBLL bll = new PerformanceRewardRecordBLL())
            {
                return bll.GetPerformanceRewardByID(ID);
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
        public List<T_HR_PERFORMANCEREWARDRECORD> GetPerformanceRewardRecordWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, DateTime startTime, DateTime endTime, string userID, string CheckState)
        {
            using (PerformanceRewardRecordBLL bll = new PerformanceRewardRecordBLL())
            {
                IQueryable<T_HR_PERFORMANCEREWARDRECORD> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, startTime, endTime, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        #endregion

        #region 薪资方案标准
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public string SalarySolutionStandardAdd(T_HR_SALARYSOLUTIONSTANDARD entity)
        {
            using (SalarySolutionStandardBLL bll = new SalarySolutionStandardBLL())
            {
                return bll.SalarySolutionStandardAdd(entity);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        [OperationContract]
        public bool SalarySolutionStandardDelete(string[] IDs)
        {
            using (SalarySolutionStandardBLL bll = new SalarySolutionStandardBLL())
            {
                int rslt = bll.SalarySolutionStandardDelete(IDs);

                return (rslt > 0);
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
        public List<T_HR_SALARYSOLUTIONSTANDARD> GetSalarySolutionStandardWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (SalarySolutionStandardBLL bll = new SalarySolutionStandardBLL())
            {
                IQueryable<T_HR_SALARYSOLUTIONSTANDARD> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region T_HR_SALARYITEM 薪资项设置

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
        public List<T_HR_SALARYITEM> GetSalaryItemSetPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string strCheckState, string userid)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                IQueryable<T_HR_SALARYITEM> q = bll.GetSalaryItemSetPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, userid);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 根据薪资项类型查询
        /// </summary>
        /// <param name="SalaryItemType">薪资项类型名称</param>
        /// <returns>返回薪资项检索结果</returns>
        [OperationContract]
        public List<T_HR_SALARYITEM> GetSalaryItemSets(string SalaryItemType)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                if (SalaryItemType == string.Empty)
                {
                    return bll.GetSalaryItemSets();
                }
                else
                {
                    return bll.GetSalaryItemSets(SalaryItemType);
                }
            }

        }

        /// <summary>
        /// 添加薪资项设置
        /// </summary>
        /// <param name="entity">薪资项设置实体</param>
        [OperationContract]
        public void SalaryItemSetAdd(T_HR_SALARYITEM entity)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                bll.Add(entity);
            }
        }

        /// <summary>
        /// 按薪资项模版自动生成薪资项
        /// </summary>
        /// <param name="entitys">薪资项设置实体集</param>
        [OperationContract]
        public bool FormulaTemplateAdd(T_HR_SALARYITEM[] entitys)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                return bll.FormulaTemplateAdd(entitys);
            }
        }

        /// <summary>
        /// 更新薪资项设置记录
        /// </summary>
        /// <param name="entity">薪资项设置实体</param>
        [OperationContract]
        public void SalaryItemSetUpdate(T_HR_SALARYITEM entity)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                bll.SalaryItemSetUpdate(entity);
            }
        }
        /// <summary>
        /// 删除薪资项设置实体
        /// </summary>
        /// <param name="SalaryItemSets">薪资项设置ID组</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool SalaryItemSetDelete(string[] SalaryItemSets)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                int rslt = bll.SalaryItemSetDelete(SalaryItemSets);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据薪资项设置ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资项设置ID</param>
        /// <returns>返回薪资项设置实体</returns>
        [OperationContract]
        public T_HR_SALARYITEM GetSalaryItemSetByID(string SalaryItemSetID)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                return bll.GetSalaryItemSetByID(SalaryItemSetID);
            }
        }

        /// <summary>
        /// 根据薪资项设置ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资项设置ID</param>
        /// <returns>返回薪资项设置实体</returns>
        [OperationContract]
        public T_HR_SALARYITEM GetSalaryItemSetByStandardID(string StandardID)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                return bll.GetSalaryItemSetByStandardID(StandardID);
            }
        }

        /// <summary>
        /// 根据薪资项名称查询
        /// </summary>
        /// <param name="SalaryItemSetName">薪资项设置名</param>
        /// <returns>返回bool类型</returns>
        [OperationContract]
        public bool GetSalaryItemSetName(string SalaryItemSetName)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                return bll.GetSalaryItemSetName(SalaryItemSetName);
            }
        }

        /// <summary>
        /// 检查计算项的值
        /// </summary>
        /// <param name="itemid">薪资项的ID</param>
        /// <returns>返回计算项的值检查结果</returns>
        [OperationContract]
        public decimal CheckCalItem(string itemid, string stitemid, ref bool ret)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
                return bll.CheckCalItem(itemid, stitemid, ref ret);
            }
        }

        /// <summary>
        /// xiedx
        /// 2012-8-24
        /// 初始化薪资项
        /// </summary>
        [OperationContract]
        public void ExecuteSalaryItemSql(T_HR_SALARYITEM salaryItem)
        {
            using (SalaryItemSetBLL bll = new SalaryItemSetBLL())
            {
               bll.ExecuteSalaryItemSql(salaryItem);
            }
        }

        #endregion

        #region T_HR_SALARYSTANDARDITEM 薪资标准薪资项

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
        public List<V_SALARYSTANDARDITEM> GetSalaryStandardItemPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string strCheckState, string userid)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                IQueryable<V_SALARYSTANDARDITEM> q = bll.GetSalaryStandardItemPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, userid);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        [OperationContract]
        public List<V_SALARYSTANDARDITEM> GetSalaryStandardItemsViewByStandarID(string standardID)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                return bll.GetSalaryStandardItemsViewByStandarID(standardID);
            }

        }

        /// <summary>
        /// 获取所有薪资项
        /// </summary>
        /// <returns>返回所有薪资项结果</returns>
        [OperationContract]
        public List<T_HR_SALARYSTANDARDITEM> GetSalaryStandardItems()
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                return bll.GetSalaryStandardItems();
            }
        }

        /// <summary>
        /// 添加薪资标准薪资项
        /// </summary>
        /// <param name="entity">薪资标准薪资项实体</param>
        [OperationContract]
        public int SalaryStandardItemAdd(T_HR_SALARYSTANDARDITEM entity)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                return bll.SalaryStandardItemAdd(entity);
            }
        }
        /// <summary>
        /// 更新薪资标准薪资项记录
        /// </summary>
        /// <param name="entity">薪资标准薪资项实体</param>
        [OperationContract]
        public void SalaryStandardItemUpdate(T_HR_SALARYSTANDARDITEM entity)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                bll.SalaryStandardItemUpdate(entity);
            }
        }
        /// <summary>
        /// 删除薪资标准薪资项实体
        /// </summary>
        /// <param name="SalaryItemSets">薪资标准薪资项ID组</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool SalaryStandardItemDelete(string[] SalaryItemSets)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                int rslt = bll.SalaryStandardItemDelete(SalaryItemSets);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据薪资标准薪资项ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资标准薪资项ID</param>
        /// <returns>返回薪资标准薪资项实体</returns>
        [OperationContract]
        public T_HR_SALARYSTANDARDITEM GetSalaryStandardItemByID(string SalaryItemSetID)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                return bll.GetSalaryStandardItemByID(SalaryItemSetID);
            }
        }

        /// <summary>
        /// 根据薪资标准ID查询实体标准薪资项
        /// </summary>
        /// <param name="SalaryItemSetID">薪资标准ID</param>
        /// <returns>返回薪资项实体集合</returns>
        [OperationContract]
        public List<T_HR_SALARYSTANDARDITEM> GetSalaryStandardItemsByStandardID(string standerID)
        {
            using (SalaryStandardItemBLL bll = new SalaryStandardItemBLL())
            {
                return bll.GetSalaryStandardItemsByStandardID(standerID);
            }
        }

        #endregion

        #region T_HR_EMPLOYEESALARYRECORDITEM 薪资记录薪资项记录

        /// <summary>
        /// 根据薪资记录ID查询实体
        /// </summary>
        /// <param name="SalaryRecordID">薪资记录ID</param>
        /// <returns>返回薪资记录薪资项实体结果集</returns>
        [OperationContract]
        public List<V_EMPLOYEESALARYRECORDITEM> GetEmployeeSalaryRecordItemByID(string SalaryRecordID)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.GetEmployeeSalaryRecordItemByID(SalaryRecordID);
            }
        }

        /// <summary>
        /// 删除薪资记录薪资项记录
        /// </summary>
        /// <param name="EmployeeSalaryRecordID">薪资记录ID</param>
        /// <returns></returns>
        [OperationContract]
        public int EmployeeSalaryRecordItemDelete(string EmployeeSalaryRecordID)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.EmployeeSalaryRecordItemDelete(EmployeeSalaryRecordID);
            }
        }

        /// <summary>
        /// 根据薪资记录薪资项ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资记录薪资项ID</param>
        /// <returns>返回薪资记录薪资项实体</returns>
        [OperationContract]
        public T_HR_EMPLOYEESALARYRECORDITEM GetSalaryItemByID(string SalaryItemSetID)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.GetSalaryItemByID(SalaryItemSetID);
            }
        }

        #endregion

        #region T_HR_SALARYTAXES 税率
        /// <summary>
        /// 检测是否允许新增
        /// </summary>
        /// <param name="salaryTaxesID">税率ID</param>
        /// <returns></returns>
        [OperationContract]
        public bool CheckSalaryTaxes(string salarySolutionID, decimal minSum)
        {
            using (SalaryTaxesBLL bll = new SalaryTaxesBLL())
            {
                return bll.CheckSalaryTaxes(salarySolutionID, minSum);
            }
        }
        [OperationContract]
        public List<T_HR_SALARYTAXES> GetSalaryTaxesBySolutionID(string ID)
        {
            using (SalaryTaxesBLL bll = new SalaryTaxesBLL())
            {
                return bll.GetSalaryTaxesBySolutionID(ID);
            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public string SalaryTaxesAdd(T_HR_SALARYTAXES obj)
        {
            using (SalaryTaxesBLL bll = new SalaryTaxesBLL())
            {
                return bll.SalaryTaxesAdd(obj);
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public string SalaryTaxesUpdate(T_HR_SALARYTAXES obj)
        {
            using (SalaryTaxesBLL bll = new SalaryTaxesBLL())
            {
                return bll.SalaryTaxesUpdate(obj);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public int SalaryTaxesDelete(string[] ids)
        {
            using (SalaryTaxesBLL bll = new SalaryTaxesBLL())
            {
                return bll.SalaryTaxesDelete(ids);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_SALARYTAXES> GetSalaryTaxesWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, string CheckState)
        {
            using (SalaryTaxesBLL bll = new SalaryTaxesBLL())
            {
                IQueryable<T_HR_SALARYTAXES> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region  薪资方案薪资项
        [OperationContract]
        public List<T_HR_SALARYSOLUTIONITEM> GetSalarySolutionItemsBySolutionID(string ID)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                return bll.GetSalarySolutionItemsBySolutionID(ID);
            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public string SalarySolutionItemAdd(T_HR_SALARYSOLUTIONITEM obj)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                return bll.SalarySolutionItemAdd(obj);
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        //[OperationContract]
        //public string SalaryTaxesUpdate(T_HR_SALARYTAXES obj)
        //{
        //    SalaryTaxesBLL bll = new SalaryTaxesBLL();
        //    return bll.SalaryTaxesUpdate(obj);
        //}
        //[OperationContract]
        //public string SalarySolutionItemsAdd(List<T_HR_SALARYSOLUTIONITEM> objs)
        //{
        //    SalarySolutionItemBLL bll = new SalarySolutionItemBLL();
        //    return bll.SalarySolutionItemsAdd(objs);
        //}
        [OperationContract]
        public string SalarySolutionItemsAdd(string filter, IList<object> para, string solutionID, string userID)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                Tracer.Debug("薪资方案完毕添加薪资方案薪资项开始");
                return bll.SalarySolutionItemsAdd(filter, para, solutionID, userID);
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public int SalarySolutionItemsDelete(string[] ids)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                return bll.SalarySolutionItemsDelete(ids);
            }
        }
        /// <summary>
        /// 根据方案薪资项目的ID获取方案薪资项目
        /// </summary>
        /// <param name="solutionItemID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SALARYSOLUTIONITEM GetSalarySolutionItemBysolutionItemID(string solutionItemID)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                return bll.GetSalarySolutionItemBysolutionItemID(solutionItemID);
            }
        }
        /// <summary>
        /// 更新薪资方案薪资项
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [OperationContract]
        public string SalarySolutionItemUpdate(T_HR_SALARYSOLUTIONITEM obj)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                return bll.SalarySolutionItemUpdate(obj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_SALARYSOLUTIONITEM> GetSalarySolutionItemsWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, string CheckState)
        {
            using (SalarySolutionItemBLL bll = new SalarySolutionItemBLL())
            {
                IQueryable<V_SALARYSOLUTIONITEM> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion
        #region T_HR_SALARYSYSTEM 薪资体系
        /// <summary>
        /// 根据ID获取薪资体系
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SALARYSYSTEM GetSalarySystemByID(string ID)
        {
            using (SalarySystemBLL bll = new SalarySystemBLL())
            {
                return bll.GetSalarySystemByID(ID);
            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public void SalarySystemAdd(T_HR_SALARYSYSTEM obj)
        {
            using (SalarySystemBLL bll = new SalarySystemBLL())
            {
                bll.SalarySystemAdd(obj);
            }
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public void SalarySystemUpdate(T_HR_SALARYSYSTEM obj)
        {
            using (SalarySystemBLL bll = new SalarySystemBLL())
            {
                bll.SalarySystemUpdate(obj);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public int SalarySystemDelete(string[] ids)
        {
            using (SalarySystemBLL bll = new SalarySystemBLL())
            {
                return bll.SalarySystemDelete(ids);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_SALARYSYSTEM> GetSalarySystemWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID, string CheckState)
        {
            using (SalarySystemBLL bll = new SalarySystemBLL())
            {
                IQueryable<T_HR_SALARYSYSTEM> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region  薪资登入核查
        /// <summary>
        /// 薪资登录核查
        /// </summary>
        /// <param name="employeid">员工</param>
        /// <param name="pwd">密码</param>
        /// <returns>返回结果</returns>
        [OperationContract]
        public bool LoginCheck(string employeid, string pwd)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                return bll.LoginCheck(employeid, pwd);
            }
        }

        [OperationContract]
        public string GetSalaryPassword(string employeName)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                return bll.GetSalaryPassword(employeName);
            }
        }

        /// <summary>
        /// 新增系统参数设置
        /// </summary>
        /// <param name="entity">系统参数设置实体</param>
        [OperationContract]
        public void AddSystemParamSet(T_HR_SYSTEMSETTING entity)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                bll.AddSystemParamSet(entity);
            }
        }

        /// <summary>
        /// 根据系统参数ID查询实体
        /// </summary>
        /// <param name="SystemParamSetID">系统参数ID</param>
        /// <returns>返回系统参数实体</returns>
        [OperationContract]
        public T_HR_SYSTEMSETTING GetSystemParamSet(string SystemParamSetID)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                return bll.GetSystemParamSet(SystemParamSetID);
            }
        }

        /// <summary>
        /// 根据系统参数类型查询实体
        /// </summary>
        /// <param name="SystemParamSetID">系统类型</param>
        /// <returns>返回系统参数实体</returns>
        [OperationContract]
        public List<T_HR_SYSTEMSETTING> GetSystemParamSetByType(string modeType)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                return bll.GetSystemParamSetByType(modeType);
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
        public List<T_HR_SYSTEMSETTING> GetSystemParamSetPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                IQueryable<T_HR_SYSTEMSETTING> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 更新系统参数
        /// </summary>
        /// <param name="entity">系统参数实体</param>
        [OperationContract]
        public void SystemParamSetUpdate(T_HR_SYSTEMSETTING entity)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                bll.SystemParamSetUpdate(entity);
            }
        }

        /// <summary>
        /// 删除系统参数记录，可同时删除多行记录
        /// </summary>
        /// <param name="SystemParamSetID">系统参数记录ID数组</param>
        /// <returns></returns>
        [OperationContract]
        public int SystemParamSetDelete(string[] SystemParamSetID)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                return bll.SystemParamSetDelete(SystemParamSetID);
            }
        }

        /// <summary>
        /// 入职新增薪资登录密码
        /// </summary>
        /// <param name="employeeid">员工自己的ID</param>
        /// <param name="employeeName">员工姓名</param>
        /// <param name="pwd">薪资登录密码</param>
        [OperationContract]
        public void AddSalaryPassword(string employeeid, string employeeName, string pwd)
        {
            using (SalaryLoginBLL bll = new SalaryLoginBLL())
            {
                bll.AddSalaryPassword(employeeid, employeeName, pwd);
            }
        }
        [OperationContract]
        public List<V_MonthDeductionTax> GetMonthDeductionTaxs(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, bool IsPageing)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.GetMonthDeductionTaxs(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, IsPageing);
            }
        }
        [OperationContract]
        public List<V_EmployeeDeductionMoney> GetEmployeeDeductionMoney(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.GetEmployeeDeductionMoney(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, year, month, IsPageing);
            }
        }
        [OperationContract]
        public List<V_SalarySummary> GetSalarySummary(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            using (EmployeeSalaryRecordItemBLL bll = new EmployeeSalaryRecordItemBLL())
            {
                return bll.GetSalarySummary(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, year, month, IsPageing);
            }
        }
        #endregion
        //创建薪资档案的薪资项目
        [OperationContract]
        public void CreateEmployeeSalaryArchiveItems(List<string> solutionID)
        {
            using (SalaryArchiveBLL bll = new SalaryArchiveBLL())
            {
                bll.CreateEmployeeSalaryArchiveItems(solutionID);
            }
        }

        #region 导出薪酬、表
        [OperationContract]
        public byte[] ExportEmployeePensionReports( string sort, string filterString, IList<object> paras, string userID,string CompanyID,DateTime Dt)
        {
            using (PensionDetailBLL bll = new PensionDetailBLL())
            {
                return bll.ExportEmployeePensionReports(sort, filterString, paras, userID, CompanyID,Dt);
            }
        }
        #endregion
        [OperationContract]
        public void PayRemindByOrgID(string strOrgType, string strOrgID, DateTime dtCurDate, ref string strMsg)
        {
            using (PaymentBLL bll = new PaymentBLL())
            {
                bll.PayRemindByOrgID(strOrgType, strOrgID, dtCurDate, ref strMsg);
            }
        }
        [OperationContract]
        public void AssignPersonMoney(string strCheckState, string Companyid, ref string strMsg)
        {
            using (SalaryRecordBatchBLL bll = new SalaryRecordBatchBLL())
            {
                bll.AssignPersonMoney(strCheckState, Companyid,ref strMsg);
            }
        }
    }
}
