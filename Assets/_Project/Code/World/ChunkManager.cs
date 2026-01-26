using System.Collections.Generic;
using UnityEngine;
using VoxelSandbox.Core;

namespace VoxelSandbox.World
{
    /// <summary>
    /// Manages all chunks in the voxel world.
    /// Handles chunk creation, loading, unloading, and voxel modifications.
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {
        [Header("World Settings")]
        [SerializeField] private Vector3Int worldSize = new Vector3Int(8, 4, 8); // in chunks
        [SerializeField] private GameObject chunkPrefab;

        private Dictionary<Vector3Int, Chunk> _chunks = new Dictionary<Vector3Int, Chunk>();
        private Dictionary<Vector3Int, GameObject> _chunkObjects = new Dictionary<Vector3Int, GameObject>();

        private void Start()
        {
            ServiceLocator.Register(this);
            Logger.Info($"ChunkManager initialized: World size {worldSize} chunks");
            
            GenerateWorld();
        }

        private void GenerateWorld()
        {
            Logger.Info("Generating world...");
            
            for (int x = 0; x < worldSize.x; x++)
            {
                for (int y = 0; y < worldSize.y; y++)
                {
                    for (int z = 0; z < worldSize.z; z++)
                    {
                        Vector3Int chunkPos = new Vector3Int(x, y, z);
                        CreateChunk(chunkPos);
                    }
                }
            }

            Logger.Info($"World generated: {_chunks.Count} chunks");
        }

        private void CreateChunk(Vector3Int chunkPos)
        {
            if (_chunks.ContainsKey(chunkPos))
                return;

            Chunk chunk = new Chunk(chunkPos);
            
            // Simple flat terrain generation (bottom layer solid)
            if (chunkPos.y == 0)
            {
                for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        chunk.SetBlock(x, 0, z, BlockType.Solid);
                    }
                }
            }

            _chunks[chunkPos] = chunk;

            // Create GameObject for chunk
            GameObject chunkObject = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
            chunkObject.transform.position = new Vector3(
                chunkPos.x * Chunk.CHUNK_SIZE,
                chunkPos.y * Chunk.CHUNK_SIZE,
                chunkPos.z * Chunk.CHUNK_SIZE
            );
            chunkObject.transform.parent = transform;

            _chunkObjects[chunkPos] = chunkObject;

            // TODO: Add ChunkRenderer component and build mesh
        }

        public BlockType GetBlock(Vector3Int worldPos)
        {
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);

            if (_chunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                return chunk.GetBlock(localPos.x, localPos.y, localPos.z);
            }

            return BlockType.Air;
        }

        public void SetBlock(Vector3Int worldPos, BlockType type)
        {
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);

            if (_chunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                chunk.SetBlock(localPos.x, localPos.y, localPos.z, type);
                
                // Mark chunk and neighbors as dirty
                MarkChunkDirty(chunkPos);
                
                // If on boundary, mark neighbor chunks dirty too
                if (localPos.x == 0) MarkChunkDirty(chunkPos + Vector3Int.left);
                if (localPos.x == Chunk.CHUNK_SIZE - 1) MarkChunkDirty(chunkPos + Vector3Int.right);
                if (localPos.y == 0) MarkChunkDirty(chunkPos + Vector3Int.down);
                if (localPos.y == Chunk.CHUNK_SIZE - 1) MarkChunkDirty(chunkPos + Vector3Int.up);
                if (localPos.z == 0) MarkChunkDirty(chunkPos + new Vector3Int(0, 0, -1));
                if (localPos.z == Chunk.CHUNK_SIZE - 1) MarkChunkDirty(chunkPos + new Vector3Int(0, 0, 1));

                Logger.Info($"Block set at {worldPos}: {type}");
            }
        }

        private void MarkChunkDirty(Vector3Int chunkPos)
        {
            if (_chunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                chunk.IsDirty = true;
                // TODO: Trigger mesh rebuild
            }
        }

        public Chunk GetChunk(Vector3Int chunkPos)
        {
            return _chunks.TryGetValue(chunkPos, out Chunk chunk) ? chunk : null;
        }

        public IEnumerable<Chunk> GetAllChunks()
        {
            return _chunks.Values;
        }
    }
}
