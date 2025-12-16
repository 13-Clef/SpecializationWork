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
    [SerializeField] private int _foodGuardianPlacementCost = 50;

    [Header("Visual Feedback Settings")]
    [SerializeField] private Transform _handDisplayParent;
    [SerializeField] private GameObject[] _foodGuardianDisplayPrefabs;
    [SerializeField] private Vector3 _displayLocalPosition = new Vector3(0.0f, 0.0f, 0.65f);
    [SerializeField] private Vector3 _displayLocalRotation = new Vector3(0f, 0f, 0f);
    private GameObject _instantiatedDisplay;

    // runtime state variables
    private Camera _mainCamera; // raycast origin
    private GameObject _currentHoverIndicator; // current indicator instance
    private GameObject _currentTargetTile; // tile to place on
    private bool _isRetrieveMode = false; // toggle between place/retrieve
    private int _selectedSlotIndex = 0; // start at slot 1
    private LayerMask _antDetectionLayer; // layer mask to ignore ant detection layer

    void Start()
    {
        _mainCamera = Camera.main;

        // ignore AntDetection Box collider by using ignore raycast layer
        _antDetectionLayer = ~LayerMask.GetMask("AntDetection");

        // display slot 1
        UpdateHandDisplay(0);
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
        for (int i = 0; i < 5; i++) // slot 1-5 (keys 1-5)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int newIndex = i; // slot index coreresponding to key pressed

                // pressing the same slot again deselects it
                if (_selectedSlotIndex == newIndex)
                {
                    // deselect by using slot 6 (which is empty)
                    _selectedSlotIndex = 5;
                    UpdateHandDisplay(5);
                }
                else
                {
                    // if slot exists and has guardian prefab assigned else slot is empty
                    if (newIndex < _foodGuardianPrefabs.Length && _foodGuardianPrefabs[newIndex] != null)
                    {
                        // check if player can afford placement
                        if (CrumbsManager.Instance != null && CrumbsManager.Instance.GetCurrentCrumbs() >= _foodGuardianPlacementCost)
                        {
                            _selectedSlotIndex = newIndex;
                            _isRetrieveMode = false; // switch back to placement mode
                            UpdateHandDisplay(_selectedSlotIndex);
                        }
                        // if player have no crumbs, use slot 6 to act as insufficient crumbs
                        else
                        {
                            //int currentCrumbs = CrumbsManager.Instance != null ? CrumbsManager.Instance.GetCurrentCrumbs() : 0;
                            _selectedSlotIndex = 5;
                            UpdateHandDisplay(5);
                        }
                    }
                    // current slot is empty so use slot 6 which is an empty slot
                    else
                    {
                        _selectedSlotIndex = 5;
                        UpdateHandDisplay(5);
                    }
                }

                // clear hover indicator since slot selection has changed
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

            // spawns the selected slot prefab and display it on hand
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
            // toggle mode (placement/retrieval)
            _isRetrieveMode = !_isRetrieveMode;
            UpdateHandDisplay(_selectedSlotIndex);

            // retrieve mode = no hand display, placement mode = current slot hand display
            if (_isRetrieveMode)
            {
                UpdateHandDisplay(5); // no hand display
            }
            else
            {
                UpdateHandDisplay(_selectedSlotIndex); // current slot hand display
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
        if (_crossHairImage == null || _mainCamera == null) return;

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastDistance, _antDetectionLayer))
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
    }

    void RetrieveFoodGuardian(GameObject foodGuardian)
    {
        if (CrumbsManager.Instance != null)
        {
            // refund food guardian crumbs by 50% of its cost
            CrumbsManager.Instance.AddCrumbs(_foodGuardianPlacementCost / 2);
        }

        RaycastHit hit;
        if (Physics.Raycast(foodGuardian.transform.position, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Occupied"))
            {
                hit.collider.tag = "PlaceableTile";
            }
        }

        Destroy(foodGuardian);
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