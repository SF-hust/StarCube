using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.BlockEntities;

namespace StarCube.Game.Levels.Chunks
{
    public abstract class Chunk
    {
        public const int ChunkSize = 16 * 16 * 16;

        public int X => Position.x;
        public int Y => Position.y;
        public int Z => Position.z;

        public bool Modify
        {
            get => modify;
            set => modify = value;
        }

        public abstract bool Writable { get; }

        public abstract bool Empty { get; }

        /* ~ BlockState start ~ */
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

        public abstract void CopyBlockStatesTo(Span<BlockState> array);

        public abstract void CopyBlockStatesTo(Span<int> array);

        public abstract Chunk CloneBlockStates();
        /* ~ BlockState end ~ */


        /* ~ BlockEntity start ~ */
        public abstract bool TryGetBlockEntity(BlockPos pos, [NotNullWhen(true)] out BlockEntity? blockEntity);

        public abstract bool TryAddBlockEntity(BlockPos pos, BlockEntity blockEntity);

        public abstract bool TryRemoveBlockEntity(BlockPos pos, [NotNullWhen(true)] out BlockEntity? blockEntity);
        /* ~ BlockEntity end ~ */


        /* ~ 生命周期 start ~ */
        public abstract void OnActive(Level level, bool active);
        /* ~ 生命周期 end ~ */

        /// <summary>
        /// 清理 Chunk 的内容
        /// </summary>
        public abstract void Clear();

        public abstract void Fill(BlockState blockState);

        public abstract void Release();

        public void Reset(ChunkPos pos)
        {
            this.pos = pos;
        }

        public override string ToString()
        {
            StringBuilder builder = StringUtil.StringBuilder;
            builder.Append("(").Append(Position.x).Append(", ").Append(Position.y).Append(", ").Append(Position.z).Append(")");
            return builder.ToString();
        }

        public Chunk(ChunkPos pos)
        {
            this.pos = pos;
        }

        public ChunkPos Position => pos;

        private ChunkPos pos;

        private bool modify = false;
    }
}
