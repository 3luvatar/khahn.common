using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using khahn.common.entity.interfaces;

namespace khahn.common.utils
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// for example
        /// <![CDATA[GetGenericTypeInstance(typeof(List<string>), typeof(IEnumerable<>)) will return type IEnumerable<string>]]> 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="genericType">must be a generic type definition</param>
        /// <returns></returns>
        public static Type GetGenericTypeInstance(Type t, Type genericType)
        {
            if (!genericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("type " + genericType.Name() + " is not a generic type definition");
            }
            if (genericType.IsInterface)
            {
                return resolveInterfaceType(t, genericType);
            }
            if (t.GetGenericTypeDefinition() == genericType)
            {
                return t;
            }
            if (!t.Inherits(genericType))
            {
                throw new ArgumentException("type " + t.Name() + " does not inherit from " + genericType.Name());
            }
            t = t.BaseType;
            while (t != typeof (object))
            {
                if (t.GetGenericTypeDefinition() == genericType)
                {
                    return t;
                }
                t = t.BaseType;
            }
            throw new ArgumentException("type " + t.Name() + " does not inherit from " + genericType.Name());
        }

        private static Type resolveInterfaceType(Type t, Type genericInterface)
        {
            return t.GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == genericInterface)
                .SingleOrDefault();
        }

        public static PropertyInfo GetPropertyInfo(LambdaExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    MemberExpression memberExpression = (MemberExpression) expression.Body;
                    return (PropertyInfo) memberExpression.Member;
            }
            throw new NotSupportedException("node type of " + expression.NodeType + " not supported");
        }

        public static bool IsTypeField(Type type)
        {
            return type.Implements<IField>();
        }

        public static bool IsNumericType(this Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}