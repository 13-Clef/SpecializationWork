using UnityEditor.VersionControl;
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
    [SerializeField] private GameObject _foodGuardianPrefab;
    [SerializeField] private float _towerYOffset = 2f;

    private Camera _mainCamera;
    private GameObject _currentHoverIndicator;
    private GameObject _currentTargetTile; // track which tile currently hovering
    private bool _isRetrieveMode = false; // track between placement and retreival mode

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
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
            CheckPlacement();
            HandlePlacement();
        }
    }

    void CheckMode()
    {
        // toggle between retrieval and placement mode with "R"
        if (Input.GetKeyDown(KeyCode.R))
        {
            _isRetrieveMode = !_isRetrieveMode;
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
            if (_currentTargetTile != null && _foodGuardianPrefab != null)
            {
                PlaceFoodGuardian(_currentTargetTile);
            }
        }
    }

    void PlaceFoodGuardian(GameObject tile)
    {
        // calculate the position for the food guardian to appear on the tile
        Vector3 spawnPosition = tile.transform.position;
        spawnPosition.y += _towerYOffset;

        // spawn the food guardian
        GameObject foodGuardian = Instantiate(_foodGuardianPrefab, spawnPosition, Quaternion.identity);

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
    }

    // public method for UI button
    public void ToggleRetrieveMode()
    {
        _isRetrieveMode = !_isRetrieveMode;
        HideHoverIndicator();

        // destroy old indicator so it switches to correct color
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
            _currentHoverIndicator = null;
        }
    }
}

