using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class GreaseInfernoZoneDamage : MonoBehaviour, IAttackInit
{
    [Header("Tick Settings")]
    [SerializeField] private float tickInterval;

    [Header("Zone Lifetime")]
    [SerializeField] private float lifetime = 2.5f;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    [Header("Debug")]
    [SerializeField] private GameObject damageSource;
    [SerializeField] private int baseDamagePerTick;

    private MovesetSystem _movesetSystem;
    private float _timer;

    private readonly HashSet<AntHealth> _targets = new HashSet<AntHealth>();

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
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    public void Init(AttackContext ctx)
    {
        damageSource = ctx.shooter;
        baseDamagePerTick = ctx.baseDamage;

        _movesetSystem = null;
        if (ctx.shooter != null)
            _movesetSystem = ctx.shooter.GetComponent<MovesetSystem>();

        if (ctx.firePoint != null)
        {
            transform.position = ctx.firePoint.position;
            transform.rotation = ctx.firePoint.rotation;
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < tickInterval) return;
        _timer = 0f;

        _targets.RemoveWhere(a => a == null || a.IsDead());
        if (_targets.Count == 0) return;

        foreach (AntHealth ant in _targets)
        {
            int dmg = GetDamagePerTick(ant);
            if (dmg <= 0) continue;

            ant.TakeDamage(dmg, damageSource);
            SpawnDamageNumber(dmg, ant);
        }
    }

    private int GetDamagePerTick(AntHealth ant)
    {
        int dmg = baseDamagePerTick;

        if (_movesetSystem != null)
        {
            float mult = _movesetSystem.GetDamageMultiplier(ant.GetElement());
            dmg = Mathf.Max(1, Mathf.RoundToInt(dmg * mult));
        }

        return dmg;
    }

    private void SpawnDamageNumber(int dmg, AntHealth ant)
    {
        if (damageNumberPrefab == null || ant == null)
            return;

        Vector3 spawnPos = ant.transform.position + damageNumberOffset;
        GameObject obj = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);

        DamageNumber dn = obj.GetComponent<DamageNumber>();
        if (dn != null)
        {
            Color color = GetEffectivenessColor(ant);
            dn.Setup(dmg, color);
        }
    }

    private Color GetEffectivenessColor(AntHealth target)
    {
        if (_movesetSystem == null)
            return Color.white;

        float multiplier = _movesetSystem.GetDamageMultiplier(target.GetElement());

        if (multiplier > 1.0f)
            return new Color(0f, 1f, 0.3f);      // Green (super effective)
        else if (multiplier < 1.0f)
            return new Color(1f, 0.5f, 0.5f);    // Pink (not very effective)

        return Color.white; // Normal
    }

    private void OnTriggerEnter(Collider other)
    {
        AntHealth ant = other.GetComponentInParent<AntHealth>();
        if (ant != null && !ant.IsDead())
            _targets.Add(ant);
    }

    private void OnTriggerExit(Collider other)
    {
        AntHealth ant = other.GetComponentInParent<AntHealth>();
        if (ant != null)
            _targets.Remove(ant);
    }
}
