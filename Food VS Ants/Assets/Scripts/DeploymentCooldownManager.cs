using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeploymentCooldownManager : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [SerializeField] private float _cooldownDuration = 5f;

    [Header("UI References")]
    [SerializeField] private Image[] _slotCooldownOverlays; // dark overlays images (making for 5 slots)
    [SerializeField] private TextMeshProUGUI[] _slotCooldownTexts; // timer text (making for 5 slots)

    private float[] _slotCooldownTimers; // remaining time for each slot
    private bool[] _slotReady; // tracks slots that are ready to be placed

    // Start is called before the first frame update
    void Start()
    {
        // initialise arrays for the slots
        _slotCooldownTimers = new float[5];
        _slotReady = new bool[5];

        // all slots start ready
        for (int i = 0; i < 5; i++)
        {
            _slotCooldownTimers[i] = 0f;
            _slotReady[i] = true;

            // hide the overlays at start
            if (i < _slotCooldownOverlays.Length && _slotCooldownOverlays[i] != null)
            {
                _slotCooldownOverlays[i].fillAmount = 0f;
            }

            // hide text at start
            if (i < _slotCooldownTexts.Length && _slotCooldownTexts[i] != null)
            {
                _slotCooldownTexts[i].gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAllCooldowns();
    }

    void UpdateAllCooldowns()
    {
        for (int i = 0; i < 5; i++)
        {
            // if slot is on cooldown (not ready)
            if (!_slotReady[i])
            {
                // decrease the timers
                _slotCooldownTimers[i] -= Time.deltaTime;

                // check if cooldown finished (ready)
                if (_slotCooldownTimers[i] <= 0f)
                {
                    _slotReady[i] = true;
                    _slotCooldownTimers[i] = 0f;

                    // hide overlay when cooldown is completed
                    if (i < _slotCooldownOverlays.Length && _slotCooldownOverlays[i] != null)
                    {
                        _slotCooldownOverlays[i].fillAmount = 0f;
                    }

                    // hide text when cooldown is completed
                    if (i < _slotCooldownTexts.Length && _slotCooldownTexts[i] != null)
                    {
                        _slotCooldownTexts[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    // cooldown still active, update the UI to show progress
                    UpdateCooldownUI(i);
                }
            }
        }
    }

    void UpdateCooldownUI(int slotIndex)
    {
        // calculate fill ammount (1 = full fill, 0 = empty fill)
        float fillAmount = _slotCooldownTimers[slotIndex] / _cooldownDuration;

        // update overlay image
        if (slotIndex < _slotCooldownOverlays.Length && _slotCooldownOverlays[slotIndex] != null)
        {
            _slotCooldownOverlays[slotIndex].fillAmount = fillAmount;
        }

        // update text
        if (slotIndex < _slotCooldownTexts.Length && _slotCooldownTexts[slotIndex] != null)
        {
            _slotCooldownTexts[slotIndex].gameObject.SetActive(true);
            _slotCooldownTexts[slotIndex].text = Mathf.Ceil(_slotCooldownTimers[slotIndex]).ToString(); // rounds up numbers to whole numbers
        }
    }

    // called when a tower has been placed
    public void StartCooldown(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 5)
        {
            _slotReady[slotIndex] = false;
            _slotCooldownTimers[slotIndex] = _cooldownDuration;

            // show overlay at full and countdown text when cooldown starts
            _slotCooldownOverlays[slotIndex].fillAmount = 1f;
            _slotCooldownTexts[slotIndex].gameObject.SetActive(true);
        }
    }

    // check if a slot is ready for placement
    public bool IsSlotReadyForPlacement(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 5)
        {
            return _slotReady[slotIndex];
        }
        return false;
    }

    // get remaining cooldown time for a slot
    public float GetRemainingCooldown(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 5)
        {
            return _slotCooldownTimers[slotIndex];
        }
        return 0f;
    }
}
