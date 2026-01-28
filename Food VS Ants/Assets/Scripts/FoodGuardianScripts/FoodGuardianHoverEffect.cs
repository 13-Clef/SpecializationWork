using UnityEngine;
using UnityEngine.UI;

public class FoodGuardianHoverEffect : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color _hoverColor = new Color(1f, 1f, 0.5f, 1f); // yellow glow
    [SerializeField] private float _outlineWidth = 5f;

    private Outline[] _outlines;
    private bool _isHovering = false;

    // Start is called before the first frame update
    void Start()
    {
        // get all outline components
        _outlines = GetComponentsInChildren<Outline>();

        // disable outline by default
        foreach (Outline outline in _outlines)
        {
            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    void OnMouseEnter()
    {
        // enable glow when mouse hovers
        _isHovering = true;
        SetOutlineEnabled(true);
    }

    void OnMouseExit()
    {
        // disable glow when mouse leaves
        _isHovering = false;
        SetOutlineEnabled(false);
    }

    void SetOutlineEnabled(bool enabled)
    {
        foreach (Outline outline in _outlines)
        {
            if (outline != null)
            {
                outline.enabled = enabled;
            }
        }
    }
}
