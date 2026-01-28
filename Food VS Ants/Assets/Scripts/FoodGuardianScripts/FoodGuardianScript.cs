using UnityEngine;
using System.Collections.Generic;

public class FoodGuardianScript : MonoBehaviour
{
    [Header("Attack Settings")]
    [HideInInspector] public float _foodGuardianAttackRate = 1.0f;
    [HideInInspector] public GameObject _projectilePrefab;
    [HideInInspector] public int _damage = 10;

    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpawnTimer = 0f;
    [SerializeField] private Transform _firePoint;

    [Header("Health Settings")]
    [SerializeField] private int _baseMaxHealth = 200;
    [SerializeField] private int _currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private HealthBar _healthBar;

    [Header("Detection Settings")]
    [SerializeField] private bool _canFoodGuardianAttack = false;

    // track ants in range using a HashSet to prevent duplicates
    private HashSet<GameObject> _antsInRange = new HashSet<GameObject>();

    [Header("UI Display")]
    [SerializeField] private string _guardianName = "Input Guardian's Name";
    [SerializeField] private Sprite _guardianIcon;
    [SerializeField] private Sprite _elementIcon;

    // current stats
    private int _maxHealth;

    private void Start()
    {
        // initialize health
        _maxHealth = _baseMaxHealth;
        _currentHealth = _baseMaxHealth;

        // set up the health bar
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_baseMaxHealth);
        }
    }

    void Update()
    {
        // clean up any null references (destroyed ants)
        _antsInRange.RemoveWhere(ant => ant == null);

        // Update attack state based on actual ant count
        _canFoodGuardianAttack = _antsInRange.Count > 0;

        // only attack if ants are being detected
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
            // Add to HashSet (automatically prevents duplicates)
            bool added = _antsInRange.Add(other.gameObject);

            if (added)
            {
                Debug.Log($"[{gameObject.name}] Ant entered range. Count: {_antsInRange.Count}");
            }
        }
    }

    // detect when ant leaves the DetectionZone (attack range)
    private void OnTriggerExit(Collider other)
    {
        // check if the colliding object is an ant
        AntHealth antHealth = other.GetComponent<AntHealth>();
        if (antHealth != null)
        {
            // Remove from HashSet regardless of dead/alive state
            bool removed = _antsInRange.Remove(other.gameObject);

            if (removed)
            {
                Debug.Log($"[{gameObject.name}] Ant left range. Count: {_antsInRange.Count}");
            }

            // Update attack state
            if (_antsInRange.Count == 0)
            {
                _canFoodGuardianAttack = false;
                _projectileSpawnTimer = 0;
                Debug.Log($"[{gameObject.name}] No more ants in range. Stopped attacking.");
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

    // moveset system sets base damage (but leveling system controls final damage)
    public void SetBaseDamage(int newBaseDamage)
    {
        _damage = newBaseDamage;
    }

    public void OnAntDied(GameObject deadAnt)
    {
        // Remove the dead ant from our tracking
        bool removed = _antsInRange.Remove(deadAnt);

        if (removed)
        {
            Debug.Log($"[{gameObject.name}] Ant died and removed. Count: {_antsInRange.Count}");
        }

        if (_antsInRange.Count == 0)
        {
            _canFoodGuardianAttack = false;
            _projectileSpawnTimer = 0;
            Debug.Log($"[{gameObject.name}] No more ants after death. Stopped attacking.");
        }
    }

    // getters
    public string GetGuardianName() => _guardianName;
    public Sprite GetElementIcon() => _elementIcon;
    public Sprite GetGuardianIcon() => _guardianIcon;
    public int GetMaxHealth() => _maxHealth;
    public int GetCurrentHealth() => _currentHealth;
    public int GetDamage() => _damage;
    public int GetAntsInRangeCount() => _antsInRange.Count; // For debugging
}