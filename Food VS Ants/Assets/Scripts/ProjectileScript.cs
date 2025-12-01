using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _maxLifetime = 5f; // timer for bullet to despawn
    [SerializeField] private float _projectileSpeed = 10f; 
    
    private Transform _antTarget;
    private int _damage;
    private float _lifetimeTimer = 0f;

    public void SetAntTarget(Transform antTarget, int damage)
    {
        _antTarget = antTarget;
        _damage = damage;
    }

    // Update is called once per frame
    void Update()
    {
        // destroy projectile if its not destroyed within 5 seconds
        _lifetimeTimer += Time.deltaTime;
        if (_lifetimeTimer >= _maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // if the targeted ant still exist
        if (_antTarget == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // move towards ant target
        Vector3 direction = (_antTarget.position - transform.position).normalized;
        transform.position += direction * _projectileSpeed * Time.deltaTime;

        // check if reached ant target
        if (Vector3.Distance(transform.position, _antTarget.position) < 0.5f)
        {
            // deal damage
            AntScript ant = _antTarget.GetComponent<AntScript>();
            if (ant != null)
            {
                ant.TakeDamage(_damage);
            }

            Destroy(gameObject);
        }

        // destroy if out of bound
        if (transform.position.z >= 30)
        {
            Destroy(gameObject);
        }
    }
}
