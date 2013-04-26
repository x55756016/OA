using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SAAS.Platform.DAL.Interface;
using SMT.SAAS.Platform.DALFactory;
using SMT.SAAS.Platform.Model;
using System.Diagnostics;

namespace SMT.SAAS.Platform.BLL
{
    public class ShortCutBLL
    {
        private static readonly IShortCutDAL dal = DataAccess.CreateShortCut();

        /// <summary>
        /// 新增一个ShortCut到数据库中
        /// </summary>
        /// <param name="model">ShortCut数据</param>
        /// <returns>是否新增成功</returns>
        public bool Add(ShortCut model)
        {
            try
            {
                return dal.Add(model);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        /// <summary>
        /// 新增一组ShortCut到数据库中
        /// </summary>
        /// <param name="models">ShortCut数据</param>
        /// <returns>是否新增成功</returns>
        public bool AddByList(List<ShortCut> models)
        {
            try
            {
                bool result = false;
                foreach (var item in models)
                {
                    result = dal.Add(item);
                }
                return result;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        public bool AddByListAndUser(List<ShortCut> models, string userid)
        {
            try
            {
                bool result = false;
                foreach (var item in models)
                {
                    result = dal.AddByUser(item, userid);
                }
                return result;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        public List<ShortCut> GetShortCutByUser(string userID)
        {
            try
            {
                return dal.GetShortCutListByUser(userID);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        public bool DeleteShortCutByUser(string shortCutID, string userID)
        {
            try
            {
                return dal.DeleteShortCutByUser(shortCutID, userID);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }
    }
}
