using UnityEngine;

public class AntScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 2f;
    [SerializeField] private Vector3 _moveDirection = Vector3.back; // move in the -Z direction to the food guardians

    [Header("Health Settings")]
    [SerializeField] private int _health = 100;

    private int _currentLane; // which lane this ant is in
    void Start()
    {
        // normalize direction to make sure consistent speed
        _moveDirection = _moveDirection.normalized;
        
        // determine which lane this ant is in
        if (LaneSystem.Instance != null)
        {
            _currentLane = LaneSystem.Instance.GetLaneFromPosition(transform.position);
            Debug.Log($"<color=yellow>Ant spawned in lane {_currentLane} at position {transform.position}</color>");
        }
    }

    void Update()
    {
        // move in a straight line in the specified direction
        transform.position += _moveDirection * _movementSpeed * Time.deltaTime;
        
        // delete if go into void
        if (transform.position.y <= -5f)
        {
            Destroy(gameObject);
        }
    }

    public int GetLane()
    {
        return _currentLane;
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}