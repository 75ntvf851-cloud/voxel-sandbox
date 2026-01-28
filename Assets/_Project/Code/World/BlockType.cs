using System;

namespace VoxelSandbox.World
{
    /// <summary>
    /// Block types in the voxel world.
    /// </summary>
    public enum BlockType : byte
    {
        Air = 0,
        Solid = 1,     // Stone
        Glass = 2,     // Transparent block
        Hull = 3,      // Ship hull / metal
        Steel = 4,     // Structural steel
        
        // Energy-dependent modules
        MotorWheel = 5,    // Motor-wheel for movement
        JetEngine = 6,     // Jet engine for propulsion
        Rotor = 7,         // Rotor for rotation mechanisms
        Piston = 8,        // Piston for linear movement
        Drill = 9          // Drill for mining/destruction
    }

    /// <summary>
    /// Block metadata and properties.
    /// </summary>
    public static class BlockProperties
    {
        public static bool IsSolid(BlockType type)
        {
            return type != BlockType.Air;
        }

        public static bool IsTransparent(BlockType type)
        {
            return type == BlockType.Air || type == BlockType.Glass;
        }

        public static bool IsWalkable(BlockType type)
        {
            return IsSolid(type);
        }

        public static bool IsEnergyDependent(BlockType type)
        {
            return type == BlockType.MotorWheel || 
                   type == BlockType.JetEngine || 
                   type == BlockType.Rotor || 
                   type == BlockType.Piston || 
                   type == BlockType.Drill;
        }

        public static float GetPowerConsumption(BlockType type)
        {
            return type switch
            {
                BlockType.MotorWheel => 10f,  // kW
                BlockType.JetEngine => 50f,   // kW
                BlockType.Rotor => 5f,        // kW
                BlockType.Piston => 15f,      // kW
                BlockType.Drill => 20f,       // kW
                _ => 0f
            };
        }

        public static string GetLocalizedName(BlockType type)
        {
            return type switch
            {
                BlockType.Air => "block_air",
                BlockType.Solid => "block_solid",
                BlockType.Glass => "block_glass",
                BlockType.Hull => "block_hull",
                BlockType.Steel => "block_steel",
                BlockType.MotorWheel => "block_motor_wheel",
                BlockType.JetEngine => "block_jet_engine",
                BlockType.Rotor => "block_rotor",
                BlockType.Piston => "block_piston",
                BlockType.Drill => "block_drill",
                _ => "block_air"
            };
        }
    }
}
