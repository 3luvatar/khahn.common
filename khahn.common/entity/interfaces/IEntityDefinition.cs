using System.Collections.Generic;

namespace khahn.common.entity.interfaces
{
    public interface IEntityDefinition
    {
        IEntityKey EntityKey { get; }
        IEnumerable<IFieldDefinition> FieldDefinitions { get; }
        IEntity GenerateEntity();
        string Name { get; }
    }
}