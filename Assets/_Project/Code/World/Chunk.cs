using UnityEngine;

namespace VoxelSandbox.World
{
    /// <summary>
    /// Represents a single chunk of voxels (default 32x32x32).
    /// </summary>
    public class Chunk
    {
        public const int CHUNK_SIZE = 32;
        
        public Vector3Int Position { get; private set; }
        public BlockType[,,] Blocks { get; private set; }
        public bool IsDirty { get; set; }

        public Chunk(Vector3Int position)
        {
            Position = position;
            Blocks = new BlockType[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
            IsDirty = true;
        }

        public BlockType GetBlock(int x, int y, int z)
        {
            if (!IsValidIndex(x, y, z))
                return BlockType.Air;

            return Blocks[x, y, z];
        }

        public void SetBlock(int x, int y, int z, BlockType type)
        {
            if (!IsValidIndex(x, y, z))
                return;

            if (Blocks[x, y, z] != type)
            {
                Blocks[x, y, z] = type;
                IsDirty = true;
            }
        }

        public void Fill(BlockType type)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        Blocks[x, y, z] = type;
                    }
                }
            }
            IsDirty = true;
        }

        private bool IsValidIndex(int x, int y, int z)
        {
            return x >= 0 && x < CHUNK_SIZE &&
                   y >= 0 && y < CHUNK_SIZE &&
                   z >= 0 && z < CHUNK_SIZE;
        }

        public static Vector3Int WorldToChunkPosition(Vector3Int worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / (float)CHUNK_SIZE),
                Mathf.FloorToInt(worldPos.y / (float)CHUNK_SIZE),
                Mathf.FloorToInt(worldPos.z / (float)CHUNK_SIZE)
            );
        }

        public static Vector3Int WorldToLocalPosition(Vector3Int worldPos)
        {
            int x = ((worldPos.x % CHUNK_SIZE) + CHUNK_SIZE) % CHUNK_SIZE;
            int y = ((worldPos.y % CHUNK_SIZE) + CHUNK_SIZE) % CHUNK_SIZE;
            int z = ((worldPos.z % CHUNK_SIZE) + CHUNK_SIZE) % CHUNK_SIZE;
            return new Vector3Int(x, y, z);
        }
    }
}
