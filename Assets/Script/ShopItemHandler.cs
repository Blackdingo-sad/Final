using System.Drawing.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShopItem;
    public Slot originalInventorySlot;

    public void Initialise(bool ShopItem)
    {
        this.isShopItem = ShopItem;
        //this.originalInventorySlot = originalSlot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isShopItem)
            {
                BuyItem(); 
            }
            else
            {
                SellItem();      
            }
        }
    }
    private void BuyItem()
    {
        // Implement buying logic here
        Item item = GetComponent<Item>();
        ShopSlot slot = GetComponentInParent<ShopSlot>();
        if (!item || !slot) return;

        if (CurrencyController.Instance.GetGold() < slot.itemPrice)
        {
            //message to player about not enough gold
            Debug.Log("Not enough gold to buy this item.");
            return;
        }
        GameObject itemPrefab = FindObjectOfType<ItemDictionary>().GetItemPrefab(item.ID);
        if (InventoryController.Instance.AddItem(itemPrefab))
        {
            CurrencyController.Instance.SpendGold(slot.itemPrice);
            //ShopController.Instance.RefreshShopDisplay();
            ShopController.Instance.RefreshPlayerInventoryDisplay();
            ShopController.Instance.RemoveItemFromShop(item.ID, 1);
        }
        else
        {
            //message to player about inventory full
            Debug.Log("Not enough space in inventory to buy this item.");
        }
    }

    private void SellItem()
    {
        Item item = GetComponent<Item>();
        ShopSlot slot = GetComponentInParent<ShopSlot>();
        if (!item || !slot || !originalInventorySlot) return;

        Item invItem = originalInventorySlot.currentItem?.GetComponent<Item>();
        if (!invItem) return;   

        if (invItem.quantity >1) invItem.RemoveFromStack(1);
        else 
        {
            Destroy(originalInventorySlot.currentItem);
            originalInventorySlot.currentItem = null;
        }

        InventoryController.Instance.RebuildItemCounts();
        CurrencyController.Instance.AddGold(slot.itemPrice);
        ShopController.Instance.RefreshPlayerInventoryDisplay(); 
        ShopController.Instance.AddItemToShop(item.ID, 1);
    }
}
