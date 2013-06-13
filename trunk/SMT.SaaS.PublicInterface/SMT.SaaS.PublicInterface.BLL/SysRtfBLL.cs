using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using SMT.Foundation.Log;
using SMT.SaaS.PublicInterface.DAL;

namespace SMT.SaaS.PublicInterface.BLL
{
    public class SysRtfBLL : BaseBLL<T_SYS_RTF>
    {
        /// <summary>
        /// 根据表单ID获取角色信息
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>T_SYS_RTF</returns>
        public byte[] GetContent(string FORMID)
        {
            try
            {

                var items = from o in dal.GetTable() where o.FORMID == FORMID select o;
                return items.FirstOrDefault().CONTENT as byte[];
            }
            catch (Exception ex)
            {
                Tracer.Debug("内容SysRtfBLL-GetContent" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 添加信息
        /// </summary>        
        /// <returns>添加结果</returns>
        public bool AddContent(T_SYS_RTF t_SYS_RTF)
        {
            try
            {
                SMT_System_EFModelContext efmodel = new SMT_System_EFModelContext();
                var items = from o in efmodel.T_SYS_RTF where o.FORMID == t_SYS_RTF.FORMID select o;
                if (items.Count() > 0)
                {
                    return false;
                }
                else
                {
                    Tracer.Debug("内容SysRtfBLL-AddContent FormID:" + t_SYS_RTF.FORMID + " SYSTEMCODE:" + t_SYS_RTF.SYSTEMCODE + " MODELNAME:" + t_SYS_RTF.MODELNAME + " DateTime:" + System.DateTime.Now.ToString());
                    int i = dal.Add(t_SYS_RTF);
                    if (i == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("内容SysRtfBLL-AddContent" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="content">内容</param>
        /// <param name="UPDATEUSERID">用户ID</param>
        /// <param name="UPDATEUSERNAME">用户名</param>
        /// <returns>bool</returns>
        public bool UpdateContent(string FormID, byte[] content, UserInfo userinfo)
        {
            try
            {
                SMT_System_EFModelContext efmodel = new SMT_System_EFModelContext();
                var items = from o in efmodel.T_SYS_RTF where o.FORMID == FormID select o;
                if (items.Count() > 0)
                {
                    var item = items.FirstOrDefault();
                    item.CONTENT = content;
                    item.UPDATEUSERID = userinfo.USERID;
                    item.UPDATEUSERNAME = userinfo.USERNAME;
                    item.UPDATEDATE = DateTime.Now;
                }
                Tracer.Debug("内容SysRtfBLL-UpdateContent FormID:" + FormID + " DateTime:" + System.DateTime.Now.ToString());
                if (efmodel.SaveChanges() == 1)
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
                Tracer.Debug("内容SysRtfBLL-UpdateContent" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>bool</returns>
        public bool DeleteContent(string FORMID)
        {
            try
            {
                var items = from o in dal.GetObjects() where o.FORMID == FORMID select o;
                if (items.Count() > 0)
                {
                    var obj = items.FirstOrDefault();
                    dal.DeleteFromContext(obj);
                    int i = dal.SaveContextChanges();
                    Tracer.Debug("内容SysRtfBLL-DeleteContent FormID:" + FORMID + " DateTime:" + System.DateTime.Now.ToString());
                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("内容SysRtfBLL-DeleteContent" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 判断数据是否存在
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>bool</returns>
        public bool IsExits(string FORMID)
        {
            try
            {
                var items = from o in dal.GetTable() where o.FORMID == FORMID select o;
                if (items.Count() > 0)
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
                Tracer.Debug("内容SysRtfBLL-IsExits" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

    }
}
