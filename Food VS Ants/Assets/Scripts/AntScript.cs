using UnityEngine;

public class AntScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 2f;
    [SerializeField] private Vector3 _moveDirection = Vector3.back; // move in the -Z direction to the food guardians

    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private HealthBar _healthBar;

    [Header("Attack Settings")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackInterval = 1f; // attacks per <?>

    private int _currentLane; // which lane this ant is in
    private bool _isAttacking = false;
    private FoodGuardianScript _targetGuardian;
    private float _attackTimer = 0f;

    void Start()
    {
        // normalize direction to make sure consistent speed
        _moveDirection = _moveDirection.normalized;

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
        // if not attacking then move
        if (!_isAttacking)
        {
            // move in a straight line in the (which is set to -Z direction)
            transform.position += _moveDirection * _movementSpeed * Time.deltaTime;
        }
        else
        {
            // food guardian detected, attack
            if (_targetGuardian != null)
            {
                _attackTimer += Time.deltaTime;

                if (_attackTimer >= _attackInterval)
                {
                    attackFoodGuardian();
                    _attackTimer = 0f;
                }
            }
            else
            {
                // food guardian has been defeated so continue moving
                _isAttacking = false;
            }
        }


        // delete if go into void
        if (transform.position.y <= -5f)
        {
            Destroy(gameObject);
        }
    }

    public int GetLane()
    {
        return _currentLane;
    }

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

    void attackFoodGuardian()
    {
        if (_targetGuardian != null)
        {
            _targetGuardian.TakeDamage(_attackDamage);
        }
    }

    // collide with food guardian to do damage
    void OnTriggerEnter(Collider other)
    {
        // check if its hitting a food guardian
        FoodGuardianScript guardian = other.GetComponent<FoodGuardianScript>();

        // deal damage
        if (guardian != null && !_isAttacking)
        {
            _isAttacking = true;
            _targetGuardian = guardian;
            _attackTimer = _attackInterval; // attacks immediately after touching the food guardian
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // if the food guardian is defeated, continue moving
        FoodGuardianScript guardian = other.GetComponent<FoodGuardianScript>();

        if (guardian != null && guardian == _targetGuardian)
        {
            _isAttacking = false;
            _targetGuardian = null;
            _attackTimer = 0f;
        }
    }
}