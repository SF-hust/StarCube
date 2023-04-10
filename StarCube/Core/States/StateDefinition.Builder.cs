using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using StarCube.Core.States.Property;

namespace StarCube.Core.States
{
    public sealed partial class StateDefinition<O, S>
        where O : class, IStateOwner<O, S>
        where S : State<O, S>
    {
        public static StateDefinition<O, S> BuildSingle(O owner, Func<O, StatePropertyList, int, S> factory)
        {
            List<S> states;
            states = new List<S>(1);
            S state = factory(owner, StatePropertyList.EMPTY, 0);
            states.Add(state);
            return new StateDefinition<O, S>(owner, states.ToImmutableArray(), state);
        }

        public static StateDefinition<O, S> BuildFromPropertyEntryList(O owner, Func<O, StatePropertyList, int, S> factory, List<StatePropertyEntry> entries)
        {
            Builder builder = Builder.Create(owner, factory);
            foreach (var entry in entries)
            {
                builder.AddPropertyAndDefaultIndex(entry.name, entry.property, entry.valueIndex);
            }
            return builder.Build();
        }

        public class Builder
        {
            private readonly O owner;
            private readonly List<StatePropertyEntry> propertyEntries = new List<StatePropertyEntry>();
            private readonly Func<O, StatePropertyList, int, S> createState;

            public Builder(O owner, Func<O, StatePropertyList, int, S> factory)
            {
                this.owner = owner;
                createState = factory;
            }

            /// <summary>
            /// 创建 Builder
            /// </summary>
            /// <param name="owner">State 的所有者</param>
            /// <param name="factory">创建 State 的工厂方法</param>
            /// <returns></returns>
            public static Builder Create(O owner, Func<O, StatePropertyList, int, S> factory)
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
            public Builder AddPropertyAndDefaultIndex(string name, StateProperty property, int defaultValueIndex)
            {
                if (!property.IndexIsValid(defaultValueIndex))
                {
                    throw new Exception($"In state definition for [{owner}] :" +
                        $" valueIndex (= {defaultValueIndex}) out of bound for property {property}");
                }

                bool exist = false;
                propertyEntries.ForEach((StatePropertyEntry entry) => exist |= entry.property.Equals(property));
                if (exist)
                {
                    throw new Exception($"In state definition for [{owner}] :" +
                        $" state property ({property}) already exists");
                }

                propertyEntries.Add(new StatePropertyEntry(name, property, defaultValueIndex));

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
            public Builder AddPropertyAndDefaultValue<VT>(string name, StateProperty<VT> property, VT defaultValue)
                where VT : struct
            {
                int index = property.GetIndexByValue(defaultValue);
                if (index == -1)
                {
                    throw new Exception($"In state definition for [{owner}] :" +
                        $" value (= {property.ValueToString(defaultValue)}) can't be taken by property ({property})");
                }

                return AddPropertyAndDefaultIndex(name, property, index);
            }

            public Builder AddRange(IEnumerable<StatePropertyEntry> propertyAndDefaults)
            {
                foreach (StatePropertyEntry entry in propertyAndDefaults)
                {
                    AddPropertyAndDefaultIndex(entry.name, entry.property, entry.valueIndex);
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
                if (propertyEntries.Count == 0)
                {
                    return BuildSingle(owner, createState);
                }

                // 计算 :
                // State 的数量 (stateCount)
                // 默认 State 在列表中的下标 (defaultStateIndex)
                // 属性按序变化时 State 在列表中下标的递增值 (indexOffsetForProperties)
                int stateCount = 1;
                int defaultStateIndex = 0;
                List<int> indexOffsetForProperties = new List<int>(propertyEntries.Count);
                foreach (var pair in propertyEntries)
                {
                    StateProperty property = pair.property;
                    int i = pair.valueIndex;
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
                foreach (var entry in propertyEntries)
                {
                    propertyListBuilder.AddProperty(entry.name, entry.property);
                }

                propertyListBuilder.StartBuild();
                for (int i = 0; i < stateCount; ++i)
                {
                    S state = createState(owner, propertyListBuilder.BuildNext(), i);
                    states.Add(state);
                }

                return states;
            }

            private ImmutableDictionary<StateProperty, S> GenerateFollowersForState(List<S> states, int index, List<int> indexOffsetForProperties)
            {
                Dictionary<StateProperty, S> followers = new Dictionary<StateProperty, S>();
                // 为每个属性创建 follower
                for (int i = 0; i < propertyEntries.Count; ++i)
                {
                    StateProperty property = propertyEntries[i].property;
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
                for (int pi = 0; pi < propertyEntries.Count; ++pi)
                {
                    StateProperty property = propertyEntries[pi].property;
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
