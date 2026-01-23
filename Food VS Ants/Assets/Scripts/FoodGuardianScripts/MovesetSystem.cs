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
    public int unlockLevel = 1; // requires level 1 to unlock

    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // different projectile for each moveset
    public float attackRate = 1.0f; // attack speed
    public int baseDamage = 10; // base damage
}

public class MovesetSystem : MonoBehaviour
{
    [Header("Guardian Element Type")]
    [SerializeField] private ElementType _guardianElement = ElementType.Fire;

    [Header("4 Moveset (Fixed Order For Now)")]
    [SerializeField] private Moveset _basicNormal; // unlocks at level 1
    [SerializeField] private Moveset _basicElement; // unlocks at level 1
    [SerializeField] private Moveset _advancedNormal; // unlocks at level 3
    [SerializeField] private Moveset _advancedElement; // unlocks at level 5

    [Header("Current Moveset")]
    [SerializeField] private int _currentMovesetIndex = 0;

    private FoodGuardianScript _foodGuardianScript;
    private FoodGuardianLevelingSystem _levelingSystem;
    private Moveset _currentMoveset;
    private Moveset[] _allMovesets;

    void Start()
    {
        _foodGuardianScript = GetComponent<FoodGuardianScript>();
        _levelingSystem = GetComponent<FoodGuardianLevelingSystem>();

        // setup moveset array
        _allMovesets = new Moveset[4]
        {
            _basicNormal,
            _basicElement,
            _advancedNormal,
            _advancedElement
        };

        // set element types automatically
        if (_basicElement != null)
        {
            _basicElement.elementType = _guardianElement;
        }

        if (_advancedElement != null)
        {
            _advancedElement.elementType = _guardianElement;
        }

        // start with basic normal attack
        SetMoveset(0);
    }

    // change to a different moveset
    public void SetMoveset(int movesetIndex)
    {
        if (movesetIndex < 0 || movesetIndex >= _allMovesets.Length) // out of range moveset
        {
            return;
        }

        // check if moveset is unlocked
        if (!IsMovesetUnlocked(movesetIndex))
        {
            Moveset lockedMoveset = _allMovesets[movesetIndex];
            return;
        }

        _currentMovesetIndex = movesetIndex;
        _currentMoveset = _allMovesets[movesetIndex];

        // update Food Guardian's stats 
        if (_foodGuardianScript != null && _currentMoveset != null)
        {
            _foodGuardianScript.SetAttackRate(_currentMoveset.attackRate);
            _foodGuardianScript.SetProjectilePrefab(_currentMoveset.projectilePrefab);
            _foodGuardianScript.SetBaseDamage(_currentMoveset.baseDamage);
        }

        Debug.Log($"[{gameObject.name}] Switched to moveset: {_currentMoveset.movesetName} ({_currentMoveset.elementType})");
    }

    // check if a moveset is unlocked
    public bool IsMovesetUnlocked(int movesetIndex)
    {
        if (movesetIndex < 0 || movesetIndex >= _allMovesets.Length)
        {
            return false;
        }

        if (_allMovesets[movesetIndex] == null)
        {
            return false;
        }

        if (_levelingSystem == null)
        {
            return true; // if no leveling system, all unlocked
        }

        int currentLevel = _levelingSystem.GetCurrentLevel();
        int requiredLevel = _allMovesets[movesetIndex].unlockLevel;

        return currentLevel >= requiredLevel;
    }

    // get damage multiplier based on weakness cycle
    public float GetDamageMultiplier(ElementType targetElement)
    {
        if (_currentMoveset == null)
        {
            return 1.0f;
        }

        ElementType attackElement = _currentMoveset.elementType;

        // normal type has no advantages (1x dmg)
        if (attackElement == ElementType.None)
        {
            return 1.0f;
        }

        if (IsStrongAgainst(attackElement, targetElement))
        {
            return 1.5f; // 1.5x damage (super effective)
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
    public Moveset GetCurrentMoveset()
    {
        return _currentMoveset;
    }

    public int GetCurrentMovesetIndex()
    {
        return _currentMovesetIndex;
    }

    public ElementType GetCurrentElement()
    {
        if (_currentMoveset != null)
        {
            return _currentMoveset.elementType;
        }
        return ElementType.None;
    }

    public Moveset[] GetAllMovesets()
    {
        return _allMovesets;
    }

    public ElementType GetGuardianElement()
    {
        return _guardianElement;
    }

    // get count of unlocked movesets
    public int GetUnlockedMovesetCount()
    {
        int count = 0;
        for (int i = 0; i < _allMovesets.Length; i++)
        {
            if (IsMovesetUnlocked(i))
            {
                count++;
            }
        }
        return count;   
    }
}
