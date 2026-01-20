using UnityEngine;

public class FoodGuardianScript : MonoBehaviour
{
    [Header("Attack Settings")]
    public float _foodGuardianAttackRate = 1.0f;

    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpawnTimer = 0f;
    public GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    public int _baseDamage = 10;

    [Header("Health Settings")]
    [SerializeField] private int _baseMaxHealth = 200;
    [SerializeField] private int _currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private HealthBar _healthBar;

    [Header("Detection Settings")]
    [SerializeField] private bool _canFoodGuardianAttack = false;
    private int _antsInRangeofFoodGuardian = 0;

    [Header("UI Display")]
    [SerializeField] private Sprite _guardianIcon;

    // current stats
    private int _maxHealth;
    private int _damage;

    private void Start()
    {
        // initialize health
        _maxHealth = _baseMaxHealth;
        _damage = _baseDamage;
        _currentHealth = _baseMaxHealth;

        // set up the health bar
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_baseMaxHealth);
        }
    }

    void Update()
    {
        // only attack if ants are being detected in the RowDetectors
        if (_canFoodGuardianAttack)
        {
            FoodGuardianAttacksAnt();
        }
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
            // spawn projectile from firepoint
            GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);

            // pass damage and shooter info to projectile
            ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();
            if (projScript != null)
            {
                projScript.SetDamage(_damage);
                projScript.SetShooter(gameObject);
            }

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

    // detect when ant enters the DetectionZone (attack range)
    private void OnTriggerEnter(Collider other)
    {
        // check if the colliding object is an ant
        AntHealth antHealth = other.GetComponent<AntHealth>();
        if (antHealth != null && !antHealth.IsDead())
        {
            _antsInRangeofFoodGuardian++;
            _canFoodGuardianAttack = true;
        }
    }

    // detect when ant leaves the DetectionZone (attack range)
    private void OnTriggerExit(Collider other)
    {
        // check if the colliding object is an ant
        AntHealth antHealth = other.GetComponent<AntHealth>();
        if (antHealth != null)
        {
            _antsInRangeofFoodGuardian--;

            // stop attacking if no ants remain in the DetectionZone (range)
            if (_antsInRangeofFoodGuardian <= 0)
            {
                _antsInRangeofFoodGuardian = 0; // prevent negative values
                _canFoodGuardianAttack = false;
                _projectileSpawnTimer = 0; // reset timer when no ants detected
            }
        }
    }

    // leveling system method
    public void SetMaxHealth(int newMaxHealth)
    {
        int healthDifference = newMaxHealth - _maxHealth;
        _maxHealth = newMaxHealth;

        // also increase current health by difference
        _currentHealth += healthDifference;

        // update health bar
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_maxHealth);
            _healthBar.SetCurrentHealth(_currentHealth);
        }
    }

    public void SetDamage(int newDamage)
    {
        _damage = newDamage;
    }

    public void HealToFull()
    {
        _currentHealth = _maxHealth;

        if (_healthBar != null)
        {
            _healthBar.SetCurrentHealth(_currentHealth);
        }
    }

    // for moveset system
    public void SetAttackRate(float newAttackRate)
    {
        _foodGuardianAttackRate = newAttackRate;
    }

    public void SetProjectilePrefab(GameObject newProjectilePrefab)
    {
        _projectilePrefab = newProjectilePrefab;
    }

    public void SetBaseDamage(int newBaseDamage)
    {
        _baseDamage = newBaseDamage;
        _damage = newBaseDamage; // also update current damage
    }

    public void OnAntDied(GameObject deadAnt)
    {
        // Decrement ant count if this dead ant was in our range
        _antsInRangeofFoodGuardian--;

        if (_antsInRangeofFoodGuardian <= 0)
        {
            _antsInRangeofFoodGuardian = 0;
            _canFoodGuardianAttack = false;
            _projectileSpawnTimer = 0;
        }
    }

    // guardian icon
    public Sprite GetGuardianIcon()
    {
        return _guardianIcon;
    }

    // getters
    public int GetMaxHealth() => _maxHealth;
    public int GetCurrentHealth() => _currentHealth;
    public int GetDamage() => _damage;
}