using System.Collections.Generic;
using UnityEngine;

public class GreaseInfernoDamage : MonoBehaviour
{
    [Header("Damage Over Time")]
    public float dps = 25f;
    public float tickInterval = 0.2f;

    [Header("Damage Source (for EXP credit)")]
    public GameObject damageSource;

    private readonly HashSet<AntHealth> _targets = new HashSet<AntHealth>();
    private float _tickTimer;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer < tickInterval) return;
        _tickTimer = 0f;

        if (_targets.Count == 0) return;

        float raw = dps * tickInterval;
        int dmg = raw > 0f ? Mathf.Max(1, Mathf.RoundToInt(raw)) : 0;
        if (dmg <= 0) return;

        var toRemove = (List<AntHealth>)null;

        foreach (var ant in _targets)
        {
            if (ant == null || ant.IsDead())
            {
                (toRemove ??= new List<AntHealth>()).Add(ant);
                continue;
            }

            ant.TakeDamage(dmg, damageSource);
        }

        if (toRemove != null)
            foreach (var dead in toRemove) _targets.Remove(dead);
    }

    private void OnTriggerEnter(Collider other)
    {
        var ant = other.GetComponentInParent<AntHealth>();
        if (ant != null) _targets.Add(ant);
    }

    private void OnTriggerExit(Collider other)
    {
        var ant = other.GetComponentInParent<AntHealth>();
        if (ant != null) _targets.Remove(ant);
    }
}
