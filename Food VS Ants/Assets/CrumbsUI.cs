using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CrumbsUI : MonoBehaviour
{
    [Header("Crumbs User Interface")]
    [SerializeField] private TextMeshProUGUI _crumbsText;
    [SerializeField] private TextMeshProUGUI _nextGenerationText;
    [SerializeField] private Image _generationProgressBar;

    [Header("Display Settings")]
    [SerializeField] private string _currencyPrefix = "Crumbs: ";

    void Start()
    {
        if (CrumbsManager.Instance != null)
        {
            // sub to currency changes
            CrumbsManager.Instance.OnCrumbsChanged += UpdateCrumbsDisplay;

            // update the initial display
            UpdateCrumbsDisplay(CrumbsManager.Instance.GetCurrentCrumbs());
        }
    }

    void Update()
    {
        if (CrumbsManager.Instance != null)
        {
            UpdateGenerationDisplay();
        }
    }

    void UpdateCrumbsDisplay(int amount)
    {
        if (_crumbsText != null)
        {
            _crumbsText.text = _currencyPrefix + amount.ToString();
        }
    }

    void UpdateGenerationDisplay()
    {
        // constantly update timer text
        if (_nextGenerationText != null)
        {
            float timeRemaining = CrumbsManager.Instance.GetTimeUntilNextGeneration();
            _nextGenerationText.text = $"Next: {timeRemaining:F1}s";
        }

        // update progress bar
        if (_generationProgressBar != null)
        {
            float progress = CrumbsManager.Instance.GetGenerationProgress();
            _generationProgressBar.fillAmount = progress;
        }
    }

    void OnDestroy()
    {
        // unsub to prevent memory leaks
        if (CrumbsManager.Instance != null)
        {
            CrumbsManager.Instance.OnCrumbsChanged -= UpdateCrumbsDisplay;
        }
    }
}