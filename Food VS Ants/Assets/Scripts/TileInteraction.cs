using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    public Ray tileDetectionRay;
    public float distance = 2f;
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        tileDetectionRay = new Ray(transform.position, transform.up * distance);
    }

    // Update is called once per frame
    void Update()
    {
        // check if ray hts a placeable surface
        if (Physics.Raycast(tileDetectionRay, out hit))
        {
            if (hit.collider.CompareTag("FoodGuardian"))
            {
                gameObject.tag = "Occupied";
            }
            else
            {
                gameObject.tag = "PlaceableTile";
            }
        }
    }

}
