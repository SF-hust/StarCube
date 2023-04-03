using System.Collections.Generic;
using System.Collections.Immutable;

using StarCube.Core.States.Property;

namespace StarCube.Core.States
{
    public sealed partial class StateDefinition<O, S>
        where O : class, IStateOwner<O, S>
        where S : State<O, S>
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
        public readonly bool singleState;

        /// <summary>
        /// 按顺序排列的属性列表
        /// </summary>
        public IEnumerable<StateProperty> PropertyDefinition => defaultState.propertyList.StateProperties;

        private StateDefinition(O owner, ImmutableArray<S> states, S defaultState)
        {
            this.owner = owner;
            this.states = states;
            this.defaultState = defaultState;
            singleState = (states.Length == 1);
        }
    }
}
