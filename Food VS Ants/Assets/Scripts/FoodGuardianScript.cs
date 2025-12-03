using UnityEngine;
using UnityEngine.InputSystem.HID;

public class FoodGuardianScript : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float _attackRange = 50f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackRate = 1f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Lane Detection Settings")]
    [SerializeField] private Vector3 _raycastDirection = Vector3.forward; // Direction to shoot raycast (toward ants)
    [SerializeField] private float _raycastWidth = 2.5f; // Width of detection (should match lane width)
    [SerializeField] private LayerMask _antLayer; // set layer mask for ants only
    [SerializeField] private bool _showDebugRays = true;

    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 200;
    [SerializeField] private int _currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private HealthBar _healthBar;

    private float _attackTimer = 0f;
    private AntScript _currentAntTarget;

    private void Start()
    {
        // Initialize health
        _currentHealth = _maxHealth;

        // set up the health bar
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_maxHealth);
        }
    }

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
                AttackAnt();
                _attackTimer = 0f;
            }
        }
    }

    void DetectAntWithRaycast()
    {
        // cast a sphere along the guardian's forward direction to detect ants
        Vector3 rayStart = _firePoint != null ? _firePoint.position : transform.position;

        // use SphereCast to detect ants in a cylinder-shaped area (like a lane)
        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(
            rayStart,
            _raycastWidth / 2f,
            _raycastDirection.normalized,
            out hit,
            _attackRange,
            _antLayer.value == 0 ? ~0 : _antLayer // use all layers if antLayer not set
        );

        // debug visualization
        if (_showDebugRays)
        {
            Debug.DrawRay(rayStart, _raycastDirection.normalized * _attackRange,
                hitSomething ? Color.red : Color.green);
        }

        if (hitSomething)
        {
            // check if we hit an ant
            AntScript ant = hit.collider.GetComponent<AntScript>();

            if (ant != null)
            {
                // food guardian finds an ant
                if (_currentAntTarget != ant)
                {
                    _currentAntTarget = ant;
                }
            }
            else
            {
                // hit something else (not ant)
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
            }
            _currentAntTarget = null;
        }
    }

    void AttackAnt()
    {
        if (_currentAntTarget == null)
        {
            return;
        }


        // check if projectile prefab is assigned
        if (_projectilePrefab == null || _firePoint == null)
        {
            Debug.LogError("projectile prefab or fire point is not assigned");
            return;
        }

        // spawn projectile from fire point
        GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity);
        ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();
        if (projScript != null)
        {
            projScript.SetAntTarget(_currentAntTarget.transform, (int)_attackDamage);
        }
        else
        {
            Debug.LogError("projectile is not assigned");
        }
    }

    // take damage from ants
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0); // use clamp to 0

        // update health bar whenever health changes
        if (_healthBar != null)
        {
            _healthBar.SetCurrentHealth(_currentHealth);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // visualize attack range and detection area in the editor
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