using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    public static LaneSystem Instance; // singleton for easy access

    [Header("Lane Configuration")]
    [SerializeField] private int _numberOfLanes = 5;
    [SerializeField] private float _laneWidth = 3f; // width of each lane
    [SerializeField] private Vector3 _laneStartPosition = new Vector3(-10f, 0f, 0f);
    [SerializeField] private Vector3 _laneDirection = Vector3.right; // direction lanes go (right)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // get which lane a position is in
    public int GetLaneFromPosition(Vector3 position)
    {
        // calculate perpendicular distance from lane start
        Vector3 perpendicularDirection = Vector3.Cross(_laneDirection, Vector3.up).normalized;
        float perpDistance = Vector3.Dot(position - _laneStartPosition, perpendicularDirection);

        int lane = Mathf.FloorToInt(perpDistance / _laneWidth);
        return Mathf.Clamp(lane, 0, _numberOfLanes - 1);
    }

    // get center position of a specific lane at a given distance
    public Vector3 GetLaneCenterPosition(int laneIndex, float distanceAlongLane)
    {
        Vector3 perpendicularDirection = Vector3.Cross(_laneDirection, Vector3.up).normalized;
        Vector3 laneOffset = perpendicularDirection * (laneIndex * _laneWidth + _laneWidth / 2f);
        return _laneStartPosition + laneOffset + (_laneDirection * distanceAlongLane);
    }
}
