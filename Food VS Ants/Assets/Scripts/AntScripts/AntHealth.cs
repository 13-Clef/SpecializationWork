using UnityEngine;
using System.Collections.Generic;

public class AntHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;

    [Header("Health Bar")]
    [SerializeField] private HealthBar _healthBar;

    [Header("Death Settings")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _deathAnimationLength = 3.333f;
    [SerializeField] private int _expDropAmount = 25;

    private bool _isDead = false;
    private HashSet<GameObject> _participatingGuardians = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = _maxHealth;

        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_maxHealth);
        }
    }

    public void TakeDamage(int damage, GameObject damageSource)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        // track participating food guardians
        if (damageSource != null)
        {
            FoodGuardianLevelingSystem levelingSystem = damageSource.GetComponent<FoodGuardianLevelingSystem>();

            if (levelingSystem != null && !_participatingGuardians.Contains(damageSource))
            {
                _participatingGuardians.Add(damageSource);
            }
        }

        // update health bar
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
        if (_isDead) return;

        _isDead = true;

        // give EXP to all participants
        GiveEXPToParticipants();

        // play death animation
        if (_animator != null)
        {
            _animator.SetBool("WalkBool", false);
            _animator.ResetTrigger("AttackTrigger");
            _animator.SetTrigger("DeathTrigger");
        }

        // notify nearby Food Guardians before disabling collider
        NotifyNearbyGuardiansOfDeath();

        // disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // notify other components
        AntMovement movement = GetComponent<AntMovement>();
        if (movement != null)
        {
            movement.DisableMovement();
        }

        AntCombat combat = GetComponent<AntCombat>();
        if (combat != null)
        {
            combat.StopAttacking();
        }

        // destroy after animation 3.333sec
        Destroy(gameObject, _deathAnimationLength);
    }

    void NotifyNearbyGuardiansOfDeath()
    {
        // find all food guardians that might be targeting this ant
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 20f);

        foreach (Collider col in nearbyColliders)
        {
            FoodGuardianScript guardian = col.GetComponent<FoodGuardianScript>();
            if (guardian != null)
            {
                guardian.OnAntDied(gameObject);
            }
        }
    }

    void GiveEXPToParticipants()
    {
        if (_participatingGuardians.Count == 0)
        {
            return;
        }

        foreach (GameObject guardian in _participatingGuardians)
        {
            if (guardian == null)
            {
                continue;
            }

            FoodGuardianLevelingSystem levelingSystem = guardian.GetComponent<FoodGuardianLevelingSystem>();

            if (levelingSystem != null)
            {
                levelingSystem.GainEXP(_expDropAmount);
            }

        }

        _participatingGuardians.Clear();
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public ElementType GetElement()
    {
        AntElement antElement = GetComponent<AntElement>();
        if (antElement != null)
        {
            return antElement.GetElementType();
        }
        return ElementType.None;
    }

    public void MultiplyHealth(float mult)
    {
        // safety clamp (prevents 0 HP bugs)
        mult = Mathf.Max(0.1f, mult);

        _maxHealth = Mathf.RoundToInt(_maxHealth * mult);
        _currentHealth = Mathf.RoundToInt(_currentHealth * mult);

        // keep current health valid
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        // update health bar instantly (important for tanks/runners)
        if (_healthBar != null)
        {
            _healthBar.SetMaxHealth(_maxHealth);
            _healthBar.SetCurrentHealth(_currentHealth);
        }
    }

}
