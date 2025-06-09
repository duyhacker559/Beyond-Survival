using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Scriptable Objects/ShopData")]
public class ShopData : ScriptableObject
{
    [SerializeField] public bool canSell = false;
    [SerializeField] public List<Trade> shop;
}
