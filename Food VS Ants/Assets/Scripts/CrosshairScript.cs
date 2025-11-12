using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private RawImage _crossHairImage;

    [Header("Placement/Retrieve Settings")]
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private GameObject _hoverIndicatorPrefab;
    [SerializeField] private float _indicatorYOffset = 2f;

    [Header("Food Guardians Settings")]
    [SerializeField] private GameObject _foodGuardianPrefab;
    [SerializeField] private float _towerYOffset = 2f;
    //[SerializeField] private LayerMask _placeableLayer;

    private Camera _mainCamera;
    private GameObject _currentHoverIndicator;
    private GameObject _currentTargetTile; // track which tile currently hovering

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckPlacement();
        HandlePlacement();
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
                ShowHoverIndicator(hit.collider.gameObject);
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

    void HandlePlacement()
    {
        // check for left mouse click, if the tile is available then place food guardian
        if (Input.GetMouseButtonDown(0))
        {
            PlaceFoodGuardian(_currentTargetTile);
        }
    }

    void PlaceFoodGuardian(GameObject tile)
    {
        // calculate the position for the food guardian to appear on the tile
        Vector3 spawnPosition = tile.transform.position;
        spawnPosition.y += _towerYOffset;

        // spawn the food guardian
        GameObject foodGuardian = Instantiate(_foodGuardianPrefab, spawnPosition, Quaternion.identity);

        //// Optional: Mark tile as occupied so you can't place another tower
        //tile.tag = "Occupied"; // Change tag so it's no longer placeable

        //Debug.Log("Food Guardian placed on: " + tile.name);
    }
    void ShowHoverIndicator(GameObject tile)
    {
        // create the indicator if it does not exist
        if (_currentHoverIndicator == null && _hoverIndicatorPrefab != null)
        {
            _currentHoverIndicator = Instantiate(_hoverIndicatorPrefab);
        }

        // position the indicator above the tile
        if (_currentHoverIndicator != null)
        {
            _currentHoverIndicator.SetActive(true);
            Vector3 position = tile.transform.position;
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

    void OnDisabled()
    {
        // clean up when script is disabled
        if (_currentHoverIndicator != null)
        {
            Destroy(_currentHoverIndicator);
        }
    }
}

