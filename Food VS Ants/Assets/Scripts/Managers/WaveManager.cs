using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AntSpawner _antSpawner;

    [Header("Wave Settings")]
    [SerializeField] private int _currentWave = 0;
    [SerializeField] private int _maxWaves = 10;

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
    [SerializeField] private GameObject _waveCompletePanel;

    // wave state
    private bool _waveActive = false;
    private bool _isSpawning = false;

    // counters
    private int _antsToSpawn = 0;
    private int _antsSpawned = 0;
    private int _antsAlive = 0;
    private int _totalAntsThisWave = 0;

    // spawn timing
    private float _currentSpawnInterval;
    private float _spawnTimer = 0f;

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
        // spawn loop (WaveManager controls WHEN to call spawner)
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

        // move on to next wave
        _currentWave++;

        if (_currentWave >= _maxWaves)
        {
            GameComplete();
            return;
        }

        Debug.Log($"Starting Wave {_currentWave}!");

        // calculate wave difficulty
        _antsToSpawn = Mathf.RoundToInt(_baseAntsPerWave + (_antIncreasePerWave * (_currentWave - 1)));
        _totalAntsThisWave = _antsToSpawn; // store total for UI
        _currentSpawnInterval = Mathf.Max(_minSpawnInterval, _baseSpawnInterval - (_spawnIntervalDecreasePerWave * (_currentWave - 1)));


        // reset ant counters
        _antsSpawned = 0;
        _antsAlive = 0;
        _spawnTimer = 0f;

        // start ant wave
        _waveActive = true;
        _isSpawning = true;

        // hide wave complete panel
        if (_waveCompletePanel != null)
        {
            _waveCompletePanel.SetActive(false);
        }

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
            SpawnOneAnt();
            _spawnTimer = 0f;
        }
    }

    void SpawnOneAnt()
    {
        // AntSpawner decides: which ant type (weighted) + which lane (random)
        GameObject ant = _antSpawner.SpawnAntAndReturn();

        // If spawner failed (no pool/spawnpoints), do nothing
        if (ant == null) return;

        // Counters
        _antsSpawned++;
        _antsAlive++;

        // Track when this ant dies (OnDestroy)
        WaveAntTracker tracker = ant.AddComponent<WaveAntTracker>();
        tracker.Initialize(this);

        UpdateWaveUI();
    }

    public void OnAntDied()
    {
        _antsAlive = Mathf.Max(0, _antsAlive - 1);
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

        if (_waveText != null)
            _waveText.text = "VICTORY!";

        if (_enemiesRemainingText != null)
            _enemiesRemainingText.text = "";

        if (_startWaveButton != null)
            _startWaveButton.gameObject.SetActive(false);

        if (_waveCompletePanel != null)
            _waveCompletePanel.SetActive(false);
    }

    void UpdateWaveUI()
    {
        if (_waveText != null)
        {
            _waveText.text = $"Wave {_currentWave}";
        }

        if (_enemiesRemainingText != null)
        {
            if (_waveActive)
            {
                int remaining = _totalAntsThisWave - (_antsSpawned - _antsAlive);
                remaining = Mathf.Clamp(remaining, 0, _totalAntsThisWave);

                _enemiesRemainingText.text =
                    $"Ant: {remaining}/{_totalAntsThisWave}";
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

// Tracks ant death by detecting when the ant GameObject is destroyed
public class WaveAntTracker : MonoBehaviour
{
    private WaveManager _waveManager;

    public void Initialize(WaveManager manager)
    {
        _waveManager = manager;
    }

    private void OnDestroy()
    {
        if (_waveManager != null)
            _waveManager.OnAntDied();
    }
}
