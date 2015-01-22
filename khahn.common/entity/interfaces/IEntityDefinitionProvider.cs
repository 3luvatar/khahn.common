namespace khahn.common.entity.interfaces
{
    public interface IEntityDefinitionProvider
    {
        IEntityDefinition GetEntityDefinition(IEntityKey entityKey);
        bool TryGetEntityDefinition(IEntityKey entityKey, out IEntityDefinition entityDefinition);
    }

    public interface IEntityDefinitionProvider<in ENTITY_KEY> : IEntityDefinitionProvider
    {
        IEntityDefinition GetEntityDefinition(ENTITY_KEY entityKey);
        bool TryGetEntityDefinition(ENTITY_KEY entityKey, out IEntityDefinition entityDefinition);
    }
}