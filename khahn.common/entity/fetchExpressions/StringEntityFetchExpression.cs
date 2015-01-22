using System;
using System.Collections.Generic;
using System.Linq;
using khahn.common.entity.interfaces;

namespace khahn.common.entity.fetchExpressions
{
    internal class StringEntityFetchExpression : EntityFetchExpression
    {
        private readonly ICollection<string> _fields;

        public StringEntityFetchExpression(ICollection<string> fields, IEntityKey entityKey) : base(entityKey)
        {
            _fields = fields;
        }

        protected override IEnumerable<IFieldDefinition> EvaluateFields(IEntityDefinition entityDefinition)
        {
            return _fields.Join(entityDefinition.FieldDefinitions,
                field => field,
                definition => definition.Name,
                (s, definition) => definition,
                StringComparer.CurrentCultureIgnoreCase);
        }
    }
}