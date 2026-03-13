using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;

namespace VoxelSandbox.World
{
    /// <summary>
    /// Implements greedy meshing algorithm for efficient voxel rendering.
    /// Combines adjacent voxel faces of the same type into larger quads.
    /// Mobile-optimized with profiling markers.
    /// </summary>
    public class GreedyMesher
    {
        private static readonly ProfilerMarker s_GenerateMeshMarker = new ProfilerMarker("GreedyMesher.GenerateMesh");
        private static readonly ProfilerMarker s_ProcessSliceMarker = new ProfilerMarker("GreedyMesher.ProcessSlice");
        
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Color> colors = new List<Color>();
        
        private readonly int chunkSize;
        
        /// <summary>
        /// Initialize greedy mesher for specific chunk size.
        /// </summary>
        /// <param name="chunkSize">Size of chunk dimension (e.g., 32 for 32x32x32)</param>
        public GreedyMesher(int chunkSize)
        {
            this.chunkSize = chunkSize;
        }
        
        /// <summary>
        /// Generate optimized mesh data for a chunk using greedy meshing.
        /// </summary>
        /// <param name="chunk">Chunk to generate mesh for</param>
        /// <param name="chunkManager">Manager to check neighbor chunks</param>
        /// <returns>Generated mesh</returns>
        public Mesh GenerateMesh(Chunk chunk, ChunkManager chunkManager)
        {
            using (s_GenerateMeshMarker.Auto())
            {
                vertices.Clear();
                triangles.Clear();
                uvs.Clear();
                normals.Clear();
                colors.Clear();
                
                // Process all 6 directions
                ProcessDirection(chunk, chunkManager, Vector3Int.right, Vector3Int.up, Vector3Int.forward);   // +X
                ProcessDirection(chunk, chunkManager, Vector3Int.left, Vector3Int.up, Vector3Int.back);       // -X
                ProcessDirection(chunk, chunkManager, Vector3Int.forward, Vector3Int.up, Vector3Int.right);   // +Z
                ProcessDirection(chunk, chunkManager, Vector3Int.back, Vector3Int.up, Vector3Int.left);       // -Z
                ProcessDirection(chunk, chunkManager, Vector3Int.up, Vector3Int.forward, Vector3Int.right);   // +Y
                ProcessDirection(chunk, chunkManager, Vector3Int.down, Vector3Int.back, Vector3Int.right);    // -Y
                
                Mesh mesh = new Mesh();
                mesh.name = $"Chunk_{chunk.ChunkPosition.x}_{chunk.ChunkPosition.y}_{chunk.ChunkPosition.z}";
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.SetNormals(normals);
                mesh.SetColors(colors);
                
                // Optimize for mobile
                mesh.Optimize();
                
                return mesh;
            }
        }
        
