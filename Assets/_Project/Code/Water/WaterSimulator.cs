using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;

namespace VoxelSandbox.Water
{
    /// <summary>
    /// Advanced water simulation using cellular automata.
    /// Implements realistic water flow with mass conservation.
    /// Mobile-optimized with time slicing.
    /// </summary>
    public class WaterSimulator : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] private bool simulationEnabled = true;
        [SerializeField] private float timeSliceBudget = 5f; // ms per frame
        [SerializeField] private int maxUpdatesPerFrame = 100;
        
        [Header("Flow Parameters")]
        [SerializeField] private float gravityFlow = 4f;      // Flow rate downward
        [SerializeField] private float lateralFlow = 2f;      // Flow rate sideways
        [SerializeField] private float upwardFlow = 1f;       // Flow rate upward when pressured
        [SerializeField] private byte minFlowAmount = 1;      // Minimum amount to flow
        [SerializeField] private byte compressionThreshold = 200; // When water starts flowing upward
        
        private WaterManager waterManager;
        private Queue<Vector3Int> updateQueue = new Queue<Vector3Int>();
        private HashSet<Vector3Int> inQueue = new HashSet<Vector3Int>();
        
        private static readonly ProfilerMarker s_SimulateMarker = new ProfilerMarker("WaterSimulator.Simulate");
        private static readonly ProfilerMarker s_FlowMarker = new ProfilerMarker("WaterSimulator.Flow");
        
        private void Awake()
        {
            waterManager = GetComponent<WaterManager>();
            if (waterManager == null)
            {
                Core.Logger.LogError("WaterSimulator requires WaterManager component");
                enabled = false;
            }
        }
        
        private void Update()
        {
            if (!simulationEnabled) return;
            
            using (s_SimulateMarker.Auto())
            {
                SimulateWaterFlow();
            }
        }
        
        /// <summary>
        /// Mark a position for water update.
        /// </summary>
        public void MarkForUpdate(Vector3Int position)
        {
            if (!inQueue.Contains(position))
            {
                updateQueue.Enqueue(position);
                inQueue.Add(position);
            }
        }
        
        /// <summary>
        /// Main water simulation step.
        /// </summary>
        private void SimulateWaterFlow()
        {
            float startTime = Time.realtimeSinceStartup * 1000f;
            int updateCount = 0;
            
            while (updateQueue.Count > 0 && updateCount < maxUpdatesPerFrame)
            {
                // Check time budget
                float elapsed = (Time.realtimeSinceStartup * 1000f) - startTime;
                if (elapsed > timeSliceBudget)
                {
                    break;
                }
                
                Vector3Int pos = updateQueue.Dequeue();
                inQueue.Remove(pos);
                
                // Get water at this position
                byte waterLevel = waterManager.GetWater(pos);
                if (waterLevel == 0) continue;
                
                using (s_FlowMarker.Auto())
                {
                    FlowFromCell(pos, waterLevel);
                }
                
                updateCount++;
            }
            
            // Log performance metrics
            if (updateCount > 0)
            {
                float totalTime = (Time.realtimeSinceStartup * 1000f) - startTime;
                if (totalTime > timeSliceBudget * 1.5f)
                {
                    Core.Logger.LogWarning($"Water simulation exceeded budget: {totalTime:F2}ms ({updateCount} updates)");
                }
            }
        }
        
        /// <summary>
        /// Simulate water flow from a single cell.
        /// </summary>
        private void FlowFromCell(Vector3Int pos, byte waterLevel)
        {
            if (waterLevel < minFlowAmount) return;
            
            // Check if there's a block at this position (water can't exist in solid blocks)
            World.BlockType blockType = Core.ServiceLocator.Get<World.ChunkManager>().GetBlock(pos);
            if (blockType != World.BlockType.Air)
            {
                // Remove water from solid blocks
                waterManager.RemoveWater(pos);
                return;
            }
            
            // 1. Try to flow downward (gravity)
            Vector3Int below = pos + Vector3Int.down;
            if (TryFlowTo(pos, below, waterLevel, gravityFlow))
            {
                return; // Flowing down, don't flow sideways yet
            }
            
            // 2. Try to flow sideways (pressure equalization)
            Vector3Int[] lateralNeighbors = new Vector3Int[]
            {
                pos + Vector3Int.right,
                pos + Vector3Int.left,
                pos + Vector3Int.forward,
                pos + Vector3Int.back
            };
            
            // Calculate average lateral water level
            int validNeighbors = 0;
            int totalNeighborWater = 0;
            
            foreach (var neighbor in lateralNeighbors)
            {
                World.BlockType neighborBlock = Core.ServiceLocator.Get<World.ChunkManager>().GetBlock(neighbor);
                if (neighborBlock == World.BlockType.Air)
                {
                    validNeighbors++;
                    totalNeighborWater += waterManager.GetWater(neighbor);
                }
            }
            
            if (validNeighbors > 0)
            {
                float averageLevel = (totalNeighborWater + waterLevel) / (float)(validNeighbors + 1);
                
                // Flow to neighbors below average
                foreach (var neighbor in lateralNeighbors)
                {
                    World.BlockType neighborBlock = Core.ServiceLocator.Get<World.ChunkManager>().GetBlock(neighbor);
                    if (neighborBlock == World.BlockType.Air)
                    {
                        byte neighborWater = waterManager.GetWater(neighbor);
                        
                        if (neighborWater < averageLevel)
                        {
                            byte flowAmount = (byte)Mathf.Min(
                                (averageLevel - neighborWater) * lateralFlow * Time.deltaTime,
                                waterLevel
                            );
                            
                            if (flowAmount >= minFlowAmount)
                            {
                                TransferWater(pos, neighbor, flowAmount);
                                waterLevel = waterManager.GetWater(pos);
                                
                                if (waterLevel < minFlowAmount)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            
            // 3. Try to flow upward if compressed
            if (waterLevel > compressionThreshold)
            {
                Vector3Int above = pos + Vector3Int.up;
                World.BlockType aboveBlock = Core.ServiceLocator.Get<World.ChunkManager>().GetBlock(above);
                
                if (aboveBlock == World.BlockType.Air)
                {
                    byte aboveWater = waterManager.GetWater(above);
                    
                    if (aboveWater < 255)
                    {
                        byte flowAmount = (byte)Mathf.Min(
                            (waterLevel - compressionThreshold) * upwardFlow * Time.deltaTime,
                            waterLevel,
                            255 - aboveWater
                        );
                        
                        if (flowAmount >= minFlowAmount)
                        {
                            TransferWater(pos, above, flowAmount);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Try to flow water to a neighbor cell.
        /// </summary>
        private bool TryFlowTo(Vector3Int from, Vector3Int to, byte waterLevel, float flowRate)
        {
            // Check if destination is air
            World.BlockType blockType = Core.ServiceLocator.Get<World.ChunkManager>().GetBlock(to);
            if (blockType != World.BlockType.Air)
            {
                return false;
            }
            
            byte destinationWater = waterManager.GetWater(to);
            
            // Can't flow if destination is full
            if (destinationWater >= 255)
            {
                return false;
            }
            
            // Calculate flow amount
            byte maxFlow = (byte)(255 - destinationWater);
            byte flowAmount = (byte)Mathf.Min(
                waterLevel * flowRate * Time.deltaTime,
                waterLevel,
                maxFlow
            );
            
            if (flowAmount < minFlowAmount)
            {
                return false;
            }
            
            // Transfer water
            TransferWater(from, to, flowAmount);
            return true;
        }
        
        /// <summary>
        /// Transfer water from one cell to another with mass conservation.
        /// </summary>
        private void TransferWater(Vector3Int from, Vector3Int to, byte amount)
        {
            if (amount == 0) return;
            
            byte sourceWater = waterManager.GetWater(from);
            byte destWater = waterManager.GetWater(to);
            
            // Ensure we don't take more than available
            amount = (byte)Mathf.Min(amount, sourceWater);
            
            // Ensure we don't overflow destination
            int newDestWater = destWater + amount;
            if (newDestWater > 255)
            {
                amount = (byte)(255 - destWater);
                newDestWater = 255;
            }
            
            // Perform transfer
            int newSourceWater = sourceWater - amount;
            
            // Update water levels
            if (newSourceWater <= 0)
            {
                waterManager.RemoveWater(from);
            }
            else
            {
                waterManager.AddWater(from, (byte)newSourceWater);
            }
            
            waterManager.AddWater(to, (byte)newDestWater);
            
            // Mark neighbors for update
            MarkForUpdate(from);
            MarkForUpdate(to);
            MarkNeighborsForUpdate(from);
            MarkNeighborsForUpdate(to);
        }
        
        /// <summary>
        /// Mark all neighbors of a position for update.
        /// </summary>
        private void MarkNeighborsForUpdate(Vector3Int pos)
        {
            MarkForUpdate(pos + Vector3Int.up);
            MarkForUpdate(pos + Vector3Int.down);
            MarkForUpdate(pos + Vector3Int.right);
            MarkForUpdate(pos + Vector3Int.left);
            MarkForUpdate(pos + Vector3Int.forward);
            MarkForUpdate(pos + Vector3Int.back);
        }
        
        /// <summary>
        /// Enable or disable water simulation.
        /// </summary>
        public void SetSimulationEnabled(bool enabled)
        {
            simulationEnabled = enabled;
        }
        
        /// <summary>
        /// Get simulation statistics.
        /// </summary>
        public (int queueSize, bool enabled) GetStats()
        {
            return (updateQueue.Count, simulationEnabled);
        }
        
        /// <summary>
        /// Clear simulation queue (for debugging or reset).
        /// </summary>
        public void ClearQueue()
        {
            updateQueue.Clear();
            inQueue.Clear();
        }
        
        /// <summary>
        /// Trigger immediate update for a region.
        /// </summary>
        public void UpdateRegion(Vector3Int min, Vector3Int max)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        byte water = waterManager.GetWater(pos);
                        
                        if (water > 0)
                        {
                            MarkForUpdate(pos);
                        }
                    }
                }
            }
        }
    }
}
