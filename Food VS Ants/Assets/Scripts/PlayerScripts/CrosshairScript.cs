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
        if (_mainCamera == null)
        {
            return;
        }

        // only check when left clicking and not in retrieve mode
        if (Input.GetMouseButtonDown(0) && !_isRetrieveMode)
        {
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _raycastDistance, _antDetectionLayer))
            {
                if (hit.collider.CompareTag("FoodGuardian"))
                {
                    // opem moveset panel for this selected food guardian
                    if (_movesetUIPanel != null)
                    {
                        _movesetUIPanel.ShowPanel(hit.collider.gameObject);
                    }
                }
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
                    // check if slot has a valid guardian prefab, whether player can afford it and it is NOT on cooldown
                    if (_foodGuardianManager != null && _foodGuardianManager.IsSlotValid(newIndex))
                    {
                        // check if food guardian is on cooldown
                        bool isReadyForPlacement = _deploymentCooldownManager == null || _deploymentCooldownManager.IsSlotReadyForPlacement(newIndex);

                        // and player can afford the food guardian
                        if (isReadyForPlacement && _foodGuardianManager.CanAffordPlacement())
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
        if (_handDisplayManager != null)
        {
            _handDisplayManager.UpdateHandDisplay(selectedIndex);
        }
    }

    void CheckMode()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // toggle mode (placement/retrieval)
            _isRetrieveMode = !_isRetrieveMode;

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
            if (hit.collider.CompareTag("FoodGuardian"))
            {
                ShowHoverIndicator(hit.collider.gameObject, true);

                if (Input.GetMouseButtonDown(0))
                {
                    if (_foodGuardianManager != null)
                    {
                        _foodGuardianManager.RetrieveFoodGuardian(hit.collider.gameObject);
                    }
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
            if (_currentTargetTile != null && _foodGuardianManager != null)
            {
                // check if slot is ready (not on cooldown)
                bool isReadyForPlacement = _deploymentCooldownManager == null || _deploymentCooldownManager.IsSlotReadyForPlacement(_selectedSlotIndex);

                if (isReadyForPlacement && _foodGuardianManager.CanAffordPlacement())
                {
                    // place the current food guardian
                    _foodGuardianManager.PlaceFoodGuardian(_currentTargetTile, _selectedSlotIndex);

                    // then start cooldown for the current food guardian
                    if (_deploymentCooldownManager != null)
                    {
                        _deploymentCooldownManager.StartCooldown(_selectedSlotIndex);
                    }
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