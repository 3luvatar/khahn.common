using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;

namespace khahn.common.entity
{
    public abstract class EntityDefinitionProvider<ENTITY_KEY> : IEntityDefinitionProvider<ENTITY_KEY>
        where ENTITY_KEY : IEntityKey
    {
        private SimpleEntityDefinitionCache _entityDefinitionCache = new SimpleEntityDefinitionCache();

        public IEntityDefinition GetEntityDefinition(ENTITY_KEY entityKey)
        {
            IEntityDefinition entityDefinition;
            TryGetEntityDefinition(entityKey, out entityDefinition);
            return entityDefinition;
        }

        public bool TryGetEntityDefinition(ENTITY_KEY entityKey, out IEntityDefinition entityDefinition)
        {
            if (CheckEntityCache(entityKey, out entityDefinition))
            {
                return true;
            }
            PreProcessEntityKey(entityKey);
            entityDefinition = NewEntityDefinition(entityKey);
            PostProcessEntityDefinition(entityDefinition);
            AddToCache(entityKey, entityDefinition);
            return entityDefinition != null;
        }

        protected virtual void PreProcessEntityKey(ENTITY_KEY entityKey)
        {
        }

        protected virtual void PostProcessEntityDefinition(IEntityDefinition entityDefinition)
        {
        }

        protected abstract IEntityDefinition NewEntityDefinition(ENTITY_KEY entityKey);

        protected virtual bool CheckEntityCache(ENTITY_KEY entityKey, out IEntityDefinition entityDefinition)
        {
            return _entityDefinitionCache.TryGetEntityDefinition(entityKey, out entityDefinition);
        }

        protected virtual void AddToCache(ENTITY_KEY entityKey, IEntityDefinition entityDefinition)
        {
            _entityDefinitionCache.Add(entityKey, entityDefinition);
        }

        IEntityDefinition IEntityDefinitionProvider.GetEntityDefinition(IEntityKey entityKey)
        {
            return GetEntityDefinition((ENTITY_KEY) entityKey);
        }

        bool IEntityDefinitionProvider.TryGetEntityDefinition(IEntityKey entityKey,
            out IEntityDefinition entityDefinition)
        {
            if (entityKey is ENTITY_KEY)
                return TryGetEntityDefinition((ENTITY_KEY) entityKey, out entityDefinition);
            entityDefinition = null;
            return false;
        }
    }
}