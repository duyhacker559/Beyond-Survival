using TMPro;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class ItemObject : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public ItemInstance Data;
    private PhotonView view;

    [SerializeField] private Transform inputText;

    private Transform target;
    private float Timer = 0f;
    [SerializeField] private LayerMask obstacleMask;

    private Transform body;
    private Transform head;

    public List<AttackData> meleeAttacks = new List<AttackData>();
    public List<AttackData> rangedAttacks = new List<AttackData>();

    private Transform item;

    Transform muzzle;
    Transform particle;

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    bool CheckIfNear(UnityEngine.Vector3 pos, float radius)
    {
        if (UnityEngine.Vector3.Distance(transform.position, pos) <= radius)
        {
            return true;
        }
        return false;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {

        string json = (string)PhotonView.Get(this).InstantiationData[0];
        int itemID = (int)PhotonView.Get(this).InstantiationData[1];

        transform.SetParent(Game.g_items.transform);
        Data = new ItemInstance(JsonUtility.FromJson<ItemInstanceSender>(json));

        Transform model = transform.Find("Model");
        if (model.childCount == 0)
        {
            GameObject newModel = Instantiate(Data.itemRef.model, model);

            newModel.transform.localPosition = Vector3.zero;

            model = newModel.transform;

            head = model.Find("Main");

            if (head != null) {
                item = head.Find("Item").GetChild(0);
            }
        }
        transform.Find("Canvas").Find("Name").GetComponent<TextMeshProUGUI>().SetText($"x{Data.amount} {Data.itemRef.itemName}");

        transform.gameObject.name = $"{itemID}";
    }

    private Transform FindPlayer(Target targetGroup)
    {
        Transform detectedPlayer = null;
        Transform group = Game.g_enemies.transform;
        if (targetGroup == Target.Player)
        {
            group = Game.g_players.transform;
        }
        float minDistance = Mathf.Infinity;
        foreach (Transform player in group)
        {
            UnityEngine.Vector2 dirToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = UnityEngine.Vector2.Distance(transform.position, player.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask);

            HealthSystem health = player.GetComponent<HealthSystem>();

            if (health != null)
            {
                if (health.CurrentHealth > 0)
                {
                    if (hit.collider == null)
                    {
                        if (minDistance > distanceToPlayer)
                        {
                            minDistance = distanceToPlayer;
                            detectedPlayer = player;
                        }
                    }
                }
            }
        }
        return detectedPlayer;
    }


    private void Start()
    {
        if (Data.itemRef.attack != null)
        {
            if (Data.itemRef.attack.meleeAttacks.Count > 0)
            {
                for (int i = 0; i < Data.itemRef.attack.meleeAttacks.Count; i++)
                {
                    meleeAttacks.Add(new AttackData(Data.itemRef.attack.meleeAttacks[i].delay, Data.itemRef.attack.meleeAttacks[i].clipSize));
                }
            }

            if (Data.itemRef.attack.rangedAttacks.Count > 0)
            {
                for (int i = 0; i < Data.itemRef.attack.rangedAttacks.Count; i++)
                {
                    rangedAttacks.Add(new AttackData(Data.itemRef.attack.rangedAttacks[i].delay, Data.itemRef.attack.rangedAttacks[i].clipSize));
                }
            }
        }
    }

    private void Attack()
    {
        muzzle = item.Find("Muzzle");
        particle = item.Find("Particle");
    }

    private void Update()
    {
        Timer += Time.fixedDeltaTime;

        for (int i = 0; i < meleeAttacks.Count; i++)
        {
            meleeAttacks[i].timer += Time.fixedDeltaTime;
        }

        for (int i = 0; i < rangedAttacks.Count; i++)
        {
            rangedAttacks[i].timer += Time.fixedDeltaTime;
        }

        if (PhotonNetwork.IsMasterClient)
        {

            if (meleeAttacks.Count > 0)
            {
                for (int i = 0; i < meleeAttacks.Count; i++)
                {
                    if (Data.itemRef._itemType.Equals(ItemType.Weapon) && Data.ammo <=1)
                    {
                        Data.ammo = 1;
                        break;
                    }

                    EnemyMeleeAttack enemyMeleeAttack = Data.itemRef.attack.meleeAttacks[i];
                    if (meleeAttacks[i].timer >= 0)
                    {
                        Transform detected = FindPlayer(enemyMeleeAttack.group);
                        if (detected != null && (CheckIfNear(detected.position, enemyMeleeAttack.Range)))
                        {
                            target = detected;
                        }
                        else
                        {
                            meleeAttacks[i].timer = -enemyMeleeAttack.Detect_Rate;
                            continue;
                        }

                        if (CheckIfNear(target.position, enemyMeleeAttack.Range) && meleeAttacks[i].ammo > 0)
                        {
                            meleeAttacks[i].timer = -enemyMeleeAttack.Attack_Rate;
                            Vector3 pos = transform.position;
                            //if (muzzle != null) pos = muzzle.position;
                            Vector3 direction = target.position - pos;
                            float lookDir = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                            GameManager.SummonAttackArea(
                                transform.position,
                                UnityEngine.Quaternion.Euler(0, 0, lookDir),
                                new AreaInstance(enemyMeleeAttack.Damage, enemyMeleeAttack.Attack_KB, enemyMeleeAttack.Attack_KBDuration, enemyMeleeAttack.Attack_Effect, enemyMeleeAttack.Attack_Hitbox, Game.g_enemies.transform, enemyMeleeAttack.areaAttackDat)
                                );

                            if (head != null)
                            {
                                head.localRotation = UnityEngine.Quaternion.Euler(0, 0, lookDir);
                            }

                            Attack();

                            meleeAttacks[i].ammo--;

                            if (Data.itemRef._itemType.Equals(ItemType.Weapon) && Data.ammo > 1)
                            {
                                Data.ammo--;
                            }
                        }

                        if (meleeAttacks[i].ammo <= 0)
                        {
                            meleeAttacks[i].timer -= enemyMeleeAttack.Attack_Reload;
                            meleeAttacks[i].ammo = enemyMeleeAttack.clipSize;
                        }
                    }
                }
            }

            if (rangedAttacks.Count > 0)
            {
                for (int i = 0; i < rangedAttacks.Count; i++)
                {
                    if (Data.itemRef._itemType.Equals(ItemType.Weapon) && Data.ammo <= 1)
                    {
                        Data.ammo = 1;
                        break;
                    }

                    EnemyRangedAttack enemyRangedAttack = Data.itemRef.attack.rangedAttacks[i];
                    if (rangedAttacks[i].timer >= 0)
                    {
                        Transform detected = FindPlayer(enemyRangedAttack.group);
                        if (detected != null && (CheckIfNear(detected.position, enemyRangedAttack.Range)))
                        {
                            target = detected;
                        }
                        else
                        {
                            rangedAttacks[i].timer = -enemyRangedAttack.Detect_Rate;
                            continue;
                        }

                        if (CheckIfNear(target.position, enemyRangedAttack.Range) && rangedAttacks[i].ammo > 0)
                        {
                            rangedAttacks[i].timer = -enemyRangedAttack.Attack_Rate;

                            for (int j = 0; j < enemyRangedAttack.Attack_Amount; j++)
                            {
                                Vector3 pos = transform.position;
                                if (head != null) pos = head.position;
                                Vector3 direction = target.position - pos;
                                float lookDir = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                                float look = lookDir + UnityEngine.Random.Range(-enemyRangedAttack.ranged_spread, enemyRangedAttack.ranged_spread);
                                if (head != null)
                                {
                                    head.localRotation = UnityEngine.Quaternion.Euler(0, 0, lookDir);
                                }
                                if (muzzle != null) pos = muzzle.position;
                                GameManager.SummonProjectile(transform.gameObject,
                                    pos,
                                    UnityEngine.Quaternion.Euler(0, 0, look),
                                    new ProjectileData(
                                        enemyRangedAttack.ranged_bulletSpeed,
                                        enemyRangedAttack.Damage,
                                        enemyRangedAttack.Attack_KB,
                                        enemyRangedAttack.Attack_KBDuration,
                                        enemyRangedAttack.ranged_bulletLifetime,
                                        enemyRangedAttack.Attack_Effect,
                                        enemyRangedAttack.ranged_projectileDat,
                                        Game.g_enemies.transform
                                    ),
                                    enemyRangedAttack.ranged_projectile
                                );

                                Attack();

                                if (particle)
                                {
                                    ParticleSystem particleEmitter = particle.GetComponent<ParticleSystem>();
                                    particleEmitter.Emit(enemyRangedAttack.emit);
                                }

                                if (muzzle)
                                {
                                    AudioSource audioSource = muzzle.GetComponent<AudioSource>();
                                    if (audioSource != null) audioSource.Play();
                                }

                                view.RPC("RPC_RangedAttack", RpcTarget.Others, i, look, transform.position);

                            }

                            rangedAttacks[i].ammo--;

                            if (Data.itemRef._itemType.Equals(ItemType.Weapon) && Data.ammo > 1)
                            {
                                Data.ammo--;
                            }
                        }

                        if (rangedAttacks[i].ammo <= 0)
                        {
                            rangedAttacks[i].timer -= enemyRangedAttack.Attack_Reload;
                            rangedAttacks[i].ammo = enemyRangedAttack.clipSize;
                        }
                    }
                }
            }

            if (Timer > DayNightCycle2D.DayDuration * Data.itemRef.lifeTime && Data.itemRef.haveLifeTime)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    private void RPC_RangedAttack(int i, float dir, UnityEngine.Vector3 pos)
    {
        EnemyRangedAttack enemyRangedAttack = Data.itemRef.attack.rangedAttacks[i];
        GameManager.SummonProjectile(transform.gameObject,
            pos,
            UnityEngine.Quaternion.Euler(0, 0, dir),
            new ProjectileData(
                enemyRangedAttack.ranged_bulletSpeed,
                enemyRangedAttack.Damage,
                enemyRangedAttack.Attack_KB,
                enemyRangedAttack.Attack_KBDuration,
                enemyRangedAttack.ranged_bulletLifetime,
                enemyRangedAttack.Attack_Effect,
                enemyRangedAttack.ranged_projectileDat,
                Game.g_players.transform
            ),
            enemyRangedAttack.ranged_projectile
        );

        if (head != null)
        {
            head.localRotation = UnityEngine.Quaternion.Euler(0, 0, dir);
        }

        Attack();

        if (particle)
        {
            ParticleSystem particleEmitter = particle.GetComponent<ParticleSystem>();
            particleEmitter.Emit(enemyRangedAttack.emit);
        }

        if (muzzle)
        {
            AudioSource audioSource = muzzle.GetComponent<AudioSource>();
            if (audioSource != null) audioSource.Play();
        }
    }
}
