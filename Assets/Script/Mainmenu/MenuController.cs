using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    [Tooltip("Index of the Inventory tab in TabController.pages array")]
    [SerializeField] private int inventoryTabIndex = 1;

    private TabController tabController;

    void Start()
    {
        menuCanvas.SetActive(false);
        tabController = menuCanvas.GetComponentInChildren<TabController>(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
                return;

            if (menuCanvas.activeSelf)
                CloseInventory();
            else
                OpenInventory();
        }
    }

    public void OpenInventory()
    {
        menuCanvas.SetActive(true);
        PauseController.SetPause(true);
        tabController?.ActivateTab(inventoryTabIndex);
    }

    public void CloseInventory()
    {
        menuCanvas.SetActive(false);
        PauseController.SetPause(false);
        tabController?.OnCloseMenu();
        SeedSelectionManager.Instance?.CancelSelection();
    }
}
