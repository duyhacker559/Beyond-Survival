using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Unity.Mathematics;
using System.Linq;

public class Player : MonoBehaviour, IPunInstantiateMagicCallback
{
    // Basic stats

    public float _currentMana = 100f;
    public float MaxMana = 100f;

    public int cash = 0;
    public bool fallen = false;
    public float CurrentMana => _currentMana;

    [SerializeField] private AudioSource _audioSource;

    // Vision Stats

    public float range = 5f; // Player range

    private float vision = 5f; // Player vision
    public float minVision = -2f; // limit zoom in
    public float maxVision = 2f; // limit zoom out
    public float zoomSpeed = 5f;  // Zoom speed
    public float moveZoomDecrease = 2f; // Vison reduce when moving

    // Movement Stats

    public float moveSpeed = 5f;
    public float maxSpeed = 10f;
    public float acceleration = 15f; // Speed up smoothly
    public float deceleration = 10f; // Slow down smoothly
    public float linearDrag = 4f; // Adjust for natural movement

    private Vector3 moveDirrection;

    public Vector2 minBounds = new Vector2(-999999, 999999);
    public Vector2 maxBounds = new Vector2(-999999, 999999);

    public float dashSpeed = 10f; // Dash speed
    public float dashDuration = 0.2f; // Dash time
    private bool isDashing = false;
    private float dashTime;

    private float dashCooldownTimer = 0f;
    public float dashCooldown = 1f;
    public float dashManaConsume = 5f;

    public Transform HandItem;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 movement;
    private float targetVision;

    private SpriteRenderer skin;
    public float lookDir;

    private float cellSize = 1f;

    private GameObject main_hand;
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

    [HideInInspector] private Transform model;

    private bool isMoving = false;
    private float legProgresion = 0f;

    private Transform leg_R;
    private Transform leg_L;
    private Transform body;

    private float leg_R_swing = 0f;
    private float leg_L_swing = 0f;

    private float body_swing = 0f;

    private PlayerInventory inventory;

    private Collider2D player_collider;

    PhotonView view;

    public Vector3 mousePosition;

    public Transform onTopDisplay;
    private UnityEngine.UI.Slider healthBar;

    public HealthSystem health;

    public PlayerClass _class;

    public Transform model_Head;
    public Transform model_Hat;
    public Transform model_Hair;
    public Transform model_Body;
    public Transform model_Pant_L;
    public Transform model_Pant_R;

    public float stunTimer = 0f;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        _class = Game.player_Class[(int)PhotonView.Get(this).InstantiationData[0]];

        Debug.Log("Loaded Class " + _class.name);

        inventory = GetComponent<PlayerInventory>();
        health = GetComponent<HealthSystem>();

        foreach (ItemInstance item in _class.loadout)
        {
            inventory.AddItem(item);
        }

        moveSpeed = _class.speed;
        maxSpeed = _class.maxSpeed;

        range = _class.range;

        MaxMana = _class.mana;
        _currentMana = MaxMana;

        health.SetMaxHealth((int)_class.health);
        health.SetHealth((int)_class.health);
        health.SetBaseDefense((int)_class.defense);
        health.SetBaseArmor(_class.armor);

        cash = _class.cash;

