using NUnit.Framework.Interfaces;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverUI;
    public GameObject damageIndicator;
    public GameObject healIndicator;

    private PhotonView photonView;

    public static int item_Index = 0;

    public static int bossCount = 0;

    void Awake()
    {
        // Khởi tạo Singleton tạm thời, không dùng DontDestroyOnLoad để tránh lỗi UI
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Đảm bảo ẩn UI khi bắt đầu
        photonView = GetComponent<PhotonView>();
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Game"); // Đổi thành tên scene gốc
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu"); // Đổi thành tên scene menu
    }

    // Game Engine
    [PunRPC]
    private void RPC_ShowDamage(Vector3 pos, int damage)
    {
        GameObject showDamage = Instantiate(damageIndicator, pos + new Vector3(UnityEngine.Random.Range(-1f,1f), UnityEngine.Random.Range(-1f, 1f), 0f), Quaternion.identity, Game.g_projectiles.transform);
        DamageIndicator damageData = showDamage.GetComponent<DamageIndicator>();
        damageData.damage = damage;
    }

    [PunRPC]
    private void RPC_Heal(Vector3 pos, int damage)
    {
        GameObject showDamage = Instantiate(healIndicator, pos + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f), Quaternion.identity, Game.g_projectiles.transform);
        DamageIndicator damageData = showDamage.GetComponent<DamageIndicator>();
        damageData.damage = damage;
    }

    public void ShowDamage(Vector3 pos, int damage)
    {
        photonView.RPC("RPC_ShowDamage", RpcTarget.All, pos, damage);
    }

    public void ShowHeal(Vector3 pos, int damage)
    {
        photonView.RPC("RPC_Heal", RpcTarget.All, pos, damage);
    }

    public static void SummonAttackArea(Vector3 pos, Quaternion dir, AreaInstance data)
    {
        GameObject area = Instantiate(Game.AreaAtkSample, pos, dir, Game.g_projectiles.transform);
        AreaAttack areaAttack = area.GetComponent<AreaAttack>();
        areaAttack.Initiate(data);
        area.SetActive(true);
        areaAttack.Attack();
    }

    public static void SummonProjectile(GameObject player, Vector3 pos, Quaternion dir, ProjectileData data, GameObject model)
    {
        GameObject bullet = Instantiate(model, pos, dir, Game.g_projectiles.transform);
        Projectile bullet_Projectile = bullet.GetComponent<Projectile>();
        bullet_Projectile.Initialize(data);
        bullet_Projectile.enabled = true;
    }

    // Command

    public static void SetHealth(GameObject target, float health)
    {
        Player player = target.transform.GetComponent<Player>();
        player.SetHealth(health);
    }

    public static void SetMana(GameObject target, float mana)
    {
        Player player = target.transform.GetComponent<Player>();
        player._currentMana = Mathf.Min(player.MaxMana, mana);
    }

    public static void SpawnEnemy(string id, Vector3 pos)
    {
        EnemyData dat = Game.GetEnemyData(id);
        object[] data = new object[] { id };
        Debug.Log(dat.path);
        GameObject e_model = PhotonNetwork.InstantiateRoomObject(dat.path, pos, Quaternion.identity, 0, data);
    }

    [PunRPC]
    public void RPC_SummonEffect(string id, Vector3 pos)
    {
        foreach (GameObject effect in Game.effects)
        {
            if (effect.name == id)
            {
                Instantiate(effect, pos, Quaternion.identity, Game.g_projectiles.transform);
            }
        }
    }

    public void SummonEffect(string id, Vector3 pos)
    {
        Debug.Log(id);
        photonView.RPC("RPC_SummonEffect", RpcTarget.All, id, pos);
    }

    public void TrySpawnEnemy(EnemyData dat, Vector3 pos)
    {
        if (dat != null)
        {
            Debug.Log(dat.ID);

            if (PhotonNetwork.IsMasterClient)
            {
                float checkDistance = 6f;
                float spawnOffset = 0.5f;

                LayerMask obstacleMask;

                obstacleMask = (1 << LayerMask.NameToLayer("Barrier"));

                Vector3 origin = pos;

                for (int angle = 0; angle < 360; angle += 10)
                {
                    float rad = angle * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                    RaycastHit2D hit = Physics2D.Raycast(origin, dir, checkDistance, obstacleMask);

                    if (hit.collider == null)
                    {
                        Vector3 pointToCheck = origin + (Vector3)(dir * (checkDistance - spawnOffset));
                        pointToCheck.z = 0f;

                        // Kiểm tra NavMesh
                        if (NavMesh.SamplePosition(pointToCheck, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                        {
                            SpawnEnemy(dat.ID, navHit.position);
                            return;
                        }
                    }
                }
            }
        }
    }

    public void SpawnItem(Item itemRef, int amount, Vector3 pos, Quaternion rot)
    {
        if (itemRef != null)
        {
            if (itemRef.isWeapon)
            {
                if (itemRef.weaponType == WeaponType.Melee)
                {
                    SpawnItem(new ItemInstance(itemRef, 0, 1), pos, rot);
                } else
                {
                    SpawnItem(new ItemInstance(itemRef, itemRef.clipSize, itemRef.durability), pos, rot);
                }
            } else
            {
                SpawnItem(new ItemInstance(itemRef, 0, amount), pos, rot);
            }
        }
    }

    public void SpawnItem(string name, int amount, Vector3 pos, Quaternion rot)
    {
        Item itemRef = Game.GetItemDataFromName(name);
        if (itemRef)
        {
            SpawnItem(itemRef, amount, pos, rot);
        }
    }

    public static void SpawnItem(ItemInstance dat, Vector3 pos, Quaternion rot)
    {
        if (dat != null)
        {
            string json = (new ItemInstanceSender(dat)).ToJson();
            Debug.Log(json);
            object[] data = new object[] { json, item_Index++ };
            PhotonNetwork.InstantiateRoomObject("ItemObject", pos, rot, 0, data);
        }
    }

    public void RequestSpawnItem(ItemInstance dat, Vector3 pos, Quaternion rot)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReceiveSpawnItem", RpcTarget.All, new ItemInstanceSender(dat).ToJson());
        }
    }
}
