using UnityEngine;
using UnityEngine.UI;
using TMPro;    
public enum ItemType
{
    None,
    Weapon,
    Hoe,
    Seed,
    WateringCan,
    Tool,
    Consumable,
    Material,
    Quest,
    Lantern
}
public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;

    public ItemType itemType = ItemType.None;
    public GameObject worldPrefab;   // dùng khi drop ra map
    public GameObject uiPrefab;      // dùng khi vào inventory

    // Chỉ dùng khi ItemType = Seed
    [HideInInspector] public CropData cropData;

    [SerializeField] private TMP_Text quantityText;

    //shpo fields
    public int buyPrice = 10;
    [Range(0,1)]
    public float sellPriceMultiplier = 0.5f; // 50% of buy price

    private void Awake()
    {
        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TMP_Text>(true);
        }
        
    }

    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiplier);
    }

    void Start()
    {
        UpdateQuantityDisplay();
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TMP_Text>(true);
        }

        if (quantityText == null)
        {
            Debug.LogWarning("Missing TMP_Text on: " + gameObject.name);
            return;
        }
        
        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject Clone = Instantiate(gameObject);
        Item itemComponent = Clone.GetComponent<Item>();
        itemComponent.quantity = newQuantity; 
        itemComponent.UpdateQuantityDisplay();
        return Clone;
    } 

    public virtual void UseItem()
    {
        Debug.Log("Using item" + Name);
    }

    public virtual void PickUp()
    {
        Sprite itemIcon = GetComponent<SpriteRenderer>().sprite;
        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }
}
