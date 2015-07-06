/*
版权信息：提莫科技
作    者：提莫科技
日    期：2009-09-22
内容摘要： 自定义数据访问参数
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace SMT.Foundation.Core
{
    public sealed class DBHelper
    {

        /// <summary>
        /// 获取值，如果obj是null，近回DBNull.Value,否则返回obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetValue(object obj)
        {
            if (obj == null || obj.ToString() == "")
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }
    }
    /// <summary>
    /// 参数实体类
    /// </summary>
    [Serializable]
    public class Parameter
    {
        // 参数名
        private string _paramName;
        // 参数值
        private object _paramValue;

        private string _paramType;

        /// <summary>
        /// 构造器
        /// </summary>
        public Parameter()
        {
            _paramName = "";
            _paramValue = null;
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="parameterValue">参数值</param>
        public Parameter(string parameterName, object parameterValue)
        {
            _paramName = parameterName;
            _paramValue = parameterValue;
        }

        /// <summary>
        /// 参数名
        /// </summary>
        public string ParameterName
        {
            get { return _paramName; }
            set { _paramName = value; }
        }

        /// <summary>
        /// 参数值
        /// </summary>
        public object ParameterValue
        {
            get { return _paramValue; }
            set { _paramValue = value; }
        }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParameterType
        {
            get { return _paramType; }
            set { _paramType = value; }
        }
    }
}
