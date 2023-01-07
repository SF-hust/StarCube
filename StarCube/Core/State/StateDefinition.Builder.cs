using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

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
            S state = factory(owner, null);
            states.Add(state);
            return new StateDefinition<O, S>(owner, states.ToImmutableArray(), state);
        }

        public class Builder
        {

            private readonly O owner;
            private readonly List<KeyValuePair<StateProperty, int>> propertyAndDefaultIndices = new List<KeyValuePair<StateProperty, int>>();
            private readonly StateHolder<O, S>.Factory stateFactory;

            protected Builder(O owner, StateHolder<O, S>.Factory factory)
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

            /// <summary>
            /// 根据各属性的组合构建所有的 State, 并设置好相应的 neighbour 和 follower, 如果没有任何属性则只会构建一个 State
            /// </summary>
            /// <returns></returns>
            public StateDefinition<O, S> Build()
            {
                int stateCount = 1;
                int defaultStateIndex = 0;
                List<int> indexOffsetForProperties = new List<int>(propertyAndDefaultIndices.Count);

                // 计算 State 的数量, 默认 State 在列表中的下标,
                // 以及每个属性顺序变化时对应的 State 在列表中下标的递增值
                foreach (var pair in propertyAndDefaultIndices)
                {
                    StateProperty property = pair.Key;
                    int i = pair.Value;
                    Debug.Assert(i >= 0 && i < property.countOfValues);
                    indexOffsetForProperties.Add(stateCount);
                    defaultStateIndex += i * stateCount;
                    stateCount *= property.countOfValues;
                }

                List<S> states;
                // 如果只有一个状态
                if (stateCount == 1)
                {
                    S state = stateFactory(owner, null);
                    states = new List<S>(1)
                    {
                        state
                    };
                    return new StateDefinition<O, S>(owner, states.ToImmutableArray(), state);
                }

                // 创建所有的可能状态
                states = new List<S>(stateCount);

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

                // 为每个 State 创建对应的 neighbours 和 followers
                List<Dictionary<StateProperty, ImmutableArray<S>>> neighboursForStates = new List<Dictionary<StateProperty, ImmutableArray<S>>>(stateCount);
                for (int i = 0; i < stateCount; ++i)
                {
                    S state = states[i];

                    // 创建 neighbours
                    Dictionary<StateProperty, ImmutableArray<S>> neighbour = new Dictionary<StateProperty, ImmutableArray<S>>();
                    for (int pi = 0; pi < propertyAndDefaultIndices.Count; ++pi)
                    {
                        StateProperty property = propertyAndDefaultIndices[pi].Key;
                        ImmutableArray<S> neighbourForProperty;

                        // 如果已经创建过所需的 neighbour 列表了, 直接引用它, 而不是再创建一遍
                        if (i % (indexOffsetForProperties[pi] * property.countOfValues) > indexOffsetForProperties[pi])
                        {
                            int index = i - indexOffsetForProperties[pi];
                            neighbourForProperty = neighboursForStates[index][property];
                        }
                        // 为一个 property 创建对应的 neighbour 列表
                        else
                        {
                            List<S> neighbourList = new List<S>(property.countOfValues);
                            for (int j = 0; j < property.countOfValues; ++j)
                            {
                                neighbourList.Add(states[i + j * indexOffsetForProperties[pi]]);
                            }
                            neighbourForProperty = neighbourList.ToImmutableArray();
                        }
                        neighbour.Add(property, neighbourForProperty);
                    }

                    // 创建 followers
                    Dictionary<StateProperty, S> follower = new Dictionary<StateProperty, S>();
                    for (int pi = 0; pi < propertyAndDefaultIndices.Count; ++pi)
                    {
                        StateProperty property = propertyAndDefaultIndices[pi].Key;
                        int followStateIndex = (i + indexOffsetForProperties[pi]) % (property.countOfValues * indexOffsetForProperties[pi]);
                        follower.Add(property, states[followStateIndex]);
                    }

                    // 设置 neighbours 和 followers
                    state.SetNeighboursAndFollowers(neighbour.ToImmutableDictionary(), follower.ToImmutableDictionary());
                }

                // 获取默认状态
                S defaultState = states[defaultStateIndex];
                return new StateDefinition<O, S>(owner, states.ToImmutableArray(), defaultState);
            }
        }
    }
}
