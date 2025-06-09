using UnityEngine;

public class Effect : MonoBehaviour
{
    Animation animation;
    ParticleSystem particle;

    public int emitAmount = 10;

    private void Awake()
    {
        animation = GetComponent<Animation>();
        particle = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (particle != null)
        {
            particle.Emit(emitAmount);
        }
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
