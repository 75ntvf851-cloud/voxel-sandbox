using NUnit.Framework;
using VoxelSandbox.World;

namespace VoxelSandbox.Tests.EditMode
{
    /// <summary>
    /// Edit mode tests for energy-dependent block properties.
    /// </summary>
    public class EnergyBlockTests
    {
        [Test]
        public void BlockProperties_IdentifiesEnergyDependentBlocks()
        {
            // Energy-dependent blocks
            Assert.IsTrue(BlockProperties.IsEnergyDependent(BlockType.MotorWheel));
            Assert.IsTrue(BlockProperties.IsEnergyDependent(BlockType.JetEngine));
            Assert.IsTrue(BlockProperties.IsEnergyDependent(BlockType.Rotor));
            Assert.IsTrue(BlockProperties.IsEnergyDependent(BlockType.Piston));
            Assert.IsTrue(BlockProperties.IsEnergyDependent(BlockType.Drill));
            
            // Non-energy-dependent blocks
            Assert.IsFalse(BlockProperties.IsEnergyDependent(BlockType.Air));
            Assert.IsFalse(BlockProperties.IsEnergyDependent(BlockType.Solid));
            Assert.IsFalse(BlockProperties.IsEnergyDependent(BlockType.Glass));
            Assert.IsFalse(BlockProperties.IsEnergyDependent(BlockType.Hull));
            Assert.IsFalse(BlockProperties.IsEnergyDependent(BlockType.Steel));
        }

        [Test]
        public void BlockProperties_ReturnsCorrectPowerConsumption()
        {
            Assert.AreEqual(10f, BlockProperties.GetPowerConsumption(BlockType.MotorWheel));
            Assert.AreEqual(50f, BlockProperties.GetPowerConsumption(BlockType.JetEngine));
            Assert.AreEqual(5f, BlockProperties.GetPowerConsumption(BlockType.Rotor));
            Assert.AreEqual(15f, BlockProperties.GetPowerConsumption(BlockType.Piston));
            Assert.AreEqual(20f, BlockProperties.GetPowerConsumption(BlockType.Drill));
            
            // Non-energy blocks should return 0
            Assert.AreEqual(0f, BlockProperties.GetPowerConsumption(BlockType.Air));
            Assert.AreEqual(0f, BlockProperties.GetPowerConsumption(BlockType.Solid));
        }

        [Test]
        public void BlockProperties_ReturnsLocalizationKeysForEnergyBlocks()
        {
            Assert.AreEqual("block_motor_wheel", BlockProperties.GetLocalizedName(BlockType.MotorWheel));
            Assert.AreEqual("block_jet_engine", BlockProperties.GetLocalizedName(BlockType.JetEngine));
            Assert.AreEqual("block_rotor", BlockProperties.GetLocalizedName(BlockType.Rotor));
            Assert.AreEqual("block_piston", BlockProperties.GetLocalizedName(BlockType.Piston));
            Assert.AreEqual("block_drill", BlockProperties.GetLocalizedName(BlockType.Drill));
        }

        [Test]
        public void EnergyBlocks_AreSolid()
        {
            // All energy-dependent blocks should be solid
            Assert.IsTrue(BlockProperties.IsSolid(BlockType.MotorWheel));
            Assert.IsTrue(BlockProperties.IsSolid(BlockType.JetEngine));
            Assert.IsTrue(BlockProperties.IsSolid(BlockType.Rotor));
            Assert.IsTrue(BlockProperties.IsSolid(BlockType.Piston));
            Assert.IsTrue(BlockProperties.IsSolid(BlockType.Drill));
        }

        [Test]
        public void EnergyBlocks_AreNotTransparent()
        {
            // Energy-dependent blocks should not be transparent
            Assert.IsFalse(BlockProperties.IsTransparent(BlockType.MotorWheel));
            Assert.IsFalse(BlockProperties.IsTransparent(BlockType.JetEngine));
            Assert.IsFalse(BlockProperties.IsTransparent(BlockType.Rotor));
            Assert.IsFalse(BlockProperties.IsTransparent(BlockType.Piston));
            Assert.IsFalse(BlockProperties.IsTransparent(BlockType.Drill));
        }

        [Test]
        public void EnergyBlocks_AreWalkable()
        {
            // Energy-dependent blocks should be walkable (solid)
            Assert.IsTrue(BlockProperties.IsWalkable(BlockType.MotorWheel));
            Assert.IsTrue(BlockProperties.IsWalkable(BlockType.JetEngine));
            Assert.IsTrue(BlockProperties.IsWalkable(BlockType.Rotor));
            Assert.IsTrue(BlockProperties.IsWalkable(BlockType.Piston));
            Assert.IsTrue(BlockProperties.IsWalkable(BlockType.Drill));
        }

        [Test]
        public void Chunk_CanSetAndGetEnergyBlocks()
        {
            var chunk = new Chunk(UnityEngine.Vector3Int.zero);
            
            // Test setting and getting each energy block type
            chunk.SetBlock(0, 0, 0, BlockType.MotorWheel);
            Assert.AreEqual(BlockType.MotorWheel, chunk.GetBlock(0, 0, 0));
            
            chunk.SetBlock(1, 1, 1, BlockType.JetEngine);
            Assert.AreEqual(BlockType.JetEngine, chunk.GetBlock(1, 1, 1));
            
            chunk.SetBlock(2, 2, 2, BlockType.Rotor);
            Assert.AreEqual(BlockType.Rotor, chunk.GetBlock(2, 2, 2));
            
            chunk.SetBlock(3, 3, 3, BlockType.Piston);
            Assert.AreEqual(BlockType.Piston, chunk.GetBlock(3, 3, 3));
            
            chunk.SetBlock(4, 4, 4, BlockType.Drill);
            Assert.AreEqual(BlockType.Drill, chunk.GetBlock(4, 4, 4));
        }

        [Test]
        public void PowerConsumption_OrderedCorrectly()
        {
            // Verify power consumption ordering (for game balance)
            // Rotor should consume least power
            Assert.Less(BlockProperties.GetPowerConsumption(BlockType.Rotor), 
                       BlockProperties.GetPowerConsumption(BlockType.MotorWheel));
            
            // JetEngine should consume most power
            Assert.Greater(BlockProperties.GetPowerConsumption(BlockType.JetEngine), 
                          BlockProperties.GetPowerConsumption(BlockType.Drill));
            Assert.Greater(BlockProperties.GetPowerConsumption(BlockType.JetEngine), 
                          BlockProperties.GetPowerConsumption(BlockType.Piston));
        }
    }
}
