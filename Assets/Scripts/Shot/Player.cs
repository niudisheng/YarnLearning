using UnityEngine;

public class Player : MonoBehaviour
{
    public BaseBullet bulletPrefab;
    [SerializeField] private BulletData bulletData;

    [ContextMenu("Shoot")]
    public void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<BaseBullet>().Init(bulletData);
    }
}
