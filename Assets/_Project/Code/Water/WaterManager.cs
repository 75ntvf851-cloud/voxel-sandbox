using System.Collections.Generic;
using UnityEngine;
using VoxelSandbox.Core;
using VoxelSandbox.World;

namespace VoxelSandbox.Water
{
    /// <summary>
    /// Represents a chunk of water cells aligned to voxel chunks.
    /// Each cell stores water amount (0-255, where 255 = full).
    /// </summary>
    public class WaterChunk
    {
        public Vector3Int Position { get; private set; }
        public byte[,,] WaterLevels { get; private set; }
        public bool IsDirty { get; set; }

        public WaterChunk(Vector3Int position, int size = 32)
        {
            Position = position;
            WaterLevels = new byte[size, size, size];
            IsDirty = false;
        }

        public byte GetWater(int x, int y, int z)
        {
            if (!IsValidIndex(x, y, z))
                return 0;

            return WaterLevels[x, y, z];
        }

        public void SetWater(int x, int y, int z, byte amount)
        {
            if (!IsValidIndex(x, y, z))
                return;

            if (WaterLevels[x, y, z] != amount)
            {
                WaterLevels[x, y, z] = amount;
                IsDirty = true;
            }
        }

        private bool IsValidIndex(int x, int y, int z)
        {
            int size = WaterLevels.GetLength(0);
            return x >= 0 && x < size && y >= 0 && y < size && z >= 0 && z < size;
        }

        public int GetTotalWaterMass()
        {
            int total = 0;
            int size = WaterLevels.GetLength(0);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        total += WaterLevels[x, y, z];
                    }
                }
            }
            
            return total;
        }
    }

    /// <summary>
    /// Manages water simulation across all chunks.
    /// </summary>
    public class WaterManager : MonoBehaviour
    {
        [Header("Simulation")]
        [SerializeField] private bool simulationEnabled = true;
        [SerializeField] private float simulationBudgetMs = 5f;
        [SerializeField] private KeyCode pauseKey = KeyCode.P;

        private Dictionary<Vector3Int, WaterChunk> _waterChunks = new Dictionary<Vector3Int, WaterChunk>();
        private ChunkManager _chunkManager;
        private long _totalWaterMass = 0;

        private void Start()
        {
            _chunkManager = ServiceLocator.Get<ChunkManager>();
            ServiceLocator.Register(this);
            
            Logger.Info("WaterManager initialized");
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                simulationEnabled = !simulationEnabled;
                Logger.Info($"Water simulation: {(simulationEnabled ? "enabled" : "disabled")}");
            }

            if (simulationEnabled)
            {
                SimulateWater();
            }
        }

        private void SimulateWater()
        {
            // TODO: Implement cellular automata simulation
            // 1. Flow downward (gravity)
            // 2. Equalize sideways (pressure)
            // 3. Limited upward when pressured
            // Track time and stay within budget
        }

        public void AddWater(Vector3Int worldPos, byte amount)
        {
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);

            if (!_waterChunks.TryGetValue(chunkPos, out WaterChunk waterChunk))
            {
                waterChunk = new WaterChunk(chunkPos);
                _waterChunks[chunkPos] = waterChunk;
            }

            byte current = waterChunk.GetWater(localPos.x, localPos.y, localPos.z);
            byte newAmount = (byte)Mathf.Min(current + amount, 255);
            
            waterChunk.SetWater(localPos.x, localPos.y, localPos.z, newAmount);
            _totalWaterMass += (newAmount - current);

            Logger.Info($"Added water at {worldPos}: {amount} (total: {newAmount})");
        }

        public void RemoveWater(Vector3Int worldPos)
        {
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);

            if (_waterChunks.TryGetValue(chunkPos, out WaterChunk waterChunk))
            {
                byte current = waterChunk.GetWater(localPos.x, localPos.y, localPos.z);
                waterChunk.SetWater(localPos.x, localPos.y, localPos.z, 0);
                _totalWaterMass -= current;

                Logger.Info($"Removed water at {worldPos}");
            }
        }

        public byte GetWater(Vector3Int worldPos)
        {
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);

            if (_waterChunks.TryGetValue(chunkPos, out WaterChunk waterChunk))
            {
                return waterChunk.GetWater(localPos.x, localPos.y, localPos.z);
            }

            return 0;
        }

        public long TotalWaterMass => _totalWaterMass;
        public bool IsSimulating => simulationEnabled;
    }
}
