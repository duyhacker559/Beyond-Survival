using UnityEngine;

public class LoadOut : MonoBehaviour
{

    [SerializeField] static public GameObject MainWP;
    [SerializeField] static public GameObject Item1;
    [SerializeField] static public GameObject Item2;
    [SerializeField] static public GameObject Item3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainWP = transform.Find("Weapon").gameObject;
        Item1 = transform.Find("L1").gameObject;
        Item2 = transform.Find("L2").gameObject;
        Item3 = transform.Find("L3").gameObject;
    }
}
