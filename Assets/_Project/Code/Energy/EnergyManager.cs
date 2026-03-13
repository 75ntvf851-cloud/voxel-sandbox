using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;

namespace VoxelSandbox.Energy
{
    /// <summary>
    /// Energy network types for different systems.
    /// </summary>
    public enum EnergyBlockType
    {
        None,
        Generator,      // Produces energy (solar panels)
        Battery,        // Stores energy
        Consumer,       // Consumes energy (lights, pumps, etc.)
        Conduit         // Transfers energy (wires, cables)
    }
    
    /// <summary>
    /// Represents a single block in the energy network.
    /// </summary>
    public class EnergyBlock
    {
        public Vector3Int Position { get; set; }
        public EnergyBlockType Type { get; set; }
        public float MaxPower { get; set; }          // Max generation/consumption/storage
        public float CurrentPower { get; set; }      // Current power level (for batteries)
        public bool IsActive { get; set; }
        public int NetworkId { get; set; }           // Which power network this belongs to
        
        public EnergyBlock(Vector3Int position, EnergyBlockType type, float maxPower)
        {
            Position = position;
            Type = type;
            MaxPower = maxPower;
            CurrentPower = 0f;
            IsActive = false;
            NetworkId = -1;
        }
    }
    
    /// <summary>
    /// Manages energy generation, storage, and consumption across the game world.
    /// Implements power network simulation for Space Engineers-style energy system.
    /// </summary>
    public class EnergyManager : MonoBehaviour
    {
        private static readonly ProfilerMarker s_UpdateNetworkMarker = new ProfilerMarker("EnergyManager.UpdateNetwork");
        private static readonly ProfilerMarker s_CalculateFlowMarker = new ProfilerMarker("EnergyManager.CalculateFlow");
        
        private Dictionary<Vector3Int, EnergyBlock> energyBlocks = new Dictionary<Vector3Int, EnergyBlock>();
        private List<PowerNetwork> powerNetworks = new List<PowerNetwork>();
        
        private int nextNetworkId = 0;
        private float updateTimer = 0f;
        private const float UPDATE_INTERVAL = 0.1f; // Update power network 10 times per second
        
        /// <summary>
        /// Power network containing connected energy blocks.
        /// </summary>
        private class PowerNetwork
        {
            public int Id { get; set; }
            public List<EnergyBlock> Generators { get; set; }
            public List<EnergyBlock> Batteries { get; set; }
            public List<EnergyBlock> Consumers { get; set; }
            public List<EnergyBlock> Conduits { get; set; }
            
            public float TotalGeneration { get; set; }
            public float TotalConsumption { get; set; }
            public float TotalStorage { get; set; }
            public float CurrentStorage { get; set; }
            
            public PowerNetwork(int id)
            {
                Id = id;
                Generators = new List<EnergyBlock>();
                Batteries = new List<EnergyBlock>();
                Consumers = new List<EnergyBlock>();
                Conduits = new List<EnergyBlock>();
            }
        }
        
        private void Awake()
        {
            // Register with ServiceLocator
            Core.ServiceLocator.Register(this);
        }
        
        private void Update()
        {
            updateTimer += Time.deltaTime;
            
            if (updateTimer >= UPDATE_INTERVAL)
            {
                updateTimer = 0f;
                UpdatePowerNetworks();
            }
        }
        
        /// <summary>
        /// Add an energy block to the system.
        /// </summary>
        public void AddEnergyBlock(Vector3Int position, EnergyBlockType type, float maxPower)
        {
            if (energyBlocks.ContainsKey(position))
            {
                Core.Logger.LogWarning($"Energy block already exists at {position}");
                return;
            }
            
            EnergyBlock block = new EnergyBlock(position, type, maxPower);
            energyBlocks[position] = block;
            
            // Rebuild networks
            RebuildNetworks();
            
            Core.Logger.LogInfo($"Added {type} energy block at {position} with max power {maxPower}");
        }
        
        /// <summary>
        /// Remove an energy block from the system.
        /// </summary>
        public void RemoveEnergyBlock(Vector3Int position)
        {
            if (energyBlocks.Remove(position))
            {
                RebuildNetworks();
                Core.Logger.LogInfo($"Removed energy block at {position}");
            }
        }
        
        /// <summary>
        /// Get energy block at position.
        /// </summary>
        public EnergyBlock GetEnergyBlock(Vector3Int position)
        {
            energyBlocks.TryGetValue(position, out EnergyBlock block);
            return block;
        }
        
