using UnityEngine;

[CreateAssetMenu(fileName = "BlackSmithData", menuName = "Scriptable Objects/BlackSmithData")]
public class BlackSmithData : ScriptableObject
{
    public int cost = 0;
    public bool canUpgrade = false;
    public string effect = "";
    public string upgrade = "";

    public bool canRepair = false;
    public float repartAmount = 0f;

    public bool addHP = false;
    public int health = 0;
    public int mana = 0;
}
