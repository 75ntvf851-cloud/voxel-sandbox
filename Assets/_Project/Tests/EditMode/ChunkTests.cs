using NUnit.Framework;
using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.Tests.EditMode
{
    /// <summary>
    /// Edit mode tests for Chunk and coordinate conversions.
    /// </summary>
    public class ChunkTests
    {
        [Test]
        public void Chunk_InitializesWithCorrectSize()
        {
            var chunk = new Chunk(Vector3Int.zero);
            
            Assert.AreEqual(Vector3Int.zero, chunk.Position);
            Assert.IsNotNull(chunk.Blocks);
            Assert.AreEqual(Chunk.CHUNK_SIZE, chunk.Blocks.GetLength(0));
            Assert.AreEqual(Chunk.CHUNK_SIZE, chunk.Blocks.GetLength(1));
            Assert.AreEqual(Chunk.CHUNK_SIZE, chunk.Blocks.GetLength(2));
        }

        [Test]
        public void Chunk_CanSetAndGetBlocks()
        {
            var chunk = new Chunk(Vector3Int.zero);
            
            chunk.SetBlock(5, 10, 15, BlockType.Solid);
            Assert.AreEqual(BlockType.Solid, chunk.GetBlock(5, 10, 15));
            
            chunk.SetBlock(5, 10, 15, BlockType.Glass);
            Assert.AreEqual(BlockType.Glass, chunk.GetBlock(5, 10, 15));
        }

        [Test]
        public void Chunk_MarksDirtyOnBlockChange()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.IsDirty = false;
            
            chunk.SetBlock(0, 0, 0, BlockType.Solid);
            Assert.IsTrue(chunk.IsDirty);
        }

        [Test]
        public void Chunk_WorldToChunkPosition_ConvertsCorrectly()
        {
            Vector3Int worldPos = new Vector3Int(64, 32, 96);
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            
            Assert.AreEqual(new Vector3Int(2, 1, 3), chunkPos);
        }

        [Test]
        public void Chunk_WorldToLocalPosition_ConvertsCorrectly()
        {
            Vector3Int worldPos = new Vector3Int(35, 17, 65);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);
            
            Assert.AreEqual(new Vector3Int(3, 17, 1), localPos);
        }

        [Test]
        public void Chunk_HandlesNegativeWorldCoordinates()
        {
            Vector3Int worldPos = new Vector3Int(-5, -10, -15);
            Vector3Int chunkPos = Chunk.WorldToChunkPosition(worldPos);
            Vector3Int localPos = Chunk.WorldToLocalPosition(worldPos);
            
            Assert.AreEqual(new Vector3Int(-1, -1, -1), chunkPos);
            Assert.IsTrue(localPos.x >= 0 && localPos.x < Chunk.CHUNK_SIZE);
            Assert.IsTrue(localPos.y >= 0 && localPos.y < Chunk.CHUNK_SIZE);
            Assert.IsTrue(localPos.z >= 0 && localPos.z < Chunk.CHUNK_SIZE);
        }

        [Test]
        public void Chunk_FillSetsAllBlocks()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.Fill(BlockType.Solid);
            
            // Check a few random positions
            Assert.AreEqual(BlockType.Solid, chunk.GetBlock(0, 0, 0));
            Assert.AreEqual(BlockType.Solid, chunk.GetBlock(15, 15, 15));
            Assert.AreEqual(BlockType.Solid, chunk.GetBlock(31, 31, 31));
        }
    }
}
