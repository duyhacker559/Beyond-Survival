using NUnit.Framework;
using System.Runtime.InteropServices.ComTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static Game Instance;

    public static GameObject g_enemies;
    public static GameObject g_items;
    public static GameObject g_players;
    public static GameObject g_projectiles;
    public static GameObject g_shops;
    public static GameObject g_blackSmiths;

    public static GameObject[] effects;

    public static Item[] items;

    public static EnemyData[] enemies;

    public static GameObject ItemObjectSample;
    public static GameObject AreaAtkSample;

    public static PlayerClass[] player_Class;

    public static GameObject prefab;

    public static GameObject localPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            g_enemies = GameObject.Find("Enemies");
            g_items = GameObject.Find("Items");
            g_players = GameObject.Find("Players");
            g_projectiles = GameObject.Find("Projectiles");
            g_shops = GameObject.Find("Shops");
            g_blackSmiths = GameObject.Find("BlackSmiths");

            ItemObjectSample = Resources.Load<GameObject>("Add/ItemObject");
            AreaAtkSample = Resources.Load<GameObject>("Add/AreaAttack");

            Game.items = Resources.LoadAll<Item>("Add/Item");
            Game.enemies = Resources.LoadAll<EnemyData>("Add/Enemy");
            Game.player_Class = Resources.LoadAll<PlayerClass>("Add/PlayerClass");

            Game.effects = Resources.LoadAll<GameObject>("Add/Effect");

            foreach (var obj in Game.enemies)
            {
                obj.ID = obj.name;
                obj.path = "Add/EnemyModel/" + obj.EnemyModel.name;
            }

            foreach (var obj in Game.effects)
            {
                Debug.Log(obj.name);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
    }

    public static PlayerClass GetClassFromName(string name)
    {
        PlayerClass playerClass = null;
        foreach (PlayerClass item in player_Class)
        {
            if (item.name == name)
            {
                playerClass = item;
                break;
            }
        }
        return playerClass;
    }

    public static EnemyData GetEnemyData(string enemyID)
    {
        EnemyData enemyData = null;
        foreach (var obj in Game.enemies)
        {
            if (enemyID == obj.ID)
            {
                enemyData = obj;
                break;
            }
        }

        return enemyData;
    } 

    public static Item GetItemDataFromName(string name)
    {
        Item itemRef = null;
        foreach(Item item in items)
        {
            if (item.itemID == name)
            {
                itemRef = item;
                break;
            }
        }
        return itemRef;
    }

    public static int ToINT(string text)
    {
        int ans = 1;

        if (int.TryParse(text, out int result))
        {
            ans = result;
        }

        return ans;
    }
}
