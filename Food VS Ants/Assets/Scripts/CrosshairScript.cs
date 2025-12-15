using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private RawImage _crossHairImage;

    [Header("Placement/Retrieving Settings")]
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private GameObject _hoverIndicatorPrefab;
    [SerializeField] private float _indicatorYOffset = 2f;
    [SerializeField] private GameObject _retrieveIndicatorPrefab;

    [Header("Food Guardians Settings")]
    [SerializeField] private GameObject[] _foodGuardianPrefabs;
    [SerializeField] private float _towerYOffset = 2f;
    [SerializeField] private int _foodGuardianPlacementCost = 20;

    [Header("Visual Feedback Settings")]
    [SerializeField] private Transform _handDisplayParent;
    [SerializeField] private GameObject[] _foodGuardianDisplayPrefabs;
    [SerializeField] private Vector3 _displayLocalPosition = new Vector3(0.0f, 0.0f, 0.65f);
    [SerializeField] private Vector3 _displayLocalRotation = new Vector3(0f, 0f, 0f);
    private GameObject _instantiatedDisplay;

    private Camera _mainCamera;
    private GameObject _currentHoverIndicator;
    private GameObject _currentTargetTile;
    private bool _isRetrieveMode = false;
    private int _selectedSlotIndex = 0; // start at slot 1

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        CheckFoodGuardianSlotSelection();
        CheckMode();

        if (_isRetrieveMode)
        {
            CheckRetrieve();
        }
        else
        {
            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _foodGuardianPrefabs.Length)
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

    void CheckFoodGuardianSlotSelection()
    {
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int newIndex = i;

                if (_selectedSlotIndex == newIndex)
                {
                    // Deselect
                    _selectedSlotIndex = 5;
                    UpdateHandDisplay(5);
                    Debug.Log($"<color=yellow>Deselected slot {newIndex + 1}</color>");
                }
                else
                {
                    // Try to select new slot
                    if (newIndex < _foodGuardianPrefabs.Length && _foodGuardianPrefabs[newIndex] != null)
                    {
                        if (CrumbsManager.Instance != null && CrumbsManager.Instance.GetCurrentCrumbs() >= _foodGuardianPlacementCost)
                        {
                            _selectedSlotIndex = newIndex;
                            _isRetrieveMode = false;
                            UpdateHandDisplay(_selectedSlotIndex);
                            Debug.Log($"<color=green>Selected slot {_selectedSlotIndex + 1}! Cost: {_foodGuardianPlacementCost} crumbs</color>");
                        }
                        else
                        {
                            int currentCrumbs = CrumbsManager.Instance != null ? CrumbsManager.Instance.GetCurrentCrumbs() : 0;
                            Debug.Log($"<color=red>Not enough crumbs! Need {_foodGuardianPlacementCost}, have {currentCrumbs}</color>");
                            _selectedSlotIndex = 5;
                            UpdateHandDisplay(5);
                        }
                    }
                    else
                    {
                        Debug.Log($"<color=red>Slot {newIndex + 1} is empty!</color>");
                        _selectedSlotIndex = 5;
                        UpdateHandDisplay(5);
                    }
                }

                // Cleanup
                HideHoverIndicator();
                if (_currentHoverIndicator != null)
                {
                    Destroy(_currentHoverIndicator);
                    _currentHoverIndicator = null;
                }

                return;
            }
        }
    }

    void UpdateHandDisplay(int selectedIndex)
    {
        if (_instantiatedDisplay != null)
        {
            Destroy(_instantiatedDisplay);
            _instantiatedDisplay = null;
        }

        if (selectedIndex >= 0 && selectedIndex < _foodGuardianDisplayPrefabs.Length && _handDisplayParent != null)
        {
            GameObject prefabToDisplay = _foodGuardianDisplayPrefabs[selectedIndex];

            if (prefabToDisplay != null)
            {
                _instantiatedDisplay = Instantiate(prefabToDisplay, _handDisplayParent);
                _instantiatedDisplay.transform.localPosition = _displayLocalPosition;
                _instantiatedDisplay.transform.localEulerAngles = _displayLocalRotation;
            }
        }
    }

    void CheckMode()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _isRetrieveMode = !_isRetrieveMode;

            if (_isRetrieveMode)
            {
                _selectedSlotIndex = 5;
                UpdateHandDisplay(5);
                Debug.Log("<color=cyan>Retrieve mode ON</color>");
            }
            else
            {
                Debug.Log("<color=cyan>Placement mode ON</color>");
            }

            HideHoverIndicator();

            if (_currentHoverIndicator != null)
            {
                Destroy(_currentHoverIndicator);
                _currentHoverIndicator = null;
            }
        }
    }

    void CheckPlacement()
    {
        if (_crossHairImage == null || _mainCamera == null) return;

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastDistance))
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
        if (_crossHairImage == null || _mainCamera == null) return;

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastDistance))
        {
            if (hit.collider.CompareTag("FoodGuardian"))
            {
                ShowHoverIndicator(hit.collider.gameObject, true);

                if (Input.GetMouseButtonDown(0))
                {
                    RetrieveFoodGuardian(hit.collider.gameObject);
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
        if (Input.GetMouseButtonDown(0))
        {
            if (_currentTargetTile != null)
            {
                if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _foodGuardianPrefabs.Length)
                {
                    GameObject prefabToPlace = _foodGuardianPrefabs[_selectedSlotIndex];

                    if (prefabToPlace != null)
                    {
                        if (CrumbsManager.Instance != null && CrumbsManager.Instance.CanAfford(_foodGuardianPlacementCost))
                        {
                            PlaceFoodGuardian(_currentTargetTile, prefabToPlace);
                        }
                        else
                        {
                            Debug.Log("<color=red>Not enough crumbs to place!</color>");
                        }
                    }
                }
            }
        }
    }

    void PlaceFoodGuardian(GameObject tile, GameObject foodGuardianPrefab)
    {
        if (CrumbsManager.Instance != null)
        {
            // place food guardian deduct crumbs from its cost
            CrumbsManager.Instance.AddCrumbs(-_foodGuardianPlacementCost);
        }

        Vector3 spawnPosition = tile.transform.position;
        spawnPosition.y += _towerYOffset;

        GameObject foodGuardian = Instantiate(foodGuardianPrefab, spawnPosition, Quaternion.identity);
        foodGuardian.tag = "FoodGuardian";

        tile.tag = "Occupied";

        // Auto-deselect
        _selectedSlotIndex = 5;
        UpdateHandDisplay(5);
    }

    void RetrieveFoodGuardian(GameObject foodGuardian)
    {
        RaycastHit hit;
        if (Physics.Raycast(foodGuardian.transform.position, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Occupied"))
            {
                hit.collider.tag = "PlaceableTile";
            }
        }

        Destroy(foodGuardian);
        Debug.Log("<color=yellow>Guardian retrieved</color>");
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
            Vector3 position;

            if (isRetrieveMode)
            {
                RaycastHit hit;
                if (Physics.Raycast(tile.transform.position, Vector3.down, out hit, 10f))
                {
                    position = hit.collider.transform.position;
                    position.y += _indicatorYOffset;
                }
                else
                {
                    position = tile.transform.position;
                    position.y += _indicatorYOffset;
                }
            }
            else
            {
                position = tile.transform.position;
                position.y += _indicatorYOffset;
            }

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

    void OnDisable()
    {
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
        }

        if (_instantiatedDisplay != null)
        {
            Destroy(_instantiatedDisplay);
        }
    }

    public void ToggleRetrieveMode()
    {
        _isRetrieveMode = !_isRetrieveMode;

        if (_isRetrieveMode)
        {
            _selectedSlotIndex = 5;
            UpdateHandDisplay(5);
        }

        HideHoverIndicator();

        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
            _currentHoverIndicator = null;
        }
    }
}