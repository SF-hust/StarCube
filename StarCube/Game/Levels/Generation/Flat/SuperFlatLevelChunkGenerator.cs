using System;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Core.Registries;
using StarCube.Core.States;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Generation.Flat
{
    public sealed class SuperFlatLevelChunkGenerator : ILevelChunkGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos)
        {
            if (pos.y != 0)
            {
                return chunkFactory.CreateEmpty(pos);
            }

            Span<int> buffer = stackalloc int[Chunk.ChunkSize];
            buffer.Slice(6 * 16 * 16, 10 * 16 * 16).Clear();
            buffer.Slice(0, 16 * 16).Fill(bedrock);
            buffer.Slice(16 * 16, 4 * 16 * 16).Fill(dirt);
            buffer.Slice(5 * 16 * 16, 16 * 16).Fill(grassBlock);
            return chunkFactory.Create(pos, buffer);
        }

        public SuperFlatLevelChunkGenerator(IChunkFactory chunkFactory)
        {
            this.chunkFactory = chunkFactory;
            bedrock = BuiltinRegistries.Block.GetAsRegistryObject(StringID.Parse("starcube:bedrock")).Value.DefaultState().IntegerID;
            dirt = BuiltinRegistries.Block.GetAsRegistryObject(StringID.Parse("starcube:dirt")).Value.DefaultState().IntegerID;
            grassBlock = BuiltinRegistries.Block.GetAsRegistryObject(StringID.Parse("starcube:grass_block")).Value.DefaultState().IntegerID;
        }

        private readonly IChunkFactory chunkFactory;

        private readonly int bedrock;
        private readonly int dirt;
        private readonly int grassBlock;
    }
}
