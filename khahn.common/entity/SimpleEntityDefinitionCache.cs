using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khahn.common.entity.interfaces;

namespace khahn.common.entity
{
    public class SimpleEntityDefinitionCache
    {
        private Dictionary<IEntityKey, IEntityDefinition> _entityDefinitions =
            new Dictionary<IEntityKey, IEntityDefinition>(new EntityKeyComparer());

        public bool TryGetEntityDefinition(IEntityKey key, out IEntityDefinition value)
        {
            return _entityDefinitions.TryGetValue(key, out value);
        }

        public IEntityDefinition GetEntityDefinition(IEntityKey key)
        {
            IEntityDefinition entityDefinition;
            TryGetEntityDefinition(key, out entityDefinition);
            return entityDefinition;
        }

        public void Add(IEntityKey entityKey, IEntityDefinition entityDefinition)
        {
            _entityDefinitions.Add(entityKey, entityDefinition);
        }
    }

    internal class EntityKeyComparer : IEqualityComparer<IEntityKey>
    {
        public bool Equals(IEntityKey x, IEntityKey y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null ||
                y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(IEntityKey obj)
        {
            return obj.GetHashCode();
        }
    }
}