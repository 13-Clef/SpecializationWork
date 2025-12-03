using UnityEngine;
using System;

public class CrumbsManager : MonoBehaviour
{
    public static CrumbsManager Instance { get; private set; }

    [Header("Currency Settings")]
    [SerializeField] private int _startingCumbs = 100;
    [SerializeField] private int _currentCrumbs;

    [Header("Generation Settings")]
    [SerializeField] private float _generationInterval = 5f; // generation per <?>
    [SerializeField] private int _crumbsPerGeneration = 10;
    [SerializeField] private float _generationSpeedUpgrade = 0.9f; // upgradable multiplier
    [SerializeField] private int _currentGenerationLevel = 1;

    [Header("Crumb Spawning Settings")]
    [SerializeField] private GameObject _crumbPrefab;
    [SerializeField] private Transform _spawnAreaMin; // set to be bottom-left corner
    [SerializeField] private Transform _spawnAreaMax; // set to be top-right corner
    [SerializeField] private int _maxCrumbsOnGround = 10;

    private float _generationTimer = 0f;
    private int _crumbsOnGround = 0;

    // evemt for UI updates
    public event Action<int> OnCrumbsChanged;

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
        OnCrumbsChanged?.Invoke(_currentCrumbs);
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

        //// set up the crumb's value
        //CrumbPickup pickup = crumb.GetComponent<CrumbPickup>();
        //if (pickup != null)
        //{
        //    pickup.SetValue(_crumbsPerGeneration);
        //}


    }
    public void OnCrumbPickedUp()
    {
        _crumbsOnGround--;
    }

    public void AddCrumbs(int amount)
    {
        _currentCrumbs += amount;
        OnCrumbsChanged?.Invoke(_currentCrumbs);
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
