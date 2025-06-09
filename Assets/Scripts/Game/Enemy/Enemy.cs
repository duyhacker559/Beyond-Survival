using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Photon.Pun;
using Spine;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IPunInstantiateMagicCallback
{
    public bool canAnimate = true;
    public bool canMove = true;

    public EnemyData data;
    private Vector3 moveDirrection;

    private EnemyAI ai_movement;

    public Transform HandItem;

    private HealthSystem health;

    public Rigidbody2D rb;
    private Vector2 movement;
    public Vector3 lookAtPos;

    private SpriteRenderer skin;
    public float lookDir;

    public GameObject main_hand;
    public float Hand_Radius = 0.5f;
    private float rotationSpeed = 10f;

    [HideInInspector] public float recoilForce = 2f;  // Base recoil force
    [HideInInspector] public float recoilRecoverySpeed = 5f; // How fast recoil returns to normal
    [HideInInspector] public float dampingFactor = 0.1f; // Smooth damping factor
    [HideInInspector] private float recoilOffset = 0f; // Recoil displacement (single float)
    [HideInInspector] private float recoilVelocity = 0f; // Smooth recoil movement

    [HideInInspector] public float swingSpeed = 10f; // Speed of swing increasing
    [HideInInspector] public float swingOffset = 30f;
    [HideInInspector] public float swingRecoil = -90f;
    [HideInInspector] private float currentSwing = 0f; // Current swing value
    [HideInInspector] private bool isSwinging = false; // Whether swinging is happening

    [HideInInspector] private float targetSwing = 0f; // The target swing angle (can be set dynamically)
    [HideInInspector] private float inverse = 1f;

    [HideInInspector] public Transform model;

    private bool isMoving = false;
    private float legProgresion = 0f;

    private Transform leg_R;
    private Transform leg_L;
    private Transform leg_R2;
    private Transform leg_L2;
    private Transform body;

    private float leg_R_swing = 0f;
    private float leg_L_swing = 0f;

    private float body_swing = 0f;

    private PlayerInventory inventory;

    private Vector2 lastPos;

    private Collider2D enemy_collider;
    private PhotonView view;

    public Transform onTopDisplay;
    private UnityEngine.UI.Slider healthBar;

    [SerializeField] private Animator animator;
    [SerializeField] private List<AnimationClip> deathAnim;

    private bool dying = false;
    private float lifeTimer = 0f;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        data = Game.GetEnemyData((string)PhotonView.Get(this).InstantiationData[0]);

        if (data.isBoss)
        {
            GameManager.bossCount++;
        }
    }

    void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        skin = GetComponent<SpriteRenderer>();
        enemy_collider = GetComponent<BoxCollider2D>();
        ai_movement = GetComponent<EnemyAI>();
        health = GetComponent<HealthSystem>();

        main_hand = transform.Find("Main").gameObject;

        HandItem = main_hand.transform.Find("Item").transform;

        model = transform.Find("Model").transform;

        leg_L = model.Find("LegL").transform;
        leg_R = model.Find("LegR").transform;

        try
        {
            leg_L2 = model.Find("LegL2").transform;
            leg_R2 = model.Find("LegR2").transform;
        } catch
        {

        }

        onTopDisplay = transform.Find("Canvas");
        if (onTopDisplay != null)
        {
            if (onTopDisplay.Find("HealthBar")!= null) healthBar = onTopDisplay.Find("HealthBar").GetComponent<UnityEngine.UI.Slider>();
        }

        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.SetParent(Game.g_enemies.transform);

        health.SetMaxHealth((int)data.Health);
        health.SetHealth((int)data.Health);

        if (canMove)
        {
            GetComponent<EnemyAI>().enabled = true;
        }
    }

    // Function to set the target swing angle dynamically
    public void TriggerSwing(float angle)
    {
        targetSwing = angle; // Change the target swing angle
    }

    public void TriggerRecoil(float intensity)
    {
        recoilOffset = recoilForce * intensity;
    }

    public void SetHealth(float amount)
    {
        HealthSystem healthSystem = transform.GetComponent<HealthSystem>();
        healthSystem.SetHealth((int)amount);
    }

    public void SetArmor(float amount)
    {
        HealthSystem healthSystem = transform.GetComponent<HealthSystem>();
        healthSystem.SetArmor((int)amount);
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
            if (randomValue < currentWeight)
                return entry.item;
        }

        return default;
    }

    void Update()
    {
        lifeTimer += Time.fixedDeltaTime;

        if (!PhotonNetwork.IsMasterClient)
        {
            Vector3 moveDirection = (rb.position - lastPos);
            lookAtPos = new Vector3(rb.position.x, rb.position.y, 0) + moveDirection.normalized * 5f;
            ai_movement.MoveDirection = new Vector3(moveDirection.x, moveDirection.y, 0);
        }

        if (canAnimate)
        {
            lastPos = Vector3.Lerp(lastPos, new Vector3(transform.position.x, transform.position.y, 0), Time.fixedDeltaTime * 10f);
            Boolean lastMovingState = isMoving;

            if (PhotonNetwork.IsMasterClient)
            {
                isMoving = (ai_movement.MoveDirection.magnitude > 0.1f);
            }
            else
            {
                isMoving = (ai_movement.MoveDirection.magnitude > 0.01f);
            }

            float forward = 1f;
            if (ai_movement.MoveDirection.x < 0)
            {
                forward = -1f;
            }

            if (isMoving)
            {
                if (lastMovingState == false)
                {
                    legProgresion = 0f;
                    body_swing = 0f;
                }

                legProgresion += ai_movement.MoveDirection.magnitude * forward * inverse * 0.2f;
                body_swing = 0.05f * Mathf.Sin(legProgresion);
                leg_L_swing = Mathf.MoveTowards(leg_L_swing, 60 * Mathf.Cos(legProgresion), Time.fixedDeltaTime * 1000);
                leg_R_swing = Mathf.MoveTowards(leg_R_swing, 60 * Mathf.Cos(legProgresion + Mathf.PI), Time.fixedDeltaTime * 1000);
            }
            else
            {
                body_swing = Mathf.MoveTowards(body_swing, 0, Time.fixedDeltaTime * 100);
                leg_L_swing = Mathf.MoveTowards(leg_L_swing, 0, Time.fixedDeltaTime * 250);
                leg_R_swing = Mathf.MoveTowards(leg_R_swing, 0, Time.fixedDeltaTime * 250);
            }

            Quaternion legLRotation = Quaternion.Euler(
                new Vector3(0, 0, leg_L_swing)
                );
            leg_L.localRotation = legLRotation;
            if (leg_R2 != null)
            {
                leg_R2.localRotation = legLRotation;
            }

            Quaternion legRRotation = Quaternion.Euler(
                new Vector3(0, 0, leg_R_swing)
                );
            leg_R.localRotation = legRRotation;
            if (leg_L2 != null)
            {
                leg_L2.localRotation = legLRotation;
            }

            model.localPosition = new Vector3(0, body_swing, 0);

            // Update Rendering
            Vector3 mousePosition = lookAtPos;
            Vector3 localScale = main_hand.transform.localScale;

            Vector3 scale = model.localScale;

            if (mousePosition.x > transform.position.x)
            {
                scale.x = Math.Abs(scale.x);
                inverse = 1f;
                localScale.y = Mathf.Abs(localScale.y);
            }
            else
            {
                scale.x = -Math.Abs(scale.x);
                inverse = -1f;
                localScale.y = -Mathf.Abs(localScale.y);
            }

            model.localScale = scale;

            main_hand.transform.localScale = localScale;

            // Hand movement

            // Smoothly interpolate recoil
            recoilOffset = Mathf.SmoothDamp(recoilOffset, 0f, ref recoilVelocity, recoilRecoverySpeed * Time.deltaTime);
            currentSwing = Mathf.MoveTowards(currentSwing, targetSwing, swingSpeed * Time.deltaTime);

            // If we reached the target swing angle, stop swinging and reset to 0
            if (currentSwing == targetSwing)
            {
                targetSwing = 0f;
            }

            Vector3 direction = mousePosition - main_hand.transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lookDir = angle;
            // Hand Update
            main_hand.transform.rotation = Quaternion.Lerp(main_hand.transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle + (currentSwing + swingOffset) * inverse)), rotationSpeed * Time.deltaTime);
            main_hand.transform.position = new Vector2(model.position.x, model.position.y) + new Vector2(mousePosition.x - rb.position.x, mousePosition.y - rb.position.y).normalized * (1 + recoilOffset) * Hand_Radius;

        }

        if (data.isBoss)
        {
            onTopDisplay.Find("HealthBar").gameObject.SetActive(true);
            healthBar.value = (float)health.CurrentHealth / (float)health.MaxHealth;
        }

        if (PhotonNetwork.IsMasterClient && !dying)
        {
            if (health.CurrentHealth <= 0)
            {

                if (data.Loot.Count > 0)
                {
                    LootPackage randomLoot = GetRandomWeightedItem<LootPackage>(data.Loot);

                    foreach (ItemInstance item in randomLoot.Items)
                    {
                        GameManager.SpawnItem(item, transform.position, transform.rotation);
                    }
                }

                if (data.canExplode)
                {
                    GameManager.SummonAttackArea(
                        transform.position,
                        UnityEngine.Quaternion.Euler(0, 0, lookDir),
                        new AreaInstance(data.explode_Damage, data.explode_KnockBack, data.explode_KnockBackDuration, data.explode_Effect, data.explode_Hitbox, Game.g_players.transform, data.explode_HitboxData)
                    );

                    if (data.explode_Emit != null)
                    {
                        GameManager.Instance.SummonEffect(data.explode_Emit.name, rb.position);
                    }
                }

                if (data.isBoss)
                {
                    GameManager.bossCount--;
                }

                dying = true;

                view.RPC("RPC_Death", RpcTarget.All, health.deathAnim);
                ai_movement.Stunned(2);
                ai_movement.enabled = false;
            }

            if (data.haveLifeTime)
            {
                if (lifeTimer >= DayNightCycle2D.DayDuration * data.lifeTime)
                {

                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_Death(string id)
    {
        if (animator != null)
        {
            animator.enabled = true;
            if (id == "")
            {
                animator.Play(deathAnim[Random.Range(0, deathAnim.Count)].name, 0);
            } else
            {
                animator.Play(id, 0);
            }
        } else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public void Dying()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
