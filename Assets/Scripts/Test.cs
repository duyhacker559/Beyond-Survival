using UnityEngine;

[CreateAssetMenu(fileName = "Test", menuName = "Scriptable Objects/Test")]
public class Test : ScriptableObject
{
    public MonoBehaviour Script;
    public Collider2D Collider;
}
