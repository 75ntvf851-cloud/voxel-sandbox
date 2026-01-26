# VoxelSandbox - Development Setup

## Unity Version
**Required**: Unity 2022.3 LTS (or latest available LTS)  
**Rendering**: Universal Render Pipeline (URP)

> **Note**: The exact Unity version used will be documented here after project creation.

---

## Prerequisites

1. **Unity Hub** (latest version)
2. **Unity Editor** (2022.3 LTS recommended)
3. **Git** (for version control)
4. **Code Editor** (Visual Studio, VS Code, or Rider recommended)

---

## Setup Steps

### 1. Clone Repository
```bash
git clone https://github.com/75ntvf851-cloud/voxel-sandbox.git
cd voxel-sandbox
```

### 2. Open in Unity
1. Launch Unity Hub
2. Click "Open" or "Add"
3. Navigate to the cloned repository folder
4. Select the project root (containing `Assets/`, `ProjectSettings/`)
5. Unity will import and compile the project (may take a few minutes)

### 3. Verify Setup
- Open `Boot.unity` scene from `Assets/_Project/Scenes/`
- Click Play - should see boot sequence and transition to main menu
- No console errors should appear

---

## Project Structure

```
/Assets
  /_Project
    /Code           (C# scripts with asmdefs)
    /Scenes         (Boot, Main, Test_Water)
    /Prefabs        (Reusable game objects)
    /Materials      (URP materials)
    /Shaders        (Custom shaders)
    /Settings       (URP settings, input maps)
    /Localization   (RU.json, EN.json)
/docs               (Documentation)
/ProjectSettings    (Unity project config)
/Packages           (Package manifest)
```

---

## Building the Project

### In Editor
1. File → Build Settings
2. Select Windows x64 (or your target platform)
3. Click "Build"

### Via Command Line (for CI)
```bash
# Example (adjust paths for your Unity installation)
/path/to/Unity -quit -batchmode -projectPath . \
  -buildTarget Win64 -buildWindows64Player ./build/VoxelSandbox.exe
```

---

## Running Tests

### Edit Mode Tests
- Window → General → Test Runner
- Select "EditMode" tab
- Click "Run All"

### Play Mode Tests
- Window → General → Test Runner
- Select "PlayMode" tab
- Click "Run All" (will enter play mode)

### Via Command Line (for CI)
```bash
# Edit mode tests
/path/to/Unity -runTests -batchmode -projectPath . \
  -testPlatform EditMode -testResults results-editmode.xml

# Play mode tests
/path/to/Unity -runTests -batchmode -projectPath . \
  -testPlatform PlayMode -testResults results-playmode.xml
```

---

## Configuration

### Localization
- Default language: **Russian**
- Switch to English: In-game Settings menu or press **F9**
- Files: `Assets/_Project/Localization/RU.json`, `EN.json`

### Settings
- Stored in: `PlayerPrefs` (platform-specific location)
- Reset settings: Delete PlayerPrefs keys or use in-game reset option

### Logs
- Location: `Application.persistentDataPath/logs/`
- Windows: `C:\Users\<User>\AppData\LocalLow\<Company>\VoxelSandbox\logs\`
- Rotation: Keeps last 5 log files

---

## Common Issues

### Issue: Unity version mismatch
**Solution**: Use Unity Hub to install the exact LTS version specified above.

### Issue: Missing packages
**Solution**: Unity should auto-resolve. If not, check `Packages/manifest.json` and ensure URP packages are listed.

### Issue: Compilation errors on open
**Solution**: Wait for Unity to finish importing. Check console for specific errors.

### Issue: Input not working
**Solution**: Ensure New Input System is enabled in Project Settings → Player → Active Input Handling.

---

## Development Workflow

1. **Pull latest code**: `git pull origin main`
2. **Create feature branch**: `git checkout -b feature/my-feature`
3. **Make changes** in Unity or code editor
4. **Test locally**: Run Edit Mode and Play Mode tests
5. **Commit changes**: `git add . && git commit -m "Epic0: Description"`
6. **Push to remote**: `git push origin feature/my-feature`
7. **Create Pull Request** on GitHub
8. **CI runs tests and build** - ensure it passes

---

## Useful Shortcuts

- **F3**: Toggle debug overlay
- **F9**: Toggle language (RU ↔ EN)
- **Esc**: Pause game
- **1, 2, 3**: Building modes (Build, Remove, Inspect)
- **R**: Rotate block
- **F**: Toggle fly mode

---

## Additional Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [URP Manual](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [C# Coding Standards](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

*Document Version: 1.0*  
*Last Updated: 2026-01-26*
