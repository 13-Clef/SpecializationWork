using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    private TextMeshPro _text;
    private float _timer = 0f;
    private float _lifetime = 1.5f;

    void Awake()
    {
        _text = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        // float upwards
        transform.position += Vector3.up * 2f * Time.deltaTime;

        // fade out
        _timer += Time.deltaTime;
        float alpha = 1f - (_timer / _lifetime);
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);

        // face camera
        if (Camera.main != null)
        {
           transform.rotation = Camera.main.transform.rotation;
        }

        // destroy when done
        if (_timer >= _lifetime)
        {
            Destroy(gameObject);
        }
    }

    // new setup with color support
    public void Setup(int damage, Color color)
    {
        _text.text = damage.ToString();
        _text.color = color;
        _timer = 0f;
    }
}