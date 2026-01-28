using UnityEngine;
using VoxelSandbox.Core;
using VoxelSandbox.World;

namespace VoxelSandbox.Building
{
    /// <summary>
    /// Building mode enumeration.
    /// </summary>
    public enum BuildingMode
    {
        Build,
        Remove,
        Inspect
    }

    /// <summary>
    /// Building controller for placing and removing blocks.
    /// </summary>
    public class BuildingController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxReachDistance = 10f;
        [SerializeField] private KeyCode buildModeKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode removeModeKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode inspectModeKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode rotateKey = KeyCode.R;

        [Header("Preview")]
        [SerializeField] private GameObject previewPrefab;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;

        private BuildingMode _currentMode = BuildingMode.Build;
        private BlockType _selectedBlock = BlockType.Solid;
        private int _rotation = 0;
        
        private ChunkManager _chunkManager;
        private LocalizationService _localization;
        private GameObject _previewObject;
        private bool _isValidPlacement = false;

        private void Start()
        {
            _chunkManager = ServiceLocator.Get<ChunkManager>();
            _localization = ServiceLocator.Get<LocalizationService>();

            ServiceLocator.Register(this);

            if (previewPrefab != null)
            {
                _previewObject = Instantiate(previewPrefab);
                _previewObject.SetActive(false);
            }

            Logger.Info("BuildingController initialized");
        }

        private void Update()
        {
            HandleModeSwitch();
            HandleRotation();
            UpdatePreview();
            HandlePlacement();
        }

        private void HandleModeSwitch()
        {
            if (Input.GetKeyDown(buildModeKey))
            {
                _currentMode = BuildingMode.Build;
                Logger.Info($"Mode: {_currentMode}");
            }
            else if (Input.GetKeyDown(removeModeKey))
            {
                _currentMode = BuildingMode.Remove;
                Logger.Info($"Mode: {_currentMode}");
            }
            else if (Input.GetKeyDown(inspectModeKey))
            {
                _currentMode = BuildingMode.Inspect;
                Logger.Info($"Mode: {_currentMode}");
            }
        }

        private void HandleRotation()
        {
            if (Input.GetKeyDown(rotateKey))
            {
                _rotation = (_rotation + 90) % 360;
                Logger.Info($"Rotation: {_rotation}°");
            }
        }

        private void UpdatePreview()
        {
            if (_previewObject == null || _chunkManager == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
            {
                Vector3Int targetPos;

                if (_currentMode == BuildingMode.Build)
                {
                    // Place adjacent to hit surface
                    targetPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);
                    _isValidPlacement = _chunkManager.GetBlock(targetPos) == BlockType.Air;
                }
                else if (_currentMode == BuildingMode.Remove)
                {
                    // Remove hit block
                    targetPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
                    _isValidPlacement = _chunkManager.GetBlock(targetPos) != BlockType.Air;
                }
                else
                {
                    _previewObject.SetActive(false);
                    return;
                }

                _previewObject.SetActive(true);
                _previewObject.transform.position = targetPos + Vector3.one * 0.5f;
                _previewObject.transform.rotation = Quaternion.Euler(0, _rotation, 0);

                // Update material
                var renderer = _previewObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = _isValidPlacement ? validMaterial : invalidMaterial;
                }
            }
            else
            {
                _previewObject.SetActive(false);
            }
        }

        private void HandlePlacement()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (_previewObject == null || !_previewObject.activeSelf) return;
            if (!_isValidPlacement) return;

            Vector3Int targetPos = Vector3Int.FloorToInt(_previewObject.transform.position);

            if (_currentMode == BuildingMode.Build)
            {
                _chunkManager.SetBlock(targetPos, _selectedBlock);
                Logger.Info($"Placed block at {targetPos}");
            }
            else if (_currentMode == BuildingMode.Remove)
            {
                _chunkManager.SetBlock(targetPos, BlockType.Air);
                Logger.Info($"Removed block at {targetPos}");
            }
        }

        public BuildingMode CurrentMode => _currentMode;
        public BlockType SelectedBlock => _selectedBlock;
        
        public void SetSelectedBlock(BlockType blockType)
        {
            _selectedBlock = blockType;
        }
    }
}
