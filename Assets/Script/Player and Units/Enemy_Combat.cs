using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{   
    public int damage = 1;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ch? gây damage n?u va ch?m ?úng Player
        if (!collision.gameObject.CompareTag("Player")) return;

        PLayerHealth playerHealth = collision.gameObject.GetComponent<PLayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(-damage);
            Debug.Log($"<color=red>[{gameObject.name}] Hit Player for {damage} damage!</color>");
        }
    }
}
