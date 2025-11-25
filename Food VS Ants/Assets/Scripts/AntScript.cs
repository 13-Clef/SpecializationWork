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

    [Header("Health Bar UI")]
    [SerializeField] private Image _healthBarFill; 
    [SerializeField] private TextMeshProUGUI _healthText; 

    private int _currentLane; // which lane this ant is in
    void Start()
    {
        // normalize direction to make sure consistent speed
        _moveDirection = _moveDirection.normalized;

        // initialize health
        _currentHealth = _maxHealth;
        UpdateHealthBar();

        // determine which lane this ant is in
        if (LaneSystem.Instance != null)
        {
            _currentLane = LaneSystem.Instance.GetLaneFromPosition(transform.position);
            Debug.Log($"<color=yellow>Ant spawned in lane {_currentLane} at position {transform.position}</color>");
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
        _currentHealth = Mathf.Max(_currentHealth, 0 ); // use clamp to 0

        UpdateHealthBar();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (_healthBarFill != null)
        {
            float healthPercent = (float)_currentHealth / _maxHealth;
            _healthBarFill.fillAmount = healthPercent;
        }

        if (_healthBarFill != null)
        {
            _healthText.text = $"{_currentHealth}/{_maxHealth}";
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}