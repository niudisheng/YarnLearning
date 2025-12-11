using UnityEngine;

public enum BulletType
{
    normal,
}
[CreateAssetMenu(fileName = "New Bullet", menuName = "Bullet")]
public class BulletData : ScriptableObject
{
    
    public BulletType bulletType;
    public float speed;
    public float damage;
    public float range;
    public float lifeTime;
    public Vector2 direction;

    public Vector2 GetVelocity()
    {
        return direction.normalized * speed;
    }
}
