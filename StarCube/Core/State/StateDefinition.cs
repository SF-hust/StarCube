using System.Collections.Immutable;

namespace StarCube.Core.State
{
    public sealed partial class StateDefinition<O, S>
        where O : class, IStateDefiner<O, S>
        where S : StateHolder<O, S>
    {
        /// <summary>
        /// 拥有此 StateDefinition 的对象
        /// </summary>
        public readonly O owner;

        /// <summary>
        /// Owner 的所有可能状态
        /// </summary>
        public readonly ImmutableArray<S> states;

        /// <summary>
        /// 默认状态
        /// </summary>
        public readonly S defaultState;

        /// <summary>
        /// 是否只含有 1 个状态
        /// </summary>
        public readonly bool isSingle;

        private StateDefinition(O owner, ImmutableArray<S> states, S defaultState)
        {
            this.owner = owner;
            this.states = states;
            this.defaultState = defaultState;
            isSingle = (states.Length == 1);
        }
    }
}
