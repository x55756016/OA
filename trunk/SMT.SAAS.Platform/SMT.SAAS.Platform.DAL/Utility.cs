using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using System.Reflection;
using SMT_Platform_EFModel;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为子系统(应用)信息的数据访问提供接口规范
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform
{
    public static class Utility
    {
        /// <summary>
        /// 异常日志公共方法
        /// </summary>
        public static void Log(string typeName, string methodName, Exception exception)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("--------------------------------异常--------------------------------");
            message.Append("来自文件  :");
            message.Append(typeName);
            message.AppendLine();
            message.Append("来自方法  :");
            message.Append(methodName);
            message.AppendLine();
            message.Append("异常信息  :");
            message.AppendLine();
            message.Append("\t");
            message.Append(exception.Message);

            if (exception.InnerException != null)
            {
                message.AppendLine();
                message.Append("内部信息  :");
                message.AppendLine();
                message.Append("\t");
                message.Append(exception.InnerException.Message);
            }
            message.AppendLine();
            message.Append("产生时间  :");

            Tracer.Debug(message.ToString());
        }

        /// <summary>
        /// 根据跟定的两个已知对象TSource，TTarget
        /// 将TSource对象中的数据克隆到TTarget中。
        /// TSource与TTarget可以为不同类型，其仅会拷贝具有相同名称和数据类型的属性值。
        /// </summary>
        /// <typeparam name="TSource">数据源类型</typeparam>
        /// <typeparam name="TTarget">数据目标类型</typeparam>
        /// <param name="Source">数据源</param>
        /// <param name="Target">数据目标</param>
        /// <returns>已包含赋值数据的结果</returns>
        public static TTarget CloneObject<TSource, TTarget>(TSource Source, TTarget Target)
        {
            try
            {
                if (Source == null)
                    throw new ArgumentNullException("Source");
                if (Target == null)
                    throw new ArgumentNullException("Target");

                PropertyInfo[] sourcePropertys = Source.GetType().GetProperties();
                PropertyInfo[] targetPropertys = Target.GetType().GetProperties();
                foreach (PropertyInfo sourcePro in sourcePropertys)
                {
                    var targetPro = targetPropertys.Where<PropertyInfo>(delegate(PropertyInfo proInfo)
                    {
                        return proInfo.Name.ToLower() == sourcePro.Name.ToLower() &&
                               proInfo.PropertyType == sourcePro.PropertyType;
                    }).FirstOrDefault<PropertyInfo>();

                    if (targetPro != null)
                        targetPro.SetValue(Target, sourcePro.GetValue(Source, null), null);
                }
                return Target;
            }
            catch (Exception ex)
            {
                throw new Exception("对象克隆异常", ex);
            }
        }

        /// <summary>
        /// 根据跟定的两个已知对象TSource，TTarget
        /// 将TSource对象中的数据克隆到TTarget中。
        /// TSource与TTarget可以为不同类型，其仅会拷贝具有相同名称和数据类型的属性值。
        /// </summary>
        /// <typeparam name="TSource">数据源类型</typeparam>
        /// <typeparam name="TTarget">数据目标类型</typeparam>
        /// <param name="Source">数据源</param>
        /// <param name="Target">数据目标</param>
        /// <returns>已包含赋值数据的结果</returns>
        public static TTarget CloneObject<TTarget>(this object Source, TTarget Target)
        {
            try
            {
                if (Source == null)
                    throw new ArgumentNullException("Source");
                if (Target == null)
                    throw new ArgumentNullException("Target");

                PropertyInfo[] sourcePropertys = Source.GetType().GetProperties();
                PropertyInfo[] targetPropertys = Target.GetType().GetProperties();
                foreach (PropertyInfo sourcePro in sourcePropertys)
                {
                    var targetPro = targetPropertys.Where<PropertyInfo>(delegate(PropertyInfo proInfo)
                    {
                        return proInfo.Name.ToLower() == sourcePro.Name.ToLower() &&
                            proInfo.PropertyType == sourcePro.PropertyType;
                    }).FirstOrDefault<PropertyInfo>();
                    if (targetPro != null)
                        targetPro.SetValue(Target, sourcePro.GetValue(Source, null), null);
                }
                return Target;
            }
            catch (Exception ex)
            {
                throw new Exception("对象克隆异常", ex);
            }
        }

        public static CommonProperty CreateCommonProperty()
        {
            return new CommonProperty()
            {
                CREATECOMPANYID = "CREATECOMPANYID",
                CREATEDATE = DateTime.Now,
                CREATEDEPARTMENTID = "CREATEDEPARTMENTID",
                CREATEPOSTID = "CREATEPOSTID",
                CREATEUSERID = "CREATEUSERID",
                CREATEUSERNAME = "CREATEUSERNAME",
                OwnerCompanyID = "OwnerCompanyID",
                OwnerDepartmentID = "OwnerDepartmentID",
                OwnerID = "OwnerID",
                OwnerName = "OwnerName",
                OwnerPostID = "OwnerPostID",
                UPDATEDATE = DateTime.Now,
                UPDATEUSERID = "UPDATEUSERID",
                UpdateUserName = "UpdateUserName"
            };
        }
    }
}
