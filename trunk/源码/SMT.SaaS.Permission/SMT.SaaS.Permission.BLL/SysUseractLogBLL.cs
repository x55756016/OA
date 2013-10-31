using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using SMT_System_EFModel;
using System.Linq.Dynamic;
using SMT.SaaS.Permission.DAL;
using SMT.Foundation.Log;


namespace SMT.SaaS.Permission.BLL
{
    public class SysUseractLogBLL : BaseBll<T_SYS_USERACTLOG>
    {
        private SysUseractLogDAL UseractDal = new SysUseractLogDAL();

        /// <summary>
        /// 用户日志修改操作
        /// </summary>
        /// <param name="UseractLog"></param>
        /// <returns></returns>
        public bool UpdateUseractLog(T_SYS_USERACTLOG UseractLog)
        {
            try
            {
                var entUserLog = from s in UseractDal.GetTable()
                                 where s.LOGID == UseractLog.LOGID  select s;
                if (entUserLog.Count() > 0)
                {
                    var entUserLogs = entUserLog.FirstOrDefault();
                    entUserLogs.LOGTYPE = UseractLog.LOGTYPE;
                    entUserLogs.OWNERCOMPANYID = UseractLog.OWNERCOMPANYID;
                    entUserLogs.LOGCONTEXT = UseractLog.LOGCONTEXT;
                    entUserLogs.OWNERDEPARTMENTID = UseractLog.OWNERDEPARTMENTID;
                    entUserLogs.OWNERID = UseractLog.OWNERID;
                    entUserLogs.OWNERPOSTID = UseractLog.OWNERPOSTID;
                    entUserLogs.POSTNAME = UseractLog.POSTNAME;
                    entUserLogs.SERVERNETRUNTIME = UseractLog.SERVERNETRUNTIME;
                    entUserLogs.SERVEROS = UseractLog.SERVEROS;
                    entUserLogs.CLIENTBROWSER = UseractLog.CLIENTBROWSER;
                    entUserLogs.CLIENTHOSTADDRESS = UseractLog.CLIENTHOSTADDRESS;
                    entUserLogs.CLIENTNETRUNTIME = UseractLog.CLIENTNETRUNTIME;
                    entUserLogs.CLIENTOS = UseractLog.CLIENTOS;
                    entUserLogs.CLIENTOSLANGUAGE = UseractLog.CLIENTOSLANGUAGE;
                    entUserLogs.COMPANYNAME = UseractLog.COMPANYNAME;
                    entUserLogs.CREATEDATE = UseractLog.CREATEDATE;
                    entUserLogs.DEPARTMENTNAME = UseractLog.DEPARTMENTNAME;
                    entUserLogs.EMPLOYEENAME = UseractLog.EMPLOYEENAME;
                    entUserLogs.ENTITYMENU = UseractLog.ENTITYMENU;

                    int i = UseractDal.Update(entUserLogs);
                    if (i == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户日志SysUseractLogBLL-UpdateUseractLog" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        
        }
        /// <summary>
        /// 添加日志操作
        /// </summary>
        /// <param name="UseractLog"></param>
        /// <returns></returns>
        public bool InsertUseractLog(T_SYS_USERACTLOG UseractLog)
        {
            try
            {
                int i = UseractDal.Add(UseractLog);
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Tracer.Debug("用户日志SysUseractLogBLL-InsertUseractLog" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (e);
            }
        }
        /// <summary>
        /// 删除日志记录
        /// </summary>
        /// <param name="UseractLog"></param>
        /// <returns></returns>
        public bool DeleteUseractLog(T_SYS_USERACTLOG UseractLog)
        {
            try
            {
                var delUserlog = from s in UseractDal.GetTable()
                                 where  s.LOGID == UseractLog.LOGID  select s;
                if (delUserlog.Count() > 0)
                {
                    var delUserlogs = delUserlog.FirstOrDefault();
                    int i = UseractDal.Delete(delUserlogs);
                    if (i == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户日志SysUseractLogBLL-DeleteUseractLog" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 日志服务端分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_SYS_USERACTLOG> GetSysUseractLogWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var userent = from s in GetObjects() select s;
                List<object> alist = new List<object>();
                alist.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref alist, userID, "T_SYS_USERACTLOG");
                if (alist.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        userent = userent.ToList().AsQueryable().Where(filterString, alist.ToArray());
                    }
                }
                userent = userent.OrderBy(sort);
                userent = Utility.Pager<T_SYS_USERACTLOG>(userent, pageIndex, pageSize, ref pageCount);
                return userent;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户日志SysUseractLogBLL-GetSysUseractLogWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 查询所有日志记录
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_SYS_USERACTLOG> GetSysUseractLogList()
        {
            try
            {
                var ends = from s in UseractDal.GetTable() select s;
                return ends;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户日志SysUseractLogBLL-GetSysUseractLogList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据ID 查询日志记录 并返回第一行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T_SYS_USERACTLOG GetSysUseractLogByID(string id)
        {
            try
            {
                var ends = from s in UseractDal.GetTable() where s.LOGID.Equals(id) select s;
                return ends.Count() > 0 ? ends.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("用户日志SysUseractLogBLL-GetSysUseractLogByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
    }
}
