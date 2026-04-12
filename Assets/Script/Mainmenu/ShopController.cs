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
    public TMP_Text playerMoneyText, shopTitleText;

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
        if (CurrencyController.Instance != null)
        { 
            CurrencyController.Instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }
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

    private void UpdateMoneyDisplay(int amount)
    {
        if (playerMoneyText != null)
        {
            playerMoneyText.text = amount.ToString();
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
            GameObject slot = Instantiate(shopSlotPrefab, shopInventoryGrid);
            if (itemDictionary != null)
            {
                if (stockItem.quantity < 0) continue;
            }
        }
    }
    public void RefreshPlayerInventoryDisplay()
    {
        if (InventoryController.Instance == null)
        {
            return;
        }
        foreach (Transform child in shopInventoryGrid)
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
                }
            }
        }
    }
}
