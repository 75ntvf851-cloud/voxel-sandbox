# VoxelSandbox - Architecture

## Module Dependency Graph

```
Core (lowest level - no dependencies)
  ↑
  ├─── Player
  ├─── UI → Core
  ├─── World → Core
  ├─── Building → Core, World
  ├─── Water → Core, World
  ├─── Physics → Core, World, Water
  ├─── SaveSystem → Core, World, Water, Building
  └─── Debug → Core
```

## Assembly Definitions

### VoxelSandbox.Core
**Path**: `/Assets/_Project/Code/Core`  
**Dependencies**: None  
**Purpose**: Core services and utilities
- LocalizationService
- SettingsService
- Logger
- ServiceLocator
- Event system
- Utility classes

### VoxelSandbox.Player
**Path**: `/Assets/_Project/Code/Player`  
**Dependencies**: Core  
**Purpose**: Player controller and input
- PlayerController
- CameraController
- InputHandler
- PlayerState

### VoxelSandbox.UI
**Path**: `/Assets/_Project/Code/UI`  
**Dependencies**: Core  
**Purpose**: UI components
- MainMenuUI
- SettingsUI
- DebugOverlayUI
- LocalizedText component
- UI utilities

### VoxelSandbox.World
**Path**: `/Assets/_Project/Code/World`  
**Dependencies**: Core  
**Purpose**: Voxel world management
- ChunkManager
- Chunk
- BlockType
- VoxelData
- GreedyMesher
- TerrainGenerator

### VoxelSandbox.Building
**Path**: `/Assets/_Project/Code/Building`  
**Dependencies**: Core, World  
**Purpose**: Building system
- BuildingController
- BuildingMode
- BlockPlacer
- BlockPreview
- BlueprintSystem

### VoxelSandbox.Water
**Path**: `/Assets/_Project/Code/Water`  
**Dependencies**: Core, World  
**Purpose**: Water simulation and rendering
- WaterManager
- WaterChunk
- WaterSimulator
- WaterMeshBuilder
- WaterDebugTools

### VoxelSandbox.Physics
**Path**: `/Assets/_Project/Code/Physics`  
**Dependencies**: Core, World, Water  
**Purpose**: Physics interactions
- BuoyancyComponent
- WaterDrag
- PhysicsHelpers

### VoxelSandbox.SaveSystem
**Path**: `/Assets/_Project/Code/SaveSystem`  
**Dependencies**: Core, World, Water, Building  
**Purpose**: Save/Load functionality
- SaveManager
- SaveData
- SaveSerializer
- SaveFileManager

### VoxelSandbox.Debug
**Path**: `/Assets/_Project/Code/Debug`  
**Dependencies**: Core  
**Purpose**: Debug tools and profiling
- DebugOverlay
- ProfilingHooks
- ChunkDebugVisualizer
- WaterDebugVisualizer

---

## Service Initialization Order

1. **Logger** - First, so all services can log
2. **LocalizationService** - Load language data
3. **SettingsService** - Load user settings
4. **ChunkManager** - Initialize voxel world
5. **WaterManager** - Initialize water simulation
6. **BuildingController** - Initialize building tools
7. **SaveManager** - Ready for save/load

---

## Scene Flow

```
Boot.unity
  ↓ (Initialize services)
MainMenu.unity
  ↓ (New Game or Load Game)
Main.unity (Gameplay)
  - Player controller active
  - Voxel world rendered
  - Building system active
  - Water simulation running
  - Debug overlay available
```

---

## Data Flow

### Voxel Modification
```
User Input → BuildingController → ChunkManager.SetVoxel()
  → Chunk.SetVoxel() → Mark chunk dirty
  → Update mesh (Greedy Meshing)
  → Update collider
  → Notify WaterManager (displacement)
```

### Water Simulation
```
WaterManager.SimulationStep()
  → For each active water chunk:
    → Calculate flow (down, sideways, up)
    → Update water amounts
    → Check mass conservation
  → Mark dirty chunks
  → WaterMeshBuilder.UpdateMesh() (render)
```

### Save/Load
```
Save: GameState → SaveManager.Save()
  → Serialize ChunkManager data
  → Serialize WaterManager data
  → Serialize entities
  → Write to file (JSON)

Load: SaveManager.Load() → Parse JSON
  → Restore ChunkManager chunks
  → Restore WaterManager water
  → Restore entities
  → Rebuild meshes
```

---

## Performance Considerations

1. **Greedy Meshing**: Reduces vertex count by merging faces
2. **Incremental Updates**: Only rebuild affected chunks
3. **Water Time Slicing**: Limit simulation per frame
4. **Chunk Culling**: Only render visible chunks (future)
5. **LOD**: Different mesh detail based on distance (future)

---

## Extensibility Points

- **New Block Types**: Add to BlockType enum, update meshing UVs
- **New Languages**: Add JSON file, update LocalizationService
- **New Building Modes**: Extend BuildingMode enum, add handlers
- **Water Enhancements**: Modify WaterSimulator algorithm
- **Save Format Versions**: Add migration logic in SaveSerializer

---

*Document Version: 1.0*  
*Last Updated: 2026-01-26*
