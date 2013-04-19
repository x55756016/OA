using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using System.Data.Objects.DataClasses;

namespace SMT.HRM.BLL
{
    public class ShiftDefineBLL : BaseBll<T_HR_SHIFTDEFINE>, ILookupEntity
    {
        public ShiftDefineBLL()
        {

        }

        #region 获取数据

        /// <summary>
        /// 获取T_HR_SHIFTDEFINE信息
        /// </summary>
        /// <param name="strShiftDefineId">主键索引</param>
        /// <returns></returns>
        public T_HR_SHIFTDEFINE GetShiftDefineByID(string strShiftDefineID)
        {
            if (string.IsNullOrEmpty(strShiftDefineID))
            {
                return null;
            }

            ShiftDefineDAL dalShiftDefine = new ShiftDefineDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strShiftDefineID))
            {
                strfilter.Append(" SHIFTDEFINEID == @0");
                objArgs.Add(strShiftDefineID);
            }

            T_HR_SHIFTDEFINE entRd = dalShiftDefine.GetShiftDefineRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 获取考勤班次信息
        /// </summary>
        /// <param name="strShiftdEfineID">考勤班次序号</param>
        /// <param name="strShiftdEfineName">考勤班次名称</param>
        /// <param name="strCompanyID">公司序号</param>        
        /// <param name="strSortKey">排序字段</param>        
        /// <returns>返回考勤班次信息</returns>
        public IQueryable<T_HR_SHIFTDEFINE> GetAllShiftDefineListByMultSearch(string strOwnerID, string strShiftDefineName, string strCompanyID, string strSortKey)
        {
            ShiftDefineDAL dalShiftdEfine = new ShiftDefineDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strShiftDefineName))
            {
                strfilter.Append(" @0.Contains(SHIFTNAME)");
                objArgs.Add(strShiftDefineName);
            }

            if (!string.IsNullOrEmpty(strCompanyID))
            {
                if (string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" COMPANYID == @" + iIndex.ToString());
                objArgs.Add(strCompanyID);
            }

            if (!string.IsNullOrEmpty(strCompanyID))
            {
                if (string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" COMPANYID == @" + iIndex.ToString());
                objArgs.Add(strCompanyID);
            }
            
            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " SHIFTDEFINEID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_OVERTIMEREWARD");

            var q = dalShiftdEfine.GetShiftDefineListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 获取考勤班次信息
        /// </summary>
        /// <param name="strShiftdEfineID">考勤班次序号</param>
        /// <param name="strShiftdEfineName">考勤班次名称</param>
        /// <param name="strCompanyID">公司序号</param>        
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回考勤班次信息</returns>
        public IQueryable<T_HR_SHIFTDEFINE> GetShiftDefineListByMultSearch(string strOwnerID, string strShiftdEfineName, string strCompanyID, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllShiftDefineListByMultSearch(strOwnerID, strShiftdEfineName, strCompanyID, strSortKey);
            return Utility.Pager(q, pageIndex, pageSize, ref pageCount); 
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增T_HR_SHIFTDEFINE信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddShiftDefine(T_HR_SHIFTDEFINE entTemp)
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

                strFilter.Append(" SHIFTNAME == @0");

                objArgs.Add(entTemp.SHIFTNAME);

                ShiftDefineDAL dalShiftDefine = new ShiftDefineDAL();
                flag = dalShiftDefine.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalShiftDefine.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改T_HR_SHIFTDEFINE信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyShiftDefine(T_HR_SHIFTDEFINE entTemp)
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

                strFilter.Append(" SHIFTDEFINEID == @0");

                objArgs.Add(entTemp.SHIFTDEFINEID);

                ShiftDefineDAL dalShiftDefine = new ShiftDefineDAL();
                flag = dalShiftDefine.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_SHIFTDEFINE entUpdate = dalShiftDefine.GetShiftDefineRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalShiftDefine.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除T_HR_SHIFTDEFINE信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteShiftDefine(string strShiftDefineId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strShiftDefineId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" SHIFTDEFINEID == @0");

                objArgs.Add(strShiftDefineId);

                ShiftDefineDAL dalShiftDefine = new ShiftDefineDAL();
                flag = dalShiftDefine.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_SHIFTDEFINE entDel = dalShiftDefine.GetShiftDefineRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalShiftDefine.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }            

            return strMsg;
        }

        #endregion

        #region ILookupEntity 成员

        //public System.Data.Objects.DataClasses.EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    ShiftDefineDAL dalShiftDefine = new ShiftDefineDAL();
        //    StringBuilder strfilter = new StringBuilder();
        //    List<string> objArgs = new List<string>();
        //    string strOrderBy = string.Empty;
        //    strOrderBy = " SHIFTDEFINEID ";

        //    IQueryable<T_HR_SHIFTDEFINE> ents = dalShiftDefine.GetShiftDefineListByMultSearch(strOrderBy, strfilter.ToString(), objArgs);

        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}
        
        
        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            //TODO:实现分页
            ShiftDefineDAL dalShiftDefine = new ShiftDefineDAL();
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();

            if (!string.IsNullOrWhiteSpace(filterString))
            {
                strfilter.Append(filterString);
            }

            if (paras != null)
            {
                if (paras.Count() > 0 && !string.IsNullOrWhiteSpace(filterString))
                {
                    objArgs.AddRange(paras);
                }
            }

            string strfilterString = strfilter.ToString();

            SetOrganizationFilter(ref strfilterString, ref objArgs, userID, "T_HR_SHIFTDEFINE");

            string strOrderBy = string.Empty;
            strOrderBy = " SHIFTDEFINEID ";

            IQueryable<T_HR_SHIFTDEFINE> ents = dalShiftDefine.GetShiftDefineListByMultSearch(strOrderBy, strfilterString, objArgs.ToArray());

            return ents.Count() > 0 ? ents.ToArray() : null;
        }
        #endregion
               
    }
}
