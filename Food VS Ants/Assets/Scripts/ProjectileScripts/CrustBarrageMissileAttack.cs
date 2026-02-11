using UnityEngine;

public class CrustBarrageMissileAttack : MonoBehaviour, IAttackInit
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    private int damage;
    private GameObject damageSource;

    public void Init(AttackContext ctx)
    {
        damage = ctx.baseDamage;
        damageSource = ctx.shooter;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        AntHealth ant = other.GetComponentInParent<AntHealth>();
        if (ant == null || ant.IsDead()) return;

        // Normal damage only (no element multiplier)
        int finalDamage = Mathf.Max(1, damage);

        ant.TakeDamage(finalDamage, damageSource);
        SpawnDamageNumber(finalDamage, ant);

        Destroy(gameObject);
    }

    private void SpawnDamageNumber(int dmg, AntHealth ant)
    {
        if (damageNumberPrefab == null || ant == null) return;

        Vector3 pos = ant.transform.position + damageNumberOffset;
        GameObject obj = Instantiate(damageNumberPrefab, pos, Quaternion.identity);

        DamageNumber dn = obj.GetComponent<DamageNumber>();
        if (dn != null)
            dn.Setup(dmg, Color.white);
    }
}
