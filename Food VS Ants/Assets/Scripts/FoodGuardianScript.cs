using UnityEngine;

public class FoodGuardianScript : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float _foodGuardianAttackRate = 1.0f;

    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpawnTimer = 0f;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 200;
    [SerializeField] private int _currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private HealthBar _healthBar;

    private void Start()
    {
        // initialize health
        _currentHealth = _maxHealth;

        // set up the health bar
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_maxHealth);
        }
    }

    void Update()
    {
        FoodGuardianAttacksAnt();
    }

    // food guardian attacking
    void FoodGuardianAttacksAnt()
    {

        // check if projectile prefab and firepoint is assigned
        if (_projectilePrefab == null || _firePoint == null)
        {
            Debug.LogError("projectile prefab or fire point is not assigned");
            return;
        }

        // timer for projectile
        _projectileSpawnTimer += Time.deltaTime;

        if (_projectileSpawnTimer > _foodGuardianAttackRate)
        {
            // spawn projectile from fire point
            Instantiate(_projectilePrefab, _firePoint.transform.position, _firePoint.transform.rotation);
            _projectileSpawnTimer = 0;
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

        // if health is zero, dies
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // after food guardian dies, find tile below and make it placeable again
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Occupied"))
            {
                hit.collider.tag = "PlaceableTile";
            }
        }
        Destroy(gameObject);
    }
}