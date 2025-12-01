using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private int _currentLane; // which lane this ant is in

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

        // determine which lane is the ant in
        if (LaneSystem.Instance != null)
        {
            _currentLane = LaneSystem.Instance.GetLaneFromPosition(transform.position);
        }
    }

    void Update()
    {
        // move in a straight line in the specified direction
        transform.position += _moveDirection * _movementSpeed * Time.deltaTime;

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

    // collide with food guardian to do damage
    void OnTriggerEnter(Collider other)
    {
        // check if its hitting a food guardian
        FoodGuardianScript guardian = other.GetComponent<FoodGuardianScript>();

        // deal damage
        if (guardian != null)
        {
         //   guardian.TakeDamage();
        }
    }
}