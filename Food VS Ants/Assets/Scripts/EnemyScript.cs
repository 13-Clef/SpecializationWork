using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float radius = 2f;        // Radius of the circle
    public float speed = 2f;         // Speed of rotation

    private Vector3 centerPosition;  // Center point of the circle
    private float angle = 0f;        // Current angle

    void Start()
    {
        // Store the starting position as the center of the circle
        centerPosition = transform.position;
    }

    void Update()
    {
        // Increment the angle based on speed and time
        angle += speed * Time.deltaTime;

        // Calculate new position using circular motion
        float x = centerPosition.x + Mathf.Cos(angle) * radius;
        float z = centerPosition.z + Mathf.Sin(angle) * radius;

        // Update the enemy's position
        transform.position = new Vector3(x, centerPosition.y, z);
    }
}