using UnityEngine;

public enum AntVariant { Basic, Tank, Runner }

public class AntSpawner : MonoBehaviour
{
    [Header("Element Ant Prefabs (Fire / Water / Electric / Earth / Grass)")]
    [SerializeField] private GameObject[] _elementAntPrefabs;

    [Header("Lane Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Variant Weights (chance)")]
    [Range(0f, 100f)][SerializeField] private float _basicWeight = 70f;
    [Range(0f, 100f)][SerializeField] private float _tankWeight = 10f;
    [Range(0f, 100f)][SerializeField] private float _runnerWeight = 20f;

    [Header("Variant Multipliers")]
    [SerializeField] private float _tankHpMult = 2.5f;
    [SerializeField] private float _tankSpeedMult = 0.7f;
    [SerializeField] private float _runnerHpMult = 0.6f;
    [SerializeField] private float _runnerSpeedMult = 1.6f;

    [Header("Debug / Visual (Optional)")]
    [SerializeField] private bool _logSpawns = false;
    [SerializeField] private bool _tintByVariant = true;
    [SerializeField] private Color _tankTint = new Color(1f, 0.9f, 0.6f);
    [SerializeField] private Color _runnerTint = new Color(0.7f, 0.9f, 1f);

    // Called by WaveManager: spawns 1 ant (random lane + random element + random variant)
    public GameObject SpawnAntAndReturn()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogWarning("[AntSpawner] No spawn points assigned.");
            return null;
        }

        if (_elementAntPrefabs == null || _elementAntPrefabs.Length == 0)
        {
            Debug.LogWarning("[AntSpawner] No element ant prefabs assigned.");
            return null;
        }

        // 1) Pick a valid lane spawn point
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("[AntSpawner] All spawn points are null.");
            return null;
        }

        // 2) Pick a valid element prefab
        GameObject prefab = GetRandomElementPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[AntSpawner] All element prefabs are null.");
            return null;
        }

        // 3) Spawn ant
        GameObject ant = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // 4) Roll variant + apply stats
        AntVariant variant = RollVariant();
        ApplyVariant(ant, variant);

        // 5) Optional debug
        if (_logSpawns)
        {
            string elementName = prefab != null ? prefab.name : "UnknownElement";
            Debug.Log($"[AntSpawner] Spawned {variant} from {elementName} at lane {spawnPoint.name}");
        }

        return ant;
    }

    private Transform GetRandomSpawnPoint()
    {
        // Try a few times to avoid null entries
        for (int tries = 0; tries < 10; tries++)
        {
            Transform t = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            if (t != null) return t;
        }
        return null;
    }

    private GameObject GetRandomElementPrefab()
    {
        for (int tries = 0; tries < 10; tries++)
        {
            GameObject p = _elementAntPrefabs[Random.Range(0, _elementAntPrefabs.Length)];
            if (p != null) return p;
        }
        return null;
    }

    private AntVariant RollVariant()
    {
        // Make sure weights cannot go negative
        float basic = Mathf.Max(0f, _basicWeight);
        float tank = Mathf.Max(0f, _tankWeight);
        float runner = Mathf.Max(0f, _runnerWeight);

        float total = basic + tank + runner;

        // If designer accidentally sets all weights to 0, default to Basic
        if (total <= 0f) return AntVariant.Basic;

        float roll = Random.Range(0f, total);

        if (roll < basic) return AntVariant.Basic;
        roll -= basic;

        if (roll < tank) return AntVariant.Tank;
        return AntVariant.Runner;
    }

    private void ApplyVariant(GameObject ant, AntVariant variant)
    {
        if (ant == null) return;

        AntHealth health = ant.GetComponent<AntHealth>();
        AntMovement movement = ant.GetComponent<AntMovement>();

        // If these are missing, your prefab setup is incomplete
        if (health == null || movement == null)
        {
            Debug.LogWarning("[AntSpawner] Spawned ant missing AntHealth or AntMovement.");
            return;
        }

        switch (variant)
        {
            case AntVariant.Tank:
                health.MultiplyHealth(_tankHpMult);
                movement.MultiplySpeed(_tankSpeedMult);
                Tint(ant, _tankTint);
                break;

            case AntVariant.Runner:
                health.MultiplyHealth(_runnerHpMult);
                movement.MultiplySpeed(_runnerSpeedMult);
                Tint(ant, _runnerTint);
                break;

            default:
                // Basic: no stat changes
                break;
        }
    }

    private void Tint(GameObject ant, Color tint)
    {
        if (!_tintByVariant) return;

        // Tints all renderers (works for 3D models; harmless if none)
        Renderer[] rends = ant.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] != null && rends[i].material != null)
                rends[i].material.color = tint;
        }
    }
}
