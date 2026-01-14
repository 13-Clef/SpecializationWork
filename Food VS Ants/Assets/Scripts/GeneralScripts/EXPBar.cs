using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EXPBar : MonoBehaviour
{
    [Header("EXP Bar Settings")]
    [SerializeField] private Image _expBarFill;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private Slider _slider;

    private int _currentEXP;
    private int _expRequired;

    // set the max EXP required for next level
    public void SetEXPRequired(int expRequired)
    {
        _expRequired = expRequired;
        _slider.maxValue = _expRequired;
        UpdateEXPBar();
    }

    // set the current EXP amount
    public void SetCurrentEXP(int currentEXP)
    {
        _currentEXP = Mathf.Clamp(currentEXP, 0, _expRequired);
        _slider.value = _currentEXP;
        UpdateEXPBar();
    }

    // update both EXP values at once
    public void UpdateEXP(int currentEXP, int expRequired)
    {
        _currentEXP = Mathf.Clamp(currentEXP, 0, expRequired);
        _expRequired = expRequired;

        _slider.maxValue = _expRequired;
        _slider.value = _currentEXP;

        UpdateEXPBar();
    }

    // show MAX level
    public void SetMaxLevel()
    {
        _slider.value = _slider.maxValue;

        if (_expBarFill != null)
        {
            _expBarFill.fillAmount = 1f;
        }

        if (_expText != null)
        {
            _expText.text = "MAX";
        }
    }

    void UpdateEXPBar()
    {
        // update fill amount
        if (_expBarFill != null)
        {
            float expPercent = 0f;

            if (_expRequired > 0)
            {
                expPercent = (float)_currentEXP / _expRequired;
            }

            _expBarFill.fillAmount = expPercent;
        }

        // update text
        if (_expText != null)
        {
            _expText.text = $"{_currentEXP}/{_expRequired} EXP";
        }
    }

    // get current progress as percentage (0 to 1 fill)
    public float GetProgress()
    {
        if (_expRequired > 0)
        {
            return (float)_currentEXP / _expRequired;
        }
        else
        {
            return 0f;
        }
    }
}
