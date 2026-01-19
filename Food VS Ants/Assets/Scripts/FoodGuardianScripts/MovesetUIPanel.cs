using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovesetUIPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject _panelObject;

    [Header("Stat Display")]
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _attackRateText;
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("EXP Bar")]
    [SerializeField] private EXPBar _expBar;
    [SerializeField] private TextMeshProUGUI _expText;

    [Header("Moveset Buttons")]
    [SerializeField] private Button _basicNormalButton;
    [SerializeField] private Button _basicElementButton;
    [SerializeField] private Button _advancedNormalButton;
    [SerializeField] private Button _advancedElementButton;

    [Header("Moveset Button Text")]
    [SerializeField] private TextMeshProUGUI _basicNormalText;
    [SerializeField] private TextMeshProUGUI _basicElementText;
    [SerializeField] private TextMeshProUGUI _advancedNormalText;
    [SerializeField] private TextMeshProUGUI _advancedElementText;

    [Header("Lock Overlays (For Advanceds)")]
    [SerializeField] private GameObject _advancedNormalLock;
    [SerializeField] private GameObject _advancedElementLock;

    [Header("Close Button")]
    [SerializeField] private Button _closeButton;

    private FoodGuardianScript _currentGuardian;
    private MovesetSystem _currentMovesetSystem;
    private FoodGuardianLevelingSystem _currentLevelingSystem;

    // Start is called before the first frame update
    void Start()
    {
        // hide panel on start
        HidePanel();

        // setup button listeners
        if (_basicNormalButton != null)
        {
            _basicNormalButton.onClick.AddListener(() => SelectMoveset(0));
        }

        if (_basicElementButton != null)
        {
            _basicElementButton.onClick.AddListener(() => SelectMoveset(1));
        }

        if (_advancedNormalButton != null)
        {
            _advancedNormalButton.onClick.AddListener(() => SelectMoveset(2));
        }

        if (_advancedElementButton != null)
        {
            _advancedElementButton.onClick.AddListener(() => SelectMoveset(3));
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(HidePanel);
        }
    }

    public void ShowPanel(GameObject foodGuardian)
    {
        if (foodGuardian == null)
        {
            return;
        }

        // get references from the food guardian
        _currentGuardian = foodGuardian.GetComponent<FoodGuardianScript>();
        _currentMovesetSystem = foodGuardian.GetComponent<MovesetSystem>();
        _currentLevelingSystem = foodGuardian.GetComponent<FoodGuardianLevelingSystem>();

        if (_currentGuardian == null || _currentMovesetSystem == null)
        {
            return;
        }

        // show panel
        if (_panelObject != null)
        {
            _panelObject.SetActive(true);
        }

        // update all displays
        UpdateStatsDisplay();
        UpdateMovesetButtons();
        UpdateEXPDisplay();
    }

    public void HidePanel()
    {
        if (_panelObject != null)
        {
            _panelObject.SetActive(false);
        }

        _currentGuardian = null;
        _currentMovesetSystem = null;
        _currentLevelingSystem = null;
    }

    void SelectMoveset(int movesetIndex)
    {
        if (_currentMovesetSystem == null)
        {
            return;
        }

        // check if moveset is unlocked
        if (_currentMovesetSystem.IsMovesetUnlocked(movesetIndex))
        {
            _currentMovesetSystem.SetMoveset(movesetIndex);
            UpdateMovesetButtons(); // refresh button states
            UpdateStatsDisplay(); // update stats since damage might change
        }
    }

    // Update is called once per frame
    void UpdateStatsDisplay()
    {
        if (_currentGuardian == null)
        {
            return;
        }

        // update damage
        if (_damageText != null)
        {
            int damage = _currentGuardian.GetDamage();
            _damageText.text = $"{damage} DMG";
        }

        // update health
        if (_healthText != null)
        {
            int currentHealth = _currentGuardian.GetCurrentHealth();
            int maxHealth = _currentGuardian.GetMaxHealth();
            _healthText.text = $"{currentHealth}/{maxHealth} HP";
        }

        // update attack rate
        if (_attackRateText != null)
        {
            float attackRate = _currentGuardian._foodGuardianAttackRate;
            _attackRateText.text = $"{attackRate:F1} Sec";
        }

        // update level
        if (_levelText != null && _currentLevelingSystem != null)
        {
            int level = _currentLevelingSystem.GetCurrentLevel();
            _levelText.text = $"LVL {level}";
        }
    }

    void UpdateMovesetButtons()
    {
        if (_currentMovesetSystem == null)
        {
            return;
        }

        int currentMovesetIndex = _currentMovesetSystem.GetCurrentMovesetIndex();
        Moveset[] allMovesets = _currentMovesetSystem.GetAllMovesets();

        // update each button
        UpdateButton(_basicNormalButton, _basicNormalText, 0, currentMovesetIndex, allMovesets);
        UpdateButton(_basicElementButton, _basicElementText, 1, currentMovesetIndex, allMovesets);
        UpdateButton(_advancedNormalButton, _advancedNormalText, 2, currentMovesetIndex, allMovesets);
        UpdateButton(_advancedElementButton, _advancedElementText, 3, currentMovesetIndex, allMovesets);

        // update lock overlays
        if (_advancedNormalLock != null)
        {
            bool isUnlocked = _currentMovesetSystem.IsMovesetUnlocked(2);
            _advancedNormalLock.SetActive(!isUnlocked);
        }

        if (_advancedElementLock != null)
        {
            bool isUnlocked = _currentMovesetSystem.IsMovesetUnlocked(3);
            _advancedElementLock.SetActive(!isUnlocked);
        }
    }

    void UpdateButton(Button button, TextMeshProUGUI buttonText, int movesetIndex, int currentIndex, Moveset[] movesets)
    {
        if (button == null)
        {
            return;
        }

        bool isUnlocked = _currentMovesetSystem.IsMovesetUnlocked(movesetIndex);
        bool isSelected = currentIndex == movesetIndex;

        // set interactable state
        button.interactable = isUnlocked;

        // update button text
        if (buttonText != null && movesets[movesetIndex] != null)
        {
            string movesetName = movesets[movesetIndex].movesetName;
            buttonText.text = movesetName;
        }

        // visual feedback for selected button
        ColorBlock colors = button.colors;
        if (isSelected)
        {
            colors.normalColor = new Color(1f, 1f, 0.5f); // yellowish tint
        }
        else
        {
            colors.normalColor = Color.white;
        }
        button.colors = colors;
    }

    void UpdateEXPDisplay()
    {
        if (_currentLevelingSystem == null)
        {
            return;
        }

        if (_currentLevelingSystem.IsMaxLevel())
        {
            _expBar.SetMaxLevel();
        }
        else
        {
            int currentEXP = _currentLevelingSystem.GetCurrentEXP();
            int requiredEXP = _currentLevelingSystem.GetEXPRequiredForNextLevel();
            _expBar.UpdateEXP(currentEXP, requiredEXP);
        }

        // update EXP Text
        if (_expText != null)
        {
            if (_currentLevelingSystem.IsMaxLevel())
            {
                _expText.text = "MAX";
            }
            else
            {
                int currentEXP = _currentLevelingSystem.GetCurrentEXP();
                int requiredEXP = _currentLevelingSystem.GetEXPRequiredForNextLevel();
                _expText.text = $"{currentEXP}/{requiredEXP} EXP";
            }
        }
    }

    void Update()
    {
        // continuously update stats while panel is open
        if (_panelObject != null && _panelObject.activeSelf && _currentGuardian != null)
        {
            UpdateStatsDisplay();
            UpdateEXPDisplay();
        }
    }

    public bool IsPanelOpen()
    {
        return _panelObject != null && _panelObject.activeSelf;
    }
}
