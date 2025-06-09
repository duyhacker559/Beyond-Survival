using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum Target
{
    Player,
    Enemy
}

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Items/ProjectileItem")]
public class ProjectileItem : ScriptableObject
{
    public float damage = 0f;
    public float knockBack = 0f;
    public float knockBackDuration = 0f;

    public bool applyAll = false;
    public bool areaAttack = false;
    public GameObject hitbox;
    public List<DamageEffect> effects = new List<DamageEffect>();
    public AreaAttackItem areaAttackDat;

    [Space]

    public bool emitEffect = false;
    public GameObject animEffect;

    [Space]

    public bool trap = false;

    [Space]

    public bool canPierc = false;
    public int piercAmount = 0;

    [Space]
    public bool overrideTarget = false;
    public Target target;

    [Space]
    public bool overrideDeathAnim = false;
    public string deathAnim = "";
}