using UnityEngine;

public class GreaseInfernoSpawner : MonoBehaviour
{
    [Header("Zone Prefab")]
    public GameObject infernoZonePrefab;

    [Header("Tile / Lane Settings")]
    [Tooltip("How many tiles forward the inferno should cover (including the hit tile).")]
    public int tilesForward = 3;

    [Tooltip("World size of 1 tile along Z/forward. If your tiles are 1 unit, keep 1.")]
    public float tileSizeWorld = 1f;

    [Header("Clamp by Boundaries (optional)")]
    public LayerMask boundaryMask;
    public float yLift = 0.2f;

    [Header("Lifetime")]
    public float zoneLifetime = 2.5f;

    [Header("Snap to Grid (optional)")]
    public bool snapToGrid = true;
    public Vector3 gridOrigin = Vector3.zero; // set to your Row/Tile origin if needed

    /// <summary>
    /// Spawn a 3-tile inferno starting at the impact position, stretching forward.
    /// </summary>
    public GameObject SpawnInferno(Vector3 impactPos, Vector3 forward, float dps, GameObject damageSource)
    {
        if (infernoZonePrefab == null) return null;

        Vector3 laneFwd = new Vector3(forward.x, 0f, forward.z);
        if (laneFwd.sqrMagnitude < 0.0001f) laneFwd = Vector3.forward;
        laneFwd.Normalize();

        // snap X/Z to tile center (optional)
        Vector3 spawnPos = impactPos;
        spawnPos.y = gridOrigin.y; // keep on ground plane (adjust if needed)

        if (snapToGrid)
        {
            spawnPos = SnapToTileCenter(spawnPos);
        }

        float maxRangeWorld = Mathf.Max(1, tilesForward) * tileSizeWorld;
        float range = maxRangeWorld;

        // Optional: clamp length if boundary is in front
        Vector3 rayOrigin = spawnPos + Vector3.up * yLift;
        if (boundaryMask.value != 0 &&
            Physics.Raycast(rayOrigin, laneFwd, out RaycastHit hit, maxRangeWorld, boundaryMask))
        {
            range = hit.distance;
        }

        range = Mathf.Max(tileSizeWorld * 0.3f, range);

        // rotate so local Z points forward
        Quaternion rot = Quaternion.LookRotation(laneFwd, Vector3.up);

        var zoneObj = Instantiate(infernoZonePrefab, spawnPos, rot);

        // Set damage params
        var dmg = zoneObj.GetComponent<GreaseInfernoDamage>();
        if (dmg != null)
        {
            dmg.dps = dps;
            dmg.damageSource = damageSource;
        }

        // Resize hitbox to match range
        var bc = zoneObj.GetComponent<BoxCollider>();
        if (bc != null)
        {
            Vector3 size = bc.size;
            size.z = range;
            bc.size = size;

            Vector3 center = bc.center;
            center.z = range * 0.5f; // start from spawnPos and extend forward
            bc.center = center;
        }

        Destroy(zoneObj, zoneLifetime);
        return zoneObj;
    }

    private Vector3 SnapToTileCenter(Vector3 worldPos)
    {
        // Snap X and Z to nearest tile center based on tileSizeWorld
        float x = worldPos.x - gridOrigin.x;
        float z = worldPos.z - gridOrigin.z;

        float snappedX = (Mathf.Round(x / tileSizeWorld) * tileSizeWorld) + gridOrigin.x;
        float snappedZ = (Mathf.Round(z / tileSizeWorld) * tileSizeWorld) + gridOrigin.z;

        return new Vector3(snappedX, worldPos.y, snappedZ);
    }
}
