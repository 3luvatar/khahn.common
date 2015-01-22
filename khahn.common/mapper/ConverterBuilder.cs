using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using khahn.common.entity.interfaces;
using khahn.common.entity.ReflectionProviders;

namespace khahn.common.mapper
{
    public class ConverterBuilder
    {
        private Dictionary<Type, Func<object, object>> converterDictionary =
            new Dictionary<Type, Func<object, object>>();

        public Func<object, object> BuilderConverterFor(IFieldDefinition fieldDefinition)
        {
            Func<object, object> converter;
            if (converterDictionary.TryGetValue(fieldDefinition.FieldType, out converter))
            {
                return converter;
            }
            converter = BuildConverter(fieldDefinition.FieldType);
            converterDictionary.Add(fieldDefinition.FieldType, converter);
            return converter;
        }

        public Func<object, object> BuildConverter(Type type)
        {
            return BuildConvertExpression(type).Compile();
        }

        public Expression<Func<object, object>> BuildConvertExpression(Type type)
        {
            ParameterExpression parameter = Expression.Parameter(typeof (object), "val");
            var returnLabel = Expression.Label(typeof (object), "returnLabel");
            Expression nullTest = checkNullExpression(parameter, returnLabel, type);
            Expression convertExpression = ConvertNonNull(parameter, returnLabel, type);
            Expression body = Expression.Block(nullTest,
                convertExpression,
                Expression.Label(returnLabel, Expression.Constant("")));
            return Expression.Lambda<Func<object, object>>(body, parameter);
        }

        private Expression checkNullExpression(Expression parameter, LabelTarget returnLabel, Type type)
        {
            ConstantExpression nullExpression = Expression.Constant(null);
            ConstantExpression dbNullExpression = Expression.Constant(DBNull.Value);
            var nullCheck = Expression.ReferenceEqual(parameter, nullExpression);
            var dbNullCheck = Expression.ReferenceEqual(parameter, dbNullExpression);
            var checkOr = Expression.OrElse(nullCheck, dbNullCheck);
            var emptyStringCheck = Expression.Equal(Expression.TypeAs(parameter, typeof (string)),
                Expression.Constant(string.Empty));
            checkOr = Expression.OrElse(checkOr, emptyStringCheck);
            var defaultValue = defaultValueExpression(type);
            return Expression.IfThen(checkOr, Expression.Return(returnLabel, defaultValue));
        }

        private Expression defaultValueExpression(Type type)
        {
            if (type.IsValueType)
            {
                return Expression.Convert(Expression.Constant(type.CreateInstance()), typeof (object));
            }
            else
            {
                return Expression.Constant(null);
            }
        }

        private Expression ConvertNonNull(ParameterExpression parameter, LabelTarget returnLabel, Type type)
        {
            Expression convertCall = null;
            var func = GetConvertFunction(type);
            if (func != null)
            {
                convertCall = Expression.Constant(func);
            }

            if (convertCall == null)
            {
                throw new NotSupportedException("type " + type.Name() + " is not supported");
            }
            return Expression.Return(returnLabel, Expression.Invoke(convertCall, parameter));
        }

        private Func<object, object> GetConvertFunction(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            Func<object, object> func = null;
            if (type == typeof (string))
            {
                func = o => o.ToString();
            }
            else if (type == typeof (int))
            {
                func = o => Convert.ToInt32(o);
            }
            else if (type == typeof (DateTime))
            {
                func = o => DateTime.Parse(o.ToString());
            }
            else if (type == typeof (double))
            {
                func = o => Convert.ToDouble(o);
            }
            else if (type == typeof (float))
            {
                func = o => Convert.ToSingle(o);
            }
            else if (type == typeof (long))
            {
                func = o => Convert.ToInt64(o);
            }
            else if (type == typeof (decimal))
            {
                func = o => Convert.ToDecimal(o);
            }
            else if (type == typeof (bool))
            {
                func = o => "Y".Equals(o) || "y".Equals(o) || 1.Equals(o);
            }
            return func;
        }
    }
}