using UnityEngine;

public struct AttackContext
{
    public GameObject shooter;   // Mr Borgor (EXP credit)
    public int baseDamage;       // from moveset
    public float attackRate;     // from moveset (optional)
    public Transform firePoint;  // where it spawned from (optional)
}