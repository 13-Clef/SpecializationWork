using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int _currentWave = 0;
    [SerializeField] private int _maxWaves = 10;
    [SerializeField] private float _timeBetweenWaves = 10f;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] _antPrefabs;
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Ant Type Distribution")]
    [SerializeField] private bool _useCustomDistribution = false;
    [SerializeField] private float[] _antTypeWeights;

    [Header("Wave Scaling")]
    [SerializeField] private int _baseAntsPerWave = 10;
    [SerializeField] private float _antIncreasePerWave = 5f;
    [SerializeField] private float _baseSpawnInterval = 2f;
    [SerializeField] private float _spawnIntervalDecreasePerWave = 0.1f;
    [SerializeField] private float _minSpawnInterval = 0.5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemiesRemainingText;
    [SerializeField] private Button _startWaveButton;
    [SerializeField] private GameObject _waveCompletePanel; // optional?

    // wave state
    private bool _waveActive = false;
    private bool _isSpawning = false;
    private int _antsToSpawn = 0;
    private int _antsSpawned = 0;
    private int _antsAlive = 0;
    private float _currentSpawnInterval;
    private float _spawnTimer = 0f;
    private float _waveBreakTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        UpdateWaveUI();

        // set up start wave button
        if (_startWaveButton != null)
        {
            _startWaveButton.onClick.AddListener(StartNextWave);
            _startWaveButton.gameObject.SetActive(true);
        }

        if (_waveCompletePanel != null)
        {
            _waveCompletePanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if wave is active and we are spawning
        if (_waveActive && _isSpawning)
        {
            SpawnAnts();
        }

        // check if wave is complete
        if (_waveActive && !_isSpawning && _antsAlive <= 0)
        {
            WaveComplete();
        }
    }

    public void StartNextWave()
    {
        if (_waveActive) return;

        _currentWave++;

        if (_currentWave >= _maxWaves)
        {
            GameComplete();
            return;
        }

        Debug.Log($"Starting Wave {_currentWave}!");

        // calculate wave difficulty
        _antsToSpawn = Mathf.RoundToInt(_baseAntsPerWave + (_antIncreasePerWave * (_currentWave - 1)));
        _currentSpawnInterval = Mathf.Max(_minSpawnInterval, _baseSpawnInterval - (_spawnIntervalDecreasePerWave * (_currentWave - 1)));

        // reset ant counters
        _antsSpawned = 0;
        _antsAlive = 0;
        _spawnTimer = 0f;

        // start ant wave
        _waveActive = true;
        _isSpawning = true;

        // hide start button
        if (_startWaveButton != null)
        {
            _startWaveButton.gameObject.SetActive(false);
        }

        UpdateWaveUI();
    }

    void SpawnAnts()
    {
        // check if there are enough ants spawned
        if (_antsSpawned >= _antsToSpawn)
        {
            _isSpawning = false;
            return;
        }

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= _currentSpawnInterval)
        {
            SpawnAnt();
            _spawnTimer = 0f;
        }
    }

    void SpawnAnt()
    {
        if (_antPrefabs == null || _antPrefabs.Length == 0 || _spawnPoints.Length == 0)
        {
            return;
        }

        // choose random lane (0-4)
        int randomLane = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[randomLane];

        // choose ant type (random or/and weighted)
        GameObject antPrefab = ChooseAntType();

        // spawn ant at that position
        GameObject ant = Instantiate(antPrefab, spawnPoint.position, spawnPoint.rotation);

        // track spawned ants
        _antsSpawned++;
        _antsAlive++;

        // add tracker component
        ant.AddComponent<WaveAntTracker>().Initialize(this);

        UpdateWaveUI();
    }

    GameObject ChooseAntType()
    {
        // if using custom distribution with weights
        if (_useCustomDistribution && _antTypeWeights != null && _antTypeWeights.Length == _antPrefabs.Length)
        {
            return GetWeightedRandomAnt();
        }
        else
        {
            // simple random selection
            int randomIndex = Random.Range(0, _antPrefabs.Length);
            return _antPrefabs[randomIndex];
        }
    }

    // weighted random selection (e.g. 70% normal, 20% fast, 10% tank)
    GameObject GetWeightedRandomAnt()
    {
        float totalWeight = 0f;
        foreach (float weight in _antTypeWeights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < _antPrefabs.Length; i++)
        {
            cumulativeWeight += _antTypeWeights[i];
            if (randomValue <= cumulativeWeight)
            {
                return _antPrefabs[i];
            }
        }

        // fallback (should never reach here)
        return _antPrefabs[0];
    }

    public void OnAntDied()
    {
        _antsAlive--;
        UpdateWaveUI();
    }

    void WaveComplete()
    {
        _waveActive = false;

        // show wave compelte panel
        if (_waveCompletePanel != null )
        {
            _waveCompletePanel.SetActive(true);
        }

        // show start next wave button
        if (_startWaveButton != null && _currentWave < _maxWaves)
        {
            _startWaveButton.gameObject.SetActive(true);
        }

        UpdateWaveUI();
    }

    void GameComplete()
    {
        Debug.Log("All waves complete! You win!");

        // TODO: Show victory screen
        if (_waveText != null)
        {
            _waveText.text = "VICTORY!";
        }

        if (_startWaveButton != null)
        {
            _startWaveButton.gameObject.SetActive(false);
        }
    }

    void UpdateWaveUI()
    {
        if (_waveText != null)
        {
            _waveText.text = $"Wave {_currentWave}/{_maxWaves}";
        }

        if (_enemiesRemainingText != null)
        {
            if (_waveActive)
            {
                _enemiesRemainingText.text = $"Enemies: {_antsAlive}/{_antsToSpawn}";
            }
            else
            {
                _enemiesRemainingText.text = "Prepare for next wave!";
            }
        }
    }
    // Public getters
    public int GetCurrentWave() => _currentWave;
    public int GetAntsAlive() => _antsAlive;
    public bool IsWaveActive() => _waveActive;
}

// Helper component to track when ants die
public class WaveAntTracker : MonoBehaviour
{
    private WaveManager _waveManager;

    public void Initialize(WaveManager manager)
    {
        _waveManager = manager;
    }

    void OnDestroy()
    {
        if (_waveManager != null)
        {
            _waveManager.OnAntDied();
        }
    }
}
