using UnityEngine;

public class AntElement : MonoBehaviour
{
    [Header("Ant Element Type")]
    [SerializeField] private ElementType _antElement = ElementType.Fire;

    public ElementType GetElement()
    {
        return _antElement;
    }

    public void SetElement(ElementType element)
    {
        _antElement = element;
    }
}
