namespace StarCube.Core.Components
{
    public interface IComponentHolder<T>
        where T : class, IComponentHolder<T>
    {
        public ComponentContainer<T> Components { get; }
    }
}
