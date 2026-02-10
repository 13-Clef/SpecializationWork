using UnityEngine;

public class GreaseInfernoSpawner : MonoBehaviour
{
    public Transform firePoint;
    public GameObject infernoPrefab;

    public float maxRangeWorld = 3f;
    public LayerMask boundaryMask;
    public float yLift = 0.2f; // keep ray above ground

    public void CastInferno()
    {
        var inferno = Instantiate(infernoPrefab, firePoint.position, firePoint.rotation, firePoint);

        float range = maxRangeWorld;

        Vector3 origin = firePoint.position + Vector3.up * yLift;
        if (Physics.Raycast(origin, firePoint.forward, out RaycastHit hit, maxRangeWorld, boundaryMask))
        {
            range = hit.distance;
        }

        // clamp so it never becomes 0
        range = Mathf.Max(0.3f, range);

        // resize hitbox
        BoxCollider bc = inferno.GetComponent<BoxCollider>();
        if (bc != null)
        {
            Vector3 size = bc.size;
            size.z = range;
            bc.size = size;

            // keep it starting from firePoint forward
            Vector3 center = bc.center;
            center.z = range * 0.5f;
            bc.center = center;
        }
    }
}
