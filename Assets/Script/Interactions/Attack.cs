using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float damage = 5f;
    public float fireRate = 0.3f;

    [Header("Required Hotbar Item")]
    [Tooltip("Chi cho phep tan cong khi dang trang bi item co ItemType nay trong HotBar")]
    public ItemType requiredItemType = ItemType.Weapon;

    private float _nextFireTime;

    private void Update()
    {
        if (PauseController.IsGamePaused) return;

        if ((Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)) && Time.time >= _nextFireTime)
        {
            if (!IsRequiredItemEquipped()) return;
            Shoot();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private bool IsRequiredItemEquipped()
    {
        Item selected = HotbarController.Instance?.GetSelectedItem();
        if (selected == null || selected.itemType != requiredItemType)
        {
            Debug.Log(
"[Attack] Cannot attack — need equip: "
 + requiredItemType + 
" | Current: "
 + (selected?.itemType.ToString() ?? 
"empty"
));
            return false;
        }
        return 
true;
    }

    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[Attack] bulletPrefab is not assigned!");
            return;
        }

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        origin.z = 0f;

        Vector3 playerPos = transform.position;
        playerPos.z = 0f;
        Vector2 direction = (mouseWorld - playerPos).normalized;

        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);

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
