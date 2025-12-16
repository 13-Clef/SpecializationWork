using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _bulletLife = 3f;
    [SerializeField] private int _damage = 10;

    // Start is called before the first frame update
    void Start()
    {
        // destroy bullet if it does not touch wall or still alive
        Destroy(gameObject, _bulletLife);   
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * _projectileSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ant"))
        {
            // deal damage
            AntScript ant = other.GetComponent<AntScript>();
            if (ant != null)
            {
                ant.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }

        if (other.gameObject.CompareTag("BulletWall"))
        {
            Destroy(gameObject);
        }
    }
}
