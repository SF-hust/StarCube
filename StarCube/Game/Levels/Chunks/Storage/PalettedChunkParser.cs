using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Chunks.Storage.Palette;

namespace StarCube.Game.Levels.Chunks.Storage
{
    public class PalettedChunkParser : IChunkParser
    {
        public bool TryParse(BsonDocument bson, ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            chunk = null;

            if (!bson.TryGetDocument("blockstates", out BsonDocument? blockStateDocument))
            {
                return false;
            }

            Span<int> buffer = stackalloc int[Chunk.ChunkSize];

            if (!TryParseBlockStates(blockStateDocument, buffer, out bool singleBlockState))
            {
                return false;
            }

            if (singleBlockState)
            {
                chunk = chunkFactory.CreateWithFill(pos, buffer[0]);
            }
            else
            {
                chunk = chunkFactory.Create(pos, buffer);
            }

            return true;
        }

        public BsonDocument ToBson(Chunk chunk)
        {
            BsonDocument bson = new BsonDocument();

            // 将 block state 数据转为 json
            BsonDocument blockStateDocument = BlockStatesToBson(chunk);
            bson.Add("blockstates", blockStateDocument);

            return bson;
        }

        private bool TryParseBlockStates(BsonDocument bson, Span<int> blockStates, out bool single)
        {
            single = false;

            if (!bson.TryGetInt32("palette", out int paletteID))
            {
                return false;
            }

            GlobalPaletteConverter? globalPaletteConverter = null;
            if (paletteID != blockStatePaletteManager.CurrentGlobalPaletteID &&
                !blockStatePaletteManager.TryGetConverter(paletteID, out globalPaletteConverter))
            {
                return false;
            }

            // 单一值的处理
            if (bson.TryGetInt32("data", out int singleValue))
            {
                single = true;
                if (globalPaletteConverter != null)
                {
                    blockStates[0] = globalPaletteConverter.ToCurrent(singleValue);
                }
                return true;
            }

            if (!bson.TryGetBinary("data", out byte[] binary))
            {
                return false;
            }

            // 检查二进制数据的尺寸
            if (binary.Length % (Chunk.ChunkSize / (sizeof(byte) * 8)) != 0)
            {
                return false;
            }
            int bitCount = binary.Length / (Chunk.ChunkSize / (sizeof(byte) * 8));
            if (bitCount < 1 || bitCount > 31)
            {
                return false;
            }

            // 解压数据
            BitUtil.Unpack(blockStates, binary, bitCount);

            // 使用了本地调色盘
            if (bson.TryGetBinary("local", out byte[] localPaletteBinary))
            {
                if (localPaletteBinary.Length % sizeof(int) != 0)
                {
                    return false;
                }

                // 构建本地调色盘
                Span<int> localPalette = MemoryMarshal.Cast<byte, int>(localPaletteBinary);
                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < localPalette.Length; i++)
                    {
                        localPaletteBinary.AsSpan(i * sizeof(int), sizeof(int)).Reverse();
                    }
                }

                // 将本地调色盘下的数据转化为全局调色盘下的数据
                for (int i = 0; i < blockStates.Length; ++i)
                {
                    int localIndex = blockStates[i];
                    if (localIndex >= localPalette.Length)
                    {
                        return false;
                    }
                    blockStates[i] = localPalette[localIndex];
                }
            }

            // 将旧全局调色盘下的数据转换为当前全局调色盘下的数据
            if (globalPaletteConverter != null)
            {
                for (int i = 0; i < blockStates.Length; ++i)
                {
                    if (!globalPaletteConverter.Valid(blockStates[i]))
                    {
                        return false;
                    }
                    blockStates[i] = globalPaletteConverter.ToCurrent(blockStates[i]);
                }
            }

            return true;
        }

        private BsonDocument BlockStatesToBson(Chunk chunk)
        {
            BsonDocument bson = new BsonDocument();

            // 设置全局 palette id
            bson.Add("palette", blockStatePaletteManager.CurrentGlobalPaletteID);

            // 获取 block state 数据拷贝
            Span<int> buffer = stackalloc int[Chunk.ChunkSize];
            chunk.CopyBlockStatesTo(buffer);

            // 收集 block state 数据信息
            int maxBlockState = 0;
            Dictionary<int, int> dict = PalettedChunkData.ThreadLocalDictionary.Value;
            dict.Clear();
            foreach (int blockState in buffer)
            {
                maxBlockState = Math.Max(maxBlockState, blockState);
                dict.TryAdd(blockState, dict.Count);
            }

            // block state 只有一种
            if (dict.Count == 1)
            {
                bson.Add("data", buffer[0]);
                return bson;
            }

            // 用全局调色盘存储时每个 block 需要的 bitCount
            int bitCountGlobal = BitUtil.BitCount(maxBlockState);
            // 用本地调色盘存储时每个 block 需要的 bitCount
            int bitCountPalette = BitUtil.BitCount(dict.Count);

            // 大致计算使用不同 palette 需要的存储空间
            int sizeGlobal = bitCountGlobal * Chunk.ChunkSize;
            int sizePalette = bitCountPalette * Chunk.ChunkSize + dict.Count * sizeof(int) + 6;

            // 使用全局调色盘
            if (sizeGlobal <= sizePalette)
            {
                byte[] binary = new byte[bitCountGlobal * (Chunk.ChunkSize / sizeof(byte) / 8)];
                BitUtil.Pack(buffer, binary, bitCountGlobal);

                bson.Add("data", binary);
            }
            // 使用本地调色盘
            else
            {
                // 构造本地调色盘
                byte[] localPaletteBinary = new byte[dict.Count * sizeof(int)];
                Span<int> localPalette = MemoryMarshal.Cast<byte, int>(localPaletteBinary.AsSpan());
                foreach (var pair in dict)
                {
                    localPalette[pair.Value] = pair.Key;
                }
                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < dict.Count; i++)
                    {
                        localPaletteBinary.AsSpan(i * sizeof(int), sizeof(int)).Reverse();
                    }
                }

                // 构造二进制数据
                for (int i = 0; i < buffer.Length; ++i)
                {
                    buffer[i] = dict[buffer[i]];
                }
                byte[] binary = new byte[bitCountPalette * (Chunk.ChunkSize / sizeof(byte) / 8)];
                BitUtil.Pack(buffer, binary, bitCountPalette);

                bson.Add("local", localPaletteBinary);
                bson.Add("data", binary);
            }

            return bson;
        }

        public PalettedChunkParser(IChunkFactory chunkFactory, GlobalPaletteManager<BlockState> blockStatePaletteManager)
        {
            this.chunkFactory = chunkFactory;
            this.blockStatePaletteManager = blockStatePaletteManager;
        }

        private readonly IChunkFactory chunkFactory;

        private readonly GlobalPaletteManager<BlockState> blockStatePaletteManager;
    }
}
