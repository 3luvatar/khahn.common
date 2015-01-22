using System;

namespace khahn.common.entity.interfaces
{
    public interface IFieldDefinition
    {
        IEntityDefinition EntityDefinition { get; }
        string Name { get; }

        string SqlName { get; }
        IField GenerateField();
        IField GetField(IEntity entity);
        Type FieldType { get; }
    }
}