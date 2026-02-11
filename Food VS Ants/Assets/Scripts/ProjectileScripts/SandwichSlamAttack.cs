using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SandwichSlamAttack : MonoBehaviour, IAttackInit
{
    [Header("Lifetime (Hitbox Object)")]
    [SerializeField] private float lifetime = 0.35f;

    [Header("Optional Wind-up")]
    [SerializeField] private float slamDelay = 0.0f; // set >0 if you want a wind-up

    [Header("Offset (in front of firepoint)")]
    [SerializeField] private float forwardOffset = 1.0f;

    [Header("Slam VFX")]
    [SerializeField] private GameObject slamVFXPrefab;
    [SerializeField] private float vfxSafetyLifetime = 3.0f;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    [Header("Damage")]
    [SerializeField] private GameObject damageSource;
    [SerializeField] private int baseDamage = 20;

    [Header("Testing / Debug")]
    [SerializeField] private bool debugLogs = true;

    private MovesetSystem _movesetSystem;
    private Collider _col;

    // One slam instance should only damage each ant once.
    private readonly HashSet<AntHealth> _hitAnts = new HashSet<AntHealth>();

    // Prevent double-starting the routine if Init is called twice (rare but possible).
    private bool _started = false;

    // Keeps track of last VFX per shooter so next slam deletes previous VFX.
    private static readonly Dictionary<int, GameObject> LastVfxByShooterId = new Dictionary<int, GameObject>();

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        // hitbox cleanup
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void OnEnable()
    {
        if (debugLogs)
            Debug.Log($"[Slam] Spawned instanceID={gameObject.GetInstanceID()} frame={Time.frameCount}", this);
    }

    public void Init(AttackContext ctx)
    {
        damageSource = ctx.shooter;
        baseDamage = ctx.baseDamage;

        _movesetSystem = null;
        if (ctx.shooter != null)
            _movesetSystem = ctx.shooter.GetComponent<MovesetSystem>();

        // Spawn at firepoint, pushed forward
        if (ctx.firePoint != null)
        {
            transform.position = ctx.firePoint.position + ctx.firePoint.forward * forwardOffset;
            transform.rotation = ctx.firePoint.rotation;
        }

        HandleVfx(ctx.shooter);

        if (!_started)
        {
            _started = true;
            StartCoroutine(SlamRoutine());
        }
        else
        {
            if (debugLogs)
                Debug.Log("[Slam] WARNING: Init called twice on same slam instance.", this);
        }
    }

    private IEnumerator SlamRoutine()
    {
        if (slamDelay > 0f)
            yield return new WaitForSeconds(slamDelay);

        // Then let Enter/Stay catch late arrivals during the remaining lifetime.
        // (If you don’t want late arrivals to get hit, comment out OnTriggerStay below.)
    }

    // --- Trigger logic (Enter + Stay safety) ---

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other, "Enter");
    }

    private void TryDamage(Collider other, string from)
    {
        if (other == null) return;

        AntHealth ant = other.GetComponentInParent<AntHealth>();
        if (ant == null || ant.IsDead()) return;

        // The core fix: even if Enter+Stay fire multiple times, damage once per ant
        if (!_hitAnts.Add(ant))
        {
            if (debugLogs)
                Debug.Log($"[Slam] Skipped duplicate hit ({from}) on {ant.name}", this);
            return;
        }

        int dmg = CalculateDamage(ant);
        ant.TakeDamage(dmg, damageSource);
        SpawnDamageNumber(dmg, ant);

        if (debugLogs)
            Debug.Log($"[Slam] Damaged {ant.name} for {dmg} via {from}", this);
    }

    // --- VFX handling (delete previous on next slam) ---

    private void HandleVfx(GameObject shooter)
    {
        if (slamVFXPrefab == null || shooter == null) return;

        int id = shooter.GetInstanceID();

        if (LastVfxByShooterId.TryGetValue(id, out GameObject oldVfx) && oldVfx != null)
            Destroy(oldVfx);

        // Spawn VFX at slam position
        GameObject vfx = Instantiate(slamVFXPrefab, transform.position, transform.rotation);

        if (vfxSafetyLifetime > 0f)
            Destroy(vfx, vfxSafetyLifetime);

        LastVfxByShooterId[id] = vfx;
    }

    // --- Damage helpers ---

    private int CalculateDamage(AntHealth target)
    {
        int dmg = baseDamage;

        if (_movesetSystem != null)
        {
            float mult = _movesetSystem.GetDamageMultiplier(target.GetElement());
            dmg = Mathf.Max(1, Mathf.RoundToInt(dmg * mult));
        }

        return dmg;
    }

    private void SpawnDamageNumber(int dmg, AntHealth ant)
    {
        if (damageNumberPrefab == null || ant == null) return;

        Vector3 spawnPos = ant.transform.position + damageNumberOffset;
        GameObject obj = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);

        DamageNumber dn = obj.GetComponent<DamageNumber>();
        if (dn != null)
            dn.Setup(dmg, GetEffectivenessColor(ant));
    }

    private Color GetEffectivenessColor(AntHealth target)
    {
        if (_movesetSystem == null) return Color.white;

        float mult = _movesetSystem.GetDamageMultiplier(target.GetElement());

        if (mult > 1.0f) return new Color(0f, 1f, 0.3f);
        if (mult < 1.0f) return new Color(1f, 0.5f, 0.5f);
        return Color.white;
    }
}
