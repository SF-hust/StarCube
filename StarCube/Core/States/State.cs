﻿using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using StarCube.Core.States.Property;

namespace StarCube.Core.States
{
    /// <summary>
    /// 一个 StateHolder 对象是 Owner 的一个状态实例, 保存着 Owner 所定义的所有状态属性以及在当前状态下各属性的取值, 示例参照 BlockState
    /// </summary>
    /// <typeparam name="O">Owner 类型</typeparam>
    /// <typeparam name="S">Holder 类型</typeparam>
    public abstract class State<O, S>
        where O : class, IStateOwner<O, S>
        where S : State<O, S>
    {
        /// <summary>
        /// 此 State 是否是其 Owner 的唯一 State
        /// </summary>
        public bool Single => propertyList == null;

        public ImmutableDictionary<StateProperty, ImmutableArray<S>> Neighbours => neighbours ?? throw new NullReferenceException(nameof(Neighbours));

        public ImmutableDictionary<StateProperty, S> Followers => followers ?? throw new NullReferenceException(nameof(Followers));


        /// <summary>
        /// 为 State 设置 neighbours 和 followers
        /// </summary>
        /// <param name="neighbours"></param>
        /// <param name="followers"></param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void SetNeighboursAndFollowers(ImmutableDictionary<StateProperty, ImmutableArray<S>> neighbours, ImmutableDictionary<StateProperty, S> followers)
        {
            if (propertyList.Count == 0 || this.neighbours != null || this.followers != null)
            {
                throw new InvalidOperationException("can't set neighbours and followers for a State twice, or this State is single");
            }
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
        public bool TrySetProperty(StateProperty property, int valueIndex, [NotNullWhen(true)] out S? state)
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

        public S SetProperty(StateProperty property, int valueIndex)
        {
            if (TrySetProperty(property, valueIndex, out S? state))
            {
                return state;
            }

            return (S)this;
        }

        /// <summary>
        /// 获取此 State 在指定 property 上的 follower
        /// </summary>
        /// <param name="property"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool TryCycleProperty(StateProperty property, [NotNullWhen(true)] out S? state)
        {
            if (followers != null && followers.TryGetValue(property, out state))
            {
                return true;
            }
            state = null;
            return false;
        }

        public S CycleProperty(StateProperty property)
        {
            if(TryCycleProperty(property, out S? state))
            {
                return state;
            }

            return (S)this;
        }

        public override int GetHashCode()
        {
            return hashcodeCache;
        }

        public State(O owner, StatePropertyList propertyList, int localID)
        {
            this.owner = owner;
            this.propertyList = propertyList;
            hashcodeCache = HashCode.Combine(propertyList, owner);
            this.localID = localID;
        }

        /// <summary>
        /// State 的 Owner
        /// </summary>
        public readonly O owner;

        /// <summary>
        /// State 的属性与取值列表
        /// </summary>
        public readonly StatePropertyList propertyList;

        private ImmutableDictionary<StateProperty, ImmutableArray<S>>? neighbours = null;
        private ImmutableDictionary<StateProperty, S>? followers = null;

        public readonly int localID;

        private readonly int hashcodeCache;
    }
}