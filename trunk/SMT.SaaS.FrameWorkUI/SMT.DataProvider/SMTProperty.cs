using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SMT
{
    /// <summary>
    /// 类属性赋值
    /// </summary>
    public class SMTProperty
    {
        public static object SetPropertyValue(PropertyInfo p, object value)
        {
            if (string.IsNullOrEmpty(value.ToString()))
            {
                return null;
            }
            object obj = "";
            switch (p.PropertyType.ToString())
            {
                #region
                case "System.String":
                    obj = value.ToString();
                    break;
                case "System.Int32":
                    obj = Convert.ToInt32(value);
                    break;
                case "System.Int64":
                    obj = Convert.ToInt64(value);
                    break;
                case "System.DateTime":
                    obj = Convert.ToDateTime(value);
                    break;
                case "System.Double":
                    obj = Convert.ToDouble(value);
                    break;
                case "System.Byte":
                    obj = Convert.ToByte(value);
                    break;
                case "System.Char":
                    obj = Convert.ToChar(value);
                    break;
                case "System.Boolean":
                    obj = Convert.ToBoolean(value);
                    break;
                case "System.Decimal":
                    obj = Convert.ToDecimal(value);
                    break;
                case "System.Single":
                    obj = Convert.ToSingle(value);
                    break;
                case "System.SByte":
                    obj = Convert.ToSByte(value);
                    break;
                default:
                    obj = value;
                    break;
                #endregion
            }
            return obj;

        }
    }
}
