using UnityEngine;

public class NotifyScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Remove()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
