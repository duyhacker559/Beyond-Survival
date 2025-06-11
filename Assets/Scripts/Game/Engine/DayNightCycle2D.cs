using JetBrains.Annotations;
using Photon.Pun;
using System.Buffers;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class WeightedItem<T>
{
    public T item;
    public int weight;

    public WeightedItem(T item, int weight)
    {
        this.item = item;
        this.weight = weight;
    }
}

[System.Serializable]
public class EnemyPackage
{
    [SerializeField] public EnemyGroup Item;
    [SerializeField] public int amount;
}

[System.Serializable]
public class ItemPackage
{
    [SerializeField] public ItemGroup Item;
    [SerializeField] public int amount;
}   

[System.Serializable]
public class DaySpawn
{
    [SerializeField] public Gen[] Data;
}

[System.Serializable]
public class ZoneGroup
{
    public List<Transform> EnemyZone;
    public List<Transform> ItemZone;
}

[System.Serializable]
public class Gen
{
    public string tag = "";
    public float time = 0f;
    [SerializeField] public EnemyPackage[] enemyPackages;
    [SerializeField] public ItemPackage[] itemPackages;
    public int zoneGroup;
}

public class DayNightCycle2D : MonoBehaviour
{
    public static Transform EnemyZone;
    public static Transform ItemZone;

    public Light2D globalLight;
    public Gradient lightColorOverTime;
    public AnimationCurve intensityOverTime;

    public static float timeScale = 1f;

    static public int currentDay = 1;
    static public int hour = 0;
    static public int minute = 0;

    private Transform enemyZone;
    private Transform itemZone;

    [UnityEngine.Range(0f, 24f)] public float timeOfDay = 6f;
    [UnityEngine.Range(0f, 24f)] public float _timeOfDay = 6f;

    public static float DayDuration;

    public float dayDuration = 60f; // 1 ngày kéo dài 60 giây

    [Space]

    public ZoneGroup[] zoneGroups;

    [Space]
    public List<DaySpawn> daySpawn = new List<DaySpawn>();

    private void Awake()
    {
        timeScale = 1f;
        enemyZone = transform.Find("EnemyZone");
        itemZone = transform.Find("ItemZone");
    }

    public static Vector2 GetRandomPositionInBarrier(Collider2D area, Vector2 enemyColliderSize)
    {
        Vector2 center = area.bounds.center;
        Vector2 size = area.bounds.size;

        float paddingX = enemyColliderSize.x / 2f;
        float paddingY = enemyColliderSize.y / 2f;

        float x = UnityEngine.Random.Range(center.x - size.x / 2f + paddingX, center.x + size.x / 2f - paddingX);
        float y = UnityEngine.Random.Range(center.y - size.y / 2f + paddingY, center.y + size.y / 2f - paddingY);

        return new Vector2(x, y);
    }

    public T GetRandomWeightedItem<T>(List<WeightedItem<T>> list)
    {
        int totalWeight = 0;

        foreach (var entry in list)
            totalWeight += entry.weight;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var entry in list)
        {
            currentWeight += entry.weight;
            if (randomValue<currentWeight)
                return entry.item;
        }

        return default;
    }

    void Generate(int _day)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int maxDay = daySpawn.Count - 1;
            int day = ((_day - 1) % maxDay + maxDay) % maxDay + 1;
            int multiplier = math.clamp((int)Mathf.Ceil(_day / maxDay),1,10);
            //Debug.Log(day);
            foreach (Gen genAction in daySpawn[day].Data)
            {
                if (genAction.time < timeOfDay && _timeOfDay < genAction.time)
                {
                    _timeOfDay = genAction.time;

                    foreach (EnemyPackage enemyPackage in genAction.enemyPackages)
                    {
                        for (int i = 0; i < enemyPackage.amount * multiplier; i++)
                        {
                            int randomPlace = UnityEngine.Random.Range(0, zoneGroups[genAction.zoneGroup].EnemyZone.Count);
                            EnemyData randomEnemy = GetRandomWeightedItem<EnemyData>(enemyPackage.Item.groups);
                            if ((randomEnemy.isBoss && GameManager.bossCount < 1 ) || !randomEnemy.isBoss)
                            {
                                GameManager.SpawnEnemy(randomEnemy.ID, GetRandomPositionInBarrier(zoneGroups[genAction.zoneGroup].EnemyZone[randomPlace].GetComponent<BoxCollider2D>(), new Vector2(0.1f, 0.1f)));
                            }
                        }
                    }

                    foreach (ItemPackage itemPackage in genAction.itemPackages)
                    {
                        for (int i = 0; i < itemPackage.amount; i++)
                        {
                            int randomPlace = UnityEngine.Random.Range(0, zoneGroups[genAction.zoneGroup].ItemZone.Count);
                            ItemInstance randomItem = GetRandomWeightedItem<ItemInstance>(itemPackage.Item.Items);
                            GameManager.SpawnItem(randomItem, GetRandomPositionInBarrier(zoneGroups[genAction.zoneGroup].ItemZone[randomPlace].GetComponent<BoxCollider2D>(), new Vector2(0.1f, 0.1f)), quaternion.identity);
                        }
                    }
                }
            }
        }
    }

    private void Start()
    {
        timeScale = 1f;
        if (PhotonNetwork.IsMasterClient)
        {
            currentDay = 1;
            hour = 0;
            minute = 0;
        }
    }

    void Update()
    {
        timeOfDay += (24f / dayDuration) * Time.deltaTime * timeScale;
        DayDuration = dayDuration;
        Generate(currentDay);

        if (timeOfDay >= 24f)
        {
            timeOfDay = 0f;
            _timeOfDay = 0f;
            currentDay++;
        }

        hour = Mathf.FloorToInt(timeOfDay);
        minute = Mathf.FloorToInt((timeOfDay - hour) * 60f);

        float percent = timeOfDay / 24f;

        // Đổi màu ánh sáng
        globalLight.color = lightColorOverTime.Evaluate(percent);
        globalLight.intensity = intensityOverTime.Evaluate(percent);
    }
}
