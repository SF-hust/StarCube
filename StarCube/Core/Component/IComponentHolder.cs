namespace StarCube.Core.Component
{
    public interface IComponentHolder<T>
        where T : class, IComponentHolder<T>
    {
        public ComponentContainer<T> Components { get; }
    }
}
