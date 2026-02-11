using System.Collections;
using UnityEngine;

public class FrystormMissileLauncher : MonoBehaviour, IAttackInit
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private int missilesPerBurst = 3;
    [SerializeField] private float delayBetweenMissiles = 0.15f;
    [SerializeField] private float cooldown = 2.5f;

    private bool isOnCooldown;
    private AttackContext _ctx;

    // Called by FoodGuardianScript
    public void Init(AttackContext ctx)
    {
        _ctx = ctx;
        StartCoroutine(FireBurst());
    }

    private IEnumerator FireBurst()
    {
        isOnCooldown = true;

        for (int i = 0; i < missilesPerBurst; i++)
        {
            GameObject missile = Instantiate(
                missilePrefab,
                _ctx.firePoint.position,
                _ctx.firePoint.rotation
            );

            missile.GetComponent<IAttackInit>()?.Init(_ctx);

            yield return new WaitForSeconds(delayBetweenMissiles);
        }

        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
