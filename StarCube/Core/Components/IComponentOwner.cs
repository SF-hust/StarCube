namespace StarCube.Core.Components
{
    public interface IComponentOwner<T>
        where T : class, IComponentOwner<T>
    {
        public ComponentContainer<T> Components { get; }
    }
}
