using UnityEngine;

public class AntScript : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AntMovement _movement;
    [SerializeField] private AntHealth _health;
    [SerializeField] private AntCombat _combat;

    void Awake()
    {
        // get components if not assigned
        if (_movement == null) _movement = GetComponent<AntMovement>();
        if (_health == null) _health = GetComponent<AntHealth>();
        if (_combat == null) _combat = GetComponent<AntCombat>();
    }

    void Start()
    {
        _movement.StartMoving();
    }

    void Update()
    {
        if (_health.IsDead()) return;
    
        if (_combat.IsAttacking())
        {
            _movement.StopMoving();
            _combat.UpdateAttack();
        }
        else
        {
            _movement.ContinueMoving();
        }
    }
}