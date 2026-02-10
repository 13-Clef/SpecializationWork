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
    private readonly HashSet<GameObject> _antsInRange = new HashSet<GameObject>();

    [Header("UI Display")]
    [SerializeField] private string _guardianName = "Input Guardian's Name";
    [SerializeField] private Sprite _guardianIcon;
    [SerializeField] private Sprite _elementIcon;

    private int _maxHealth;

    private void Start()
    {
        _maxHealth = _baseMaxHealth;
        _currentHealth = _baseMaxHealth;

        if (_healthBar != null)
            _healthBar.SetMaxHealth(_baseMaxHealth);
    }

    void Update()
    {
        _antsInRange.RemoveWhere(ant => ant == null);

        _canFoodGuardianAttack = _antsInRange.Count > 0;

        if (_canFoodGuardianAttack)
            FoodGuardianAttacksAnt();
        else
            _projectileSpawnTimer = 0f; // optional: stop charging when no ants
    }

    void FoodGuardianAttacksAnt()
    {
        if (_projectilePrefab == null || _firePoint == null)
            return;

        _projectileSpawnTimer += Time.deltaTime;

        if (_projectileSpawnTimer >= _foodGuardianAttackRate)
        {
            GameObject spawned = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);

            // One unified init path for ALL attack types
            var init = spawned.GetComponent<MonoBehaviour>() as IAttackInit;
            if (init != null)
            {
                AttackContext ctx = new AttackContext
                {
                    shooter = gameObject,
                    baseDamage = _damage,
                    attackRate = _foodGuardianAttackRate,
                    firePoint = _firePoint
                };

                init.Init(ctx);
            }
            else
            {
                Debug.LogWarning(
                    $"[{name}] Spawned prefab '{_projectilePrefab.name}' has no script implementing IAttackInit. " +
                    $"Add an attack script that implements IAttackInit, or it won’t receive damage/shooter stats."
                );
            }

            _projectileSpawnTimer = 0f;
        }
    }

    // take damage from ants
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (_healthBar != null)
            _healthBar.SetCurrentHealth(_currentHealth);

        if (_currentHealth <= 0)
            Die();
    }

    void Die()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Occupied"))
                hit.collider.tag = "PlaceableTile";
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // safer: works if ant collider is on child
        AntHealth antHealth = other.GetComponentInParent<AntHealth>();
        if (antHealth != null && !antHealth.IsDead())
            _antsInRange.Add(antHealth.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        AntHealth antHealth = other.GetComponentInParent<AntHealth>();
        if (antHealth != null)
        {
            _antsInRange.Remove(antHealth.gameObject);

            if (_antsInRange.Count == 0)
            {
                _canFoodGuardianAttack = false;
                _projectileSpawnTimer = 0f;
            }
        }
    }

    // for moveset system
    public void SetAttackRate(float newAttackRate) => _foodGuardianAttackRate = newAttackRate;
    public void SetProjectilePrefab(GameObject newProjectilePrefab) => _projectilePrefab = newProjectilePrefab;
    public void SetBaseDamage(int newBaseDamage) => _damage = newBaseDamage;

    public void OnAntDied(GameObject deadAnt)
    {
        _antsInRange.Remove(deadAnt);

        if (_antsInRange.Count == 0)
        {
            _canFoodGuardianAttack = false;
            _projectileSpawnTimer = 0f;
        }
    }

    public void ResetAttackTimer()
    {
        // shoot immediately after switching moveset
        _projectileSpawnTimer = _foodGuardianAttackRate;
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        int diff = newMaxHealth - _maxHealth;
        _maxHealth = newMaxHealth;

        // keep current health in sync (increase by diff)
        _currentHealth += diff;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

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
            _healthBar.SetCurrentHealth(_currentHealth);
    }


    // getters
    public string GetGuardianName() => _guardianName;
    public Sprite GetElementIcon() => _elementIcon;
    public Sprite GetGuardianIcon() => _guardianIcon;
    public int GetMaxHealth() => _maxHealth;
    public int GetCurrentHealth() => _currentHealth;
    public int GetDamage() => _damage;
    public int GetAntsInRangeCount() => _antsInRange.Count;
}
