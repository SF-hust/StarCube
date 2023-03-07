using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using StarCube.Core.State.Property;

namespace StarCube.Core.State
{
    public sealed partial class StateDefinition<O, S>
        where O : class, IStateDefiner<O, S>
        where S : StateHolder<O, S>
    {
        public static StateDefinition<O, S> BuildSingle(O owner, StateHolder<O, S>.Factory factory)
        {
            List<S> states;
            states = new List<S>(1);
            S state = factory(owner, StatePropertyList.EMPTY);
            states.Add(state);
            return new StateDefinition<O, S>(owner, states.ToImmutableArray(), state);
        }

        public class Builder
        {
            private readonly O owner;
            private readonly List<KeyValuePair<StateProperty, int>> propertyAndDefaultIndices = new List<KeyValuePair<StateProperty, int>>();
            private readonly StateHolder<O, S>.Factory stateFactory;

            public Builder(O owner, StateHolder<O, S>.Factory factory)
            {
                this.owner = owner;
                stateFactory = factory;
            }

            /// <summary>
            /// 创建 Builder
            /// </summary>
            /// <param name="owner">State 的所有者</param>
            /// <param name="factory">创建 State 的工厂方法</param>
            /// <returns></returns>
            public static Builder Create(O owner, StateHolder<O, S>.Factory factory)
            {
                return new Builder(owner, factory);
            }

            /// <summary>
            /// 添加一个 Property 及其在默认状态中的取值的下标, 推荐使用此版本而非值版本
            /// </summary>
            /// <param name="property"></param>
            /// <param name="defaultValueIndex"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public Builder AddPropertyAndDefaultIndex(StateProperty property, int defaultValueIndex)
            {
                if (!property.IndexIsValid(defaultValueIndex))
                {
                    throw new Exception($"In state definition for [{owner}] :" +
                        $" valueIndex (= {defaultValueIndex}) out of bound for property {property}");
                }

                bool exist = false;
                propertyAndDefaultIndices.ForEach((KeyValuePair<StateProperty, int> pair) => exist |= pair.Key.Equals(property));
                if (exist)
                {
                    throw new Exception($"In state definition for [{owner}] :" +
                        $" state property ({property}) already exists");
                }

                propertyAndDefaultIndices.Add(new KeyValuePair<StateProperty, int>(property, defaultValueIndex));

                return this;
            }

            /// <summary>
            /// 添加一个 Property 及其在默认状态中的取值
            /// </summary>
            /// <typeparam name="VT"></typeparam>
            /// <param name="property"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public Builder AddPropertyAndDefaultValue<VT>(StateProperty<VT> property, VT defaultValue)
                where VT : struct
            {
                int index = property.GetIndexByValue(defaultValue);
                if (index == -1)
                {
                    throw new Exception($"In state definition for [{owner}] :" +
                        $" value (= {property.ValueToString(defaultValue)}) can't be taken by property ({property})");
                }

                return AddPropertyAndDefaultIndex(property, index);
            }

            public Builder AddRange(IEnumerable<KeyValuePair<StateProperty, int>> propertyAndDefaults)
            {
                foreach (KeyValuePair<StateProperty, int> pair in propertyAndDefaults)
                {
                    AddPropertyAndDefaultIndex(pair.Key, pair.Value);
                }

                return this;
            }

            /// <summary>
            /// 根据各属性的组合构建所有的 State, 并设置好相应的 neighbour 和 follower, 如果没有任何属性则只会构建一个 State
            /// </summary>
            /// <returns></returns>
            public StateDefinition<O, S> Build()
            {
                // 如果无属性定义，只有一个状态，直接构造返回
                if (propertyAndDefaultIndices.Count == 0)
                {
                    return BuildSingle(owner, stateFactory);
                }

                // 计算 :
                // State 的数量 (stateCount)
                // 默认 State 在列表中的下标 (defaultStateIndex)
                // 属性按序变化时 State 在列表中下标的递增值 (indexOffsetForProperties)
                int stateCount = 1;
                int defaultStateIndex = 0;
                List<int> indexOffsetForProperties = new List<int>(propertyAndDefaultIndices.Count);
                foreach (var pair in propertyAndDefaultIndices)
                {
                    StateProperty property = pair.Key;
                    int i = pair.Value;
                    indexOffsetForProperties.Add(stateCount);
                    defaultStateIndex += i * stateCount;
                    stateCount *= property.countOfValues;
                }

                // 生成所有的 state 对象
                List<S> states = GenerateAllStates(stateCount);

                // 找到默认状态
                S defaultState = states[defaultStateIndex];

                // 为每个 State 创建并设置对应的 neighbours 和 followers
                for (int i = 0; i < stateCount; ++i)
                {
                    ImmutableDictionary<StateProperty, ImmutableArray<S>> neighbours = GenerateNeighboursForState(states, i, indexOffsetForProperties);
                    ImmutableDictionary<StateProperty, S> followers = GenerateFollowersForState(states, i, indexOffsetForProperties);

                    states[i].SetNeighboursAndFollowers(neighbours, followers);
                }

                return new StateDefinition<O, S>(owner, states.ToImmutableArray(), defaultState);
            }

            private List<S> GenerateAllStates(int stateCount)
            {
                List<S> states = new List<S>(stateCount);

                StatePropertyList.Builder propertyListBuilder = StatePropertyList.Builder.Create();
                foreach (var pair in propertyAndDefaultIndices)
                {
                    propertyListBuilder.AddProperty(pair.Key);
                }

                propertyListBuilder.StartBuild();
                for (int i = 0; i < stateCount; ++i)
                {
                    S state = stateFactory(owner, propertyListBuilder.BuildNext());
                    states.Add(state);
                }

                return states;
            }

            private ImmutableDictionary<StateProperty, S> GenerateFollowersForState(List<S> states, int index, List<int> indexOffsetForProperties)
            {
                Dictionary<StateProperty, S> followers = new Dictionary<StateProperty, S>();
                // 为每个属性创建 follower
                for (int i = 0; i < propertyAndDefaultIndices.Count; ++i)
                {
                    StateProperty property = propertyAndDefaultIndices[i].Key;
                    int groupSize = property.countOfValues * indexOffsetForProperties[i];
                    int currentGroupBaseIndex = index / groupSize * groupSize;
                    int followerIndex = (index + indexOffsetForProperties[i]) % groupSize + currentGroupBaseIndex;
                    followers.Add(property, states[followerIndex]);
                }

                return followers.ToImmutableDictionary();
            }

            private ImmutableDictionary<StateProperty, ImmutableArray<S>> GenerateNeighboursForState(List<S> states, int index, List<int> indexOffsetForProperties)
            {
                Dictionary<StateProperty, ImmutableArray<S>> neighbours = new Dictionary<StateProperty, ImmutableArray<S>>();
                // 为每个属性创建邻居列表
                for (int pi = 0; pi < propertyAndDefaultIndices.Count; ++pi)
                {
                    StateProperty property = propertyAndDefaultIndices[pi].Key;
                    ImmutableArray<S> neighboursForProperty;

                    // 如果已经创建过所需的 neighbour 列表了, 直接引用它, 而不是再创建一遍
                    int neighourIndex = index - indexOffsetForProperties[pi];
                    if (index % (indexOffsetForProperties[pi] * property.countOfValues) >= indexOffsetForProperties[pi])
                    {
                        neighboursForProperty = states[neighourIndex].Neighbours[property];
                    }
                    // 为一个 property 创建对应的 neighbour 列表
                    else
                    {
                        List<S> neighbourList = new List<S>(property.countOfValues);
                        for (int j = 0; j < property.countOfValues; ++j)
                        {
                            neighbourList.Add(states[(index + j * indexOffsetForProperties[pi]) % states.Count]);
                        }
                        neighboursForProperty = neighbourList.ToImmutableArray();
                    }
                    neighbours.Add(property, neighboursForProperty);
                }

                return neighbours.ToImmutableDictionary();
            }
        }
    }
}
