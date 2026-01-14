using UnityEngine;

public class AntCollisionHandler : MonoBehaviour
{
    private AntHealth _health;
    private AntCombat _combat;

    void Awake()
    {
        _health = GetComponent<AntHealth>();
        _combat = GetComponent<AntCombat>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_health.IsDead()) return;

        // check for Food Guardian
        FoodGuardianScript guardian = other.GetComponent<FoodGuardianScript>();

        if (guardian != null && !_combat.IsAttacking())
        {
            _combat.StartAttacking(guardian);
        }

        // check for out of bounds
        if (other.CompareTag("AntWall"))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_health.IsDead()) return;

        // if food guardian is defeated, continue moving
        FoodGuardianScript guardian = other.GetComponent<FoodGuardianScript>();

        if (guardian != null)
        {
            _combat.StopAttacking();
        }
    }
}
