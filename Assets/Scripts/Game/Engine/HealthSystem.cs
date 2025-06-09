using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

[RequireComponent(typeof(SpriteRenderer))]
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private bool _destroyOnDeath = true;

    [Header("Armor Settings")]
    [SerializeField] private float _maxArmor = 0f;
    [SerializeField] private float _currentArmor = 0f;
    public float CurrentArmor => _currentArmor;

    [Header("Visual Feedback")]
    [SerializeField] private Color _damageColor = Color.red;
    [SerializeField] private Color _healColor = Color.green;

    private Color hitColor = Color.white;

    private List<SpriteRenderer> parts = new List<SpriteRenderer>();

    private List<Transform> renderedPart = new List<Transform>();
    private List<Color> partOriginalColor = new List<Color>();

    private float _currentHealth;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float MaxArmor => _maxArmor;

    public float _defense = 0f;
    public float baseArmor = 50f;
    public float baseDefense = 0f;
    public float baseRegenArmor = 0f;

    public float speedBonus = 0f;
    public float damageBonus = 0f;
    public float defenseBonus = 0f;

    private PhotonView view;

    // Effect

    private bool isDamaged = false;
    private float timer = 0f;
    private float duration = 0.2f;

    private float armorCD = 0f;
    private float _armorGain = 0f;

    public string deathAnim = "";

    public AudioSource hurt;

    private List<DamageEffect> effects = new List<DamageEffect>();

    private void Awake()
    {
        view = GetComponent<PhotonView>();

        foreach (Transform i in transform.Find("Model").transform)
        {
            renderedPart.Add(i);
            parts.Add(i.GetComponent<SpriteRenderer>());
            partOriginalColor.Add(i.GetComponent<SpriteRenderer>().color);
        }
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
        armorCD += Time.fixedDeltaTime;

        if (isDamaged)
        {
            timer += Time.fixedDeltaTime;
            float t = timer / duration;

            for (int i = 0; i < renderedPart.Count; i++)
            {
                Color lerpColor = Color.Lerp(hitColor, partOriginalColor[i], t);

                parts[i].color = lerpColor;
            }

            if (timer >= duration)
            {
                timer = 0f;
                isDamaged = false;
            }
        }

        if (_currentArmor < _maxArmor)
        {
            if (armorCD >= 0f)
            {
                armorCD -= 2f;
                AddArmor((int)_armorGain);
            }
        }

        if (effects.Count > 0)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].lifeTime <= 0f)
                {
                    effects[i].lifeTime = 0f;
                }
                else
                {
                    effects[i].timer += Time.fixedDeltaTime;
                    if (effects[i].timer >= effects[i].tick)
                    {
                        float amount = Mathf.Floor(effects[i].timer / effects[i].tick);

                        foreach (Modify mod in effects[i].effects)
                        {
                            switch (mod.modify_ID)
                            {
                                case "HP":
                                    if (mod.modify_IntValue > 0)
                                    {
                                        TakeDamage((int)(mod.modify_IntValue * amount));
                                    } else
                                    {
                                        Heal((int)((0-mod.modify_IntValue) * amount));
                                    }
                                    break;
                                case "MP":
                                    Player player = transform.GetComponent<Player>();
                                    if (player != null)
                                    {
                                        if (mod.modify_IntValue > 0)
                                        {
                                            player.LoseMana(amount * mod.modify_IntValue);
                                        }
                                        else
                                        {
                                            player.GainMana(amount * (0-mod.modify_IntValue));
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        effects[i].timer -= amount * effects[i].tick;
                        effects[i].lifeTime -= amount * effects[i].tick;
                    }
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        float damageDeal = damage;
        if (damageDeal > 0) damageDeal = Mathf.Max(1, (float)damage - _defense);
        armorCD = -10f;
        if (_currentArmor > 0 && damage>0)
        {
            if (_currentArmor > damageDeal)
            {
                _currentArmor -= damageDeal;
                UpdateStats();
                return;
            }
            else
            {
                damageDeal -= _currentArmor;
                _currentArmor = 0f;
            }
        }

        if (_currentHealth <= 0) return;
       
        if (damage > 0)
        {
            view.RPC("RPC_FlashEffect", RpcTarget.All);
            GameManager.Instance.ShowDamage(transform.position, (int)damageDeal);
        } else
        {
            if (damage < 0)
            {
                view.RPC("RPC_FlashEffectHeal", RpcTarget.All);
                GameManager.Instance.ShowHeal(transform.position, (int)-damageDeal);
            }
        }

        _currentHealth = Mathf.Clamp(_currentHealth - damageDeal, 0, MaxHealth);
        UpdateStats();

    }

    public void addEffect(DamageEffect effect)
    {
        //Debug.Log("Adding Effect");
        if (CurrentArmor == 0)
        {
            int amount = effects.Count;
            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (effects[i].lifeTime <= 0f && i < amount)
                    {
                        DamageEffect damageEffect = new DamageEffect(effect);
                        effects[i] = damageEffect;
                        return;
                    }
                }
            }
            effects.Add(new DamageEffect(effect));
        }
    }

    public void calculateArmor(List<ItemInstance> items, ItemInstance holdingItem)
    {
        float maxArmor = baseArmor;
        float regenArmor = baseRegenArmor;
        float defense = baseDefense;

        if (holdingItem != null && holdingItem.itemRef)
        {
            maxArmor += holdingItem.itemRef.armor;
            regenArmor += holdingItem.itemRef.armor_regen;
            defense += holdingItem.itemRef.defense;
        }

        foreach (ItemInstance item in items)
        {
            if (item != null && item.itemRef != null)
            {
                maxArmor += item.itemRef.armor;
                regenArmor += item.itemRef.armor_regen;
                defense += item.itemRef.defense;
            }
        }

        _maxArmor = maxArmor;
        _defense = defense;
        _armorGain = regenArmor;

        _currentArmor = Mathf.Min(_currentArmor, _maxArmor);
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        view.RPC("RPC_FlashEffectHeal", RpcTarget.All);
        GameManager.Instance.ShowHeal(transform.position, (int)amount);
        UpdateStats();
    }

    [PunRPC]
    public void RPC_UpdateStats(float health, float armor)
    {
        _currentHealth = health;
        _currentArmor = armor;
    }

    public void UpdateStats()
    {
        view.RPC("RPC_UpdateStats", RpcTarget.Others, CurrentHealth, _currentArmor);
    }

    [PunRPC]
    public void RPC_UpdateMaxHP(float newHealth)
    {
        _maxHealth = newHealth;
    }

    public void AddMaxHP(float amount)
    {
        _maxHealth += amount;
        view.RPC("RPC_UpdateMaxHP", RpcTarget.Others, _maxHealth);
    }

    public void AddArmor(int amount)
    {
        _currentArmor = Mathf.Min(_maxArmor, _currentArmor + amount);
        UpdateStats();
    }

    public void SetBaseDefense(int amount)
    {
        baseDefense = amount;
    }

    public void SetBaseRegenArmor(int amount)
    {
        baseRegenArmor = amount;
    }


    public void SetBaseArmor(float amount)
    {
        baseArmor = amount;
    }

    public void SetHealth(int amount)
    {
        _currentHealth = amount;
        UpdateStats();
    }

    public void SetMaxHealth(int amount)
    {
        _maxHealth = amount;
        SetHealth(amount);
    }


    public void SetArmor(int amount)
    {
        _currentArmor = amount;
        UpdateStats();
    }

    [PunRPC]
    public void RPC_FlashEffect()
    {
        if (hurt != null)
        {
            if (!hurt.isPlaying) hurt.Play();
        }
        timer = 0f;
        isDamaged = true;
        hitColor = _damageColor;
        foreach (SpriteRenderer part in parts)
        {
            part.color = _damageColor;
        }
    }

    [PunRPC]
    public void RPC_FlashEffectHeal()
    {
        timer = 0f;
        isDamaged = true;
        hitColor = _healColor;
        foreach (SpriteRenderer part in parts)
        {
            part.color = _healColor;
        }
    }

    private IEnumerator FlashEffect()
    {
        foreach (Transform i in renderedPart)
        {
            i.GetComponent<SpriteRenderer>().color = _damageColor;
        }

        yield return new WaitForSeconds(0.25f);

        for (int i=0; i<renderedPart.Count; i++)
        {
            renderedPart[i].GetComponent<SpriteRenderer>().color = partOriginalColor[i];
        }
    }

    private IEnumerator FlashEffectHeal()
    {
        foreach (Transform i in renderedPart)
        {
            i.GetComponent<SpriteRenderer>().color = _healColor;
        }

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < renderedPart.Count; i++)
        {
            renderedPart[i].GetComponent<SpriteRenderer>().color = partOriginalColor[i];
        }
    }
}