using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

[System.Serializable]
public class DamageEffect
{
    public bool _show = false;
    public float timer = 0f;
    public float tick = 1f;
    public float lifeTime = 0f;

    public string id = "";
    public int power = 1;
    public bool canStack = false;
    public bool canOveride = false;

    public List<Modify> effects = new List<Modify>();

    public DamageEffect(float lifeTime, float tick, List<Modify> effects)
    {
        this.lifeTime = lifeTime;
        this.tick = tick;
        this.effects = new List<Modify>();
        timer = 0f;
        foreach (Modify item in effects)
        {
            Modify modify = new Modify();
            modify.modify_BoolValue = item.modify_BoolValue;
            modify.modify_FloatValue = item.modify_FloatValue;
            modify.modify_IntValue = item.modify_IntValue;
            modify.modify_StringValue = item.modify_StringValue;
            modify.modify_Des = item.modify_Des;
            modify.modify_ID = item.modify_ID;

            effects.Add(modify);
        }
    }

    public DamageEffect(DamageEffect effect)
    {
        this.lifeTime = effect.lifeTime;
        this.tick = effect.tick;
        this.effects = new List<Modify>();
        timer = 0f;
        foreach (Modify item in effect.effects)
        {
            Modify modify = new Modify();
            modify.modify_BoolValue = item.modify_BoolValue;
            modify.modify_FloatValue = item.modify_FloatValue;
            modify.modify_IntValue = item.modify_IntValue;
            modify.modify_StringValue = item.modify_StringValue;
            modify.modify_Des = item.modify_Des;
            modify.modify_ID = item.modify_ID;

            effects.Add(modify);
        }
    }
}


[System.Serializable]
public class AttackScript
{
    public MonoBehaviour Script;
    public Collider2D Collider;
}

[System.Serializable]
public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    QuestItem,
    Building
}

[System.Serializable]
public enum WeaponType
{
    Melee,
    Shooting
}

public enum ArmorModelType
{
    Texture,
    Model
}

public enum ArmorType
{
    Head,
    Body,
    Pant
}

[System.Serializable]
public enum ValueType
{
    Int,
    Float,
    Bool,
    String
}

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemType;
    public ItemType _itemType;
    public Sprite icon;
    public string itemID;          
    public string itemDescription;

    public KeyCode inputKey = KeyCode.E;

    public GameObject model;

    // Storage Stats

    public float weight = 0f;

    public bool canSell = false;
    public int value = 0;
    public int amountThreshold = 1;

    public float lifeTime = 2;
    public bool haveLifeTime = true;

    public ItemAttack attack;

    // Wearable Stats

    public ArmorModelType armor_modelType;
    public GameObject armor_Model;
    public Sprite armor_Sprite;

    public bool hide_Hair = false;

    public ArmorType armorType = ArmorType.Head;
    public int wearSlot = 0;
    public float armor = 0f;
    public float armor_regen = 0f;
    public float defense = 0f;

    public float range = 0f;
    public float speed = 0f;

    // Consuming Stats

    // Pickup Stats
    public bool useOnDelete = false;

    public bool isConsumable = false;
    public string effectDescription = "";

    public List<Modify> consumeEffect = new List<Modify>();

    // Attack Stats
    public bool isWeapon = false;
    public bool canAttack = false;

        // Animation

        public float swing = 0f;
        public float recoil = 0f;
        public float swingOffset = 0f;

        // Stats

        public float damage = 0f;
        public float cooldown = 0f;
        public float knockBack = 0f;
        public float knockBack_Duration = 1f;
        public float userKnockBack = 0f;
        public float userKnockBack_Duration = 1f;
        public int durability = 100;
        public WeaponType weaponType;

        public List<DamageEffect> effects = new List<DamageEffect>(); 

        // Melee
        public bool canMelee = false;
        public GameObject hitbox;
        public AreaAttackItem hitboxData;
        public float mele_manaConsume = 0f;
        // Shooting

        public bool canShoot = false;
        public int fireAmount = 1;
        public int maxAmmo = 1;
        public float spread = 0f;
        public float bulletSpeed = 5f;
        public float bulletLifetime = 5f;
        public float shooting_manaConsume = 0f;
        public float reload_manaConsume = 0f;

        public int clipSize = 0;
        public float reload = 0f;
        public int emit = 0;

        public GameObject projectile;
        public ProjectileItem projectileDat;
        public string projectilePath = "";

        // Other
        public List<MonoBehaviour> options;
}
