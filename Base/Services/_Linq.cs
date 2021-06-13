using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Base.Services
{
    /// <summary>
    /// ?? ExtensionOfLinq
    /// </summary>
    public static class _Linq
    {
        //
        #region return IOrderedEnumerable
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="T">Entity泛型型別</typeparam>
        /// <param name="source">資料</param>
        /// <param name="fieldName">排序欄位名稱</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> Sort<T>(this IQueryable<T> source, string fieldName) where T : class
        {
            var resultExp = GenerateMethodCall<T>(source, "OrderBy", fieldName);
            return source.Provider.CreateQuery<T>(resultExp) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// 遞減排序
        /// </summary>
        /// <typeparam name="T">Entity泛型型別</typeparam>
        /// <param name="source">資料</param>
        /// <param name="fieldName">排序欄位名稱</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> SortDescending<T>(this IQueryable<T> source, string fieldName) where T : class
        {
            var resultExp = GenerateMethodCall<T>(source, "OrderByDescending", fieldName);
            return source.Provider.CreateQuery<T>(resultExp) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// 第一個條件排序
        /// </summary>
        public static IOrderedEnumerable<T> OrderFirst<T>(this IEnumerable<T> source, string propName, bool isAsc, object value)
        {
            //如果沒有特別指定值 單純依照值的順序排序
            if (value == null)
            {
                return (isAsc)
                    ? source.OrderBy(FnGetValue<T>(propName))
                    : source.OrderByDescending(FnGetValue<T>(propName));
            }

            if (isAsc)
                return source.OrderBy(GetPropertyLambda<T>(propName, value));
            else
                return source.OrderByDescending(GetPropertyLambda<T>(propName, value));
        }

        /// <summary>
        /// 第二個條件排序
        /// </summary>
        public static IOrderedEnumerable<T> OrderSecond<T>(this IOrderedEnumerable<T> source, string propName, bool isAsc, object value)
        {
            //如果沒有特別指定值 單純依照值的順序排序
            if (value == null)
            {
                if (isAsc)
                    return source.ThenBy(FnGetValue<T>(propName));
                else
                    return source.ThenByDescending(FnGetValue<T>(propName));
            }

            if (isAsc)
                return source.ThenBy(GetPropertyLambda<T>(propName, value));
            else
                return source.ThenByDescending(GetPropertyLambda<T>(propName, value));
        }
        #endregion
        //

        //
        private static LambdaExpression GenerateSelector<T>(String propName, out System.Type resultType) where T : class
        {
            // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
            var parameter = Expression.Parameter(typeof(T), "Entity");
            //  create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propName.Contains('.'))
            {
                // support to be sorted on child fields.
                var childProperties = propName.Split('.');
                property = typeof(T).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (var i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(T).GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }
            resultType = property.PropertyType;
            // Create the order by expression.
            return Expression.Lambda(propertyAccess, parameter);
        }

        private static MethodCallExpression GenerateMethodCall<T>(IQueryable<T> source, string methodName, String fieldName) where T : class
        {
            var type = typeof(T);
            System.Type selectorResultType;
            var selector = GenerateSelector<T>(fieldName, out selectorResultType);
            var resultExp = Expression.Call(typeof(Queryable), methodName,
                            new System.Type[] { type, selectorResultType },
                            source.Expression, Expression.Quote(selector));
            return resultExp;
        }

        #region get Func
        public static Func<T, object> FnGetValue<T>(string propName)
        {
            var prop = typeof(T).GetProperty(propName);
            return p => prop.GetValue(p, null);
        }

        /// <summary>
        /// object數值比對 用 == 可能會出問題。請用equal
        /// </summary>
        public static Func<T, object> GetPropertyLambda<T>(string propName, object value)
        {
            var property = typeof(T).GetProperty(propName);
            return p => (property.GetValue(p, null).Equals(value));
        }
        #endregion

        public static object GetValueByName<T>(this T value, string propName)
        {
            return value.GetType().GetProperty(propName).GetValue(value, null);
        }

    } //class
}
