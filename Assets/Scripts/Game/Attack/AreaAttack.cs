using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

[System.Serializable]
public class AreaInstance
{
    // Basic Stats
    public float damage;
    public float kb;
    public float stun;
    public GameObject collider;
    public Transform group;
    public List<DamageEffect> effects;
    public AreaAttackItem special;

    public AreaInstance(float damage, float kb, float stun, List<DamageEffect> effects, GameObject collider, Transform group, AreaAttackItem special)
    {
        this.damage = damage;
        this.kb = kb;
        this.stun = stun;
        this.group = group;
        this.collider = collider;
        this.effects = effects;
        this.special = special;
    }
}

public class AreaAttack : MonoBehaviour
{
    public AreaInstance data;
    GameObject hitbox;

    private Collider2D attackCollider;
    private HashSet<HealthSystem> piercedTargets = new HashSet<HealthSystem>();
    private LayerMask enemyLayer;

    public void Initiate(AreaInstance areaInstance)
    {
        data = areaInstance;
        hitbox = Instantiate(data.collider, transform);

        attackCollider = hitbox.GetComponent<PolygonCollider2D>();

        enemyLayer = (1 << data.group.gameObject.layer);
    }

    public void Knockback(Rigidbody2D rb, float kb)
    {
        if (rb == null) return;
        Vector2 direction = (rb.position - new Vector2(transform.position.x, transform.position.y)).normalized;
        rb.linearVelocity = Vector2.zero;

        rb.AddForce(direction * kb, ForceMode2D.Impulse);
    }

    public void Attack()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int hitCount = attackCollider.Overlap(filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            if (results[i].transform.IsChildOf(data.group))
            {
                HealthSystem health = results[i].GetComponent<HealthSystem>();

                if (health != null)
                {
                    if (health.CurrentHealth > 0 && !piercedTargets.Contains(health))
                    {
                        piercedTargets.Add(health);

                        health.TakeDamage((int)data.damage);

                        if (health.CurrentHealth <= 0f && data.special.overrideDeathAnim)
                        {
                            health.deathAnim = data.special.deathAnim;
                        }

                        try
                        {
                            EnemyAI enemyAI = results[i].GetComponent<EnemyAI>();
                            if (enemyAI != null)
                            {
                                enemyAI.Stunned(data.stun);
                            }
                        }
                        catch
                        {

                        }

                        Knockback(results[i].GetComponent<Rigidbody2D>(), data.kb);

                        foreach (DamageEffect effect in data.effects.ToList<DamageEffect>())
                        {
                            health.addEffect(effect);
                        }
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}
