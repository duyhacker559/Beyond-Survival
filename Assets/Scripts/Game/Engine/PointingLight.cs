using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PointingLight : MonoBehaviour
{

    [SerializeField] float OffsetRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent.parent != null)
        {
            Vector3 euler = transform.parent.parent.parent.eulerAngles;
            euler.z += OffsetRotation;

            transform.rotation = Quaternion.Euler(euler);
        }  
    }
}
