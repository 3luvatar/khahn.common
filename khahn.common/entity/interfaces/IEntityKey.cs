namespace khahn.common.entity
{
    public interface IEntityKey
    {
        bool Equals(IEntityKey key);
    }

    public abstract class EntityKey : IEntityKey
    {
        public abstract bool Equals(IEntityKey key);
    }
}