using UnityEngine;

public class RotateHighlight : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 50f;

    void Update()
    {
        // rotate around the Z-axis
        transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
    }
}