        fallen = false;
    }

    void Start()
    {
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            Destroy(transform.Find("Minimap").gameObject);
        }

        gameObject.name = view.Owner.NickName;

        rb = GetComponent<Rigidbody2D>();
        skin = GetComponent<SpriteRenderer>();

        player_collider = GetComponent<BoxCollider2D>();
        inventory = GetComponent<PlayerInventory>();

        main_hand = transform.Find("Main").gameObject;

        HandItem = main_hand.transform.Find("Item").transform;

        model = transform.Find("Model").transform;

        leg_L = model.Find("LegL").transform;
        leg_R = model.Find("LegR").transform;

        model_Pant_L = leg_L;
        model_Pant_R = leg_R;

        model_Body = model.Find("Body").transform;
        model_Head = model.Find("Head").transform;
        model_Hat = model_Head.Find("Hat").transform;
        model_Hair = model_Head.Find("Hair").transform;

        model_Pant_L.GetComponent<SpriteRenderer>().sprite = _class.Leg;
        model_Pant_R.GetComponent<SpriteRenderer>().sprite = _class.Leg;

        model_Body.GetComponent<SpriteRenderer>().sprite = _class.Body;
        model_Head.GetComponent<SpriteRenderer>().sprite = _class.Head;
        model_Hair.GetComponent<SpriteRenderer>().sprite = _class.Hair;

        main_hand.GetComponent<SpriteRenderer>().sprite = _class.Hand;

        Tilemap tilemap = GetComponent<Tilemap>();
        if (tilemap != null)
        {
            cellSize = tilemap.layoutGrid.cellSize.x;
        }

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.linearDamping = linearDrag;
        cam = Camera.main;
        targetVision = vision;

        transform.SetParent(Game.g_players.transform);
        onTopDisplay = transform.Find("Canvas");
        healthBar = onTopDisplay.Find("HealthBar").GetComponent<UnityEngine.UI.Slider>();

        if (view.IsMine)
        {
            transform.name = PhotonNetwork.LocalPlayer.NickName;
            Debug.Log(PhotonNetwork.NickName);

            Transform UI = GameObject.Find("PlayerUI").transform;
            Transform UICam = GameObject.Find("UICam").transform;

            Transform Stats = UI.Find("PlayerStats");
            Stats.GetComponent<PlayerUI>().Current_Player = gameObject;
            Stats.GetComponent<PlayerUI>().enabled = true;
            Stats.Find("UI").gameObject.SetActive(true);

            UI.Find("Loadout").GetComponent<LoadOut>().gameObject.SetActive(true);

            UICam.Find("MinimapCam").GetComponent<FollowObject>().TargetObject = gameObject;
            UICam.Find("PlayerTrackerCam").GetComponent<FollowObject>().TargetObject = gameObject;

            Game.localPlayer = transform.gameObject;
        }
        else
        {
            onTopDisplay.Find("HealthBar").gameObject.SetActive(true);
        }

        onTopDisplay.Find("NameTag").GetComponent<TextMeshProUGUI>().SetText(view.Owner.NickName);
        onTopDisplay.Find("NameTag").gameObject.SetActive(true);

        health.calculateArmor(inventory.Armor, inventory.holdingItem);
    }

    private void Awake()
    {
        _audioSource = transform.GetComponent<AudioSource>();
    }

    void Update()
    {
        stunTimer += Time.fixedDeltaTime;

        if (stunTimer <= 0)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (view.IsMine && !fallen)
        {
            // If the player presses Q and is not already dashing, start the dash
            dashCooldownTimer += Time.fixedDeltaTime;

            if (stunTimer > 0f)
            {
                // Get Input (WASD or Arrow Keys)
                if (!PlayerUI.isChatting)
                {
                    movement.x = Input.GetAxis("Horizontal");
                    movement.y = Input.GetAxis("Vertical");

                    if (Input.GetKeyDown(KeyCode.Q) && !isDashing && dashCooldownTimer > 0f)
                    {
                        if (ConsumeMana(dashManaConsume))
                        {
                            // Set the dash direction to the movement direction
                            if (rb.linearVelocity.magnitude > 0)
                            {
                                moveDirrection = rb.linearVelocity.normalized;
                            }

                            // Start dashing
                            isDashing = true;
                            dashCooldownTimer = -dashCooldown;
                            dashTime = 0;
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(0)) // Left mouse button (fire)
            {
                Vector2 fireDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
                // Example: Normal shot with base recoil

                // Example: Charged shot with **double recoil**
                // TriggerRecoil(fireDirection, 2f);
            }

            // Normalize diagonal movement
            if (movement.magnitude > 1)
                movement.Normalize();

            if (movement.magnitude > 0.1f)
            {
                // Reduce vision when moving
                targetVision = Mathf.Max(range + minVision, range + maxVision - moveZoomDecrease);
            }
            else
            {
                // Icrease vision when idle
                targetVision = range + maxVision;
            }

            // Smooth zoom
            vision = Mathf.Lerp(vision, targetVision * cellSize, Time.deltaTime * zoomSpeed);

            // Camera update
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, vision, Time.deltaTime * zoomSpeed);
            else
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, vision * zoomSpeed, Time.deltaTime * zoomSpeed);

            // Smooth Camera Follow
            Vector3 targetCamPos = new Vector3(rb.position.x, rb.position.y, -10);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetCamPos, Time.deltaTime * 3f);

            mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        healthBar.value = (float)health.CurrentHealth / (float)health.MaxHealth;
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

    [PunRPC]
    public void UpdateMana(float amount)
    {
        _currentMana = amount;
    }

    [PunRPC]
    public void RPC_UpdateCash(int amount)
    {
        cash = amount;
    }

    [PunRPC]
    private void RPC_Stunned(float time)
    {
        stunTimer = -time;
    }

    public void Stunned(float time)
    {
        view.RPC("RPC_Stunned", RpcTarget.All, time);
    }

    [PunRPC]
    private void RPC_Knockback(float kb, Vector3 dir, float stun)
    {
        if (view.IsMine)
        {
            if (rb == null) return;
            Stunned(stun);
            Vector2 direction = dir.normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * kb, ForceMode2D.Impulse);
        }
    }

    public void Knockback(float kb, Vector3 dir, float stun)
    {
        view.RPC("RPC_Knockback", RpcTarget.All, kb, dir, stun);
    }

    public void UpdateCash(int amount)
    {
        cash = amount;
        view.RPC("RPC_UpdateCash", RpcTarget.Others, cash);
    }

    public bool ConsumeMana(float value)
    {
        if (_currentMana >= value)
        {
            _currentMana -= value;
            if (PhotonNetwork.IsMasterClient)
            {
                view.RPC("UpdateMana", RpcTarget.Others, _currentMana);
            }
            return true;
        }
        return false;
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

    public void GainMana(float value)
    {
        _currentMana = Mathf.Min(MaxMana, _currentMana + value);
        view.RPC("UpdateMana", RpcTarget.Others, _currentMana);
    }

    public void LoseMana(float value)
    {
        _currentMana = Mathf.Max(0, _currentMana - value);
        view.RPC("UpdateMana", RpcTarget.Others, _currentMana);
    }

    public void Revive()
    {
        health.SetHealth((int)health.MaxHealth/2);
        fallen = false;

        view.RPC("RPC_Fallen", RpcTarget.All, false);
    }

    [PunRPC]
    private void RPC_Fallen(bool toggle)
    {
        fallen = toggle;

        if (fallen)
        {
            if (view.IsMine)
            {
                Transform UI = GameObject.Find("PlayerUI").transform;
                Transform Stats = UI.Find("PlayerStats");
                Stats.Find("UI").gameObject.SetActive(false);
            }
            onTopDisplay.Find("HealthBar").gameObject.SetActive(false);
        } else
        {
            if (view.IsMine)
            {
                Transform UI = GameObject.Find("PlayerUI").transform;
                Transform Stats = UI.Find("PlayerStats");
                Stats.Find("UI").gameObject.SetActive(true);
                onTopDisplay.Find("HealthBar").gameObject.SetActive(false);
            }
            else
            {
                onTopDisplay.Find("HealthBar").gameObject.SetActive(true);
            }
        }
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (health.CurrentHealth <= 0 && !fallen)
            {
                fallen = true;
                view.RPC("RPC_Fallen", RpcTarget.All, true);
            }
        }

        if (inventory.holdingItem != null && inventory.holdingItem.itemRef !=null)
        {
            range = _class.range + inventory.holdingItem.itemRef.range;
            moveSpeed = _class.speed + inventory.holdingItem.itemRef.speed;
            maxSpeed = _class.maxSpeed + inventory.holdingItem.itemRef.speed;

        } else
        {
            range = _class.range;
            moveSpeed = _class.speed;
            maxSpeed = _class.maxSpeed;
        }

        if (fallen)
        {
            model.localRotation = Quaternion.Euler(0f, 0f, 90f);
            rb.linearVelocity = moveDirrection * 0;
            return;
        }
        else
        {
            model.localRotation = Quaternion.identity;
        }

        if (view.IsMine)
        {
            if (stunTimer<0)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (isDashing)
            {
                // Move the player in the dash direction
                rb.linearVelocity = moveDirrection * dashSpeed;

                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);

                // Timer for the dash duration
                dashTime += Time.fixedDeltaTime;

                // Stop dashing after the duration
                if (dashTime >= dashDuration)
                {
                    Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
                    isDashing = false;
                }
            }

            if (fallen)
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Stop();
                }
            } else
            {
                if (movement.magnitude > 0 && stunTimer > 0)
                {
                    // Apply acceleration-based force for smoother movement
                    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, movement * maxSpeed, Time.fixedDeltaTime * acceleration);
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.Play();
                    }
                }
                else
                {
                    // Apply deceleration to slow down smoothly
                    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * deceleration);
                    if (_audioSource.isPlaying)
                    {
                        _audioSource.Stop();
                    }
                }
            }

            Boolean lastMovingState = isMoving;
            isMoving = (rb.linearVelocity.magnitude > 0.2f);
            float forward = 1f;
            if (rb.linearVelocity.x < 0)
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

                legProgresion += rb.linearVelocity.magnitude * forward * inverse * 0.04f;
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

            Quaternion legRRotation = Quaternion.Euler(
                new Vector3(0, 0, leg_R_swing)
                );
            leg_R.localRotation = legRRotation;

            model.localPosition = new Vector3(0, body_swing, 0);

            // Keep player inside bounds
            rb.position = new Vector2(
                    Mathf.Clamp(rb.position.x, minBounds.x, maxBounds.x),
                    Mathf.Clamp(rb.position.y, minBounds.y, maxBounds.y)
                );

            // Update Renderin 
            // Mouse pos
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
    }
}