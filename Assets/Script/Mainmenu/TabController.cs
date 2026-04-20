using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
    public GameObject itemPopupContainer; // Gán ItemPopupContainer vào ?ây trong Inspector

    void Start()
    {
        DisablePageRaycast();
        ActivateTab(0);
    }

    // T?t Raycast Target trên t?t c? background Image trong các page
    // ?? tránh b? block click vào tab buttons
    private void DisablePageRaycast()
    {
        foreach (GameObject page in pages)
        {
            Image[] images = page.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                // Ch? t?t raycast trên Image tr?c ti?p c?a page (background)
                // không t?t raycast trên các Button/Interactive elements bên trong
                if (img.GetComponent<Button>() == null &&
                    img.GetComponent<UnityEngine.EventSystems.EventTrigger>() == null)
                {
                    img.raycastTarget = false;
                }
            }
        }
    }

    // Old debug code (comment):
    // private System.Collections.IEnumerator DebugRaycastBlockers() { ... }
    // private string GetFullPath(Transform t) { ... }

    public void ActivateTab(int tabNO)
    {
        Debug.Log($"ActivateTab called with index: {tabNO}");

        // ?n ItemPopupContainer khi chuy?n tab ?? tránh b? che
        if (itemPopupContainer != null)
        {
            itemPopupContainer.SetActive(false);
        }

        for (int i = 0; i < tabImages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        pages[tabNO].SetActive(true);
        tabImages[tabNO].color = Color.white;

        Debug.Log($"ActivateTab done: {pages[tabNO].name} is now active");
    }
}
