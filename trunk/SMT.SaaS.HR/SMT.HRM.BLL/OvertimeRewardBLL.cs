
/*
 * 文件名：OvertimeRewardBLL.cs
 * 作  用：加班报酬 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-2-25 15:23:26
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{
    public class OvertimeRewardBLL : BaseBll<T_HR_OVERTIMEREWARD>, ILookupEntity
    {
        public OvertimeRewardBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取加班报酬信息
        /// </summary>
        /// <param name="strOvertimeRewardId">主键索引</param>
        /// <returns></returns>
        public T_HR_OVERTIMEREWARD GetOvertimeRewardByID(string strOvertimeRewardId)
        {
            if (string.IsNullOrEmpty(strOvertimeRewardId))
            {
                return null;
            }

            OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strOvertimeRewardId))
            {
                strfilter.Append(" OVERTIMEREWARDID == @0");
                objArgs.Add(strOvertimeRewardId);
            }

            T_HR_OVERTIMEREWARD entRd = dalOvertimeReward.GetOvertimeRewardRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取加班报酬信息
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strOverTimePayType">加班报酬方式：1 调休方式；2 加班工资方式；3 先调休再付工资方式；4 无报酬方式；</param>
        /// <param name="strOverTimeValID">加班生效方式：1 审核通过的加班申请；2 超过工作时间外自动累计；3 仅节假日累计；</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>返回加班报酬信息</returns>
        public IQueryable<T_HR_OVERTIMEREWARD> GetAllOvertimeRewardRdListByMultSearch(string strOwnerID, string strOverTimePayType, 
            string strOverTimeValID, string strSortKey)
        {
            OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strOverTimePayType))
            {
                strfilter.Append(" OVERTIMEPAYTYPE == @" + strOverTimePayType);
                objArgs.Add(strOverTimePayType);
            }

            if (!string.IsNullOrEmpty(strOverTimeValID))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" OVERTIMEVALID == @" + iIndex.ToString());
                objArgs.Add(strOverTimeValID);
            }
                        
            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " OVERTIMEREWARDID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_OVERTIMEREWARD");

            var q = dalOvertimeReward.GetOvertimeRewardRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取加班报酬信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strOverTimePayType">加班报酬方式：1 调休方式；2 加班工资方式；3 先调休再付工资方式；4 无报酬方式；</param>
        /// <param name="strOverTimeValID">加班生效方式：1 审核通过的加班申请；2 超过工作时间外自动累计；3 仅节假日累计；</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>返回加班报酬信息</returns>
        public IQueryable<T_HR_OVERTIMEREWARD> GetOvertimeRewardRdListByMultSearch(string strOwnerID, string strOverTimePayType, string strOverTimeValID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllOvertimeRewardRdListByMultSearch(strOwnerID, strOverTimePayType, strOverTimeValID, strSortKey);

            return Utility.Pager<T_HR_OVERTIMEREWARD>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增加班报酬信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddOvertimeReward(T_HR_OVERTIMEREWARD entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" OVERTIMEREWARDNAME == @0");
                objArgs.Add(entTemp.OVERTIMEREWARDNAME);

                OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();
                flag = dalOvertimeReward.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }
                
                dalOvertimeReward.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改加班报酬信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyOvertimeReward(T_HR_OVERTIMEREWARD entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" OVERTIMEREWARDID == @0");

                objArgs.Add(entTemp.OVERTIMEREWARDID);

                OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();
                flag = dalOvertimeReward.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_OVERTIMEREWARD entUpdate = dalOvertimeReward.GetOvertimeRewardRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                Utility.CloneEntity(entTemp, entUpdate);

                dalOvertimeReward.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除加班报酬信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteOvertimeReward(string strOvertimeRewardId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strOvertimeRewardId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" OVERTIMEREWARDID == @0");

                objArgs.Add(strOvertimeRewardId);

                OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();

                flag = dalOvertimeReward.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_OVERTIMEREWARD entDel = dalOvertimeReward.GetOvertimeRewardRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                if (entDel!=null)
                {
                    //查找考勤方案定义是否有使用
                    AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
                    StringBuilder strFilterOver = new StringBuilder();
                    List<string> objArgss = new List<string>();
                    strFilterOver.Append(" T_HR_OVERTIMEREWARD.OVERTIMEREWARDID == @0");
                    objArgss.Add(entDel.OVERTIMEREWARDID);
                    bool flagfalse = false;
                    flagfalse = dalAttendanceSolution.IsExistsRd(strFilterOver.ToString(), objArgss.ToArray());
                    //如果flagfalse为true，那么说明此加班设置有用到，不容许删除
                    if (flagfalse)
                    {
                        strMsg = "falseOver";
                    }
                    else
                    {
                        dalOvertimeReward.Delete(entDel);
                        strMsg = "{DELETESUCCESSED}";
                    } 
                }
                
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion


        #region ILookupEntity 成员

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{            
        //    OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();
        //    StringBuilder strfilter = new StringBuilder();
        //    List<string> objArgs = new List<string>();
        //    string strOrderBy = string.Empty;
        //    strOrderBy = " OVERTIMEREWARDID ";

        //    IQueryable<T_HR_OVERTIMEREWARD> ents = dalOvertimeReward.GetOvertimeRewardRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs);

        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            OvertimeRewardDAL dalOvertimeReward = new OvertimeRewardDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;
            strOrderBy = " OVERTIMEREWARDID ";

            IQueryable<T_HR_OVERTIMEREWARD> ents = dalOvertimeReward.GetOvertimeRewardRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        #endregion
    }
}

