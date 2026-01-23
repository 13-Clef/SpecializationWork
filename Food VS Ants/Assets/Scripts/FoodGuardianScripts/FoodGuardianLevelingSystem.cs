using UnityEngine;
using TMPro;

public class FoodGuardianLevelingSystem : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int _maxLevel = 5;
    [SerializeField] private int _currentLevel = 1;

    [Header("EXP Settings")]
    [SerializeField] private int _currentEXP = 0;
    [SerializeField] private int _baseEXPRequired = 100;
    [SerializeField] private float _expScaling = 1.5f;

    [Header("Stat Scaling Per Level")]
    [SerializeField] private int _healthIncreasePerLevel = 50;

    [Header("UI Display Settings")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _expText;

    [Header("References")]
    [SerializeField] private EXPBar _expBar;
    private MovesetSystem _movesetSystem;

    // references to other scripts
    private FoodGuardianScript _foodGuardianScript;
    private int _expRequiredForNextLevel;

    // base stats (stored at start)
    private int _baseMaxHealth;

    // Start is called before the first frame update
    void Start()
    {
        _foodGuardianScript = GetComponent<FoodGuardianScript>();
        _movesetSystem = GetComponent<MovesetSystem>();

        // store base stats
        _baseMaxHealth = _foodGuardianScript.GetMaxHealth();

        // calculate EXP needed for next level
        _expRequiredForNextLevel = CalculateEXPForLevel(_currentLevel + 1);

        if (_expBar != null)
        {
            _expBar.SetEXPRequired(_expRequiredForNextLevel);
            _expBar.SetCurrentEXP(_currentEXP);
        }

        // update UI
        UpdateLevelUI();
    }

    // called when an ant dies 
    public void GainEXP(int amount)
    {
        if (_currentLevel >= _maxLevel)
        {
            return; // food guardian is already max level
        }

        // gain exp if not max
        _currentEXP += amount;

        // check if leveled up
        while (_currentEXP >= _expRequiredForNextLevel && _currentLevel < _maxLevel)
        {
            LevelUpFoodGuardian();
        }

        UpdateLevelUI();
    }

    void LevelUpFoodGuardian()
    {
        _currentLevel++;
        _currentEXP -= _expRequiredForNextLevel; // for carrying over extra EXP

        // increase stats after leveling up
        IncreaseStats();

        // calculate next level requirements
        if (_currentLevel < _maxLevel)
        {
            _expRequiredForNextLevel = CalculateEXPForLevel(_currentLevel + 1);
        }

        // check moveset unlock
        CheckMovesetUnlock();

        // visual feedback
        // ShowLevelUpEffect();
    }

    void IncreaseStats()
    {
        // calculate new max health based on current level
        int newMaxHealth = _baseMaxHealth + (_healthIncreasePerLevel * (_currentLevel - 1));

        // apply new stats afterwards
        _foodGuardianScript.SetMaxHealth(newMaxHealth);

        // heal to full when leveling up
        _foodGuardianScript.HealToFull();
    }

    void CheckMovesetUnlock()
    {
        if (_movesetSystem == null)
        {
            return;
        }

        // check which movesets unlock at this level
        if (_currentLevel == 3)
        {
            Debug.Log($"<color=yellow>[{gameObject.name}] NEW MOVE UNLOCKED: Advanced Normal Attack!</color>");
            // TODO: Show UI notification
        }
        else if (_currentLevel == 5)
        {
            Debug.Log($"<color=yellow>[{gameObject.name}] NEW MOVE UNLOCKED: Advanced Element Attack!</color>");
            // TODO: Show UI notification
        }
    }

    int CalculateEXPForLevel(int level)
    {
        return Mathf.RoundToInt(_baseEXPRequired * Mathf.Pow(_expScaling, level - 2));
    }

    //void ShowLevelUpEffect()
    //{

    //}

    void UpdateLevelUI()
    {
        if (_levelText != null)
        {
            _levelText.text = $"Lvl. {_currentLevel}";
        }

        if (_expBar != null)
        {
            if (IsMaxLevel())
            {
                _expBar.SetMaxLevel();
            }
            else
            {
                _expBar.UpdateEXP(_currentEXP, _expRequiredForNextLevel);
            }
        }

        if (_expText != null)
        {
            if (IsMaxLevel())
            {
                _expText.text = "MAX";
            }
            else
            {
                _expText.text = $"{_currentEXP}/{_expRequiredForNextLevel} EXP";
            }
        }

    }

    public int GetCurrentEXP() => _currentEXP;
    public int GetEXPRequiredForNextLevel() => _expRequiredForNextLevel;
    public bool IsMaxLevel() => _currentLevel >= _maxLevel;
    public float GetEXPProgress() => (float)_currentEXP / _expRequiredForNextLevel; 
    public int GetCurrentLevel() => _currentLevel;
    
}
