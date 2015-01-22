using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using khahn.common.entity.interfaces;

namespace khahn.common.entity.ReflectionProviders
{
    public class ReflectionEntityDefinitionProvider : EntityDefinitionProvider<ReflectedEntityKey>
    {
        protected override IEntityDefinition NewEntityDefinition(ReflectedEntityKey entityKey)
        {
            var entityDefinition = new ReflectionEntityDefinition(entityKey);
            foreach (var propertyInfo in entityKey.EntityType.Properties())
            {
                if (propertyInfo.HasAttribute<IgnoreFieldAttribute>())
                {
                    continue;
                }
                ReflectionFieldDefinition fieldDefinition = GetFieldDefinition(propertyInfo, entityDefinition);
                entityDefinition.FieldDefinitions.Add(fieldDefinition);
            }

            return entityDefinition;
        }

        internal ReflectionFieldDefinition GetFieldDefinition(PropertyInfo propertyInfo,
            IEntityDefinition entityDefinition)
        {
            return new ReflectionFieldDefinition(propertyInfo, entityDefinition);
        }
    }
}