using UnityEngine;

public class AntSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _antPrefab;
    [SerializeField] private float _spawnInterval = 3f;

    [Header("Lane Settings")]
    [SerializeField] private Transform[] _spawnPoints; // 5 spawns (one per lane)

    private float _spawnTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _spawnTimer += Time.deltaTime;

        // check if its time to spawn
        if (_spawnTimer >= _spawnInterval)
        {
            SpawnAnt();
            _spawnTimer = 0f;
        }
    }

    void SpawnAnt()
    {
        if (_antPrefab == null || _spawnPoints.Length == 0) return;

        // choose a random lane out of the 5
        int randomLane = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[randomLane];

        // spawn the ant at that lane's spawn point
        GameObject ant = Instantiate(_antPrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log("Enemy spawned in lane: " + randomLane);
    }
}
