using UnityEngine;

public class CrumbPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int _crumbValue = 10;
    [SerializeField] private bool _autoRotate = true;
    [SerializeField] private float _rotationSpeed = 50f;
    [SerializeField] private bool _bobUpDown = true;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _bobHeight = 0.3f;

    private Vector3 _startPosition;
    //private AudioSource _audioSource;

    void Start()
    {
        _startPosition = transform.position;
        //_audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // rotate crumb for visual appeal
        if (_autoRotate)
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        // bob up and down
        if (_bobUpDown)
        {
            float newY = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public void SetValue(int value)
    {
        _crumbValue = value;
    }

    void OnTriggerEnter(Collider other)
    {
        // check if player picked the crumbs
        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        // add crumbs to player
        if (CrumbsManager.Instance != null)
        {
            CrumbsManager.Instance.AddCrumbs(_crumbValue);
            CrumbsManager.Instance.OnCrumbPickedUp();
        }

        Destroy(gameObject);
    }
}