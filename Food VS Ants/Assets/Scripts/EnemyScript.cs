using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float movementSpeed = 2f;         // Speed of rotation
    public Vector3 moveDirection = Vector3.back;  // Moves in -Z direction

    void Start()
    {
        // Make sure the direction is normalized
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        // Move in a straight line in the specified direction
        transform.position += moveDirection * movementSpeed * Time.deltaTime;

        // Optional: Make enemy face the direction it's moving
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}