using System;
using System.Linq.Expressions;

namespace DirectoryIndexer.Tests
{
    class PropertyDescription
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public PropertyDescription(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public static PropertyDescription FromPropExpression<T>(Expression<Func<T, object>> propertyExpression)
        {
            var unaryExpression = propertyExpression.Body as UnaryExpression;
            var memberExpression = unaryExpression != null ? (MemberExpression)unaryExpression.Operand : (MemberExpression)propertyExpression.Body;
            return new PropertyDescription(memberExpression.Member.Name, memberExpression.Type);
        }

        public override bool Equals(object obj)
        {
            var other = obj as PropertyDescription;
            return other != null && other.Name == Name && other.Type == Type;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }
    }

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
