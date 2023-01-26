using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace StarCube.Core.State
{
    /// <summary>
    /// 一个 StateHolder 对象是 Owner 的一个状态实例, 保存着 Owner 所定义的所有状态属性以及在当前状态下各属性的取值, 示例参照 BlockState
    /// </summary>
    /// <typeparam name="O">Owner 类型</typeparam>
    /// <typeparam name="S">Holder 类型</typeparam>
    public abstract class StateHolder<O, S>
        where O : class, IStateDefiner<O, S>
        where S : StateHolder<O, S>
    {
        /// <summary>
        /// 一个可以根据 owner 和 propertyList 创建相应 State 的工厂委托
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public delegate S Factory(O owner, StatePropertyList properties);

        public StateHolder(O owner, StatePropertyList propertyList)
        {
            this.owner = owner;
            this.propertyList = propertyList;
            constructed = (propertyList == null);
            hashcodeCache = 31 * owner.GetHashCode() + (propertyList == null ? 0 : propertyList.GetHashCode());
        }

        /// <summary>
        /// State 的属性与取值列表
        /// </summary>
        public readonly StatePropertyList propertyList;

        /// <summary>
        /// 此 State 是否是其 Owner 的唯一 State
        /// </summary>
        public bool IsSingle => propertyList == null;

        /// <summary>
        /// State 的 Owner
        /// </summary>
        public readonly O owner;

        public ImmutableDictionary<StateProperty, ImmutableArray<S>> Neighbours => neighbours!;

        public ImmutableDictionary<StateProperty, S> Followers => followers!;

        private ImmutableDictionary<StateProperty, ImmutableArray<S>>? neighbours = null;
        private ImmutableDictionary<StateProperty, S>? followers = null;

        private readonly int hashcodeCache;

        /// <summary>
        /// 指示 State 是否已经构造完成
        /// </summary>
        private bool constructed;

        /// <summary>
        /// 为 StateHolder 设置 neighbours 和 followers
        /// </summary>
        /// <param name="neighbours"></param>
        /// <param name="followers"></param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void SetNeighboursAndFollowers(ImmutableDictionary<StateProperty, ImmutableArray<S>> neighbours, ImmutableDictionary<StateProperty, S> followers)
        {
            if (constructed)
            {
                throw new InvalidOperationException("can't set neighbours and followers for a StateHolder twice");
            }
            Debug.Assert(neighbours != null && followers != null);
            constructed = true;
            this.neighbours = neighbours;
            this.followers = followers;
        }

        /// <summary>
        /// 获取此 State 在指定 property 上取到指定下标的值的 neighbour
        /// </summary>
        /// <param name="property"></param>
        /// <param name="valueIndex"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool SetProperty(StateProperty property, int valueIndex, [NotNullWhen(true)] out S? state)
        {
            if (neighbours != null &&
                property.IndexIsValid(valueIndex) &&
                neighbours.TryGetValue(property, out var states))
            {
                state = states[valueIndex];
                return true;
            }
            state = null;
            return false;
        }

        /// <summary>
        /// 获取此 State 在指定 property 上的 follower
        /// </summary>
        /// <param name="property"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool CycleProperty(StateProperty property, [NotNullWhen(true)] out S? state)
        {
            if (followers != null && followers!.TryGetValue(property, out state))
            {
                return true;
            }
            state = null;
            return false;
        }

        public override int GetHashCode()
        {
            return hashcodeCache;
        }
    }
}