using UnityEngine;

public class FoodGuardianScript : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float _attackRange = 10f;
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackRate = 1f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;

    private int _myLane; // which lane this food guardian is in
    private float _attackTimer = 0f;
    private AntScript _currentAntTarget;

    void Start()
    {
        // determine which lane this food guardian is in
        if (LaneSystem.Instance != null)
        {
            _myLane = LaneSystem.Instance.GetLaneFromPosition(transform.position);
            Debug.Log($"<color=green>Guardian '{gameObject.name}' in lane {_myLane} at position {transform.position}</color>");
        }
    }

    void Update()
    {
        // find target ant in the same lane
        FindAntTargetInLane();

        // attack if we have an ant target
        if (_currentAntTarget != null)
        {
            _attackTimer += Time.deltaTime;
            
            if (_attackTimer >= 1f / _attackRate)
            {
                Attack();
                _attackTimer = 0f;
            }
        }
    }

    void FindAntTargetInLane()
    {
        // when food guardian has an ant to target, only change if it
        // clear ant target if its dead or out of range
        if (_currentAntTarget == null || Vector3.Distance(transform.position, _currentAntTarget.transform.position) > _attackRange)
        {
            _currentAntTarget = null;
        }

        // find all ants
        GameObject[] ants = GameObject.FindGameObjectsWithTag("Ant");
        float closestDistance = _attackRange;

        foreach (GameObject antObj in ants)
        {
            AntScript ant = antObj.GetComponent<AntScript>();
            if (ant != null)
            {
                int antLane = ant.GetLane();  // Store the lane number
                float distance = Vector3.Distance(transform.position, ant.transform.position);

                Debug.Log($"Guardian(Lane {_myLane}) checking Ant(Lane {antLane}) - Match: {antLane == _myLane}, Distance: {distance:F2}");

                // check if ant is in the same lane
                if (antLane == _myLane)
                {
                    if (distance <= _attackRange && distance < closestDistance)
                    {
                        _currentAntTarget = ant;
                        closestDistance = distance;
                        Debug.Log($"<color=green>Guardian Lane {_myLane} LOCKED onto Ant Lane {antLane}</color>");
                    }
                }
            }
        }
    }

    void Attack()
    {
        if (_currentAntTarget == null) return;
        
        // spawn projectile
        if (_projectilePrefab != null && _firePoint != null)
        {
            GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity);
            ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();
            if (projScript != null)
            {
                projScript.SetAntTarget(_currentAntTarget.transform, (int)_attackDamage);
            }
        }
    }

    // draw range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}