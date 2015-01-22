using System;
using khahn.common.entity.interfaces;
using khahn.common.utils;

namespace khahn.common.entity.ReflectionProviders
{
    public class ReflectedEntityKey : IEntityKey
    {
        private readonly Type _entityType;

        public Type EntityType
        {
            get { return _entityType; }
        }

        public static ReflectedEntityKey EntityKey<T>() where T : IEntity
        {
            return
                TypeCache<T, ReflectedEntityKey>.Value ?? (TypeCache<T, ReflectedEntityKey>.Value =
                    new ReflectedEntityKey(typeof (T)));
        }

        private ReflectedEntityKey(Type entityType)
        {
            _entityType = entityType;
        }

        public bool Equals(IEntityKey key)
        {
            var reflectedEntityKey = key as ReflectedEntityKey;
            return reflectedEntityKey != null && reflectedEntityKey._entityType == _entityType;
        }
    }
}