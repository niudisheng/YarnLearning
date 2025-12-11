using System;
using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    private BulletData bulletData;
    private Rigidbody2D rb;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(BulletData bulletData1)
    {
        this.bulletData = bulletData1;
        Debug.Log("Bullet velocity: " + rb.linearVelocity);
        rb.linearVelocity = bulletData.GetVelocity();
        
    }
}
