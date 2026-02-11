using UnityEngine;

public class AntMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 2f;
    [SerializeField] private Vector3 _moveDirection = Vector3.back;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    private bool _isMoving = false;

    void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _moveDirection = _moveDirection.normalized;
    }

    public void StartMoving()
    {
        _isMoving = true;
        if (_animator != null)
        {
            _animator.SetBool("WalkBool", true);
        }
    }

    public void StopMoving()
    {
        _isMoving = false;
        if (_animator != null)
        {
            _animator.SetBool("WalkBool", false);
        }
    }

    public void ContinueMoving()
    {
        if (!_isMoving)
        {
            StartMoving();
        }

        transform.position += _moveDirection * _movementSpeed * Time.deltaTime;
    }

    public void DisableMovement()
    {
        _movementSpeed = 0f;
        StopMoving();
    }

    public void MultiplySpeed(float mult)
    {
        mult = Mathf.Max(0.1f, mult);
        _movementSpeed *= mult;
    }
}
