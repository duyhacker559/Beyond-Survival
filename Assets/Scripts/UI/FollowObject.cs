using UnityEngine;

public class FollowObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeReference] public GameObject TargetObject;
    [SerializeField] bool followInstant = false;
    [SerializeField] Vector3 offset = Vector3.zero;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (followInstant)
            {
                transform.position = new Vector3(TargetObject.transform.position.x + offset.x, TargetObject.transform.position.y + offset.y, -100 + offset.z);
            }
            else
            {
                Vector3 targetCamPos = new Vector3(TargetObject.transform.position.x + offset.x, TargetObject.transform.position.y + offset.y, -100 + offset.z);
                transform.position = Vector3.Lerp(transform.position, targetCamPos, Time.deltaTime * 3f);
            }
        }
        catch
        {

        }
    }
}
