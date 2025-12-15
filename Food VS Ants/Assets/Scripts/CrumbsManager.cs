using UnityEngine;
using System;
using TMPro;

public class CrumbsManager : MonoBehaviour
{
    public static CrumbsManager Instance { get; private set; }

    [Header("Currency Settings")]
    [SerializeField] private int _startingCumbs = 100;
    [SerializeField] private int _currentCrumbs;
    [SerializeField] private TextMeshProUGUI _crumbsText;

    [Header("Generation Settings")]
    [SerializeField] private float _generationInterval = 5f; // generation per <?>
    [SerializeField] private float _generationSpeedUpgrade = 0.9f; // upgradable multiplier
    [SerializeField] private int _currentGenerationLevel = 1;

    [Header("Crumb Spawning Settings")]
    [SerializeField] private GameObject _crumbPrefab;
    [SerializeField] private Transform _spawnAreaMin; // set to be bottom-left corner
    [SerializeField] private Transform _spawnAreaMax; // set to be top-right corner
    [SerializeField] private int _maxCrumbsOnGround = 10;
    [SerializeField] private TextMeshProUGUI _maxSpawnText;

    private float _generationTimer = 0f;
    private int _crumbsOnGround = 0;

    void Awake()
    {
        // singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _currentCrumbs = _startingCumbs;

        // update current crumbs
        UpdateCrumbsUI(_currentCrumbs);

        // update spawned crumbs
        UpdateSpawnedCrumbsUI(_crumbsOnGround);
    }

    // Update is called once per frame
    void Update()
    {
        // crumbs generate over time
        _generationTimer += Time.deltaTime;

        float currentInterval = _generationInterval * Mathf.Pow(_generationSpeedUpgrade, _currentGenerationLevel - 1);

        if (_generationTimer >= currentInterval)
        {
            GenerateCrumbs();
            _generationTimer = 0f;
        }
    }

    public bool CanAfford(int cost)
    {
        return _currentCrumbs >= cost;
    }

    public bool DeductCrumbs(int amount)
    {
        if (CanAfford(amount))
        {
            _currentCrumbs -= amount;
            UpdateCrumbsUI(_currentCrumbs);
            return true;
        }
        return false;
    }

    void GenerateCrumbs()
    {
        // spawn physical crumbs on the ground if below max amount
        if (_crumbPrefab != null && _spawnAreaMin != null && _spawnAreaMax != null)
        {
            if (_crumbsOnGround < _maxCrumbsOnGround)
            {
                SpawnCrumbOnGround();
            }
        }
    }

    void SpawnCrumbOnGround()
    {
        // spawns crumb randomly in the platform area
        Vector3 randomPos = new Vector3(
            UnityEngine.Random.Range(_spawnAreaMin.position.x, _spawnAreaMax.position.x), _spawnAreaMin.position.y,
            UnityEngine.Random.Range(_spawnAreaMin.position.z, _spawnAreaMax.position.z));

        GameObject crumb = Instantiate(_crumbPrefab, randomPos, Quaternion.identity);
        _crumbsOnGround++;
        UpdateSpawnedCrumbsUI(_crumbsOnGround);
    }
    public void OnCrumbPickedUp()
    {
        _crumbsOnGround--;
        UpdateSpawnedCrumbsUI(_crumbsOnGround);
    }

    public void AddCrumbs(int amount)
    {
        _currentCrumbs += amount;
        UpdateCrumbsUI(_currentCrumbs);
        
    }

    // update the UI text
    void UpdateCrumbsUI(int crumbAmount)
    {
        if (_crumbsText != null)
        {
            _crumbsText.text = $"Crumbs: {crumbAmount}";
        }
    }

    // updates spawn UI text
    void UpdateSpawnedCrumbsUI(int maxSpawnedAmount)
    {
        if (_maxSpawnText != null)
        {
            _maxSpawnText.text = $"Spawned: {maxSpawnedAmount}/{_maxCrumbsOnGround}";
        }
    }

    // getters for UI
    public int GetCurrentCrumbs()
    {
        return _currentCrumbs;
    }

    public float GetGenerationProgress()
    {
        float currentInterval = _generationInterval * Mathf.Pow(_generationSpeedUpgrade, _currentGenerationLevel - 1);
        return _generationTimer / currentInterval;
    }

    public float GetTimeUntilNextGeneration()
    {
        float currentInterval = _generationInterval * Mathf.Pow(_generationSpeedUpgrade, _currentGenerationLevel - 1);
        return currentInterval - _generationTimer;
    }
}
