using UnityEngine;

public class AntElement : MonoBehaviour
{
    [Header("Ant Element Type")]
    [SerializeField] private ElementType _elementType = ElementType.None;
    public ElementType GetElementType()
    {
        return _elementType;
    }

    public void SetElement(ElementType newType)
    {
        _elementType = newType;
    }
}
