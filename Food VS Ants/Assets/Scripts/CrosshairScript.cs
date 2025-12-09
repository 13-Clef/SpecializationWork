using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private RawImage _crossHairImage;

    [Header("Placeing/Retrieving Settings")]
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private GameObject _hoverIndicatorPrefab; // green indicator for placement
    [SerializeField] private float _indicatorYOffset = 2f;
    [SerializeField] private GameObject _retrieveIndicatorPrefab; // red indictor for retreival

    [Header("Food Guardians Settings")]
    [SerializeField] private GameObject[] _foodGuardianPrefabs; // array for 5 food guardians (for now)
    [SerializeField] private float _towerYOffset = 2f;

    [Header("Visual Feedback Settings")]
    [SerializeField] private Transform _handDisplayParent;
    [SerializeField] private GameObject[] _foodGuardianDisplayPrefabs; // array for 5 preview food guardians (for now)
    [SerializeField] private Vector3 _displayLocalPosition = new Vector3(0.0f, 0.0f, 0.65f); // tweakable position
    [SerializeField] private Vector3 _displayLocalRotation = new Vector3(0f, 0f, 0f); // tweakable rotation
    private GameObject _instantiatedDisplay;

    private Camera _mainCamera;
    private GameObject _currentHoverIndicator;
    private GameObject _currentTargetTile; // track which tile currently hovering
    private bool _isRetrieveMode = false; // track between placement and retreival mode

    private int _selectedSlotIndex = 0; // start at slot 1

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;

        // initialize the hand display for the starting slot (index 0)
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _foodGuardianPrefabs.Length)
        {
            // Pass the index (0) to the new helper method
            UpdateHandDisplay(_selectedSlotIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // check for number key inputs (1-5)
        CheckFoodGuardianSlotSelection();

        // check if player pressed R to switch mode (place/retrieve)
        CheckMode();

        if (_isRetrieveMode)
        {
            // retrieve mode
            CheckRetrieve();
        }
        else
        {
            // placement mode
            // check if the index is valid (0 to 4) for placement
            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _foodGuardianPrefabs.Length)
            {
                CheckPlacement();
                HandlePlacement();
            }
            else
            {
                // ensure indicators are hidden if no slot is selected (index is 5)
                _currentTargetTile = null;
                HideHoverIndicator();
            }
        }
    }

    void CheckFoodGuardianSlotSelection()
    {
        // check keys 1-5 (i = 0 which means KeyCode.Alpha1)
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // index 0 is slot 1, index 4 is slot 5
                int newIndex = i;

                // check if the pressed key is the current selected slot
                if (_selectedSlotIndex == newIndex)
                {
                    // deselect if the same key is pressed again
                    _selectedSlotIndex = 5; // select index to 5 (our "no slot" state)
                    UpdateHandDisplay(5); // Pass 5 to signal no display
                }
                else
                {
                    // select the new slot, but only if it exists in the array
                    if (newIndex < _foodGuardianPrefabs.Length)
                    {
                        _selectedSlotIndex = newIndex;
                        // switch to placement mode when a slot is selected
                        _isRetrieveMode = false;
                        // show the new display
                        UpdateHandDisplay(_selectedSlotIndex);
                    }
                    else
                    {
                        // if key is pressed but no prefab is assigned for that slot
                        _selectedSlotIndex = 5;
                        // hide the display if selection is invalid
                        UpdateHandDisplay(5);
                    }
                }

                // clean up visual state regardless of selection/deselection
                HideHoverIndicator();
                if (_currentHoverIndicator != null)
                {
                    Destroy(_currentHoverIndicator);
                    _currentHoverIndicator = null;
                }

                // exit the loop after handling the key press
                return;
            }
        }
    }

    void UpdateHandDisplay(int selectedIndex)
    {
        // clean up existing display
        if (_instantiatedDisplay != null)
        {
            Destroy(_instantiatedDisplay);
            _instantiatedDisplay = null; // clear the reference
        }

        // check for valid index (0-4) and if the parent is assigned
        if (selectedIndex >= 0 && selectedIndex < _foodGuardianDisplayPrefabs.Length && _handDisplayParent != null)
        {
            // get the dedicated display prefab
            GameObject prefabToDisplay = _foodGuardianDisplayPrefabs[selectedIndex];

            if (prefabToDisplay != null)
            {
                // instantiate the display prefab, parent it to the Hand
                _instantiatedDisplay = Instantiate(prefabToDisplay, _handDisplayParent);

                // set local position and rotation for the preview
                _instantiatedDisplay.transform.localPosition = _displayLocalPosition;
                _instantiatedDisplay.transform.localEulerAngles = _displayLocalRotation;
            }
        }
    }

    void CheckMode()
    {
        // toggle between retrieval and placement mode with "R"
        if (Input.GetKeyDown(KeyCode.R))
        {
            _isRetrieveMode = !_isRetrieveMode;
            // deselect any slot when switching to retrieve mode
            if (_isRetrieveMode)
            {
                _selectedSlotIndex = 5; // use index 5 as the "no slot" state
                UpdateHandDisplay(5); // hide display when entering retrieve mode
            }
            HideHoverIndicator();

            // destroy old indicator when switching modes
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

        // cast ray from center of the screen where crosshair is at
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // check if ray hts a placeable surface
        if (Physics.Raycast(ray, out hit, _raycastDistance))
        {
            // check if hit object is a valid placement tile
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
            // check if crosshair is hovering over a Food Guardian
            if (hit.collider.CompareTag("FoodGuardian"))
            {
                ShowHoverIndicator(hit.collider.gameObject, true);

                // click to remove food guardian
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
        // check for left mouse click, if the tile is available then place food guardian
        if (Input.GetMouseButtonDown(0))
        {
            if (_currentTargetTile != null)
            {
                // check if theres a valid index (0-4) before trying to access the array
                if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _foodGuardianPrefabs.Length)
                {
                    // Placement uses the actual Food Guardian Prefab array
                    GameObject prefabToPlace = _foodGuardianPrefabs[_selectedSlotIndex];
                    if (prefabToPlace != null)
                    {
                        PlaceFoodGuardian(_currentTargetTile, prefabToPlace);
                    }
                }
            }
        }
    }

    void PlaceFoodGuardian(GameObject tile, GameObject foodGuardianPrefab)
    {
        // calculate the position for the food guardian to appear on the tile
        Vector3 spawnPosition = tile.transform.position;
        spawnPosition.y += _towerYOffset;

        // spawn the food guardian
        GameObject foodGuardian = Instantiate(foodGuardianPrefab, spawnPosition, Quaternion.identity);

        // tag the food guardian if it is not tagged
        foodGuardian.tag = "FoodGuardian";
    }

    void RetrieveFoodGuardian(GameObject foodGuardian)
    {
        // find the tile below the food guardian and make it placeable again
        RaycastHit hit;
        if (Physics.Raycast(foodGuardian.transform.position, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Occupied"))
            {
                hit.collider.tag = "PlaceableTile"; // change the tag from "Occupied" to "PlaceableTile" in order to make the empty tile placeable again
            }
        }

        // after changing tag, destroy the food guardian
        Destroy(foodGuardian);
    }

    void ShowHoverIndicator(GameObject tile, bool isRetrieveMode)
    {
        // create the indicator if it does not exist
        if (_currentHoverIndicator == null)
        {
            // use different indicator based on mode
            GameObject prefabToUse = isRetrieveMode ? _retrieveIndicatorPrefab : _hoverIndicatorPrefab;
            _currentHoverIndicator = Instantiate(prefabToUse);
        }

        // position the indicator above the tile
        if (_currentHoverIndicator != null)
        {
            _currentHoverIndicator.SetActive(true);
            Vector3 position;

            if (isRetrieveMode)
            {
                // for retrieve mode: place at same level as food guardian (above tile)
                RaycastHit hit;
                if (Physics.Raycast(tile.transform.position, Vector3.down, out hit, 10f))
                {
                    // place indicator on the tile with the food guardian to show which food guardian to retrieve
                    position = hit.collider.transform.position; // tile position
                    position.y += _indicatorYOffset; // slightly above tile
                }
                else
                {
                    // if raycast fails, use food guardian position
                    position = tile.transform.position;
                    position.y += _indicatorYOffset;
                }
            }

            else
            {
                // for placement mode: place above tile
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
        // clean up when script is disabled
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
        }
        // clean up the instantiated hand display
        if (_instantiatedDisplay != null)
        {
            Destroy(_instantiatedDisplay);
        }
    }

    // public method for UI button
    public void ToggleRetrieveMode()
    {
        _isRetrieveMode = !_isRetrieveMode;
        // deselect the slot when toggling mod using UI
        if (_isRetrieveMode)
        {
            _selectedSlotIndex = 5; // use index 5 as the "no slot" state
            UpdateHandDisplay(5); // hide display when entering retrieve mode
        }
        HideHoverIndicator();

        // destroy old indicator so it switches to correct color
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
            _currentHoverIndicator = null;
        }
    }
}