        private void ProcessDirection(Chunk chunk, ChunkManager chunkManager, Vector3Int normal, Vector3Int axisU, Vector3Int axisV)
        {
            using (s_ProcessSliceMarker.Auto())
            {
                // Determine which axis we're processing
                int normalAxis = GetPrimaryAxis(normal);
                int uAxis = GetPrimaryAxis(axisU);
                int vAxis = GetPrimaryAxis(axisV);
                
                // Mask to track processed faces
                bool[,] mask = new bool[chunkSize, chunkSize];
                BlockType[,] blockMask = new BlockType[chunkSize, chunkSize];
                
                // Iterate through each slice along the normal direction
                for (int d = 0; d < chunkSize; d++)
                {
                    // Clear mask
                    System.Array.Clear(mask, 0, mask.Length);
                    
                    // Build mask for this slice
                    for (int u = 0; u < chunkSize; u++)
                    {
                        for (int v = 0; v < chunkSize; v++)
                        {
                            Vector3Int pos = GetPosition(normalAxis, d, uAxis, u, vAxis, v);
                            Vector3Int neighborPos = pos + normal;
                            
                            BlockType currentBlock = chunk.GetBlock(pos);
                            BlockType neighborBlock = GetBlockAt(chunk, chunkManager, neighborPos);
                            
                            // Face is visible if current is solid and neighbor is air/transparent
                            if (currentBlock != BlockType.Air && 
                                currentBlock.IsSolid() && 
                                (neighborBlock == BlockType.Air || !neighborBlock.IsSolid()))
                            {
                                mask[u, v] = true;
                                blockMask[u, v] = currentBlock;
                            }
                        }
                    }
                    
                    // Greedy mesh the mask
                    for (int u = 0; u < chunkSize; u++)
                    {
                        for (int v = 0; v < chunkSize; v++)
                        {
                            if (!mask[u, v]) continue;
                            
                            BlockType blockType = blockMask[u, v];
                            
                            // Compute width
                            int width = 1;
                            while (u + width < chunkSize && 
                                   mask[u + width, v] && 
                                   blockMask[u + width, v] == blockType)
                            {
                                width++;
                            }
                            
                            // Compute height
                            int height = 1;
                            bool done = false;
                            while (v + height < chunkSize && !done)
                            {
                                for (int k = 0; k < width; k++)
                                {
                                    if (!mask[u + k, v + height] || blockMask[u + k, v + height] != blockType)
                                    {
                                        done = true;
                                        break;
                                    }
                                }
                                if (!done) height++;
                            }
                            
                            // Create quad
                            Vector3Int pos = GetPosition(normalAxis, d, uAxis, u, vAxis, v);
                            AddQuad(pos, normal, axisU, axisV, width, height, blockType);
                            
                            // Clear processed faces from mask
                            for (int du = 0; du < width; du++)
                            {
                                for (int dv = 0; dv < height; dv++)
                                {
                                    mask[u + du, v + dv] = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void AddQuad(Vector3Int pos, Vector3Int normal, Vector3Int axisU, Vector3Int axisV, int width, int height, BlockType blockType)
        {
            // Offset position if normal is negative
            Vector3 basePos = pos;
            if (normal.x < 0 || normal.y < 0 || normal.z < 0)
            {
                basePos = pos + Vector3Int.one;
            }
            
            // Calculate quad corners
            Vector3 v0 = basePos + normal * 0.5f;
            Vector3 v1 = v0 + (Vector3)axisU * width;
            Vector3 v2 = v0 + (Vector3)axisV * height;
            Vector3 v3 = v1 + (Vector3)axisV * height;
            
            // Add vertices
            int vertIndex = vertices.Count;
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            
            // Add triangles
            triangles.Add(vertIndex);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 1);
            
            triangles.Add(vertIndex + 1);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 3);
            
            // Add UVs
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(width, 0));
            uvs.Add(new Vector2(0, height));
            uvs.Add(new Vector2(width, height));
            
            // Add normals
            Vector3 normalVec = normal;
            for (int i = 0; i < 4; i++)
            {
                normals.Add(normalVec);
            }
            
            // Add colors based on block type (for vertex coloring)
            Color blockColor = GetBlockColor(blockType);
            for (int i = 0; i < 4; i++)
            {
                colors.Add(blockColor);
            }
        }
        
        private BlockType GetBlockAt(Chunk chunk, ChunkManager chunkManager, Vector3Int localPos)
        {
            // Check if position is within chunk
            if (localPos.x >= 0 && localPos.x < chunkSize &&
                localPos.y >= 0 && localPos.y < chunkSize &&
                localPos.z >= 0 && localPos.z < chunkSize)
            {
                return chunk.GetBlock(localPos);
            }
            
            // Position is in neighboring chunk
            Vector3Int worldPos = chunk.ChunkPosition * chunkSize + localPos;
            return chunkManager.GetBlock(worldPos);
        }
        
        private int GetPrimaryAxis(Vector3Int vec)
        {
            if (vec.x != 0) return 0;
            if (vec.y != 0) return 1;
            return 2;
        }
        
        private Vector3Int GetPosition(int normalAxis, int d, int uAxis, int u, int vAxis, int v)
        {
            Vector3Int pos = Vector3Int.zero;
            pos[normalAxis] = d;
            pos[uAxis] = u;
            pos[vAxis] = v;
            return pos;
        }
        
        private Color GetBlockColor(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Solid: return new Color(0.6f, 0.6f, 0.6f);
                case BlockType.Glass: return new Color(0.8f, 0.9f, 1.0f, 0.5f);
                case BlockType.Hull: return new Color(0.4f, 0.45f, 0.5f);
                case BlockType.Steel: return new Color(0.7f, 0.7f, 0.75f);
                default: return Color.white;
            }
        }
    }
}
