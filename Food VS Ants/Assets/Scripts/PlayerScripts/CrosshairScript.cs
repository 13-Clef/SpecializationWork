using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [Header("Placement/Retrieving Settings")]
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private GameObject _hoverIndicatorPrefab;
    [SerializeField] private float _indicatorYOffset = 2f;
    [SerializeField] private GameObject _retrieveIndicatorPrefab;

    [Header("Manager References")]
    [SerializeField] private HandDisplayManager _handDisplayManager;
    [SerializeField] private FoodGuardianManager _foodGuardianManager;
    [SerializeField] private DeploymentCooldownManager _deploymentCooldownManager;

    [Header("Moveset Panel")]
    [SerializeField] private MovesetUIPanel _movesetUIPanel;

    private Camera _mainCamera;
    private GameObject _currentHoverIndicator;
    private GameObject _currentTargetTile;
    private bool _isRetrieveMode = false;
    private int _selectedSlotIndex = 0;
    private LayerMask _antDetectionLayer;

    void Start()
    {
        _mainCamera = Camera.main;
        _antDetectionLayer = ~LayerMask.GetMask("AntDetection");
        UpdateHandDisplay(0);
    }

    void Update()
    {
        CheckFoodGuardianSlotSelection();
        CheckMode();
        CheckFoodGuardianSelection();

        if (_isRetrieveMode)
        {
            CheckRetrieve();
        }
        else
        {
            if (_foodGuardianManager != null && _foodGuardianManager.IsSlotValid(_selectedSlotIndex))
            {
                CheckPlacement();
                HandlePlacement();
            }
            else
            {
                _currentTargetTile = null;
                HideHoverIndicator();
            }
        }
    }

    void CheckFoodGuardianSelection()
    {
        if (_mainCamera == null) return;

        if (Input.GetMouseButtonDown(0) && !_isRetrieveMode)
        {
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _raycastDistance, _antDetectionLayer))
            {
                FoodGuardianScript guardianScript = hit.collider.GetComponentInParent<FoodGuardianScript>();

                if (guardianScript != null && _movesetUIPanel != null)
                {
                    _movesetUIPanel.ShowPanel(guardianScript.gameObject);
                }
            }
        }
    }

    void CheckFoodGuardianSlotSelection()
    {
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int newIndex = i;

                if (_selectedSlotIndex == newIndex)
                {
                    DeselectSlot();
                }
                else
                {
                    if (_foodGuardianManager != null && _foodGuardianManager.IsSlotValid(newIndex))
                    {
                        if (IsSlotReady(newIndex) && _foodGuardianManager.CanAffordPlacement())
                        {
                            _selectedSlotIndex = newIndex;
                            _isRetrieveMode = false;
                            UpdateHandDisplay(_selectedSlotIndex);
                        }
                        else
                        {
                            DeselectSlot();
                        }
                    }
                    else
                    {
                        DeselectSlot();
                    }
                }

                ClearHoverIndicator();
                return;
            }
        }
    }

    void CheckMode()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRetrieveMode();
        }
    }

    void CheckPlacement()
    {
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastDistance, _antDetectionLayer))
        {
            if (hit.collider.CompareTag("PlaceableTile"))
            {
                _currentTargetTile = hit.collider.gameObject;
                ShowHoverIndicator(hit.collider.gameObject, false);
            }
            else
            {
                _currentTargetTile = null;
                HideHoverIndicator();
            }
        }
        else
        {
            _currentTargetTile = null;
            HideHoverIndicator();
        }
    }

    void CheckRetrieve()
    {
        if (_mainCamera == null) return;

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastDistance, _antDetectionLayer))
        {
            FoodGuardianScript guardianScript = hit.collider.GetComponentInParent<FoodGuardianScript>();

            if (guardianScript != null)
            {
                GameObject foodGuardian = guardianScript.gameObject;
                ShowHoverIndicator(foodGuardian, true);

                if (Input.GetMouseButtonDown(0) && _foodGuardianManager != null)
                {
                    _foodGuardianManager.RetrieveFoodGuardian(foodGuardian);
                }
            }
            else
            {
                HideHoverIndicator();
            }
        }
        else
        {
            HideHoverIndicator();
        }
    }

    void HandlePlacement()
    {
        if (Input.GetMouseButtonDown(0) && _currentTargetTile != null && _foodGuardianManager != null)
        {
            if (IsSlotReady(_selectedSlotIndex) && _foodGuardianManager.CanAffordPlacement())
            {
                _foodGuardianManager.PlaceFoodGuardian(_currentTargetTile, _selectedSlotIndex);

                if (_deploymentCooldownManager != null)
                {
                    _deploymentCooldownManager.StartCooldown(_selectedSlotIndex);
                }
            }
        }
    }

    void ShowHoverIndicator(GameObject tile, bool isRetrieveMode)
    {
        if (_currentHoverIndicator == null)
        {
            GameObject prefabToUse = isRetrieveMode ? _retrieveIndicatorPrefab : _hoverIndicatorPrefab;
            _currentHoverIndicator = Instantiate(prefabToUse);
        }

        if (_currentHoverIndicator != null)
        {
            _currentHoverIndicator.SetActive(true);
            Vector3 position = tile.transform.position;

            if (isRetrieveMode)
            {
                RaycastHit hit;
                if (Physics.Raycast(tile.transform.position, Vector3.down, out hit, 10f))
                {
                    position = hit.collider.transform.position;
                }
            }

            position.y += _indicatorYOffset;
            _currentHoverIndicator.transform.position = position;
        }
    }

    void HideHoverIndicator()
    {
        if (_currentHoverIndicator != null)
        {
            _currentHoverIndicator.SetActive(false);
        }
    }

    void ClearHoverIndicator()
    {
        HideHoverIndicator();
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
            _currentHoverIndicator = null;
        }
    }

    void UpdateHandDisplay(int selectedIndex)
    {
        if (_handDisplayManager != null)
        {
            _handDisplayManager.UpdateHandDisplay(selectedIndex);
        }
    }

    void DeselectSlot()
    {
        _selectedSlotIndex = 5;
        UpdateHandDisplay(5);
    }

    bool IsSlotReady(int slotIndex)
    {
        return _deploymentCooldownManager == null || _deploymentCooldownManager.IsSlotReadyForPlacement(slotIndex);
    }

    void OnDisable()
    {
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
        }
    }

    public void ToggleRetrieveMode()
    {
        _isRetrieveMode = !_isRetrieveMode;

        if (_isRetrieveMode)
        {
            UpdateHandDisplay(5);
        }
        else
        {
            UpdateHandDisplay(_selectedSlotIndex);
        }

        ClearHoverIndicator();
    }
}