using UnityEngine;

public class AntCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackInterval = 1f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    private bool _isAttacking = false;
    private FoodGuardianScript _targetGuardian;
    private float _attackTimer = 0f;

    void Awake()
    {
        if (_animator != null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void UpdateAttack()
    {
        if (_targetGuardian != null)
        {
            _attackTimer += Time.deltaTime;

            if (_attackTimer >= _attackInterval)
            {
                AttackFoodGuardian();
                _attackTimer = 0f;
            }
        }
        else
        {
            // target destroyed, stop attacking
            _isAttacking = false;
        }
    }

    void AttackFoodGuardian()
    {
        if (_targetGuardian != null)
        {
            // play attack animation
            if (_animator != null)
            {
                _animator.SetTrigger("AttackTrigger");
            }

            // deal damage
            _targetGuardian.TakeDamage(_attackDamage);
        }
    }

    public void StartAttacking(FoodGuardianScript target)
    {
        _isAttacking = true;
        _targetGuardian = target;
        _attackTimer = _attackInterval; // immediately attack
    }

    public void StopAttacking()
    {
        _isAttacking = false;
        _targetGuardian = null;
        _attackTimer = 0f;
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }
}
