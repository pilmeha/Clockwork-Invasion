using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShooter : MonoBehaviour
{
    public GameObject bullet;
    public Transform shootOrigin;
    public float cooldown;
    private bool canShoot;

    public float range;
    public LayerMask ShootMask;

    private GameObject Target;

    private void Start()
    {
        Invoke("ResetCooldown", cooldown);
    }

    private void Update()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, range, ShootMask);

        if (hit.collider)
        {
            Target = hit.collider.gameObject;
            Shoot();
        }
    }

    void ResetCooldown()
    {
        canShoot = true;
    }

    void Shoot()
    {
        if (!canShoot) return;
        canShoot = false;
        Invoke("ResetCooldown", cooldown);

        GameObject myBullet = Instantiate(bullet, shootOrigin.position, Quaternion.identity);
    }
}
