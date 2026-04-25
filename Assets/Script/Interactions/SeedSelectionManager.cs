using TMPro;
using UnityEngine;

// Attach to UICanvas. Manages the state of waiting for player to select a seed from Inventory.
public class SeedSelectionManager : MonoBehaviour
{
    public static SeedSelectionManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

    private FieldPlot _targetPlot;

    public bool IsSelecting => _targetPlot != null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (promptPanel != null)
            promptPanel.SetActive(false);
    }

    // Called when player interacts with a plot
    public void StartSelection(FieldPlot plot)
    {
        _targetPlot = plot;
        ShowPrompt("Please choose seed from your inventory to plant.");

        MenuController menu = Object.FindFirstObjectByType<MenuController>();
        Debug.Log($"[SeedSelectionManager] StartSelection called. MenuController found: {menu != null}");

        if (menu != null)
        {
            menu.OpenInventory();
            Debug.Log("[SeedSelectionManager] OpenInventory called.");
        }
        else
        {
            Debug.LogWarning("[SeedSelectionManager] MenuController NOT FOUND in scene!");
        }
    }

    // Called when player clicks an item in the inventory
    public void TrySelectItem(Item item)
    {
        if (!IsSelecting) return;

        if (item.itemType == ItemType.Seed)
        {
            _targetPlot.PlantSeed(item);

            // Consume 1 seed
            item.RemoveFromStack(1);
            if (item.quantity <= 0)
            {
                Slot slot = item.transform.parent?.GetComponent<Slot>();
                if (slot != null) slot.currentItem = null;
                Object.Destroy(item.gameObject);
                InventoryController.Instance?.RebuildItemCounts();
            }

            CancelSelection();
            MenuController menu = Object.FindFirstObjectByType<MenuController>();
            menu?.CloseInventory();
        }
        else
        {
            ShowPrompt("Please choose Seed to plant.");
        }
    }

    // Called when inventory is closed
    public void CancelSelection()
    {
        _targetPlot = null;
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }

    void ShowPrompt(string message)
    {
        if (promptPanel != null) promptPanel.SetActive(true);
        if (promptText != null) promptText.text = message;
    }
}
