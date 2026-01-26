# VoxelSandbox - Detailed Roadmap

## Project Overview
**Goal**: Build a product-grade pre-alpha of a Space Engineers-like voxel sandbox with water simulation, grid building, and save/load functionality.

**Timeline**: Sequential Epic-based development (Epic 0 → Epic 6)

**Tech Stack**: Unity LTS (URP), C#, New Input System

**Languages**: Russian (default) + English with runtime switching

---

## Risk Assessment

### High Risk Areas
1. **Greedy Meshing Performance**
   - Risk: Complex algorithm may have bugs or performance issues
   - Mitigation: Implement incrementally, profile early, use debug visualization
   - Fallback: Start with naive meshing, optimize later

2. **Water Simulation Stability**
   - Risk: Cellular automata may have mass conservation or stability issues
   - Mitigation: Implement step-by-step with unit tests, add debug tools
   - Fallback: Simplify to static water levels initially

3. **Save/Load Complexity**
   - Risk: Corrupted saves, version incompatibility, large file sizes
   - Mitigation: Design simple format v1, add validation, compression later
   - Fallback: Save only essential data (voxels + water)

4. **Performance with Large Worlds**
   - Risk: Too many chunks, water sim too slow, meshing bottlenecks
   - Mitigation: Profile early, add budgeting, limit world size initially
   - Fallback: Reduce default chunk size or world bounds

5. **Localization Complexity**
   - Risk: Missing keys, formatting issues, UI not updating live
   - Mitigation: Centralized system, fallback handling, comprehensive keys
   - Fallback: Hard-coded bilingual strings (not ideal but works)

### Medium Risk Areas
6. **CI/CD Setup**: GitHub Actions may need Unity license configuration
7. **Water Physics**: Buoyancy calculation complexity
8. **Blueprint System**: Region selection and serialization edge cases

### Low Risk Areas
9. **Player Controller**: Standard FPS controller
10. **Settings System**: Simple key-value storage
11. **Main Menu**: Standard UI implementation

---

## Epic Breakdown

### EPIC 0: Product Foundation
**Goal**: Ship-ready skeleton with localization, settings, logging, and basic player controller  
**Estimated Complexity**: Medium (3-4 hours)

**Tasks**: Project setup, Assembly definitions, Localization system, Settings system, Logging, Boot scene, Main menu, Debug overlay, Player controller, CI setup

**Definition of Done**:
- ✓ Boot scene loads without errors
- ✓ Main menu displays in Russian by default
- ✓ Language can be switched to English at runtime
- ✓ Player can move, look, jump, fly in empty scene
- ✓ Debug overlay shows FPS and info
- ✓ All tests pass
- ✓ CI builds successfully

---

### EPIC 1: Voxel World Foundation
**Goal**: Chunked voxel world with Greedy Meshing  
**Estimated Complexity**: High (5-6 hours)

**Tasks**: Voxel data structures, Chunk manager, Greedy meshing algorithm, Chunk rendering, Terrain generation, Debug tools, Testing

**Definition of Done**:
- ✓ Main.unity shows voxel terrain
- ✓ Greedy meshing generates efficient mesh
- ✓ Voxel modifications update mesh incrementally
- ✓ Tests pass

---

### EPIC 2: Building System
**Goal**: Space Engineers-like grid building  
**Estimated Complexity**: Medium-High (4-5 hours)

**Tasks**: Building modes, Raycast targeting, Block preview, Placement rules, Block removal, Block palette UI, Blueprint system

**Definition of Done**:
- ✓ Can place and remove blocks
- ✓ Block preview functional
- ✓ Rotation works
- ✓ Blueprints work
- ✓ Tests pass

---

### EPIC 3: Water Simulation
**Goal**: Stable, mass-conserving cellular water simulation  
**Estimated Complexity**: High (5-6 hours)

**Tasks**: Water data structure, Water manager, Simulation algorithm, Mass conservation, Voxel interaction, Performance budgeting, Debug tools, Test scene

**Definition of Done**:
- ✓ Water flows realistically
- ✓ Mass conserved
- ✓ Tests pass

---

### EPIC 4: Water Rendering
**Goal**: Surface mesh generation for water  
**Estimated Complexity**: Medium (3-4 hours)

**Tasks**: Surface detection, Mesh generation, URP water shader, Incremental updates

**Definition of Done**:
- ✓ Water renders with transparency
- ✓ Mesh updates incrementally

---

### EPIC 5: Water Physics
**Goal**: Buoyancy and drag  
**Estimated Complexity**: Medium (3-4 hours)

**Tasks**: Buoyancy component, Drag calculation, Player water interaction, Demo setup

**Definition of Done**:
- ✓ Objects float
- ✓ Tests pass

---

### EPIC 6: Save/Load System
**Goal**: Persistent world state  
**Estimated Complexity**: Medium-High (4-5 hours)

**Tasks**: Save format design, Serialization, Deserialization, Save/Load UI, File management

**Definition of Done**:
- ✓ Can save and load
- ✓ State restores correctly
- ✓ Tests pass

---

## Commit Strategy
Use labeled commits: `Epic0: <description>`, `Epic1: <description>`, etc.

---

*Document Version: 1.0*  
*Last Updated: 2026-01-26*
