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

    [Header("Animation Settings")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _deathAnimationLength = 3.333f;

    private bool _isAttacking = false;
    private bool _isDead = false;
    private FoodGuardianScript _targetGuardian;
    private float _attackTimer = 0f;

    // animation parameter names (

    void Start()
    {
        // set up the animator
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        // set up the health bar
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_maxHealth);
        }

        // initialize health
        _currentHealth = _maxHealth;

        // normalize direction to make sure consistent speed with walking animation
        _moveDirection = _moveDirection.normalized;

        // start walking after spawn
        if (_animator != null)
        {
            _animator.SetBool("WalkBool", true);
        }

    }

    void Update()
    {
        // do nothing if dead
        if (_isDead)
        {
            return;
        }

        // if not attacking then move
        if (!_isAttacking)
        {
            // move in a straight line in the (which is set to -Z direction)
            transform.position += _moveDirection * _movementSpeed * Time.deltaTime;

            // make sure if ant walking animation is playing
            if (_animator != null)
            {
                _animator.SetBool("WalkBool", true);
            }
        }
        else
        {
            // stop walking to attack
            if (_animator != null)
            {
                _animator.SetBool("WalkBool", false);
            }

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

    }

    public void TakeDamage(int damage)
    {
        // do nothing if dead
        if (_isDead)
        {
            return;
        }

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
        // do nothing if dead
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        // death animation
        _animator.SetTrigger("DeathTrigger");

        // disable all movement and start dying
        _isAttacking = true;
        _targetGuardian = null;
        _movementSpeed = 0f;

        // stop any current animation
        if (_animator != null)
        {
            _animator.SetBool("WalkBool", false);
            _animator.ResetTrigger("AttackTrigger");
            _animator.SetTrigger("DeathTrigger");
        }

        // disable collider so it doesn't interfere with other ants
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // destroy after animation
        Destroy(gameObject, _deathAnimationLength);
    }

    void attackFoodGuardian()
    {
        if (_targetGuardian != null)
        {
            // play the attack animation
            if (_animator != null)
            {
                _animator.SetTrigger("AttackTrigger");
            }

            // the targeted food guardian takes damage
            _targetGuardian.TakeDamage(_attackDamage);
        }
    }

    // collide with food guardian to do damage
    void OnTriggerEnter(Collider other)
    {
        // do nothing if dead
        if (_isDead)
        {
            return;
        }

        // check if its hitting a food guardian
        FoodGuardianScript guardian = other.GetComponent<FoodGuardianScript>();

        // deal damage
        if (guardian != null && !_isAttacking)
        {
            _isAttacking = true;
            _targetGuardian = guardian;
            _attackTimer = _attackInterval; // attacks immediately after touching the food guardian
        }

        // if touches AntDetection, FoodGuardian can attack

        // delete if go out of bound
        if (other.CompareTag("AntWall"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // do nothing if dead
        if (_isDead)
        {
            return;
        }

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