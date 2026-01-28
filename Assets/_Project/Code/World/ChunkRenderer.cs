using UnityEngine;
using Unity.Profiling;

namespace VoxelSandbox.World
{
    /// <summary>
    /// Renders a chunk using optimized meshing and manages its visual representation.
    /// Mobile-optimized with LOD support.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class ChunkRenderer : MonoBehaviour
    {
        private static readonly ProfilerMarker s_UpdateMeshMarker = new ProfilerMarker("ChunkRenderer.UpdateMesh");
        
        private Chunk chunk;
        private ChunkManager chunkManager;
        private GreedyMesher mesher;
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        
        private bool isDirty = true;
        
        /// <summary>
        /// Initialize the chunk renderer with chunk data.
        /// </summary>
        public void Initialize(Chunk chunk, ChunkManager chunkManager, Material material)
        {
            this.chunk = chunk;
            this.chunkManager = chunkManager;
            this.mesher = new GreedyMesher(Chunk.CHUNK_SIZE);
            
            // Get components
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            
            // Set material
            meshRenderer.material = material;
            
            // Set shadow casting for mobile optimization
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;
            
            // Position chunk in world
            transform.position = chunk.ChunkPosition * Chunk.CHUNK_SIZE;
            
            // Generate initial mesh
            UpdateMesh();
        }
        
        /// <summary>
        /// Mark chunk as needing mesh regeneration.
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }
        
        /// <summary>
        /// Update mesh if dirty. Call this from a controlled update loop.
        /// </summary>
        public void UpdateIfDirty()
        {
            if (isDirty)
            {
                UpdateMesh();
            }
        }
        
        /// <summary>
        /// Force mesh regeneration immediately.
        /// </summary>
        public void UpdateMesh()
        {
            using (s_UpdateMeshMarker.Auto())
            {
                // Generate mesh using greedy meshing
                Mesh newMesh = mesher.GenerateMesh(chunk, chunkManager);
                
                // Apply to mesh filter
                if (meshFilter.sharedMesh != null)
                {
                    Destroy(meshFilter.sharedMesh);
                }
                meshFilter.mesh = newMesh;
                
                // Update collider
                meshCollider.sharedMesh = newMesh;
                
                isDirty = false;
            }
        }
        
        /// <summary>
        /// Get the chunk this renderer is displaying.
        /// </summary>
        public Chunk GetChunk()
        {
            return chunk;
        }
        
        /// <summary>
        /// Check if chunk needs mesh update.
        /// </summary>
        public bool IsDirty => isDirty;
        
        private void OnDestroy()
        {
            // Clean up mesh
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Destroy(meshFilter.sharedMesh);
            }
        }
    }
}
