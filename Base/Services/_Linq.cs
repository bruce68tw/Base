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
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TEntity">Entity泛型型別</typeparam>
        /// <param name="source">資料</param>
        /// <param name="fieldName">排序欄位名稱</param>
        /// <returns></returns>
        public static IOrderedQueryable<TEntity> Sort<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            var resultExp = GenerateMethodCall<TEntity>(source, "OrderBy", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// 遞減排序
        /// </summary>
        /// <typeparam name="TEntity">Entity泛型型別</typeparam>
        /// <param name="source">資料</param>
        /// <param name="fieldName">排序欄位名稱</param>
        /// <returns></returns>
        public static IOrderedQueryable<TEntity> SortDescending<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            var resultExp = GenerateMethodCall<TEntity>(source, "OrderByDescending", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        //
        private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out System.Type resultType) where TEntity : class
        {
            // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
            var parameter = Expression.Parameter(typeof(TEntity), "Entity");
            //  create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propertyName.Contains('.'))
            {
                // support to be sorted on child fields.
                var childProperties = propertyName.Split('.');
                property = typeof(TEntity).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (var i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }
            resultType = property.PropertyType;
            // Create the order by expression.
            return Expression.Lambda(propertyAccess, parameter);
        }

        private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName) where TEntity : class
        {
            var type = typeof(TEntity);
            System.Type selectorResultType;
            var selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
            var resultExp = Expression.Call(typeof(Queryable), methodName,
                            new System.Type[] { type, selectorResultType },
                            source.Expression, Expression.Quote(selector));
            return resultExp;
        }


        public static Func<T, object> GetPropertyLambda<T>(string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            return p => property.GetValue(p, null);
        }

        /// <summary>
        /// object數值比對 用 == 可能會出問題。請用equal
        /// </summary>
        public static Func<T, object> GetPropertyLambda<T>(string propertyName, object value)
        {
            var property = typeof(T).GetProperty(propertyName);
            return p => (property.GetValue(p, null).Equals(value));
        }

        /// <summary>
        /// 第一個條件排序
        /// </summary>
        public static IOrderedEnumerable<T> OrderFirst<T>(this IEnumerable<T> source, string propertyName, bool isASC, object value)
        {
            //如果沒有特別指定值 單純依照值的順序排序
            if (value == null)
            {
                return (isASC)
                    ? source.OrderBy(GetPropertyLambda<T>(propertyName))
                    : source.OrderByDescending(GetPropertyLambda<T>(propertyName));
            }

            if (isASC)
                return source.OrderBy(GetPropertyLambda<T>(propertyName, value));
            else
                return source.OrderByDescending(GetPropertyLambda<T>(propertyName, value));
        }

        /// <summary>
        /// 第二個條件排序
        /// </summary>
        public static IOrderedEnumerable<T> OrderSecond<T>(this IOrderedEnumerable<T> source, string propertyName, bool isASC, object value)
        {
            //如果沒有特別指定值 單純依照值的順序排序
            if (value == null)
            {
                if (isASC)
                    return source.ThenBy(GetPropertyLambda<T>(propertyName));
                else
                    return source.ThenByDescending(GetPropertyLambda<T>(propertyName));
            }

            if (isASC)
                return source.ThenBy(GetPropertyLambda<T>(propertyName, value));
            else
                return source.ThenByDescending(GetPropertyLambda<T>(propertyName, value));
        }

        public static object GetValueByName<T>(this T value, string propertyName)
        {
            return value.GetType().GetProperty(propertyName).GetValue(value, null);
        }

    } //class
}
