using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [SerializeField] private Image _healthBarFill;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Slider _slider;

    public void SetMaxHealth(int maxHealth)
    {
        // set current health as max health
        _maxHealth = maxHealth;
        _currentHealth = _maxHealth;

        // slider update
        _slider.maxValue = _maxHealth;
        _slider.value = _currentHealth;

        UpdateHealthBar();
    }

    public void SetCurrentHealth(int currentHealth)
    {
        // updates current health
        _currentHealth = Mathf.Clamp(currentHealth, 0, _maxHealth);

        _slider.value = _currentHealth;

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (_healthBarFill != null)
        {
            float healthPercent = _maxHealth > 0 ? (float) _currentHealth / _maxHealth : 0;
            _healthBarFill.fillAmount = healthPercent;
        }

        if (_healthText != null)
        {
            _healthText.text = $"HP: {_currentHealth}/{_maxHealth}";
        }
    }
}