        /// <summary>
        /// Check if there is power available at a position.
        /// </summary>
        public bool HasPower(Vector3Int position)
        {
            if (energyBlocks.TryGetValue(position, out EnergyBlock block))
            {
                if (block.NetworkId >= 0 && block.NetworkId < powerNetworks.Count)
                {
                    PowerNetwork network = powerNetworks[block.NetworkId];
                    return network.CurrentStorage > 0 || network.TotalGeneration > 0;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Rebuild all power networks by finding connected components.
        /// </summary>
        private void RebuildNetworks()
        {
            powerNetworks.Clear();
            nextNetworkId = 0;
            
            // Reset all network IDs
            foreach (var block in energyBlocks.Values)
            {
                block.NetworkId = -1;
            }
            
            // Find connected components using flood fill
            foreach (var block in energyBlocks.Values)
            {
                if (block.NetworkId == -1)
                {
                    PowerNetwork network = new PowerNetwork(nextNetworkId++);
                    FloodFillNetwork(block, network);
                    powerNetworks.Add(network);
                }
            }
        }
        
        /// <summary>
        /// Flood fill to find all connected blocks in a network.
        /// </summary>
        private void FloodFillNetwork(EnergyBlock startBlock, PowerNetwork network)
        {
            Queue<EnergyBlock> queue = new Queue<EnergyBlock>();
            queue.Enqueue(startBlock);
            startBlock.NetworkId = network.Id;
            
            while (queue.Count > 0)
            {
                EnergyBlock current = queue.Dequeue();
                
                // Add to appropriate list in network
                switch (current.Type)
                {
                    case EnergyBlockType.Generator:
                        network.Generators.Add(current);
                        break;
                    case EnergyBlockType.Battery:
                        network.Batteries.Add(current);
                        break;
                    case EnergyBlockType.Consumer:
                        network.Consumers.Add(current);
                        break;
                    case EnergyBlockType.Conduit:
                        network.Conduits.Add(current);
                        break;
                }
                
                // Check 6 neighbors
                Vector3Int[] neighbors = new Vector3Int[]
                {
                    current.Position + Vector3Int.right,
                    current.Position + Vector3Int.left,
                    current.Position + Vector3Int.up,
                    current.Position + Vector3Int.down,
                    current.Position + Vector3Int.forward,
                    current.Position + Vector3Int.back
                };
                
                foreach (var neighborPos in neighbors)
                {
                    if (energyBlocks.TryGetValue(neighborPos, out EnergyBlock neighbor))
                    {
                        if (neighbor.NetworkId == -1)
                        {
                            neighbor.NetworkId = network.Id;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Update all power networks - calculate generation, consumption, and storage.
        /// </summary>
        private void UpdatePowerNetworks()
        {
            using (s_UpdateNetworkMarker.Auto())
            {
                foreach (var network in powerNetworks)
                {
                    UpdateNetwork(network);
                }
            }
        }
        
        /// <summary>
        /// Update a single power network.
        /// </summary>
        private void UpdateNetwork(PowerNetwork network)
        {
            using (s_CalculateFlowMarker.Auto())
            {
                // Calculate total generation
                network.TotalGeneration = 0f;
                foreach (var generator in network.Generators)
                {
                    if (generator.IsActive)
                    {
                        network.TotalGeneration += generator.MaxPower * UPDATE_INTERVAL;
                    }
                }
                
                // Calculate total consumption demand
                network.TotalConsumption = 0f;
                foreach (var consumer in network.Consumers)
                {
                    if (consumer.IsActive)
                    {
                        network.TotalConsumption += consumer.MaxPower * UPDATE_INTERVAL;
                    }
                }
                
                // Calculate total storage capacity
                network.TotalStorage = 0f;
                network.CurrentStorage = 0f;
                foreach (var battery in network.Batteries)
                {
                    network.TotalStorage += battery.MaxPower;
                    network.CurrentStorage += battery.CurrentPower;
                }
                
                // Power flow simulation
                float availablePower = network.TotalGeneration + network.CurrentStorage;
                float powerDeficit = network.TotalConsumption - network.TotalGeneration;
                
                if (powerDeficit > 0)
                {
                    // Need to draw from batteries
                    float powerFromBatteries = Mathf.Min(powerDeficit, network.CurrentStorage);
                    DistributePowerFromBatteries(network, powerFromBatteries);
                }
                else if (powerDeficit < 0)
                {
                    // Excess power, charge batteries
                    float excessPower = -powerDeficit;
                    float availableStorage = network.TotalStorage - network.CurrentStorage;
                    float powerToBatteries = Mathf.Min(excessPower, availableStorage);
                    ChargeBatteries(network, powerToBatteries);
                }
                
                // Update consumer active states based on power availability
                bool hasSufficientPower = availablePower >= network.TotalConsumption;
                foreach (var consumer in network.Consumers)
                {
                    consumer.IsActive = hasSufficientPower;
                }
            }
        }
        
        /// <summary>
        /// Distribute power draw from batteries.
        /// </summary>
        private void DistributePowerFromBatteries(PowerNetwork network, float totalPower)
        {
            if (network.Batteries.Count == 0) return;
            
            float powerPerBattery = totalPower / network.Batteries.Count;
            foreach (var battery in network.Batteries)
            {
                float drain = Mathf.Min(powerPerBattery, battery.CurrentPower);
                battery.CurrentPower -= drain;
            }
        }
        
        /// <summary>
        /// Charge batteries with excess power.
        /// </summary>
        private void ChargeBatteries(PowerNetwork network, float totalPower)
        {
            if (network.Batteries.Count == 0) return;
            
            float powerPerBattery = totalPower / network.Batteries.Count;
            foreach (var battery in network.Batteries)
            {
                float availableCapacity = battery.MaxPower - battery.CurrentPower;
                float charge = Mathf.Min(powerPerBattery, availableCapacity);
                battery.CurrentPower += charge;
            }
        }
        
        /// <summary>
        /// Get power network statistics for UI display.
        /// </summary>
        public (float generation, float consumption, float storage, float storagePercent) GetNetworkStats(Vector3Int position)
        {
            if (energyBlocks.TryGetValue(position, out EnergyBlock block))
            {
                if (block.NetworkId >= 0 && block.NetworkId < powerNetworks.Count)
                {
                    PowerNetwork network = powerNetworks[block.NetworkId];
                    float storagePercent = network.TotalStorage > 0 ? (network.CurrentStorage / network.TotalStorage) * 100f : 0f;
                    return (network.TotalGeneration, network.TotalConsumption, network.CurrentStorage, storagePercent);
                }
            }
            return (0, 0, 0, 0);
        }
        
        private void OnDestroy()
        {
            Core.ServiceLocator.Unregister<EnergyManager>();
        }
    }
}
