using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[System.Serializable]
public class ProjectileData
{
    // Basic Stats
    public float speed;
    public float damage;
    public float lifeTime = 2f;
    public float kb = 0f;
    public float stun = 0f;
    public Transform group;
    public List<DamageEffect> effect;
    public ProjectileItem special;

    public ProjectileData(float speed, float damage, float kb, float stun, float lifeTime, List<DamageEffect> effect, ProjectileItem special, Transform group)
    {
        this.speed = speed;
        this.damage = damage;
        this.kb = kb;
        this.stun = stun;
        this.lifeTime = lifeTime;
        this.effect = effect;
        this.special = special;
        this.group = group;
    }

    public ProjectileData(ProjectileData item)
    {
        this.speed = item.speed;
        this.damage = item.damage;
        this.kb = item.kb;
        this.stun = item.stun;
        this.lifeTime = item.lifeTime;
        this.effect = item.effect;
        this.special = item.special;
        this.group = item.group;
    }
}


public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lookVT;

    private HashSet<HealthSystem> piercedTargets = new HashSet<HealthSystem>();
    [SerializeField] public ProjectileData itemData;

    public void Initialize(ProjectileData data)
    {
        itemData = data;
    }

    void Start()
    {
        lookVT = transform.right;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * itemData.speed; // bắn theo hướng mặt phải
        Destroy(gameObject, itemData.lifeTime);

        if (itemData.special.overrideTarget)
        {
            if (itemData.special.target == Target.Player)
            {
                itemData.group = Game.g_players.transform;
            } else
            {
                itemData.group = Game.g_enemies.transform;
            }
        }
    }

    public void Knockback(Rigidbody2D rb, float kb)
    {
        if (rb == null) return;
        Vector2 direction = lookVT.normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * kb, ForceMode2D.Impulse);
    }

    private void OnDestroy()
    {
        AreaAttack();
    }

    private void AreaAttack()
    {
        try
        {
            if (itemData.special.areaAttack)
            {
                GameManager.SummonAttackArea(
                 transform.position,
                 Quaternion.Euler(0, 0, 0),
                 new AreaInstance(
                     itemData.special.damage,
                     itemData.special.knockBack,
                     itemData.special.knockBackDuration,
                     itemData.special.effects,
                     itemData.special.hitbox,
                     Game.g_enemies.transform,
                     itemData.special.areaAttackDat
                     )
                );
            }
        }
        catch
        {

        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Gây sát thương nếu cần
        if (other.CompareTag(itemData.group.gameObject.tag))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();

            if (health != null)
            {

                if (health.CurrentHealth > 0f && !piercedTargets.Contains(health))
                {
                    piercedTargets.Add(health);

                    health.TakeDamage((int)itemData.damage);

                    if (health.CurrentHealth <= 0f && itemData.special.overrideDeathAnim)
                    {
                        health.deathAnim = itemData.special.deathAnim;
                    }

                    try
                    {
                        EnemyAI enemyAI = other.GetComponent<EnemyAI>();
                        if (enemyAI != null)
                        {
                            enemyAI.Stunned(itemData.stun);
                        }
                    }
                    catch { }

                    Knockback(other.GetComponent<Rigidbody2D>(), itemData.kb);

                    foreach (DamageEffect effect in itemData.effect)
                    {
                        health.addEffect(effect);
                    }

                    if (itemData.special.emitEffect)
                    {
                        GameManager.Instance.SummonEffect(itemData.special.animEffect.name, rb.position);
                    }
    
                    if (itemData.special.applyAll)
                    {
                        AreaAttack();
                    }

                    if (!itemData.special.canPierc)
                    {
                        Destroy(gameObject);
                        return;
                    }

                    if (piercedTargets.Count >= itemData.special.piercAmount)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }

        // Chạm tường xoá đạn
        if (other.CompareTag("Barrier"))
        {

            if (itemData.special.emitEffect)
            {
                GameManager.Instance.SummonEffect(itemData.special.animEffect.name, rb.position);
            }

            Destroy(gameObject);
        }
    }
}