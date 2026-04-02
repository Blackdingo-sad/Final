using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryController = Object.FindFirstObjectByType<InventoryController>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {   
            Item item = collision.GetComponent<Item>();
            if (item != null)
            {
                bool itemAdded = inventoryController.AddItem(collision.gameObject);
                if (itemAdded)
                {
                    item.PickUp();
                    Destroy(collision.gameObject);
                }              
            }
        }
    }
}
