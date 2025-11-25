using UnityEngine;

public class FoodGuardianScript : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float _attackRange = 50f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackRate = 1f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Detection Settings")]
    [SerializeField] private Vector3 _raycastDirection = Vector3.forward; // Direction to shoot raycast (toward ants)
    [SerializeField] private float _raycastWidth = 2.5f; // Width of detection (should match lane width)
    [SerializeField] private LayerMask _antLayer; // Optional: set layer mask for ants only
    [SerializeField] private bool _showDebugRays = true;

    private float _attackTimer = 0f;
    private AntScript _currentAntTarget;

    void Update()
    {
        // Detect ant in front using raycast
        DetectAntWithRaycast();

        // Attack if we have a target
        if (_currentAntTarget != null)
        {
            _attackTimer += Time.deltaTime;

            if (_attackTimer >= 1f / _attackRate)
            {
                Attack();
                _attackTimer = 0f;
            }
        }
    }

    void DetectAntWithRaycast()
    {
        // Cast a sphere along the guardian's forward direction to detect ants
        Vector3 rayStart = _firePoint != null ? _firePoint.position : transform.position;

        // Use SphereCast to detect ants in a cylinder-shaped area (like a lane)
        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(
            rayStart,
            _raycastWidth / 2f,
            _raycastDirection.normalized,
            out hit,
            _attackRange,
            _antLayer.value == 0 ? ~0 : _antLayer // Use all layers if antLayer not set
        );

        // Debug visualization
        if (_showDebugRays)
        {
            Debug.DrawRay(rayStart, _raycastDirection.normalized * _attackRange,
                hitSomething ? Color.red : Color.green);
        }

        if (hitSomething)
        {
            // Check if we hit an ant
            AntScript ant = hit.collider.GetComponent<AntScript>();

            if (ant != null)
            {
                // Found an ant!
                if (_currentAntTarget != ant)
                {
                    _currentAntTarget = ant;
                    Debug.Log($"<color=green>Guardian detected Ant at {hit.point} (Distance: {hit.distance:F1})</color>");
                }
            }
            else
            {
                // Hit something else, not an ant
                if (_currentAntTarget != null)
                {
                    Debug.Log($"<color=yellow>Lost target - hit {hit.collider.name} instead</color>");
                }
                _currentAntTarget = null;
            }
        }
        else
        {
            // No hit - clear target
            if (_currentAntTarget != null)
            {
                Debug.Log("<color=yellow>No ant detected in range</color>");
            }
            _currentAntTarget = null;
        }
    }

    void Attack()
    {
        if (_currentAntTarget == null)
        {
            return;
        }

        Debug.Log($"<color=magenta>Guardian ATTACKING Ant at {_currentAntTarget.transform.position}</color>");

        // Check if projectile prefab is assigned
        if (_projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab is NOT assigned in Inspector!");
            return;
        }

        // Check if fire point is assigned
        if (_firePoint == null)
        {
            Debug.LogError("Fire Point is NOT assigned in Inspector!");
            return;
        }

        // Spawn projectile
        GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity);
        ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();
        if (projScript != null)
        {
            projScript.SetAntTarget(_currentAntTarget.transform, (int)_attackDamage);
            Debug.Log("<color=green>Projectile spawned!</color>");
        }
        else
        {
            Debug.LogError("Projectile prefab has no ProjectileScript component!");
        }
    }

    // Visualize attack range and detection area in the editor
    private void OnDrawGizmosSelected()
    {
        Vector3 start = _firePoint != null ? _firePoint.position : transform.position;
        Vector3 direction = _raycastDirection.normalized;

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start, _attackRange);

        // Draw detection ray
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + direction * _attackRange);

        // Draw detection width (sphere at start and end)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(start, _raycastWidth / 2f);
        Gizmos.DrawWireSphere(start + direction * _attackRange, _raycastWidth / 2f);

        // Draw cylinder outline
        Gizmos.color = Color.cyan;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * (_raycastWidth / 2f);
        Gizmos.DrawLine(start + perpendicular, start + direction * _attackRange + perpendicular);
        Gizmos.DrawLine(start - perpendicular, start + direction * _attackRange - perpendicular);
    }
}