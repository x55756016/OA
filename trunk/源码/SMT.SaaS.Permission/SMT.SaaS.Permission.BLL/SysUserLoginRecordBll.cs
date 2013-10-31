using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using SMT.SaaS.Permission.DAL.views;
using System.Linq.Dynamic;
using SMT.Foundation.Log;

namespace SMT.SaaS.Permission.BLL
{


    #region 用户登录表
    
    
    public class SysUserLoginRecordBll : BaseBll<T_SYS_USERLOGINRECORD>
    {
        
        /// <summary>
        /// 添加用户登录记录
        /// </summary>
        /// <param name="UserLoginObj"></param>
        /// <returns></returns>
        public bool AddUserLoginInfo(T_SYS_USERLOGINRECORD UserLoginObj)
        {
            SysUserLoginRecordDAL UserLoginDal = new SysUserLoginRecordDAL();
            try
            {
                //int i = dal.Add(UserLoginObj);
                dal.AddToContext(UserLoginObj);
                int i = dal.SaveContextChanges();
                if (i >0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-AddUserLoginInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 修改用户登录状态
        /// </summary>
        /// <param name="UserLoginObj"></param>
        /// <returns></returns>
        public bool UpdateUserLoginRecordStateInfo(T_SYS_USERLOGINRECORD UserLoginObj)
        {
            SysUserLoginRecordDAL UserLoginDal = new SysUserLoginRecordDAL();
            try
            {
                var entity = from ent in UserLoginDal.GetTable()
                             where ent.LOGINRECORDID == UserLoginObj.LOGINRECORDID
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    entitys.LOGINDATE = UserLoginObj.LOGINDATE;
                    entitys.LOGINIP = UserLoginObj.LOGINIP;
                    entitys.LOGINTIME = UserLoginObj.LOGINTIME;
                    entitys.ONLINESTATE = UserLoginObj.ONLINESTATE;
                    entitys.REMARK = UserLoginObj.REMARK;
                    entitys.USERNAME = UserLoginObj.USERNAME;
                                        
                    if (UserLoginDal.Update(entitys) == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-UpdateUserLoginRecordStateInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        public T_SYS_USERLOGINRECORD GetLoginUserRecordInfsByRecordID(string StruserID, string StrState)
        {
            SysUserLoginRecordDAL UserLoginDal = new SysUserLoginRecordDAL();
            try
            {
                var entity = from ent in UserLoginDal.GetTable()
                             where ent.LOGINRECORDID == StruserID && ent.ONLINESTATE == StrState
                             select ent;

                

                return entity.Count() > 0  ? entity.FirstOrDefault() : null;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-GetLoginUserRecordInfsByRecordID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }

        //public bool UpdateUserLoginRecordInfo(T_SYS_USERLOGINRECORD obj)
        //{
        //    return false;
        //}

        ///// <summary>
        ///// 用户登录记录
        ///// </summary>
        ///// <returns></returns>
        //public IQueryable<T_SYS_USERLOGINRECORD> GetAllUserLoginRecordInfos()
        //{
        //    var ents = from a in UserLoginDal.GetTable()
        //               select a;
        //    ents = ents.OrderByDescending(s => s.LOGINDATE);
        //    return ents;
        //}
        /// <summary>
        /// 用户登录记录
        /// </summary>
        /// <returns></returns>
        public IQueryable<V_UserLoginRecord> GetAllUserLoginRecordInfos()
        {

            try
            {                
                var ents = from p in base.GetObjects().Include("T_SYS_USER")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID                        
                           
                           select new V_UserLoginRecord
                           {
                               userloginrecord = p,
                               sysuser = u
                           };
                //T_SYS_ROLEMENUPERMISSION p = new T_SYS_ROLEMENUPERMISSION();
                //p.T_SYS_PERMISSION.PERMISSIONID

                List<V_UserLoginRecord> rl = ents.ToList();
                
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-GetAllUserLoginRecordInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }
        /// <summary>
        /// 服务器端分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_UserLoginRecord> GetAllUserLoginRecordInfosWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from p in base.GetObjects().Include("T_SYS_USER")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                           join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID

                           select new V_UserLoginRecord
                           {
                               userloginrecord = p,
                               sysuser = u
                           };
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_USERLOGINRECORD");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<V_UserLoginRecord>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-GetAllUserLoginRecordInfosWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }

        /// <summary>
        /// 查询用户登录记录
        /// </summary>
        /// <param name="StrState"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <returns></returns>
        public IQueryable<V_UserLoginRecord> GetUserLoginRecordInfosBySearch(string StrState, DateTime DtStart, DateTime DtEnd)
        {
            try
            {
                var q = from p in base.GetObjects().Include("T_SYS_USER")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                        join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID

                        select new V_UserLoginRecord
                        {
                            userloginrecord = p,
                            sysuser = u
                        };



                if (!string.IsNullOrEmpty(StrState))
                {
                    q = q.Where(s => s.userloginrecord.ONLINESTATE == StrState);
                }

                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.userloginrecord.LOGINDATE);
                    q = q.Where(s => DtEnd > s.userloginrecord.LOGINDATE);
                }


                q = q.OrderByDescending(s => s.userloginrecord.LOGINDATE);
                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-GetUserLoginRecordInfosBySearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public IQueryable<T_SYS_USERLOGINRECORD> GetUserLoginRecordByDate(string employeeid,DateTime DtStart, DateTime DtEnd)
        {
            try
            {
                var q = from p in base.GetObjects().Include("T_SYS_USER")//.Include("T_SYS_ROLEENTITYMENU.T_SYS_ENTITYMENU")
                        join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID

                        select p;




                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.LOGINDATE);
                    q = q.Where(s => DtEnd > s.LOGINDATE);
                }


                q = q.OrderByDescending(s => s.LOGINDATE);
                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录SysUserLoginRecordBll-GetUserLoginRecordByDate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

    }

    #endregion

    #region 用户登录历史记录表


    public class SysUserLoginRecordHistoryBll : BaseBll<T_SYS_USERLOGINRECORDHIS>
    {
        
        /// <summary>
        /// 添加用户登录记录
        /// </summary>
        /// <param name="UserLoginObj"></param>
        /// <returns></returns>
        public bool AddUserLoginHistoryInfo(T_SYS_USERLOGINRECORDHIS LoginHistoryObj)
        {
            SysUserLoginRecordHistroryDAL LoginHistoryDal = new SysUserLoginRecordHistroryDAL();
            try
            {
                int i = LoginHistoryDal.Add(LoginHistoryObj);
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录历史SysUserLoginRecordHistoryBll-GetOnLineUsers" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 修改用户登录状态
        /// </summary>
        /// <param name="UserLoginObj"></param>
        /// <returns></returns>
        public bool UpdateUserLoginHistoryRecordStateInfo(T_SYS_USERLOGINRECORDHIS UserLoginObj)
        {
            SysUserLoginRecordHistroryDAL LoginHistoryDal = new SysUserLoginRecordHistroryDAL();
            try
            {
                var entity = from ent in LoginHistoryDal.GetTable()
                             where ent.LOGINRECORDHISID == UserLoginObj.LOGINRECORDHISID
                             select ent;

                if (entity.Count() > 0)
                {
                    var entitys = entity.FirstOrDefault();
                    entitys.LOGINDATE = UserLoginObj.LOGINDATE;
                    entitys.LOGINIP = UserLoginObj.LOGINIP;
                    entitys.LOGINTIME = UserLoginObj.LOGINTIME;
                    entitys.ONLINESTATE = UserLoginObj.ONLINESTATE;
                    entitys.REMARK = UserLoginObj.REMARK;
                    entitys.USERNAME = UserLoginObj.USERNAME;

                    if (LoginHistoryDal.Update(entitys) == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录历史SysUserLoginRecordHistoryBll-GetOnLineUsers" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }


        ///// <summary>
        ///// 用户登录历史记录
        ///// </summary>
        ///// <returns></returns>
        //public IQueryable<T_SYS_USERLOGINRECORDHIS> GetAllUserLoginHistoryRecordInfos()
        //{
        //    var ents = from a in LoginHistoryDal.GetTable()
        //               select a;
        //    ents = ents.OrderByDescending(s => s.LOGINDATE);
        //    return ents;
        //}
        /// <summary>
        /// 用户登录历史记录
        /// </summary>
        /// <returns></returns>
        public IQueryable<V_UserLoginRecordHistory> GetAllUserLoginHistoryRecordInfos()
        {
            try
            {
                var ents = from p in GetObjects().Include("T_SYS_USER")
                           join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID

                           select new V_UserLoginRecordHistory
                           {
                               historyrecord = p,
                               sysuser = u
                           };
                //T_SYS_ROLEMENUPERMISSION p = new T_SYS_ROLEMENUPERMISSION();
                //p.T_SYS_PERMISSION.PERMISSIONID

                List<V_UserLoginRecordHistory> rl = ents.ToList();

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录历史SysUserLoginRecordHistoryBll-GetOnLineUsers" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 服务器端分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_UserLoginRecordHistory> GetAllUserLoginHistoryRecordInfosWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from p in dal.GetObjects<T_SYS_USERLOGINRECORDHIS>().Include("T_SYS_USER")
                           join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID

                           select new V_UserLoginRecordHistory
                           {
                               historyrecord = p,
                               sysuser = u
                           };
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_SYS_USERLOGINRECORDHIS");
                //SysUserBLL bll = new SysUserBLL();
                //List<object> queryParas1 = new List<object>();
                //if (paras != null)
                //{
                //    queryParas1.AddRange(paras);
                //}
                //bll.SetOrganizationFilterToNewFrame(ref filterString, ref queryParas1, userID, "T_OA_APPROVALINFO", "1");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<V_UserLoginRecordHistory>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录历史SysUserLoginRecordHistoryBll-GetOnLineUsers" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }

        }

        /// <summary>
        /// 查询用户历史登录记录
        /// </summary>
        /// <param name="StrState"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <returns></returns>
        public IQueryable<V_UserLoginRecordHistory> GetUserLoginHistoryRecordInfosBySearch(string StrState,DateTime DtStart,DateTime DtEnd)
        {
            try
            {
                var q = from p in dal.GetObjects<T_SYS_USERLOGINRECORDHIS>().Include("T_SYS_USER")
                        join u in dal.GetObjects<T_SYS_USER>() on p.USERNAME equals u.SYSUSERID

                        select new V_UserLoginRecordHistory
                        {
                            historyrecord = p,
                            sysuser = u
                        };



                if (!string.IsNullOrEmpty(StrState))
                {
                    q = q.Where(s => s.historyrecord.ONLINESTATE == StrState);
                }

                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.historyrecord.LOGINDATE);
                    q = q.Where(s => DtEnd > s.historyrecord.LOGINDATE);
                }


                q = q.OrderByDescending(s => s.historyrecord.LOGINDATE);
                return q;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户登录历史SysUserLoginRecordHistoryBll-GetOnLineUsers" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }



    }


    #endregion
}
