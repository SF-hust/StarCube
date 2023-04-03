using System.Collections.Immutable;

namespace StarCube.Core.State
{
    /// <summary>
    /// 需要定义状态 (如 Block 之于 BlockState) 的类需要继承这个接口, 并实现对应接口, 示例见 Block 类
    /// </summary>
    /// <typeparam name="O"></typeparam>
    /// <typeparam name="S"></typeparam>
    public interface IStateOwner<O, S>
        where O : class, IStateOwner<O, S>
        where S : StateHolder<O, S>
    {
        /// <summary>
        /// 状态定义对象
        /// </summary>
        public StateDefinition<O, S> StateDefinition { get; }
    }

    public static class StateOwnerExtension
    {
        public static ImmutableArray<S> States<O, S>(this O owner)
            where O : class, IStateOwner<O, S>
            where S : StateHolder<O, S>
        {
            return owner.StateDefinition.states;
        }

        public static bool SingleState<O, S>(this O owner)
            where O : class, IStateOwner<O, S>
            where S : StateHolder<O, S>
        {
            return owner.StateDefinition.singleState;
        }

        public static S DefaultState<O, S>(this O owner)
            where O : class, IStateOwner<O, S>
            where S : StateHolder<O, S>
        {
            return owner.StateDefinition.defaultState;
        }
    }
}
