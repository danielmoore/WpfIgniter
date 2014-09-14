using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Igniter
{
    internal static class ExpressionUtil
    {
        internal static string GetPropertyName<TSource, TProp>(Expression<Func<TSource, TProp>> propertySelector)
        {
            var memberExpr = propertySelector.Body as MemberExpression;

            if (memberExpr == null) throw new ArgumentException("must be a member accessor", "propertySelector");

            var propertyInfo = memberExpr.Member as PropertyInfo;

            if (propertyInfo == null || propertyInfo.DeclaringType != null && propertyInfo.DeclaringType.IsAssignableFrom(typeof(TSource)))
                throw new ArgumentException("must yield a single property on the given object", "propertySelector");

            return propertyInfo.Name;
        } 
    }
}