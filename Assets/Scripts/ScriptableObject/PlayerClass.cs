using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClass", menuName = "Items/PlayerClass")]
public class PlayerClass : ScriptableObject
{
    [Header("Info")]
    public int id = 0;
    public string class_Name = "";
    public string des = "";

    [Space]

    public Sprite icon;

    [Space]

    [Header("Stats")]

    public int cash = 25;

    [Space]

    public float health = 100f;
    public float mana = 100f;
    public int defense = 0;
    public float armor = 0f;
    public float regenArmor = 0f;

    [Space]

    public float speed = 5f;
    public float maxSpeed = 5f;

    [Space]
    public float range = 5f;

    [Space]

    [Header("Model")]

    public Sprite Head;
    public Sprite Hair;
    public Sprite Body;
    public Sprite Leg;
    public Sprite Hand;

    [Space]

    [Header("Starter Inventory")]

    public List<ItemInstance> loadout;
}
