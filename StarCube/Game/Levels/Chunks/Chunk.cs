using System;
using System.Text;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using System.Threading;

namespace StarCube.Game.Levels.Chunks
{
    public abstract class Chunk
    {
        public const int ChunkSize = 16 * 16 * 16;

        public static ThreadLocal<int[]> ThreadLocalChunkDataBuffer = new ThreadLocal<int[]>(() => new int[ChunkSize]);

        public int X => pos.x;
        public int Y => pos.y;
        public int Z => pos.z;


        public abstract bool Writable { get; }

        public abstract bool Empty { get; }

        public abstract BlockState GetBlockState(int x, int y, int z);
        public virtual BlockState GetBlockState(BlockPos pos)
        {
            return GetBlockState(pos.x, pos.y, pos.z);
        }

        public abstract void SetBlockState(int x, int y, int z, BlockState blockState);
        public virtual void SetBlockState(BlockPos pos, BlockState blockState)
        {
            SetBlockState(pos.x, pos.y, pos.z, blockState);
        }

        public abstract void FillBlockState(int x0, int y0, int z0, int x1, int y1, int z1, BlockState blockState);
        public virtual void FillBlockState(BlockPos start, BlockPos end, BlockState blockState)
        {
            FillBlockState(start.x, start.y, start.z, end.x, end.y, end.z, blockState);
        }

        public abstract BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState);
        public virtual BlockState GetAndSetBlockState(BlockPos pos, BlockState blockState)
        {
            return GetAndSetBlockState(pos.x, pos.y, pos.z, blockState);
        }

        public abstract void CopyBlockStatesTo(BlockState[] array);

        public abstract void CopyBlockStatesTo(Span<int> array);

        public virtual void CopyBlockStatesTo(int[] array)
        {
            CopyBlockStatesTo(array.AsSpan());
        }

        public abstract void StoreTo(BsonDocument bson);

        public abstract Chunk Clone();

        public override string ToString()
        {
            StringBuilder builder = StringUtil.StringBuilder;
            builder.Append("(").Append(pos.x).Append(", ").Append(pos.y).Append(", ").Append(pos.z).Append(")");
            return builder.ToString();
        }

        public Chunk(ChunkPos pos)
        {
            this.pos = pos;
        }

        public readonly ChunkPos pos;
    }
}
