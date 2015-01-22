using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using khahn.common.entity.interfaces;
using khahn.common.entity.ReflectionProviders;
using khahn.common.utils;

namespace khahn.common.entity.fetchExpressions
{
    internal class PropertyEntityFetchExpression : EntityFetchExpression<ReflectionEntityDefinition>
    {
        private readonly IEnumerable<PropertyInfo> _propertyInfos;

        public PropertyEntityFetchExpression(IEnumerable<LambdaExpression> expressions, ReflectedEntityKey entityKey)
            : base(entityKey)
        {
            _propertyInfos = expressions.Select(ReflectionUtils.GetPropertyInfo);
        }

        public PropertyEntityFetchExpression(IEnumerable<PropertyInfo> propertyInfos, ReflectedEntityKey entityKey)
            : base(entityKey)
        {
            _propertyInfos = propertyInfos;
        }

        protected override IEnumerable<IFieldDefinition> EvaluateFields(ReflectionEntityDefinition entityDefinition)
        {
            return _propertyInfos.Join(entityDefinition.FieldDefinitions,
                info => info,
                definition => definition.PropertyInfo,
                (expression, definition) => definition);
        }
    }
}