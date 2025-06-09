using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LootPackage
{
    [SerializeField] public List<ItemInstance> Items;
}

[System.Serializable]
public class EnemyMeleeAttack
{
    public float delay = 0f;
    public float waitTime = 0f;
    public int soundIndex = -1;
    public int animIndex = -1;
    [SerializeField] public float Damage = 10f;
    [SerializeField] public float Range = 5f;
    [SerializeField] public float Attack_Rate = 1f;
    [SerializeField] public float Attack_KB = 0f;
    [SerializeField] public float Attack_KBDuration = 0f;
    [SerializeField] public int Attack_Amount = 1;
    [SerializeField] public float Attack_Reload = 0f;
    [SerializeField] public List<DamageEffect> Attack_Effect = new List<DamageEffect>();
    [SerializeField] public GameObject Attack_Hitbox;
    [SerializeField] public AreaAttackItem areaAttackDat;
    [SerializeField] public float Detect_Rate = 0.1f;
    public int clipSize = 1;
    [SerializeField] public Target group;
}

[System.Serializable]
public class EnemyRangedAttack
{
    public float delay = 0f;
    public float waitTime = 0f;
    public int soundIndex = -1;
    public int animIndex = -1;
    [SerializeField] public float Damage = 10f;
    [SerializeField] public float Range = 5f;
    [SerializeField] public float Attack_Rate = 1f;
    [SerializeField] public float Attack_KB = 0f;
    [SerializeField] public float Attack_KBDuration = 0f;
    [SerializeField] public int Attack_Amount = 1;
    [SerializeField] public float Attack_Reload = 0f;
    [SerializeField] public List<DamageEffect> Attack_Effect = new List<DamageEffect>();
    [SerializeField] public GameObject Attack_Hitbox;
    [SerializeField] public float Detect_Rate = 0.1f;
    [SerializeField] public Target group;

    public float ranged_spread = 0f;
    public float ranged_bulletSpeed = 5f;
    public float ranged_bulletLifetime = 5f;
    public int clipSize = 1;
    public int emit = 1;
    public ProjectileItem ranged_projectileDat;
    public GameObject ranged_projectile;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Items/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] public string ID;
    [SerializeField] public int Point = 100;

    [SerializeField] public float Health = 80;
    [SerializeField] public float Speed = 5f;

    [SerializeField] public int Armor = 0;
    [SerializeField] public int Defense = 0;

    public bool haveLifeTime = false;
    public float lifeTime = 0f;

    public bool isBoss = false;
    public float stun_Resistance = 0f;
    public float huntsDelay = 2f;

    [SerializeField] public List<EnemyMeleeAttack> meleeAttacks;
    [Space]
    [SerializeField] public List<EnemyRangedAttack> rangedAttacks;
    [Space]

    [SerializeField] public float WaitTime = 0.5f;

    [SerializeField] public GameObject EnemyModel;

    [SerializeField] public float Detection_Range = 10f;
    [SerializeField] public float Detection_Rate = 0.2f;

    [SerializeField] public float Chasing_Time_Threshold = 0.2f;

    [SerializeField] public string path = "";

    [SerializeField] public List<WeightedItem<LootPackage>> Loot;

    public bool canExplode = false;
    public float explode_Damage = 0f;
    public float explode_KnockBack = 0f;
    public float explode_KnockBackDuration = 0f;
    public GameObject explode_Emit;
    public GameObject explode_Hitbox;
    public AreaAttackItem explode_HitboxData;
    public List<DamageEffect> explode_Effect = new List<DamageEffect>();

    public bool cloacked = false;
    public float unCloakRange = 1f;

    public bool holdEnemy = false;
    public float holdRange = 1f;
}
