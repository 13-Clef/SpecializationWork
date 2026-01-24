using UnityEngine;

public class DebugColliders : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"=== Checking Colliders on {gameObject.name} ===");

        // Check this object
        Collider[] collidersOnThis = GetComponents<Collider>();
        Debug.Log($"Colliders on root object: {collidersOnThis.Length}");
        foreach (Collider col in collidersOnThis)
        {
            Debug.Log($"  - {col.GetType().Name} on {gameObject.name}, Layer: {LayerMask.LayerToName(gameObject.layer)}");
        }

        // Check all children
        Collider[] collidersInChildren = GetComponentsInChildren<Collider>();
        Debug.Log($"Total colliders (including children): {collidersInChildren.Length}");
        foreach (Collider col in collidersInChildren)
        {
            Debug.Log($"  - {col.GetType().Name} on {col.gameObject.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}");
        }

        Debug.Log($"Root position: {transform.position}");
    }
}