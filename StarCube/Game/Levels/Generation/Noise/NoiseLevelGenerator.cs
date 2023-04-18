using System;

using DotnetNoise;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Core.Registries;
using StarCube.Core.States;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Generation.Noise
{
    public sealed class NoiseLevelGenerator : ILevelGenerator
    {
        private void GenerateChunkData(ChunkPos pos, Span<int> buffer)
        {
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    int height = (int)(noise.GetPerlin((x + (pos.x << 4)) / 64.0f, (z + (pos.z << 4)) / 64.0f) * 31.0f + 64.0f);
                    height -= pos.y << 4;
                    int h = Math.Min(height, 16);
                    for (int y = 0; y < h; ++y)
                    {
                        buffer[BlockPos.InChunkPosToIndex(x, y, z)] = baseBlock;
                    }
                    if (height >= 0 && height < 16)
                    {
                        buffer[BlockPos.InChunkPosToIndex(x, height, z)] = surfaceBlock;
                    }
                    if (height >= -1 && height < 15 && height % 3 == 1)
                    {
                        buffer[BlockPos.InChunkPosToIndex(x, height + 1, z)] = decroBlock;
                    }
                }
            }
        }

        public Chunk GenerateChunk(ChunkPos pos)
        {
            if (pos.y > 5)
            {
                return chunkFactory.CreateEmpty(pos);
            }
            Span<int> buffer = stackalloc int[Chunk.ChunkSize];
            buffer.Clear();
            GenerateChunkData(pos, buffer);
            Chunk chunk = chunkFactory.Create(pos, buffer);
            return chunk;
        }

        public NoiseLevelGenerator(IChunkFactory chunkFactory)
        {
            this.chunkFactory = chunkFactory;
            baseBlock = BuiltinRegistries.Block.GetAsRegistryObject(StringID.Parse("starcube:dirt")).Value.DefaultState().IntegerID;
            surfaceBlock = BuiltinRegistries.Block.GetAsRegistryObject(StringID.Parse("starcube:grass_block")).Value.DefaultState().IntegerID;
            decroBlock = BuiltinRegistries.Block.GetAsRegistryObject(StringID.Parse("starcube:grass")).Value.DefaultState().IntegerID;
            noise.UsedNoiseType = FastNoise.NoiseType.Perlin;
        }

        private readonly IChunkFactory chunkFactory;

        private readonly FastNoise noise = new FastNoise();

        private readonly int baseBlock;
        private readonly int surfaceBlock;
        private readonly int decroBlock;
    }
}
