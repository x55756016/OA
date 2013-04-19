using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.OA.DAL.Views;
using System.Web.Security;
using System.Web;
using System.Security.Principal;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region  地区差异补贴
        /// <summary>
        /// 添加地区差异补贴
        /// </summary>
        /// <param name="obj">地区差异补贴实例</param>
        [OperationContract]
        public void AreaAllowanceADD(T_OA_AREAALLOWANCE obj, string travelSolutionsId)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                bll.AreaAllowanceAdd(obj, travelSolutionsId);
            }
        }
        /// <summary>
        /// 更新地区差异补贴
        /// </summary>
        /// <param name="obj">地区差异补贴实例</param>
        [OperationContract]
        public void AreaAllowanceUpdate(T_OA_AREAALLOWANCE obj)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                bll.AreaAllowanceUpdate(obj);
            }
        }
        /// <summary>
        /// 地区差异补贴
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void AreaAllowance(List<T_OA_AREAALLOWANCE> objs, string travelSolutionsId)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                bll.AreaAllowance(objs, travelSolutionsId);
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
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
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
        public List<T_OA_AREAALLOWANCE> GetAreaAllowanceByAreaID(string ID, string SolutionID)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                return bll.GetAreaAllowanceByAreaID(ID, SolutionID);
            }
        }
        /// <summary>
        /// 根据ID获取地区差异补贴
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_AREAALLOWANCE GetAreaAllowanceByID(string ID)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                return bll.GetAreaAllowanceByID(ID);
            }
        }
        /// <summary>
        /// 根据获取地区差异分类
        /// </summary>
        /// <returns>地区差异分类集</returns>
        [OperationContract]
        public List<T_OA_AREADIFFERENCE> GetAreaCategory()
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
        public List<T_OA_AREACITY> GetAreaCityByCategory(string categoryID)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                return bll.GetAreaCityByCategory(categoryID);
            }
        }
        /// <summary>
        /// 增加城市
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void AreaCityAdd(T_OA_AREACITY obj)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
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
        public Dictionary<string, string> AreaCityCheck(List<T_OA_AREACITY> objs, string areadifferenceid)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                return bll.AreaCityCheck(objs, areadifferenceid);
            }
        }

        /// <summary>
        /// 根据地区类型删除城市
        /// </summary>
        /// <param name="CategoryID">地区类型ID</param>
        /// <returns>返回布尔类型结果</returns>
        [OperationContract]
        public bool AreaCityByCategoryDelete(string CategoryID,string delCode)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {
                return bll.AreaCityByCategoryDelete(CategoryID,delCode);
            }
        }

        /// <summary>
        /// 多个城市增加
        /// </summary>
        /// <param name="objs"></param>
        [OperationContract]
        public string AreaCityLotsofAdd(List<T_OA_AREACITY> objs)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
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
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
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
        public T_OA_AREADIFFERENCE GetAreaCategoryByID(string ID)
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
        public void AreaCategoryUpdate(T_OA_AREADIFFERENCE obj)
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
        public string AreaCategoryADD(T_OA_AREADIFFERENCE obj, string solutionsId, string companyId)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {
                return bll.AreaCategoryADD(obj, solutionsId, companyId);
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
        public List<T_OA_AREAALLOWANCE> GetAreaAllowanceWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {

                IQueryable<T_OA_AREAALLOWANCE> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
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
        public List<T_OA_AREACITY> GetAreaCityWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (AreaAllowanceBll bll = new AreaAllowanceBll())
            {

                IQueryable<T_OA_AREACITY> q = bll.QueryCityWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount);
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
        public List<T_OA_AREADIFFERENCE> GetAreaWithPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string StrCompanyid, string strSoluid)
        {
            using (AreaCategoryBLL bll = new AreaCategoryBLL())
            {

                IQueryable<T_OA_AREADIFFERENCE> q = bll.QueryAreaWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, StrCompanyid, strSoluid);
                return q != null ? q.ToList() : null;
            }
        }

        #endregion


        #region 出差方案
        /// <summary>
        /// 添加出差解决方案
        /// </summary>
        /// <param name="EntObj"></param>
        /// <param name="ListTransport"></param>
        /// <param name="ListPlane"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddTravleSolution(T_OA_TRAVELSOLUTIONS EntObj, List<T_OA_TAKETHESTANDARDTRANSPORT> ListTransport, List<string> companyids)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.AddTravleSolution(EntObj, ListTransport, companyids);
            }
        }
        /// <summary>
        /// 修改出差方案
        /// </summary>
        /// <param name="EntTravle"></param>
        /// <param name="TransportObj"></param>
        /// <param name="PlaneObj"></param>
        /// <param name="IsChange"></param>
        /// <returns></returns>
        [OperationContract]
        public int UpdateTravleSolution(T_OA_TRAVELSOLUTIONS EntTravle, List<T_OA_TAKETHESTANDARDTRANSPORT> TransportObj, List<string> companyids, bool IsChange)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.UpdateTravleSolutionInfo(EntTravle, TransportObj, companyids, IsChange);
            }
        }
        /// <summary>
        /// 删除解决方案
        /// </summary>
        /// <param name="SolutionID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool DeleteTravleSolution(string SolutionID)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.DeleteTravleSolution(SolutionID);
            }
        }
        /// <summary>
        /// 获取解决方案
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_TRAVELSOLUTIONS> GetTravelSolutionFlow(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            //using (TravleSolutionBLL bll = new TravleSolutionBLL())
            //{
            TravleSolutionBLL bll = new TravleSolutionBLL();
            IQueryable<T_OA_TRAVELSOLUTIONS> SolutionList = null;
            SolutionList = bll.GetTravelSolutionFlow(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

            return SolutionList != null ? SolutionList.ToList() : null;
            //}
        }

        /// <summary>
        /// 根据方案ID获取 交通工具标准，飞机路线
        /// </summary>
        /// <param name="SolutionID"></param>
        /// <param name="PlaneList"></param>
        /// <param name="VechileStandardList"></param>
        [OperationContract]
        public void GetVechileStandardAndPlaneLine(string SolutionID, ref List<T_OA_CANTAKETHEPLANELINE> PlaneList, ref List<T_OA_TAKETHESTANDARDTRANSPORT> VechileStandardList)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                bll.GetVechileStandardAndPlaneLine(SolutionID, ref PlaneList, ref VechileStandardList);
            }
        }

        /// <summary>
        /// 根据方案ID查询对应的城市分类
        /// </summary>
        /// <param name="SolutionID">方案ID</param>
        [OperationContract]
        public List<T_OA_AREACITY> GetQueryPlanCity(string SolutionID, ref List<T_OA_AREADIFFERENCE> ListAREADIFFERENCE)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.GetQueryPlanCity(SolutionID, ref ListAREADIFFERENCE);
            }
        }

        /// <summary>
        /// 查询对应的补贴
        /// </summary>
        /// <param name="SolutionID">方案ID</param>
        [OperationContract]
        public List<T_OA_AREAALLOWANCE> GetQueryProgramSubsidies(string SolutionID)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.GetQueryProgramSubsidies(SolutionID);
            }
        }

        /// <summary>
        /// 根据公司ID获取对应的 解决方案  
        /// 并返回对应的 交通工具标准、飞机路线
        /// </summary>
        /// <param name="Companyid"></param>
        /// <param name="PlaneObj"></param>
        /// <param name="StandardObj"></param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_TRAVELSOLUTIONS GetTravelSolutionByCompanyID(string Companyid, ref List<T_OA_CANTAKETHEPLANELINE> PlaneObj, ref List<T_OA_TAKETHESTANDARDTRANSPORT> StandardObj)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.GetTravelSolutionByCompanyID(Companyid, ref  PlaneObj, ref StandardObj);
            }
        }
        /// <summary>
        /// 根据城市 和岗位值取得 津贴信息
        /// </summary>
        /// <param name="postvalue"></param>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        [OperationContract]
        public T_OA_AREAALLOWANCE GetTravleAreaAllowanceByCompanyID(string postvalue, string cityvalue)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.GetTravleAreaAllowanceByCompanyID(postvalue, cityvalue);
            }

        }

        /// <summary>
        /// 根据多对应的津贴信息 并返回多有的城市类别 和岗位值取得 津贴信息
        /// </summary>
        /// <param name="postvalue"></param>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_AREAALLOWANCE> GetTravleAreaAllowanceByPostValue(string postvalue, string solutionId, ref List<T_OA_AREACITY> citys)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.GetTravleAreaAllowanceByPostValue(postvalue, solutionId, ref citys);
            }
        }
        /// <summary>
        /// 添加 出差方案的设置
        /// </summary>
        /// 
        /// <param name="SolutionID"></param>
        /// <param name="companyids"></param>
        /// <param name="ownercompanyid"></param>
        /// <param name="ownerpostid"></param>
        /// <param name="ownerdepartmentid"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddTravleSolutionSet(string SolutionID, List<string> companyids, string ownercompanyid, string ownerpostid, string ownerdepartmentid, string userid)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.AddTravleSolutionSet(SolutionID, companyids, ownercompanyid, ownerpostid, ownerdepartmentid, userid);
            }
        }
        /// <summary>
        /// 修改出差方案设置
        /// </summary>
        /// <param name="SolutionID"></param>
        /// <param name="companyids"></param>
        /// <param name="ownercompanyid"></param>
        /// <param name="ownerpostid"></param>
        /// <param name="ownerdepartmentid"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateTravleSolutionSet(string SolutionID, List<string> companyids, string ownercompanyid, string ownerpostid, string ownerdepartmentid, string userid)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.UpdateTravleSolutionSet(SolutionID, companyids, ownercompanyid, ownerpostid, ownerdepartmentid, userid);
            }
        }
        /// <summary>
        /// 获取方案设置信息
        /// </summary>
        /// <param name="SolutionId"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_PROGRAMAPPLICATIONS> GetTravleSolutionSetBySolutionID(string SolutionId)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                IQueryable<T_OA_PROGRAMAPPLICATIONS> SetList = bll.GetTravleSolutionSetBySolutionID(SolutionId);

                return SetList != null ? SetList.ToList() : null;
            }

        }
        #endregion

        /// <summary>
        /// 复制差解决方案
        /// </summary>
        /// <param name="EntObj">方案实体</param>
        /// <param name="travleSolutionId">旧方案ID</param>
        /// <param name="companyids">公司ID</param>
        /// <returns></returns>
        [OperationContract]
        public string GetCopyTravleSolution(T_OA_TRAVELSOLUTIONS EntObj, string OldtravleSolutionId)
        {
            using (TravleSolutionBLL bll = new TravleSolutionBLL())
            {
                return bll.GetCopyTravleSolution(EntObj, OldtravleSolutionId);
            }
        }
    }
}