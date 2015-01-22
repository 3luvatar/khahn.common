using System.Collections.Generic;
using khahn.common.entity.interfaces;

namespace khahn.common.entity.fetchExpressions
{
    internal class AllFieldsFetchExpression : EntityFetchExpression
    {
        public AllFieldsFetchExpression(IEntityKey entityKey) : base(entityKey)
        {
        }

        protected override IEnumerable<IFieldDefinition> EvaluateFields(IEntityDefinition entityDefinition)
        {
            return entityDefinition.FieldDefinitions;
        }
    }
}