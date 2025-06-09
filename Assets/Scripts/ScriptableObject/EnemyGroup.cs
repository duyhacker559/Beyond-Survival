using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyGroup", menuName = "Scriptable Objects/EnemyGroup")]
public class EnemyGroup : ScriptableObject
{
    [SerializeField] public List<WeightedItem<EnemyData>> groups = new List<WeightedItem<EnemyData>>(); 
}
