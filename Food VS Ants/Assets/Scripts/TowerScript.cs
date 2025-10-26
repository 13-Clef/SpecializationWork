using UnityEngine;

public class TowerScript : MonoBehaviour
{
    public Transform enemy;              // Reference to the enemy
    public GameObject bulletPrefab;      // Bullet prefab to shoot
    public Transform firePoint;          // Where bullets spawn from
    public float fireRate = 1f;          // Shots per second
    public float rotationSpeed = 5f;     // How fast tower rotates to face enemy

    private float fireTimer = 0f;

    void Start()
    {

    }

    void Update()
    {
        if (enemy == null) return;

        // Rotate tower to face the enemy
        Vector3 direction = enemy.position - transform.position;
        direction.y = 0; // Keep rotation on horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Fire at intervals
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Create bullet at fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Optional: Add velocity to bullet if it has a Rigidbody
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * 10f; // Adjust speed as needed
        }
    }
}