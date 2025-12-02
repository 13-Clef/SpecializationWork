using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (_mainCamera != null)
        {
            // always face the camera
            transform.LookAt(transform.position + _mainCamera.transform.forward);
        }
    }
}
