using System;
using System.Linq.Expressions;

namespace DirectoryIndexer.Tests
{
    public static class Properties
    {
        public static string GetName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var unaryExpression = propertyExpression.Body as UnaryExpression;
            var memberExpression = unaryExpression != null ? (MemberExpression)unaryExpression.Operand : (MemberExpression)propertyExpression.Body;
            return memberExpression.Member.Name;
        }
    }
}
