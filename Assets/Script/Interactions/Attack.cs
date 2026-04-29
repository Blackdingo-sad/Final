using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float damage = 5f;
    public float fireRate = 0.3f;

    private float _nextFireTime;

    private void Update()
    {
        if (PauseController.IsGamePaused) return;

        if ((Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)) && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[Attack] bulletPrefab is not assigned!");
            return;
        }

        // L?y v? trí chu?t trong world space
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        origin.z = 0f;

        // Tính h??ng t? player (transform g?c) ??n chu?t, không dùng firePoint
        // ?? tránh sai l?ch do camera offset/look-ahead
        Vector3 playerPos = transform.position;
        playerPos.z = 0f;
        Vector2 direction = (mouseWorld - playerPos).normalized;

        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);

        // Xoay bullet sprite theo h??ng
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Init(direction, damage);
        }
        else
        {
            Debug.LogWarning("[Attack] bulletPrefab is missing Bullet component!");
        }

        Debug.Log($"[Attack] Fired bullet toward {direction}, damage={damage}");
    }
}
