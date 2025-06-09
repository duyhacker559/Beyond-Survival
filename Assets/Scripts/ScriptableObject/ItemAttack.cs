using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemAttack", menuName = "Scriptable Objects/ItemAttack")]
public class ItemAttack : ScriptableObject
{
    [SerializeField] public List<EnemyMeleeAttack> meleeAttacks;
    [Space]
    [SerializeField] public List<EnemyRangedAttack> rangedAttacks;
}