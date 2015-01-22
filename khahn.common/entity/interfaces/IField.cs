namespace khahn.common.entity.interfaces
{
    public interface IField
    {
        IFieldDefinition FieldDefinition { get; }
        object Value { get; set; }
        bool Provided { get; }
    }
}