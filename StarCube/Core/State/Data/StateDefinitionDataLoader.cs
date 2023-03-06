using System.Collections.Generic;

using StarCube.Data;
using StarCube.Data.Loading;

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
                if(dataProvider.TryLoad(StateDefinitionData.DataRegistry, stateDefiner.ID, StateDefinitionData.DataReader, out StateDefinitionData? data))
                {
                    StateDefinition<O, S>.Builder builder = new StateDefinition<O, S>.Builder(stateDefiner, factory);
                    builder.AddRange(data.propertyToDefaultValueIndex);
                    stateDefiner.StateDefinition = builder.Build();
                }
                else
                {
                    stateDefiner.StateDefinition = StateDefinition<O, S>.BuildSingle(stateDefiner, factory);
                }
            }
        }

        public StateDefinitionDataLoader(IEnumerable<O> stateDefiners, StateHolder<O, S>.Factory factory)
        {
            this.stateDefiners = stateDefiners;
            this.factory = factory;
        }

        private readonly IEnumerable<O> stateDefiners;

        private readonly StateHolder<O, S>.Factory factory;
    }
}
