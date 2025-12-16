using System.Collections.Generic;
using UnityEngine;

public class AntDetectorScript : MonoBehaviour
{
    [SerializeField] private int _rowNumber; // which lane this detector belongs to
    //[SerializeField] private TestGuardianScript _foodGuardian;

    private List<AntScript> _antsInLane = new List<AntScript>();

    private void OnTriggerEnter(Collider other)
    {
        // check if an ant entered the lane
        AntScript antScript = other.GetComponent<AntScript>();
        if (antScript != null && !_antsInLane.Contains(antScript))
        {
            _antsInLane.Add(antScript);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // check if an ant left the lane
        AntScript antScript = other.GetComponent<AntScript>();
        if (antScript != null && _antsInLane.Contains(antScript))
        {
            _antsInLane.Remove(antScript);
        }
    }

    // clean up null references (for ants that were destroyed)
    void Update()
    {

    }
}
