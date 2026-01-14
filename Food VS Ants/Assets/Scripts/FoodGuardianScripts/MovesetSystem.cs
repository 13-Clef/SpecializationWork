using UnityEngine;

// 5 elements
public enum ElementType
{
    None,
    Fire,
    Water,
    Electric,
    Earth,
    Grass
}

// moveset
[System.Serializable]
public class Moveset
{
    [Header("Moveset Info")]
    public string movesetName = "Basic Attack";
    public ElementType elementType = ElementType.None;
    //public Sprite movesetIcon;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Different projectile for each moveset
    public float attackRate = 1.0f; // Attack speed
    public int baseDamage = 10; // Base damage
}

public class MovesetSystem : MonoBehaviour
{
    [Header("Movesets Available")]
    [SerializeField] private Moveset[] _movesetsAvailable; // 4 moveset (for guardian)

    [Header("Moveset Currently")]
    [SerializeField] private int _currentMovesetIndex = 0; // current moveset selected

    private FoodGuardianScript _foodGuardianScript;
    private Moveset _currentMoveset;

    void Start()
    {
        _foodGuardianScript = GetComponent<FoodGuardianScript>();

        if (_movesetsAvailable.Length > 0)
        {
            SetMoveset(_currentMovesetIndex);
        }
    }

    // change to a different moveset
    public void SetMoveset(int movesetIndex)
    {
        if (movesetIndex < 0 || movesetIndex >= _movesetsAvailable.Length) // out of range moveset
        {
            return;
        }

        _currentMovesetIndex = movesetIndex;
        _currentMoveset = _movesetsAvailable[movesetIndex];

        // update Food Guardian's stats based on selected moveset
        if (_foodGuardianScript != null)
        {
            _foodGuardianScript.SetAttackRate(_currentMoveset.attackRate);
            _foodGuardianScript.SetProjectilePrefab(_currentMoveset.projectilePrefab);
            _foodGuardianScript.SetBaseDamage(_currentMoveset.baseDamage);
        }

        Debug.Log($"[{gameObject.name}] Switched to moveset: {_currentMoveset.movesetName} ({_currentMoveset.elementType})");
    }

    // Cycle to next moveset (for testing or UI button)
    public void CycleMoveset()
    {
        int nextIndex = (_currentMovesetIndex + 1) % _movesetsAvailable.Length;
        SetMoveset(nextIndex);
    }

    // get damage multiplier based on weakness cycle
    public float GetDamageMultiplier(ElementType targetElement)
    {
        ElementType attackElement = _currentMoveset.elementType;

        // check weakness cycle (fire < water < electric < earth < grass)
        if (IsStrongAgainst(attackElement, targetElement))
        {
            return 2.0f; // 2x damage (super effective)
        }
        else if (IsWeakAgainst(attackElement, targetElement))
        {
            return 0.5f; // 0.5x damage (not very effective)
        }

        return 1.0f; // normal damage
    }

    bool IsStrongAgainst(ElementType attacker, ElementType target)
    {
        // fire > grass > earth > electric > water > fire (cycle)
        switch (attacker)
        {
            case ElementType.Fire:
                return target == ElementType.Grass;
            case ElementType.Grass:
                return target == ElementType.Earth;
            case ElementType.Earth:
                return target == ElementType.Electric;
            case ElementType.Electric:
                return target == ElementType.Water;
            case ElementType.Water:
                return target == ElementType.Fire;
            default:
                return false;
        }
    }

    bool IsWeakAgainst(ElementType attacker, ElementType target)
    {
        // fire < water < electric < grass <  earth (weakness cycle)
        switch (attacker)
        {
            case ElementType.Fire:
                return target == ElementType.Water;
            case ElementType.Water:
                return target == ElementType.Electric;
            case ElementType.Electric:
                return target == ElementType.Earth;
            case ElementType.Earth:
                return target == ElementType.Grass;
            case ElementType.Grass:
                return target == ElementType.Fire;
            default:
                return false;
        }
    }

    // getters
    public Moveset GetCurrentMoveset() => _currentMoveset;
    public int GetCurrentMovesetIndex() => _currentMovesetIndex;
    public ElementType GetCurrentElement() => _currentMoveset.elementType;
    public Moveset[] GetAllMovesets() => _movesetsAvailable;
}
