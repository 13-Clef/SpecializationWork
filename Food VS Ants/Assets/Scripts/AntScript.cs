using Unity.VisualScripting;
using UnityEngine;

public class AntScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 2f;
    [SerializeField] private Vector3 _moveDirection = Vector3.back; // move in the -Z direction to the food guardians

    [Header("Health Settings")]
    [SerializeField] private int _health = 100;

    void Start()
    {
        // normalize direction to make sure consistent speed
        _moveDirection = _moveDirection.normalized;
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

    public void TakeDamage(int damage)
    {
        if (_health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
        Debug.Log("Ant Died!");
    }

    //// Optional: Destroy enemy if it reaches the end
    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("EndPoint"))
    //    {
    //        // Enemy reached the end, damage player or destroy
    //        Destroy(gameObject);
    //        Debug.Log("Enemy reached the end!");
    //    }
    //}
}