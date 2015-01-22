using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using khahn.common.entity.interfaces;
using khahn.common.entity.ReflectionProviders;

namespace khahn.common.entity.fetchExpressions
{
    public abstract class EntityFetchExpression
    {
        #region static constructors

        public static EntityFetchExpression AllFieldsExpression<T>() where T : IEntity
        {
            return new AllFieldsFetchExpression(ReflectedEntityKey.EntityKey<T>());
        }

        public static EntityFetchExpression AllFieldsExpression(IEntityKey entityKey)
        {
            return new AllFieldsFetchExpression(entityKey);
        }

        public static EntityFetchExpression ForExpression<T>(params Expression<Func<T, bool>>[] expressions)
            where T : IEntity
        {
            return new PropertyEntityFetchExpression(expressions, ReflectedEntityKey.EntityKey<T>());
        }

        public static EntityFetchExpression ForFields<T>(params string[] fields) where T : IEntity
        {
            return new StringEntityFetchExpression(fields, ReflectedEntityKey.EntityKey<T>());
        }

        #endregion static constructors

        protected EntityFetchExpression(IEntityKey entityKey)
        {
            _entityKey = entityKey;
        }

        private List<IFieldDefinition> _fieldDefinitions;
        private readonly IEntityKey _entityKey;

        public IEntityKey EntityKey
        {
            get { return _entityKey; }
        }



        public IReadOnlyList<IFieldDefinition> Fields(IEntityDefinition entityDefinition)
        {
            if (_fieldDefinitions != null)
            {
                return _fieldDefinitions;
            }
            _fieldDefinitions = EvaluateFields(entityDefinition).ToList();
            return _fieldDefinitions;
        }

        public IReadOnlyList<IFieldDefinition> Fields(IEntityDefinitionProvider entityDefinitionProvider)
        {
            if (_fieldDefinitions != null)
            {
                return _fieldDefinitions;
            }
            _fieldDefinitions = EvaluateFields(entityDefinitionProvider).ToList();
            return _fieldDefinitions;
        }

        protected abstract IEnumerable<IFieldDefinition> EvaluateFields(IEntityDefinition entityDefinition);

        protected virtual IEnumerable<IFieldDefinition> EvaluateFields(
            IEntityDefinitionProvider entityDefinitionProvider)
        {
            return EvaluateFields(entityDefinitionProvider.GetEntityDefinition(EntityKey));
        }
    }

    public abstract class EntityFetchExpression<T> : EntityFetchExpression where T : IEntityDefinition
    {
        protected EntityFetchExpression(IEntityKey entityKey) : base(entityKey)
        {
        }

        protected override IEnumerable<IFieldDefinition> EvaluateFields(IEntityDefinition entityDefinition)
        {
            return EvaluateFields((T) entityDefinition);
        }

        protected abstract IEnumerable<IFieldDefinition> EvaluateFields(T entityDefinition);
    }
}