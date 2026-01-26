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
        Steel = 4      // Structural steel
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

        public static string GetLocalizedName(BlockType type)
        {
            return type switch
            {
                BlockType.Air => "block_air",
                BlockType.Solid => "block_solid",
                BlockType.Glass => "block_glass",
                BlockType.Hull => "block_hull",
                BlockType.Steel => "block_steel",
                _ => "block_air"
            };
        }
    }
}
