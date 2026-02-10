using UnityEngine;

public class ProjectileScript : MonoBehaviour, IAttackInit
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _maxLifetime = 5f;
    [SerializeField] private GameObject _damageNumberPrefab;

    private int _damage = 10; // default 10 if no moveset set
    private GameObject _shooter;
    private float _lifetimeTimer = 0f;

    // Implement IAttackInit interface
    public void Init(AttackContext ctx)
    {
        _shooter = ctx.shooter;
        _damage = ctx.baseDamage;
    }

    void Update()
    {
        // move forward
        transform.position += transform.forward * _speed * Time.deltaTime;

        // destroy after max lifetime
        _lifetimeTimer += Time.deltaTime;
        if (_lifetimeTimer >= _maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AntHealth ant = other.GetComponent<AntHealth>();
        if (ant != null && !ant.IsDead())
        {
            // calculate final damage with multiplier
            int finalDamage = CalculateDamage(ant);

            MovesetSystem ms = _shooter != null ? _shooter.GetComponent<MovesetSystem>() : null;
            Debug.Log($"Shooter={_shooter?.name}, AntElement={ant.GetElement()}, Mult={ms?.GetDamageMultiplier(ant.GetElement())}");

            // deal damage
            ant.TakeDamage(finalDamage, _shooter);

            // show damage number
            ShowDamageNumber(other.transform.position, finalDamage, ant);

            // destroy bullet
            Destroy(gameObject);
        }
    }

    private int CalculateDamage(AntHealth target)
    {
        // start with base damage
        int finalDamage = _damage;

        // if we have a shooter with moveset system, use elemental calculations
        if (_shooter != null)
        {
            MovesetSystem movesetSystem = _shooter.GetComponent<MovesetSystem>();
            if (movesetSystem != null)
            {
                // get the ant's element
                ElementType antElement = target.GetElement();

                // get the damage multiplier
                float multiplier = movesetSystem.GetDamageMultiplier(antElement);

                // apply multiplier
                finalDamage = Mathf.RoundToInt(_damage * multiplier);
            }
        }

        return finalDamage;
    }

    private void ShowDamageNumber(Vector3 position, int damage, AntHealth target)
    {
        if (_damageNumberPrefab == null)
            return;

        Vector3 spawnPos = position + Vector3.up * 2f;
        GameObject damageObj = Instantiate(_damageNumberPrefab, spawnPos, Quaternion.identity);

        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            Color damageColor = GetEffectivenessColor(target);
            damageNumber.Setup(damage, damageColor);
        }
    }

    private Color GetEffectivenessColor(AntHealth target)
    {
        if (_shooter == null)
            return Color.white;

        MovesetSystem movesetSystem = _shooter.GetComponent<MovesetSystem>();
        if (movesetSystem == null)
            return Color.white;

        ElementType antElement = target.GetElement();
        float multiplier = movesetSystem.GetDamageMultiplier(antElement);

        if (multiplier > 1.0f)
            return new Color(0f, 1f, 0.3f); // Green for super effective
        else if (multiplier < 1.0f)
            return new Color(1f, 0.5f, 0.5f); // Pink for not very effective

        return Color.white; // Normal damage
    }
}