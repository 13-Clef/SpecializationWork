using UnityEngine;

public class FrystormMissileAttack : MonoBehaviour, IAttackInit
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    private int damage;
    private GameObject damageSource;
    private MovesetSystem movesetSystem;

    public void Init(AttackContext ctx)
    {
        damage = ctx.baseDamage;
        damageSource = ctx.shooter;

        if (damageSource != null)
            movesetSystem = damageSource.GetComponent<MovesetSystem>();

        // (Optional) snap missile to firePoint, if you want guaranteed correct spawn
        // if (ctx.firePoint != null)
        // {
        //     transform.position = ctx.firePoint.position;
        //     transform.rotation = ctx.firePoint.rotation;
        // }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        AntHealth ant = other.GetComponentInParent<AntHealth>();
        if (ant == null || ant.IsDead()) return;

        int finalDamage = CalculateDamage(ant);
        ant.TakeDamage(finalDamage, damageSource);

        SpawnDamageNumber(finalDamage, ant);

        Destroy(gameObject);
    }

    private int CalculateDamage(AntHealth target)
    {
        int finalDamage = damage;

        if (movesetSystem != null)
        {
            float mult = movesetSystem.GetDamageMultiplier(target.GetElement());
            finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * mult));
        }

        return finalDamage;
    }

    private void SpawnDamageNumber(int dmg, AntHealth ant)
    {
        if (damageNumberPrefab == null || ant == null) return;

        Vector3 pos = ant.transform.position + damageNumberOffset;
        GameObject obj = Instantiate(damageNumberPrefab, pos, Quaternion.identity);

        DamageNumber dn = obj.GetComponent<DamageNumber>();
        if (dn != null)
        {
            dn.Setup(dmg, GetEffectivenessColor(ant));
        }
    }

    // SAME colors as your ProjectileScript
    private Color GetEffectivenessColor(AntHealth target)
    {
        if (movesetSystem == null) return Color.white;

        float multiplier = movesetSystem.GetDamageMultiplier(target.GetElement());

        if (multiplier > 1.0f)
            return new Color(0f, 1f, 0.3f);      // Green (super effective)
        else if (multiplier < 1.0f)
            return new Color(1f, 0.5f, 0.5f);    // Pink (not very effective)

        return Color.white;
    }
}
