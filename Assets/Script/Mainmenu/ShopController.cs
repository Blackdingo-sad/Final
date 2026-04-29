using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance;
    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid, playerInventoryGrid;
    public GameObject shopSlotPrefab;
    public TMP_Text shopTitleText;

    private ItemDictionary itemDictionary;
    private ShopNPC currentShop;
    private float lastCloseTime = -1f;
    private bool wasShopOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        shopPanel.SetActive(false);
    }

    void Update()
    {
        if (wasShopOpen && !shopPanel.activeSelf)
        {
            wasShopOpen = false;
            currentShop = null;
            lastCloseTime = Time.unscaledTime;
            PauseController.SetPause(false);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;
        shopPanel.SetActive(true);
        wasShopOpen = true;
        if (shopTitleText != null)
        {
            shopTitleText.text = shop.shopkeeperName + "'s Shop";
        }
        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();

        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        wasShopOpen = false;
        shopPanel.SetActive(false);
        currentShop = null;
        lastCloseTime = Time.unscaledTime;
        PauseController.SetPause(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public bool WasRecentlyClosed()
    {
        return Time.unscaledTime - lastCloseTime < 0.2f;
    }

    public void RefreshShopDisplay()
    {
        if (currentShop == null)
        {
            return;
        }
        foreach (Transform child in shopInventoryGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var stockItem in currentShop.GetCurrentStock())
        {
            if (itemDictionary == null || stockItem.quantity < 0) continue;
            CreateShopSlot(shopInventoryGrid, stockItem.itemID, stockItem.quantity, true);
        }
    }
    public void RefreshPlayerInventoryDisplay()
    {
        if (InventoryController.Instance == null)
        {
            return;
        }
        foreach (Transform child in playerInventoryGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
        {
            Slot inventorySlot = slotTransform.GetComponent<Slot>();
            if (inventorySlot = slotTransform.GetComponent<Slot>())
            {
                if (inventorySlot.currentItem != null)
                {
                    Item originalItem = inventorySlot.currentItem.GetComponent<Item>();
                    CreateShopSlot(playerInventoryGrid, originalItem.ID, originalItem.quantity, false, inventorySlot);
                }
            }
        }
    }

    private void CreateShopSlot(Transform grid, int itemID, int quantity, bool isShop, Slot originalSlot = null)
    {
        GameObject slotObj = Instantiate(shopSlotPrefab, grid);
        GameObject itemPrefab = itemDictionary.GetItemPrefab(itemID);
        if (itemPrefab == null) return;

        Item sourceItem = itemPrefab.GetComponent<Item>();
        GameObject prefabToInstantiate = sourceItem != null && sourceItem.uiPrefab != null ? sourceItem.uiPrefab : itemPrefab;

        GameObject itemInstance = Instantiate(prefabToInstantiate, slotObj.transform);
        itemInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        if (itemInstance.GetComponent<CanvasGroup>() == null)
        {
            itemInstance.AddComponent<CanvasGroup>();
        }
        if (itemInstance.GetComponent<ItemDragHandler>() == null)
        {
            itemInstance.AddComponent<ItemDragHandler>();
        }

        Item item = itemInstance.GetComponent<Item>();
        if (sourceItem != null)
        {
            item.ID = sourceItem.ID;
        }
        item.quantity = quantity;
        item.UpdateQuantityDisplay();

        int price = isShop ? item.buyPrice : item.GetSellPrice();

        ShopSlot slot = slotObj.GetComponent<ShopSlot>();
        slot.isShopSlot = isShop;
        slot.SetItem(itemInstance, price);

        ItemDragHandler dragHandler = itemInstance.GetComponent<ItemDragHandler>();
        if (dragHandler)
        {
            dragHandler.enabled = false;

            ShopItemHandler handler = itemInstance.AddComponent<ShopItemHandler>();
            handler.Initialise(isShop);
            if (!isShop)
            {
                handler.originalInventorySlot = originalSlot;
            }
        }
    }

    public void AddItemToShop(int itemID, int quantity)
    {
        if (!currentShop) return;
        currentShop.AddtoStock(itemID, quantity);
        RefreshShopDisplay();
    }

    public bool RemoveItemFromShop(int itemID, int quantity)
    {
        if (!currentShop) return false;
        bool success = currentShop.RemoveFromStock(itemID, quantity);
        if (success) RefreshShopDisplay();
        return success;
    }
}
