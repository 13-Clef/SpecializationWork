using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;

    private Transform _antTarget;
    private int _damage;

    public void SetAntTarget(Transform antTarget, int damage)
    {
        _antTarget = antTarget;
        _damage = damage;
    }

    // Update is called once per frame
    void Update()
    {
        if (_antTarget == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // move towards ant target
        Vector3 direction = (_antTarget.position - transform.position).normalized;
        transform.position += direction * _speed * Time.deltaTime;

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
