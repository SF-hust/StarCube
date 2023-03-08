using System.Collections.Generic;

using StarCube.Data;
using StarCube.Data.Loading;
using StarCube.Data.Provider;

namespace StarCube.Core.State.Data
{
    public class StateDefinitionDataLoader<O, S> : IDataLoader
        where O : class, IStateDefiner<O, S>, IStringID
        where S : StateHolder<O, S>
    {
        public void Run(IDataProvider dataProvider)
        {
            foreach (O stateDefiner in stateDefiners)
            {
                if(dataProvider.TryLoad(StateDefinitionData.DataRegistry, prefix, stateDefiner.ID, StateDefinitionData.DataReader, out StateDefinitionData? data))
                {
                    StateDefinition<O, S>.Builder builder = new StateDefinition<O, S>.Builder(stateDefiner, stateFactory);
                    builder.AddRange(data.propertyToDefaultValueIndex);
                    stateDefiner.StateDefinition = builder.Build();
                }
                else
                {
                    stateDefiner.StateDefinition = StateDefinition<O, S>.BuildSingle(stateDefiner, stateFactory);
                }
            }
        }

        public StateDefinitionDataLoader(string prefix, IEnumerable<O> stateDefiners, StateHolder<O, S>.Factory stateFactory)
        {
            this.prefix = prefix;
            this.stateDefiners = stateDefiners;
            this.stateFactory = stateFactory;
        }

        private readonly string prefix;

        private readonly IEnumerable<O> stateDefiners;

        private readonly StateHolder<O, S>.Factory stateFactory;
    }
}
