using System.Collections.Immutable;

namespace StarCube.Core.State
{
    public interface IStateDefiner
    {
    }

    /// <summary>
    /// 需要定义状态 (如 BlockState) 的类需要继承这个接口, 并实现对应接口, 示例见 Block 类
    /// </summary>
    /// <typeparam name="O"></typeparam>
    /// <typeparam name="S"></typeparam>
    public interface IStateDefiner<O, S> : IStateDefiner
        where O : class, IStateDefiner<O, S>
        where S : StateHolder<O, S>
    {
        /// <summary>
        /// 状态定义对象
        /// </summary>
        public StateDefinition<O, S> StateDefinition { get; set; }

        /// <summary>
        /// 默认状态
        /// </summary>
        public S DefaultState => StateDefinition.defaultState;

        /// <summary>
        /// 所有可用状态
        /// </summary>
        public ImmutableArray<S> States => StateDefinition.states;

        /// <summary>
        /// 是否只有一个状态
        /// </summary>
        public bool IsSingleState => StateDefinition.isSingleState;
    }
}
