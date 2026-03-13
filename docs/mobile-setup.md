# Mobile Development Setup Guide

This guide helps you set up Unity for Android mobile development and build the VoxelSandbox APK.

## Prerequisites

### Required Software
1. **Unity Hub** (latest version)
2. **Unity 2022.3.17f1 LTS** (exact version required)
3. **Android SDK** (installed via Unity or Android Studio)
4. **JDK** (Java Development Kit 11+)
5. **Git** (for version control)

### Unity Modules Required
When installing Unity 2022.3.17f1, ensure these modules are selected:
- ✅ Android Build Support
  - ✅ Android SDK & NDK Tools
  - ✅ OpenJDK
- ✅ Universal Windows Platform Build Support (optional, for testing)
- ✅ Documentation (recommended)

## Step-by-Step Setup

### 1. Install Unity

1. Download and install [Unity Hub](https://unity.com/download)
2. Open Unity Hub
3. Go to **Installs** tab
4. Click **Install Editor**
5. Select **Unity 2022.3.17f1** (LTS)
6. Select required modules (see above)
7. Click **Install**

### 2. Clone Repository

```bash
# Clone the repository
git clone https://github.com/75ntvf851-cloud/voxel-sandbox.git

# Navigate to project directory
cd voxel-sandbox
```

### 3. Open Project in Unity

1. Open Unity Hub
2. Click **Open** → **Add project from disk**
3. Navigate to `voxel-sandbox` folder
4. Click **Select Folder**
5. Wait for Unity to import project (5-10 minutes first time)

### 4. Configure Android Build Settings

#### In Unity Editor:

1. **File → Build Settings**
2. Select **Android** platform
3. Click **Switch Platform** (if not already Android)
4. Wait for platform switch to complete

#### Configure Player Settings:

1. In Build Settings, click **Player Settings**
2. Navigate to **Player** tab

**Company and Product:**
- Company Name: `YourCompany`
- Product Name: `VoxelSandbox`

**Other Settings:**
- Package Name: `com.yourcompany.voxelsandbox`
- Version: `0.2.0`
- Bundle Version Code: `2` (increment for each release)
- Minimum API Level: `Android 7.0 'Nougat' (API level 24)`
- Target API Level: `Automatic (highest installed)`
- Scripting Backend: `IL2CPP`
- API Compatibility Level: `.NET Standard 2.1`
- Target Architectures: 
  - ✅ ARM64 (required for Google Play)
  - ❌ ARMv7 (optional, for older devices)

**Graphics:**
- Graphics APIs: **OpenGL ES 3** (remove Vulkan for compatibility)
- Color Space: **Linear**
- Auto Graphics API: ❌ (unchecked)

**Rendering:**
- Rendering Path: **Forward**
- Multithreaded Rendering: ✅
- Static Batching: ✅
- Dynamic Batching: ✅

### 5. Configure URP (Universal Render Pipeline)

1. **Edit → Project Settings → Graphics**
2. Scriptable Render Pipeline Settings: Select **URP-Mobile-Renderer**
3. If URP assets don't exist, create them:
   - Right-click in Project → **Create → Rendering → URP Asset (with Universal Renderer)**
   - Rename to `URP-Mobile-Renderer`
   - Configure settings:
     - Rendering Path: Forward
     - Depth Texture: ✅ On
     - Opaque Texture: ❌ Off
     - MSAA: 2x (or 4x for higher quality)
     - Render Scale: 1.0
     - Main Light: Per Pixel
     - Additional Lights: Disabled (for performance)
     - Shadows: Soft
     - Shadow Resolution: Medium
     - Shadow Distance: 50

### 6. Setup Input System

1. **Edit → Project Settings → Player → Other Settings**
2. Active Input Handling: **Both** (or **Input System Package**)
3. Install Input System package:
   - **Window → Package Manager**
   - Search: **Input System**
   - Click **Install**

4. Create Input Actions:
   - Right-click in Project → **Create → Input Actions**
   - Name: `PlayerInputActions`
   - Double-click to edit
   - Add Action Maps:
     - **Player**: Move, Look, Jump, Interact, Fly
     - **Vehicle**: Accelerate, Brake, Steer
     - **UI**: Navigate, Submit, Cancel
   - Generate C# Class
   - Save

### 7. Create Essential Scenes

#### Boot Scene
1. **File → New Scene**
2. Add GameObject: **Bootstrap**
3. Add Component: **Bootstrap** script
4. **File → Save As**: `Assets/_Project/Scenes/Boot.unity`
5. **File → Build Settings**: Add scene (index 0)

#### MainMenu Scene
1. **File → New Scene**
2. Add **Canvas** (right-click Hierarchy → UI → Canvas)
3. Add UI elements:
   - Panel: MainMenuPanel
   - Buttons: New Game, Load Game, Settings, Exit
4. Add **EventSystem** (auto-created)
5. Add **MainMenuUI** script to Canvas
6. **File → Save As**: `Assets/_Project/Scenes/MainMenu.unity`
7. **File → Build Settings**: Add scene (index 1)

#### Main (Game) Scene
1. **File → New Scene**
2. Add **Directional Light** (Sun)
3. Add GameObject: **Player**
   - Add **CharacterController**
   - Add **PlayerController** script
   - Add **Camera** as child
4. Add GameObject: **ChunkManager**
   - Add **ChunkManager** script
5. Add GameObject: **BuildingController**
   - Add **BuildingController** script
6. Add GameObject: **WaterManager**
   - Add **WaterManager** script
   - Add **WaterSimulator** script
7. Add GameObject: **EnergyManager**
   - Add **EnergyManager** script
8. Add GameObject: **DayNightCycle**
   - Add **DayNightCycle** script
   - Assign Sun Light
9. Add **Canvas** for HUD
   - Add **GameHUD** script
   - Create UI elements (energy bar, water bar, hotbar, etc.)
10. **File → Save As**: `Assets/_Project/Scenes/Main.unity`
11. **File → Build Settings**: Add scene (index 2)

### 8. Create Essential Materials

#### Voxel Material
1. Right-click Project → **Create → Material**
2. Name: `VoxelMaterial`
3. Shader: **Universal Render Pipeline/Lit**
4. Surface Type: Opaque
5. Rendering Options:
   - Receive Shadows: ✅
   - Cast Shadows: ✅
6. Save

#### Water Material (Placeholder)
1. Right-click Project → **Create → Material**
2. Name: `WaterMaterial`
3. Shader: **Universal Render Pipeline/Lit**
4. Surface Type: **Transparent**
5. Base Color: Light blue with alpha ~0.5
6. Smoothness: 0.9
7. Save

### 9. Build Android APK

#### Debug Build (for testing)
1. **File → Build Settings**
2. Platform: **Android**
3. Development Build: ✅
4. Compression Method: **LZ4**
5. Click **Build** or **Build And Run**
6. Choose output folder
7. Wait for build to complete (5-15 minutes)

#### Release Build (for distribution)
1. **File → Build Settings**
2. Development Build: ❌
3. Compression Method: **LZ4HC** (smaller size)
4. **Build Settings → Player Settings → Publishing Settings**
5. Create Keystore (first time only):
   - Keystore Manager → Create New
   - Password: (secure password)
   - Alias: voxelsandbox
   - Password: (secure password)
   - Save keystore file safely!
6. Select Keystore and Alias
7. Enter passwords
8. Click **Build**
9. Wait for build (10-20 minutes for first IL2CPP build)

### 10. Test on Device

#### Via USB Debugging
1. Enable Developer Options on Android device:
   - Settings → About Phone
   - Tap Build Number 7 times
2. Enable USB Debugging:
   - Settings → Developer Options
   - USB Debugging: ✅
3. Connect device via USB
4. In Unity: **Build And Run**
5. Unity will install and launch on device

#### Via APK File
1. Copy APK to device
2. Open file on device
3. Allow "Install from Unknown Sources" if prompted
4. Install and run

## Troubleshooting

### Build Errors

**"Unable to find Android SDK"**
- Install Android SDK via Unity Hub (Installs → Modify → Android SDK)
- Or set path: Edit → Preferences → External Tools → Android SDK

**"Unable to find JDK"**
- Install OpenJDK via Unity Hub
- Or set path: Edit → Preferences → External Tools → JDK

**"Gradle build failed"**
- Delete `Library` and `Temp` folders
- Restart Unity
- Rebuild

**"Insufficient storage"**
- Free up disk space (IL2CPP builds need 10+ GB)
- Change temp directory: Edit → Preferences → General

### Runtime Errors

**"NullReferenceException"**
- Check all serialized fields in Inspector are assigned
- Verify ServiceLocator registration in Bootstrap

**"Missing scenes"**
- Ensure all scenes are added to Build Settings
- Check scene indices match Bootstrap script

**"Black screen"**
- Check Camera is attached to Player
- Verify Main Camera tag
- Check Directional Light exists

**"Low FPS"**
- Reduce Shadow Distance in URP settings
- Disable Additional Lights
- Reduce MSAA to 2x or Off
- Lower Render Scale

## Performance Optimization

### Mobile-Specific Settings

**Quality Settings** (Edit → Project Settings → Quality):
- Create "Mobile" quality level:
  - Pixel Light Count: 1
  - Texture Quality: Full Res
  - Anisotropic Textures: Per Texture
  - Anti Aliasing: 2x Multi Sampling
  - Soft Particles: ✅
  - Realtime Reflection Probes: ❌
  - Billboards Face Camera Position: ✅
  - Shadow Resolution: Medium Resolution
  - Shadow Projection: Stable Fit
  - Shadow Distance: 50
  - Shadow Cascades: Two Cascades
  - V Sync Count: Every V Blank

**URP Settings**:
- Render Scale: 0.9 (for low-end devices)
- MSAA: 2x
- Depth Texture: Required by some shaders
- Opaque Texture: Off (expensive)
- HDR: Off (for mobile)
- Main Light: Per Pixel
- Additional Lights: Disabled

**Physics Settings** (Edit → Project Settings → Physics):
- Fixed Timestep: 0.02 (50 Hz)
- Default Contact Offset: 0.01
- Sleep Threshold: 0.005

### Code Optimizations

Implemented in code:
- ✅ Time slicing (water simulation, chunk generation)
- ✅ Profiler markers (Unity Profiler integration)
- ✅ Object pooling (TODO: implement for particles)
- ✅ Greedy meshing (reduced vertex count)
- ✅ Face culling (only visible faces)

## CI/CD Setup (GitHub Actions)

### Configure Secrets

In GitHub repository → Settings → Secrets and variables → Actions:

1. **UNITY_LICENSE**: 
   - Get from Unity Hub → Manage Licenses → Manual Activation
   - Or use Personal License file content

2. **UNITY_EMAIL**: Your Unity account email

3. **UNITY_PASSWORD**: Your Unity account password

4. **Android Keystore** (for Release builds):
   - **ANDROID_KEYSTORE_BASE64**: Base64-encoded keystore file
     ```bash
     base64 your-keystore.keystore | tr -d '\n' > keystore.txt
     ```
   - **ANDROID_KEYSTORE_PASS**: Keystore password
   - **ANDROID_KEYALIAS_NAME**: Key alias name
   - **ANDROID_KEYALIAS_PASS**: Key password

### Trigger Builds

**Automatic**:
- Push to `main` → Debug build
- Push tag `v*` → Release build + GitHub Release

**Manual**:
- Actions tab → unity-android-build → Run workflow

### Download APK

**From Actions**:
- Actions → Latest workflow run
- Artifacts section
- Download `VoxelSandbox-Android-Debug` or `VoxelSandbox-Android-Release`

**From Releases**:
- Releases tab
- Latest release
- Download APK asset

## Next Steps

After completing this setup:

1. Review [docs/handoff.md](handoff.md) for detailed implementation status
2. Check [docs/architecture.md](architecture.md) for system design
3. See [docs/controls.md](controls.md) for input reference
4. Build and test on physical Android device
5. Profile performance with Unity Profiler
6. Iterate on mobile optimizations

## Resources

- [Unity Manual - Android](https://docs.unity3d.com/Manual/android.html)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [GameCI Documentation](https://game.ci/docs)
- [Unity Profiler](https://docs.unity3d.com/Manual/Profiler.html)

---

*Last Updated: 2026-01-28*
