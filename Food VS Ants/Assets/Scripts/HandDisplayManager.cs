using UnityEngine;

public class HandDisplayManager : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    [SerializeField] private Transform _handDisplayParent;
    [SerializeField] private GameObject[] _foodGuardianDisplayPrefabs;
    [SerializeField] private Vector3 _displayLocalPosition = new Vector3(0.0f, 0.0f, 0.65f);
    [SerializeField] private Vector3 _displayLocalRotation = new Vector3(0f, 0f, 0f);

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

            // spawn the selected slot prefab and display it on hand
            _instantiatedDisplay = Instantiate(prefabToDisplay, _handDisplayParent);
            _instantiatedDisplay.transform.localPosition = _displayLocalPosition;
            _instantiatedDisplay.transform.localEulerAngles = _displayLocalRotation;
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
