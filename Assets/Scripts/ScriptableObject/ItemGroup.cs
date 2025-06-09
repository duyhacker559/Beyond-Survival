using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemGroup", menuName = "Scriptable Objects/ItemGroup")]
public class ItemGroup : ScriptableObject
{
    [SerializeField] public List<WeightedItem<ItemInstance>> Items = new List<WeightedItem<ItemInstance>>();
}
