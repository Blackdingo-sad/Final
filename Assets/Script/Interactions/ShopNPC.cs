using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour, IInteractable
{
    public string shopID = "shop_merchant_01";
    public string shopkeeperName = "Merchant";

    public List<ShopStockItem> defaultShopStock = new();
    private List<ShopStockItem> currentShopStock = new();

    private bool isInitialized = false;

    [System.Serializable]

    public class ShopStockItem
    {
        public int itemID;
        public int quantity;
    }
    void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        if (isInitialized) return;
        currentShopStock = new List<ShopStockItem>();
        foreach (var stockItem in defaultShopStock)
        {
            currentShopStock.Add(new ShopStockItem
            {
                itemID = stockItem.itemID,
                quantity = stockItem.quantity
            });
        }
        isInitialized = true;
    }
    public bool CanInteract()
    {
        if (ShopController.Instance != null && ShopController.Instance.shopPanel.activeSelf)
        {
            return false;
        }
        return true;
    }

    public void Interact()
    {
        if (ShopController.Instance == null)
        {
            return;
        }
        if (!ShopController.Instance.shopPanel.activeSelf && !ShopController.Instance.WasRecentlyClosed())
        {
            ShopController.Instance.OpenShop(this);
        }
    }   

    public List<ShopStockItem> GetCurrentStock()
    {
        return currentShopStock;
    }

    public void SetStock(List<ShopStockItem> stock)
    {
        currentShopStock = stock;
    }

    public void AddtoStock(int itemID, int quantity)
    {
        ShopStockItem existing = currentShopStock.Find(s => s.itemID == itemID);
        if ( existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            currentShopStock.Add(new ShopStockItem { itemID = itemID, quantity = quantity });
        }
    }

    public bool RemoveFromStock(int itemID, int quantity)
    {
        ShopStockItem existing = currentShopStock.Find(s => s.itemID == itemID);
        if (existing != null && existing.quantity >= quantity)
        {
            existing.quantity -= quantity;
            if (existing.quantity <= 0)
            {
                currentShopStock.Remove(existing);
            }
            return true;
        }
        return false;
    }

}
