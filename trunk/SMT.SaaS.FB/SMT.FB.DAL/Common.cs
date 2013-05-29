using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

using SMT_FB_EFModel;
using System.Reflection;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices;

namespace SMT.FB.DAL
{
    /// <summary>
    /// 用于把QueryExpression 轮化为Expresion的辅助类
    /// </summary>
    public static class QueryExpressionHelper
    {

        public static Expression CovertToExpression(Type type, QueryExpression qExpression)
        {
            if (string.IsNullOrEmpty(qExpression.PropertyName))
            {
                return null;
            }
            ParameterExpression param = Expression.Parameter(type, "c");
            Expression condition = null;
            // Merege Expression
            List<QueryExpression> list = new List<QueryExpression>();
            list.Add(qExpression);
            QueryExpression rE = qExpression.RelatedExpression;

            while (rE != null)
            {
                list.Add(rE);
                rE = rE.RelatedExpression;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                QueryExpression currentExpression = list[i];
                if (string.IsNullOrEmpty(currentExpression.PropertyName))
                {
                    continue;
                }


                Expression left = GetLeft(param, currentExpression.PropertyName);
                Type propertyType = left.Type;

                Expression right = null;
                if (list[i].RightPropertyValue != null)
                {
                    right = Expression.Constant(list[i].RightPropertyValue);
                }
                else
                {
                    object cValue = TryConvertValue(propertyType, currentExpression.PropertyValue);
                    right = Expression.Constant(cValue, propertyType);
                }

                Expression filter = QueryExpressionHelper.MergeExpression(currentExpression.Operation, left, right);

                if (condition == null)
                {
                    condition = filter;
                }
                else if (currentExpression.RelatedType == QueryExpression.RelationType.And)
                {
                    condition = Expression.And(condition, filter);
                }
                else
                {
                    condition = Expression.Or(condition, filter);
                }

            }

            if (condition == QueryExpressionHelper.EmptyExpression)
            {
                return null;
            }

            return Expression.Lambda(condition, param);

        }

        public static Expression GetLeft(Expression param, string propertyName)
        {
            Expression left = param;
            string[] ps = propertyName.Split('.');
            for (int i = 0; i < ps.Length; i++)
            {
                left = Expression.Property(left, ps[i]);
            }
            return left;
        }
        public static Expression MergeExpression(QueryExpression.Operations op, Expression left, Expression right)
        {
            try
            {
                switch (op)
                {

                    case QueryExpression.Operations.Equal:
                        return Expression.Equal(left, right);

                    case QueryExpression.Operations.GreaterThan:
                        return Expression.GreaterThan(left, right);

                    case QueryExpression.Operations.GreaterThanOrEqual:
                        return Expression.GreaterThanOrEqual(left, right);

                    case QueryExpression.Operations.LessThan:
                        return Expression.LessThan(left, right);

                    case QueryExpression.Operations.LessThanOrEqual:
                        return Expression.LessThanOrEqual(left, right);
                    case QueryExpression.Operations.NotEqual:
                        return Expression.NotEqual(left, right);
                    case QueryExpression.Operations.IsChildFrom:
                        MethodInfo mInfo = right.Type.GetMethod("Contains");
                        return Expression.Call(right, mInfo, left);
                    case QueryExpression.Operations.Like:
                        MethodInfo mInfoL = right.Type.GetMethod("Contains");
                        return Expression.Call(right, mInfoL, left);
                    default:
                        throw new ArgumentException("parameter: op is not correct");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Expression _EmptyExpression = null;

        public static Expression EmptyExpression
        {
            get
            {
                if (_EmptyExpression == null)
                {
                    Expression left = Expression.Constant(1);
                    Expression right = Expression.Constant(1);
                    _EmptyExpression = Expression.Equal(left, right);
                }
                return _EmptyExpression;
            }

        }


        public static object TryConvertValue(Type t, object source)
        {
            try
            {
                object tryValue = default(object);
                if (source == null)
                {
                    return tryValue;
                }
                else if (source.GetType() == t)
                {
                    return source;
                }

                if (t.IsValueType)
                {
                    if (t.BaseType == typeof(System.Enum))
                    {
                        return System.Enum.Parse(t, source.ToString(), true);
                    }

                    else if (t.Name.IndexOf("Nullable`") == 0)
                    {
                        t = t.GetProperty("Value").PropertyType;

                    }
                    else if (t == typeof(bool))
                    {
                        return Convert.ToBoolean(source);
                    }
                    else if (source.GetType() == typeof(bool))
                    {
                        source = Convert.ToInt32(source);
                    }

                    if (t == typeof(decimal))
                    {
                        return decimal.Parse(Convert.ToString(source));
                    }

                    source = Convert.ToString(source);
                    MethodInfo methodInfo = t.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => { return m.Name == "Parse"; });
                    tryValue = methodInfo.Invoke(null, new object[] { Convert.ToString(source) });

                }
                else
                {
                    tryValue = source;
                }
                return tryValue;
            }
            catch (Exception ex)
            {
                throw new Exception("TryConvertValue error, P1: " + t.Name + ", P2:" + Convert.ToString(source), ex);
            }
        }

    }

    /// <summary>
    /// 权限过滤的扩展方法
    /// </summary>
    public static class ObjectQueryExtension
    {
        public static ObjectQuery<TEntity> SetOrganizationFilter<TEntity>(this ObjectQuery<TEntity> tables, QueryExpression qe)
        {
            IQueryable<TEntity> innerResult = tables;
            if (string.IsNullOrEmpty(qe.QueryType) || string.IsNullOrEmpty(qe.VisitUserID) || qe.IsUnCheckRight)
            {
                return tables;
            }
            Utility utility = new Utility();
            string filterString = "";
            List<Object> listParams = new List<object>();

            utility.SetOrganizationFilter(ref filterString, ref listParams, qe.VisitUserID, qe.RightType);


            if (!string.IsNullOrEmpty(filterString))
            {
                innerResult = tables.Where(filterString, listParams.ToArray());
            }
            return innerResult as ObjectQuery<TEntity>;
        }
    }
}
