using UnityEngine;

public class FoodGuardianManager : MonoBehaviour
{
    [Header("Food Guardian Prefabs")]
    [SerializeField] private GameObject[] _foodGuardianPrefabs;

    [Header("Placement Settings")]
    [SerializeField] private float _towerYOffset = 2f;
    [SerializeField] private int _foodGuardianPlacementCost = 50;

    // places a food guardian on the specified tile
    public void PlaceFoodGuardian(GameObject tile, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _foodGuardianPrefabs.Length)
        {
            Debug.LogWarning("invalid slot index!");
            return;
        }

        GameObject prefabToPlace = _foodGuardianPrefabs[slotIndex];
        if (prefabToPlace == null)
        {
            Debug.LogWarning($"No Prefab assigned to slot {slotIndex}!");
            return;
        }

        // deduct crumbs
        if (CrumbsManager.Instance != null)
        {
            CrumbsManager.Instance.AddCrumbs(-_foodGuardianPlacementCost);
        }

        // calculate spawn position
        Vector3 spawnPosition = tile.transform.position;
        spawnPosition.y += _towerYOffset;

        // instantiate the tower
        GameObject foodGuardian = Instantiate(prefabToPlace, spawnPosition, Quaternion.identity);
        foodGuardian.tag = "FoodGuardian";

        // mark tile as occupied after placing
        tile.tag = "Occupied";
    }

    // retrieves a food guardian and refund 50% of the cost
    public void RetrieveFoodGuardian(GameObject foodguardian)
    {
        if (foodguardian == null)
        {
            return;
        }

        // refund 50% of the placement cost
        if (CrumbsManager.Instance != null)
        {
            CrumbsManager.Instance.AddCrumbs(_foodGuardianPlacementCost / 2);
        }

        // find the tile underneath and mark it as placeable again after retrieving
        RaycastHit hit;
        if (Physics.Raycast(foodguardian.transform.position, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Occupied"))
            {
                hit.collider.tag = "PlaceableTile";
            }
        }

        // destroy the tower
        Destroy(foodguardian);
    }

    // checks if the player can afford to place a food guardian
    public bool CanAffordPlacement()
    {
        if (CrumbsManager.Instance != null)
        {
            return CrumbsManager.Instance.CanAfford(_foodGuardianPlacementCost);
        }
        return false;
    }

    // get the placement cost
    public int GetPlacementCost()
    {
        return _foodGuardianPlacementCost;
    }

    // check if a lot has a valid prefab assigned
    public bool IsSlotValid(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _foodGuardianPrefabs.Length)
        {
            return false;
        }
        return _foodGuardianPrefabs[slotIndex] != null;
    }

    // get the number of available slots
    public int GetSlotCount()
    {
        if (_foodGuardianPrefabs != null)
        {
            return _foodGuardianPrefabs.Length;
        }
        else
        {
            return 0;
        }
    }

    // get the prefab for a specific slot for preview display purposes
    public GameObject GetPrefabAtSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _foodGuardianPrefabs.Length)
        {
            return _foodGuardianPrefabs[slotIndex];
        }
        return null;
    }
}
