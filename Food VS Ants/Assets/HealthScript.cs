using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HealthScript : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    [Header("Health Bar (Optional)")]
    [SerializeField] private UnityEngine.UI.Image _healthBarFill; // The fill image (set Fill Type to Filled)
    [SerializeField] private GameObject _healthBarCanvas; // The entire health bar UI

    [Header("Events")]
    public UnityEvent OnDeath; // Triggered when health reaches 0
    public UnityEvent<float> OnHealthChanged; // Triggered when health changes (passes current health %)

    private bool _isDead = false;

    void Awake()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
    }

    // Take damage
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0f); // Clamp to 0

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {_currentHealth}/{_maxHealth}");

        UpdateHealthBar();
        OnHealthChanged?.Invoke(GetHealthPercent());

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    // Heal
    public void Heal(float amount)
    {
        if (_isDead) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth); // Clamp to max

        Debug.Log($"{gameObject.name} healed {amount}. Health: {_currentHealth}/{_maxHealth}");

        UpdateHealthBar();
        OnHealthChanged?.Invoke(GetHealthPercent());
    }

    // Get current health
    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    // Get max health
    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    // Get health as percentage (0-1)
    public float GetHealthPercent()
    {
        return _currentHealth / _maxHealth;
    }

    // Check if dead
    public bool IsDead()
    {
        return _isDead;
    }

    // Update health bar visual
    private void UpdateHealthBar()
    {
        if (_healthBarFill != null)
        {
            float healthPercent = GetHealthPercent();
            _healthBarFill.fillAmount = healthPercent;
        }
    }

    // Handle death
    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        Debug.Log($"<color=red>{gameObject.name} has died!</color>");

        // Hide health bar
        if (_healthBarCanvas != null)
        {
            _healthBarCanvas.SetActive(false);
        }

        // Trigger death event
        OnDeath?.Invoke();

        // Destroy this GameObject after a short delay (optional)
        Destroy(gameObject, 0.5f);
    }

    // Optional: Instantly kill
    public void InstantKill()
    {
        _currentHealth = 0f;
        Die();
    }

    // Optional: Reset health to full
    public void ResetHealth()
    {
        _isDead = false;
        _currentHealth = _maxHealth;
        UpdateHealthBar();

        if (_healthBarCanvas != null)
        {
            _healthBarCanvas.SetActive(true);
        }
    }
}