using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Transform textTransfrom;
    public int damage = 0;

    void Start()
    {
        textTransfrom.GetComponent<TextMeshProUGUI>().SetText(damage.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
