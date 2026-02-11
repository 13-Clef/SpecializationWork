using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class JuiceSplashProjectile : MonoBehaviour, IAttackInit
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;

    [Header("Impact Behaviour")]
    [SerializeField] private bool destroyOnAnyHit = false; // set TRUE if you want it to die on walls/ground too

    [Header("Splash Damage")]
    [SerializeField] private float splashRadius = 2.2f;
    [SerializeField, Range(0f, 1f)] private float splashMultiplier = 0.5f;
    [SerializeField] private LayerMask antMask = ~0; // set to Ant layer

    [Header("Impact VFX (VFX-only prefab)")]
    [SerializeField] private GameObject juiceSplashVfxPrefab;
    [SerializeField] private float vfxLifetime = 2.0f; // safety cleanup if VFX doesn't auto-destroy

    [Header("Damage Numbers")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    private int damage;
    private GameObject damageSource;
    private MovesetSystem movesetSystem;

    private bool _hit;

    private Collider _col;
    private Rigidbody _rb;

    public void Init(AttackContext ctx)
    {
        // Even if Init isn't called, projectile still moves; Init just supplies damage + source.
        damage = ctx.baseDamage;
        damageSource = ctx.shooter;
        movesetSystem = (damageSource != null) ? damageSource.GetComponent<MovesetSystem>() : null;
    }

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();

        if (_col != null) _col.isTrigger = true;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        // Always clean up eventually
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (_hit) return;
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hit) return;

        // If you want it to disappear when touching *anything* (ground/walls/etc)
        // this prevents lingering projectiles.
        if (destroyOnAnyHit)
        {
            _hit = true;
            Vector3 anyHitPoint = other.ClosestPoint(transform.position);
            SpawnImpactVfx(anyHitPoint);
            KillImmediate();
            return;
        }

        // Otherwise: only destroy when it hits an ant
        AntHealth main = other.GetComponentInParent<AntHealth>();
        if (main == null || main.IsDead()) return;

        _hit = true;

        Vector3 hitPoint = other.ClosestPoint(transform.position);

        SpawnImpactVfx(hitPoint);

        // Full damage to main target
        DealDamageToAnt(main, 1.0f);

        // Splash to others
        DoSplash(hitPoint, main);

        KillImmediate();
    }

    private void DoSplash(Vector3 center, AntHealth mainAlreadyHit)
    {
        if (splashRadius <= 0.01f || splashMultiplier <= 0f) return;

        Collider[] hits = Physics.OverlapSphere(center, splashRadius, antMask, QueryTriggerInteraction.Collide);

        HashSet<AntHealth> damaged = new HashSet<AntHealth> { mainAlreadyHit };

        foreach (var h in hits)
        {
            AntHealth ant = h.GetComponentInParent<AntHealth>();
            if (ant == null || ant.IsDead()) continue;

            if (!damaged.Add(ant)) continue; // dedupe multi-collider ants

            DealDamageToAnt(ant, splashMultiplier);
        }
    }

    private void DealDamageToAnt(AntHealth ant, float extraMult)
    {
        // If Init wasn't called, damage defaults to 0. Clamp to at least 1 so it still works.
        float mult = extraMult;

        if (movesetSystem != null)
            mult *= movesetSystem.GetDamageMultiplier(ant.GetElement());

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * mult));

        ant.TakeDamage(finalDamage, damageSource);
        SpawnDamageNumber(finalDamage, ant);
    }

    private void SpawnImpactVfx(Vector3 hitPoint)
    {
        if (juiceSplashVfxPrefab == null) return;

        // Safety: VFX prefab must NOT have this projectile script
        if (juiceSplashVfxPrefab.GetComponent<JuiceSplashProjectile>() != null)
        {
            Debug.LogError("Assign a VFX-only prefab (particles only) to JuiceSplashVfxPrefab.", this);
            return;
        }

        GameObject vfx = Instantiate(juiceSplashVfxPrefab, hitPoint, Quaternion.identity);

        if (vfxLifetime > 0f)
            Destroy(vfx, vfxLifetime);
    }

    private void KillImmediate()
    {
        if (_col != null) _col.enabled = false;

        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        Destroy(gameObject);
    }

    private void SpawnDamageNumber(int dmg, AntHealth ant)
    {
        if (damageNumberPrefab == null || ant == null) return;

        Vector3 pos = ant.transform.position + damageNumberOffset;
        GameObject obj = Instantiate(damageNumberPrefab, pos, Quaternion.identity);

        DamageNumber dn = obj.GetComponent<DamageNumber>();
        if (dn != null)
            dn.Setup(dmg, GetEffectivenessColor(ant));
    }

    private Color GetEffectivenessColor(AntHealth target)
    {
        if (movesetSystem == null) return Color.white;

        float multiplier = movesetSystem.GetDamageMultiplier(target.GetElement());
        if (multiplier > 1.0f) return new Color(0f, 1f, 0.3f);
        if (multiplier < 1.0f) return new Color(1f, 0.5f, 0.5f);
        return Color.white;
    }

    private void OnDrawGizmosSelected()
    {
        if (splashRadius > 0.01f)
            Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
