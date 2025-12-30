using UnityEngine;

public class HandDisplayManager : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    [SerializeField] private Transform _handDisplayParent;
    [SerializeField] private GameObject[] _foodGuardianDisplayPrefabs;

    [Header("Per-Slot Transform Settings")]
    [SerializeField] private Vector3[] _displayLocalPositions;
    [SerializeField] private Vector3[] _displayLocalRotations;
    [SerializeField] private Vector3[] _displayScales;

    private GameObject _instantiatedDisplay;

    public void UpdateHandDisplay(int selectedIndex)
    {
        // clear the existing display
        if (_instantiatedDisplay != null)
        {
            Destroy(_instantiatedDisplay);
            _instantiatedDisplay = null;
        }

        // check if valid index and parent exist
        if (selectedIndex >= 0 && selectedIndex < _foodGuardianDisplayPrefabs.Length && _handDisplayParent != null)
        {
            GameObject prefabToDisplay = _foodGuardianDisplayPrefabs[selectedIndex];

            // instanitate without parent first to avoid scale issue
            _instantiatedDisplay = Instantiate(prefabToDisplay);
            // then set parent with worldPositionStays = false to use local coordinates
            _instantiatedDisplay.transform.SetParent(_handDisplayParent, false);
            // apply position set in inspector (using array)
            if (_displayLocalPositions != null && selectedIndex < _displayLocalPositions.Length)
            {
                _instantiatedDisplay.transform.localPosition = _displayLocalPositions[selectedIndex];
            }

            // apply rotation set in inspector (using array)
            if (_displayLocalRotations != null && selectedIndex < _displayLocalRotations.Length)
            {
                _instantiatedDisplay.transform.localEulerAngles = _displayLocalRotations[selectedIndex];
            }

            // apply scale set in inspector (using array)
            if (_displayScales != null && selectedIndex < _displayScales.Length)
            {
                _instantiatedDisplay.transform.localScale = _displayScales[selectedIndex];
            }
            else
            {
                _instantiatedDisplay.transform.localScale = Vector3.one;
            }
        }       
    }

    // clears the current hand display
    public void ClearHandDisplay()
    {
        if (_instantiatedDisplay != null)
        {
            Destroy(_instantiatedDisplay);
            _instantiatedDisplay = null;
        }
    }

    // get the number of display prefabs available
    public int GetDisplayPrefabCount()
    {
        if (_foodGuardianDisplayPrefabs != null)
        {
            return _foodGuardianDisplayPrefabs.Length;
        }
        else
        {
            return 0;
        }
    }

    private void OnDisable()
    {
        ClearHandDisplay();
    }
}
