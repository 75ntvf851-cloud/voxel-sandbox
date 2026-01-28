# VoxelSandbox - Performance Notes

## Overview
This document tracks performance metrics, profiling results, and optimization notes throughout development.

---

## Target Performance
- **FPS**: ≥60 on mid-range hardware (minimum 30 FPS acceptable in alpha)
- **Chunk Meshing**: <50ms per chunk rebuild
- **Water Simulation**: <5ms per frame
- **Memory**: <2GB RAM usage (moderate world size)

---

## Epic 0: Product Foundation
**Status**: Baseline established

### Metrics
- Boot time: ~2-3 seconds (initial load)
- Main menu: 60 FPS (trivial scene)
- Player controller: 60 FPS (empty scene)

### Notes
- Localization system: minimal overhead (<1ms)
- Settings load: <10ms from PlayerPrefs
- Logger: async file writes to avoid frame stalls

---

## Epic 1: Voxel World Foundation
**Status**: To be measured

### Expected Bottlenecks
1. **Greedy Meshing**: Iterating through 32x32x32 voxels (32,768 cells)
2. **Mesh Generation**: Creating Unity mesh (vertices, triangles, UVs)
3. **Collider Generation**: MeshCollider can be slow

### Optimization Strategy
- Implement greedy meshing to reduce vertex count
- Update only dirty chunks (incremental updates)
- Consider async mesh generation for large chunks (future)
- Profile with Unity Profiler

### Profiling Hooks
- Log meshing time per chunk
- Track total vertices and triangles
- Monitor GC allocations during meshing

---

## Epic 2: Building System
**Status**: To be measured

### Expected Bottlenecks
- Raycast every frame (camera → voxel world)
- Preview mesh updates

### Optimization Strategy
- Cache raycast results (no need to update every frame)
- Use simple preview mesh (low poly cube)
- Batch voxel updates if placing multiple blocks

---

## Epic 3: Water Simulation
**Status**: To be measured

### Expected Bottlenecks
1. **Simulation Algorithm**: Iterating all water cells per frame
2. **Mass Conservation**: Calculating total mass
3. **Neighbor Checks**: Reading adjacent cells (cache misses)

### Optimization Strategy
- Implement time slicing (limit to 5ms per frame)
- Only simulate active water chunks (with water > 0)
- Use fixed-point math for determinism (if needed)
- Optimize inner loops (avoid allocations)

### Profiling Hooks
- Track simulation time per frame
- Monitor active water cell count
- Log mass delta per step

---

## Epic 4: Water Rendering
**Status**: To be measured

### Expected Bottlenecks
- Mesh generation for water surface
- Overdraw from transparency
- Shader complexity

### Optimization Strategy
- Generate mesh only for surface cells (not full cubes)
- Use simple shader (avoid expensive effects in alpha)
- Update meshes incrementally (only changed chunks)
- Consider mesh batching (future)

---

## Epic 5: Water Physics
**Status**: To be measured

### Expected Bottlenecks
- Buoyancy sampling (multiple water cell lookups)
- Physics calculations per Rigidbody

### Optimization Strategy
- Limit sampling points per object (e.g., 8 points)
- Cache water lookups per frame
- Use simpler drag model

---

## Epic 6: Save/Load System
**Status**: To be measured

### Expected Bottlenecks
- JSON serialization (large data sets)
- File I/O (writing/reading chunks)

### Optimization Strategy
- Compress chunk data (gzip) if files too large
- Save asynchronously (don't block main thread)
- Show loading progress bar for large worlds

---

## Profiling Workflow

### Unity Profiler
1. Window → Analysis → Profiler
2. Record gameplay session
3. Identify spikes in CPU/GPU
4. Deep dive into expensive methods

### Custom Timers
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// Code to measure
sw.Stop();
Logger.Log($"Operation took {sw.ElapsedMilliseconds}ms");
```

### Frame Debugger
- Window → Analysis → Frame Debugger
- Analyze draw calls and shader usage

---

## Optimization Checklist

- [ ] Profile with Unity Profiler after each Epic
- [ ] Measure FPS in typical gameplay scenarios
- [ ] Check memory usage (Profiler → Memory)
- [ ] Validate no GC spikes during gameplay
- [ ] Ensure update methods optimized (avoid allocations)
- [ ] Use object pooling for frequent allocations (if needed)
- [ ] Test on target hardware (mid-range PC)

---

## Known Issues / Future Work

- **Chunk Loading**: May stutter when generating many chunks at once
  - Solution: Async generation, spread over frames
- **Water Simulation**: May slow down with thousands of active cells
  - Solution: Further optimize algorithm, reduce update rate
- **Save Files**: May become large (MB range)
  - Solution: Compression, binary format

---

## Hardware Baselines

### Development Machine
- **CPU**: [To be filled]
- **GPU**: [To be filled]
- **RAM**: [To be filled]
- **FPS**: [To be filled after testing]

### Minimum Spec (Target)
- **CPU**: Intel Core i5 / AMD Ryzen 5 (4+ cores)
- **GPU**: NVIDIA GTX 1050 / AMD RX 560 (2GB VRAM)
- **RAM**: 8GB
- **FPS**: ≥30

### Recommended Spec
- **CPU**: Intel Core i7 / AMD Ryzen 7 (6+ cores)
- **GPU**: NVIDIA GTX 1660 / AMD RX 580 (4GB VRAM)
- **RAM**: 16GB
- **FPS**: ≥60

---

*Document Version: 1.0*  
*Last Updated: 2026-01-26*
