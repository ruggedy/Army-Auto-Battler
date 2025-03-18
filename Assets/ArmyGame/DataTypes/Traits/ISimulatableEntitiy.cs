namespace Logic.Units.Interfaces
{
    public interface ISimulatableEntitiy
    {

        public bool TryGetComponent<T>(out T component);

    }
}