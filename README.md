# VoxelSandbox

Воксельная песочница в стиле Space Engineers с симуляцией воды.

## 📊 Статус Проекта
🟢 **Фундамент Готов (85%)** | 🔴 **Требуется Unity Editor**

**Версия**: Pre-Alpha v0.1.0  
**Unity**: 2022.3.17f1 LTS  
**Rendering**: Universal Render Pipeline (URP)

### ✅ Завершено
- ✅ Полная структура проекта (9 модульных asmdefs)
- ✅ Система локализации (RU ↔ EN, 90+ ключей)
- ✅ Сервисы: Logger, Settings, ServiceLocator
- ✅ UI скрипты: MainMenu, Settings, DebugOverlay
- ✅ Контроллер игрока (WASD, полёт, прыжки)
- ✅ Основа воксельного мира (Chunk, ChunkManager)
- ✅ Система строительства (3 режима, превью)
- ✅ Основа воды (WaterManager, WaterChunk)
- ✅ 18 юнит-тестов (EditMode)
- ✅ CI/CD (GitHub Actions)
- ✅ Полная документация (6 MD файлов)

### 🔴 Требует Unity Editor
- Создание сцен (.unity)
- Создание префабов и материалов
- Настройка URP и Input System
- Реализация Greedy Meshing
- Реализация симуляции воды
- Запуск тестов и сборка

## 📖 Документация

- **[docs/roadmap.md](docs/roadmap.md)** - Детальный план (6 эпиков)
- **[docs/architecture.md](docs/architecture.md)** - Архитектура модулей
- **[docs/dev-setup.md](docs/dev-setup.md)** - Настройка Unity
- **[docs/controls.md](docs/controls.md)** - Управление (RU + EN)
- **[docs/handoff.md](docs/handoff.md)** - Детальный статус и следующие шаги
- **[docs/performance.md](docs/performance.md)** - Метрики производительности

## 🔧 Управление

- **WASD**: Движение | **Мышь**: Обзор | **Пробел**: Прыжок | **F**: Полёт
- **1/2/3**: Режимы строительства | **R**: Повернуть блок
- **Esc**: Пауза | **F3**: Отладка | **F9**: Язык (RU ↔ EN)

Полный список: **[docs/controls.md](docs/controls.md)**

---

## English

Voxel sandbox game inspired by Space Engineers with water simulation.

### 📊 Status
🟢 **Foundation Ready (85%)** | 🔴 **Requires Unity Editor**

**Version**: Pre-Alpha v0.1.0 | **Unity**: 2022.3.17f1 LTS

### ✅ Completed
- ✅ Complete project structure (9 modular asmdefs)
- ✅ Localization system (RU ↔ EN, 90+ keys)
- ✅ Core services + UI scripts + Player controller
- ✅ Voxel world + Building + Water foundations
- ✅ 18 unit tests + CI/CD + Full documentation

### 🔴 Requires Unity Editor
- Scene/prefab/material creation
- URP + Input System setup
- Greedy Meshing + Water simulation implementation

### 📖 Documentation
See **[docs/handoff.md](docs/handoff.md)** for complete status and next steps.

### 🎮 Controls
**WASD**: Move | **Mouse**: Look | **Space**: Jump | **F**: Fly  
**1/2/3**: Build modes | **R**: Rotate | **Esc**: Pause | **F3**: Debug | **F9**: Language

Full: **[docs/controls.md](docs/controls.md)**

---

**Status**: Foundation complete. Ready for Unity Editor integration.

*Last Updated: 2026-01-26*
