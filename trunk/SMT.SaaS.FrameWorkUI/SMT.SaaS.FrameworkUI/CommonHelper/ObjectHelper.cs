using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace System
{
    public static class ObjectHelper
    {
        public static PropertyInfo GetPropertyInfo(string propertyName, ref object currentObject)
        {

            if (propertyName.StartsWith("{") && propertyName.EndsWith("}"))
            {
                propertyName = propertyName.Trim('{', '}');
            }

            string[] propertyParts = propertyName.Split('.');


            for (int i = 1; i < propertyParts.Length; i++)
            {
                string subObjectName = propertyParts[i - 1];
                currentObject = currentObject.InnerGetObjValue(subObjectName);
                if (currentObject == null)
                {
                    return null;
                }
            }

            PropertyInfo p = currentObject.GetType().GetProperty(propertyParts[propertyParts.Length - 1]);

            return p;
        }
        public static void SetObjValue(this object entityObject, string propertyName, object value)
        {
            object currentObject = entityObject;
            PropertyInfo p = GetPropertyInfo(propertyName, ref currentObject);
            object newValue = default(object);

            if (p != null)
            {

                if (p.PropertyType.IsValueType)
                {

                    newValue = value.Convert(p.PropertyType);

                }
                else if (p.PropertyType == typeof(string))
                {
                    newValue = Convert.ToString(value);
                }
                else
                {
                    newValue = value;
                }

                p.SetValue(currentObject, newValue, null);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine(propertyName + " 不存在");
            }
        }

        public static object GetObjValue(this object entityObject, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || entityObject == null)
            {
                return null;
            }
            object currentObject = entityObject;
            PropertyInfo p = GetPropertyInfo(propertyName, ref currentObject);

            if (p != null)
            {
                return p.GetValue(currentObject, null);
            }
            return null;
        }

        private static object InnerGetObjValue(this object currentObject, string propertyName)
        {

            int indexLeftS = propertyName.IndexOf('[');
            object[] indexP = null;
            object result = null;

            if (indexLeftS > 0)
            {
                string rightStr = propertyName.Substring(indexLeftS).Trim('[', ']');
                propertyName = propertyName.Substring(0, indexLeftS);
                indexP = new object[] { int.Parse(rightStr) };
                currentObject = currentObject.InnerGetObjValue(propertyName);
                IList list = currentObject as IList;
                if (list != null)
                {
                    return list[int.Parse(rightStr)];
                }
                
                propertyName = ((DefaultMemberAttribute)currentObject.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;
            }

            Type tempType = currentObject.GetType();
            PropertyInfo tempProperty = tempType.GetProperty(propertyName);
            if (tempProperty == null)
            {
                throw new Exception("error : 找不到相应的属性,path=" + propertyName + ", 属性 " + propertyName + " 不存在");
            }
            try
            {
                result = tempProperty.GetValue(currentObject, indexP);
            }
            catch
            {
                return null;
            }
            return result;
        }

        public static void CopyTo<TEntity>(this TEntity sourceFBEntity, TEntity targetFBEntity, string propertyName)
        {
            object value = sourceFBEntity.GetObjValue(propertyName);
            targetFBEntity.SetObjValue(propertyName, value);
        }

        public static void CopyTo<TEntity>(TEntity source, TEntity target)
        {
            List<PropertyInfo> listP = typeof(TEntity).GetProperties().ToList();
            listP.ForEach(property =>
            {
                bool isCopyProperty = property.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), true).Count() > 0;
                if (isCopyProperty)
                {
                    source.CopyTo<TEntity>(target, property.Name);
                }
            });
        }
    }
}
