# Energy-Dependent Block Modules

## Overview
This document describes the energy-dependent block modules added to the VoxelSandbox project.

## New Block Types

### 1. Motor-Wheel (Мотор-Колесо)
- **BlockType**: `MotorWheel`
- **Power Consumption**: 10 kW
- **Purpose**: Provides movement and traction for vehicles
- **Use Case**: Ground-based locomotion systems

### 2. Jet Engine (Реактивный Двигатель)
- **BlockType**: `JetEngine`
- **Power Consumption**: 50 kW
- **Purpose**: Provides powerful thrust for propulsion
- **Use Case**: High-speed flight and space travel
- **Note**: Highest power consumption due to high thrust output

### 3. Rotor (Ротор)
- **BlockType**: `Rotor`
- **Power Consumption**: 5 kW
- **Purpose**: Provides rotational mechanisms
- **Use Case**: Rotating platforms, turrets, doors
- **Note**: Lowest power consumption

### 4. Piston (Поршень)
- **BlockType**: `Piston`
- **Power Consumption**: 15 kW
- **Purpose**: Provides linear extension/retraction movement
- **Use Case**: Elevators, landing gear, hangar doors

### 5. Drill (Бур)
- **BlockType**: `Drill`
- **Power Consumption**: 20 kW
- **Purpose**: Mining and block destruction
- **Use Case**: Resource gathering, tunnel boring

## API Reference

### BlockProperties Class Extensions

#### IsEnergyDependent(BlockType type)
Returns `true` if the block type requires power to operate.

```csharp
bool needsPower = BlockProperties.IsEnergyDependent(BlockType.MotorWheel);
// Returns: true
```

#### GetPowerConsumption(BlockType type)
Returns the power consumption in kilowatts (kW) for the given block type.

```csharp
float power = BlockProperties.GetPowerConsumption(BlockType.JetEngine);
// Returns: 50.0
```

## Localization

All new block types have localization support in both Russian and English:

### Russian (RU)
- `block_motor_wheel`: "Мотор-Колесо"
- `block_jet_engine`: "Реактивный Двигатель"
- `block_rotor`: "Ротор"
- `block_piston`: "Поршень"
- `block_drill`: "Бур"

### English (EN)
- `block_motor_wheel`: "Motor-Wheel"
- `block_jet_engine`: "Jet Engine"
- `block_rotor`: "Rotor"
- `block_piston`: "Piston"
- `block_drill`: "Drill"

### Additional Energy System Keys
- `energy_consumption`: "Потребление: {0} кВт" / "Consumption: {0} kW"
- `energy_status_active`: "Активен" / "Active"
- `energy_status_inactive`: "Неактивен" / "Inactive"
- `energy_status_no_power`: "Нет энергии" / "No Power"

## Power Consumption Hierarchy

From lowest to highest power consumption:
1. Rotor: 5 kW (rotation mechanisms)
2. Motor-Wheel: 10 kW (ground movement)
3. Piston: 15 kW (linear movement)
4. Drill: 20 kW (mining/destruction)
5. Jet Engine: 50 kW (high-thrust propulsion)

## Block Properties

All energy-dependent blocks share these characteristics:
- **Solid**: Yes (can be walked on)
- **Transparent**: No (not see-through)
- **Walkable**: Yes (solid surface)
- **Energy Dependent**: Yes (requires power)

## Future Expansion

### Planned Features
1. **Energy Generation Blocks**: Reactors, solar panels, batteries
2. **Energy Network**: Power distribution system between blocks
3. **Block Activation**: Toggle blocks on/off to manage power consumption
4. **Energy UI**: Display total power consumption and available power
5. **Functional Behavior**: Implement actual movement/rotation/drilling mechanics

### Integration Points
- **Physics Module**: Connect motors/pistons to physics simulation
- **Building System**: Add power connection validation
- **Save System**: Persist block activation states
- **UI Module**: Energy management interface

## Testing

Comprehensive unit tests have been added in `EnergyBlockTests.cs`:
- Energy-dependent block identification
- Power consumption validation
- Localization key mapping
- Block property verification (solid, transparent, walkable)
- Chunk storage of energy blocks
- Power consumption ordering

All tests verify that the new blocks integrate seamlessly with the existing voxel world system.

## Implementation Notes

### Design Decisions
1. **Power as Float**: Using float for power consumption allows fractional values and future balancing
2. **Enum Extension**: Adding to existing BlockType enum maintains compatibility
3. **Static Properties**: BlockProperties methods are static for performance
4. **Localization First**: All blocks have proper localization from the start

### Code Location
- Block definitions: `/Assets/_Project/Code/World/BlockType.cs`
- Localization: `/Assets/_Project/Localization/RU.json` and `EN.json`
- Tests: `/Assets/_Project/Tests/EditMode/EnergyBlockTests.cs`

---

*Document Version: 1.0*  
*Created: 2026-01-28*  
*Status: Foundation Complete - Awaiting Gameplay Implementation*
