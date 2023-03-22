using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Level.Generation;

namespace StarCube.Game.Level.Chunk.Source
{
    public sealed class ServerChunkSource : ChunkSource
    {
        public override LevelChunk GetChunkSync(ChunkPos pos)
        {
            return generator.GenerateChunk(pos);
        }

        public override Task<LevelChunk> GetChunkAsync(ChunkPos pos)
        {
            Task<LevelChunk> task = new Task<LevelChunk>(()=>
            {
                return generator.GenerateChunk(pos);
            });
            task.Start();
            return task;
        }

        public ServerChunkSource(ILevelGenerator generator)
        {
            this.generator = generator;
        }

        private readonly ILevelGenerator generator;
    }
}
