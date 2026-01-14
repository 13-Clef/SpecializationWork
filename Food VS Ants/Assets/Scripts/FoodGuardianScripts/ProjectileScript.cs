using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _bulletLife = 3f;
    [SerializeField] private int _damage = 10;

    // track which food guardian shot this projectile
    private GameObject _shooter;

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

    // allow food guardian to set custom damage based on level
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    // set who shot this projectile
    public void SetShooter(GameObject shooter)
    {
        _shooter = shooter;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ant"))
        {
            // deal damage
            AntHealth antHealth = other.GetComponent<AntHealth>();
            if (antHealth != null)
            {
                antHealth.TakeDamage(_damage, _shooter);
                Destroy(gameObject);
            }
        }

        if (other.gameObject.CompareTag("BulletWall"))
        {
            Destroy(gameObject);
        }
    }

    int CalculateElementalDamage(AntHealth ant)
    {
        if (_shooter == null)
        {
            return _damage;
        }

        // get moveset system from shooter
        MovesetSystem movesetSystem = _shooter.GetComponent<MovesetSystem>();
        if (movesetSystem == null)
        {
            return _damage; // No elemental system, return base damage
        }

        // get damage multiplier based on elements
        ElementType antElement = ant.GetElement();
        float multiplier = movesetSystem.GetDamageMultiplier(antElement);

        // calculate final damage
        int finalDamage = Mathf.RoundToInt(_damage * multiplier);

        return finalDamage;
    }
}
