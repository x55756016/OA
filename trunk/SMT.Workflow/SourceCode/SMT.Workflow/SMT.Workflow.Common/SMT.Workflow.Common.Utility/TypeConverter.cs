/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： 数据格式转换帮助类
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.Common.Utility
{
    public class TypeConverter
    {
        #region ToInt

        /// <summary>
        /// 将指定值解析成Int类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>Int值</returns>
        public static int ToInt(object o)
        {
            return ToInt(o, 0);
        }

        /// <summary>
        /// 将指定值解析成Int类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">转换失败后的默认值</param>
        /// <returns>Int值</returns>
        public static int ToInt(object o, int defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt32(o);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToBoolean

        /// <summary>
        /// 将指定值解析成Bool类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>Bool值</returns>
        public static bool ToBoolean(object o)
        {
            return ToBoolean(o, false);
        }

        /// <summary>
        /// 将指定值解析成Bool类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">转换失败后的默认值</param>
        /// <returns>Bool值</returns>
        public static bool ToBoolean(object o, bool defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToBoolean(o);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToDecimal

        /// <summary>
        /// 将指定值解析成decimal类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>decimal值</returns>
        public static decimal ToDecimal(object o)
        {
            return ToDecimal(o, 0.0m);
        }

        /// <summary>
        /// 将指定值解析成decimal类型，解析失败时返回传入的默认值
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">解析失败的值</param>
        /// <returns>decimal值</returns>
        public static decimal ToDecimal(object o, decimal defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDecimal(o);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToDouble

        /// <summary>
        /// 将指定值解析成double类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>double值</returns>
        public static double ToDouble(object o)
        {
            return ToDouble(o, 0.0d);
        }

        /// <summary>
        /// 将指定值解析成double类型，解析失败时返回传入的默认值
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">解析失败的值</param>
        /// <returns>double值</returns>
        public static double ToDouble(object o, double defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDouble(o);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToDateTime

        /// <summary>
        /// 将指定值解析成DateTime类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>DateTime值</returns>
        public static DateTime ToDateTime(object o)
        {
            return ToDateTime(o, DateTime.Now);
        }

        /// <summary>
        /// 将指定值解析成DateTime类型，解析失败时返回传入的默认值
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">解析失败的值</param>
        /// <returns>DateTime值</returns>
        public static DateTime ToDateTime(object o, DateTime defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDateTime(o);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// 将指定值解析成String类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>String值</returns>
        public static string ToString(object o)
        {
            return ToString(o, string.Empty);
        }

        /// <summary>
        /// 将指定值解析成String类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">转换失败后的默认值</param>
        /// <returns>String值</returns>
        public static string ToString(object o, string defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToString(o);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToGuid

        /// <summary>
        /// 将指定值解析成Guid类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns>Guid值</returns>
        public static Guid ToGuid(object o)
        {
            return ToGuid(o, Guid.NewGuid());
        }

        /// <summary>
        /// 将指定值解析成Guid类型
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="defaultValue">转换失败后的默认值</param>
        /// <returns>Guid值</returns>
        public static Guid ToGuid(object o, Guid defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return new Guid(o.ToString());
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region ToMoneyString

        /// <summary>
        /// 将指定值显示为金额方式
        /// </summary>
        /// <param name="o">指定值</param>
        /// <returns></returns>
        public static string ToMoneyString(object o)
        {
            return ToMoneyString(o, "0.00");
        }

        /// <summary>
        /// 将指定值显示为金额方式
        /// </summary>
        /// <param name="o">指定值</param>
        /// <param name="format">格式</param>
        /// <returns></returns>
        public static string ToMoneyString(object o, string format)
        {
            if (o == null || o == DBNull.Value)
            {
                return string.Empty;
            }

            if (o is decimal)
            {
                decimal money = (decimal)o;
                return money.ToString(format);
            }
            else if (o is float)
            {
                float money = (float)o;
                return money.ToString(format);
            }
            else if (o is double)
            {
                double money = (double)o;
                return money.ToString(format);
            }
            else if (o is int)
            {
                int money = (int)o;
                return money.ToString(format);
            }
            else if (o is long)
            {
                long money = (long)o;
                return money.ToString(format);
            }
            else
            {
                return o.ToString();
            }
        }

        #endregion
    }
}
