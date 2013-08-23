/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： 数据访问异常类
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Common.DataAccess.Exception
{
    /// <summary>
    /// 技术异常类
    /// </summary>
    [Serializable]
    public class TechException : ApplicationException
    {
        private string _id;
        private string _machineName;
        private DateTime _errorTime;
        private int _userID;
        private string _desc;

        // 初始化异常信息
        private void Initialize()
        {
            try
            {
                this._id = System.Guid.NewGuid().ToString();
                _machineName = Environment.MachineName;
                _errorTime = DateTime.Now;
                //m_userID = SessionData.Current.UserID;
                _desc = "";
            }
            catch { };
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="message">异常信息</param>
        public TechException(string message)
            : base(message)
        {
            Initialize();
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="message">自定义异常信息</param>
        /// <param name="description">异常详细的描述信息</param>
        /// <param name="innerException">引发此异常的异常对象</param>
        public TechException(string message, string description, System.Exception innerException)
            : base(message, innerException)
        {
            Initialize();
            _desc = description;
        }

        /// <summary>
        /// 异常ID
        /// </summary>
        public string ID
        {
            get { return this._id; }
            set { this._id = value; }
        }

        /// <summary>
        /// 异常所在机器名
        /// </summary>
        public string MachineName
        {
            get { return _machineName; }
            set { _machineName = value; }
        }

        /// <summary>
        /// 异常发成时间
        /// </summary>
        public DateTime ExceptionTime
        {
            get { return _errorTime; }
            set { _errorTime = value; }
        }

        /// <summary>
        /// 引发异常的用户ID
        /// </summary>
        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        /// <summary>
        /// 自定义的异常描述信息
        /// </summary>
        public string Description
        {
            get { return _desc; }
            set { _desc = value; }
        }
    }
}
