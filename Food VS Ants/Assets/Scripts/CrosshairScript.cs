using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [SerializeField] private RawImage _crossHairImage;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _validPlacementColor = Color.green;
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private GameObject _hoverIndicatorPrefab;
    [SerializeField] private float _indicatorYOffset = 2f;
    //[SerializeField] private LayerMask _placeableLayer;

    private Camera _mainCamera;
    private GameObject _currentHoverIndicator;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        if (_crossHairImage != null)
        {
            _crossHairImage.color = _normalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckPlacement();
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
                _crossHairImage.color = _validPlacementColor;
                ShowHoverIndicator(hit.collider.gameObject);
            }
            else
            {
                _crossHairImage.color = _normalColor;
                HideHoverIndicator();
            }
        }

        else
        {
            _crossHairImage.color = _normalColor;
            HideHoverIndicator();
        }
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

