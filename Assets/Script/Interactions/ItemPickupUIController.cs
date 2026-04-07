using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickupUIController : MonoBehaviour
{
    public static ItemPickupUIController Instance { get; private set; }
    
    public GameObject popupPrefab;
    public int MaxPopups = 5;
    public float popupDuration;

    private readonly Queue<GameObject> activePopups = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogError("Multiple ItemPickupUIController detected! Destroy the extra.");
            Destroy(gameObject);
        }
    }

    public void ShowItemPickup(string itemName, Sprite itemIcon)
    {
        GameObject newPopup = Instantiate(popupPrefab, transform);
        newPopup.GetComponentInChildren<TMP_Text>().text = itemName;

        Image itemImage = newPopup.transform.Find("ItemIcon")?.GetComponent<Image>();

        if (itemImage)
        {
            itemImage.sprite = itemIcon;
        }

        activePopups.Enqueue(newPopup);
        if (activePopups.Count > MaxPopups)
        {
            Destroy(activePopups.Dequeue());
        }
        StartCoroutine(FadeOutAndDestroy(newPopup));    
    }

    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popupDuration);
        if (popup == null) yield break;

        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        for (float timePassed = 0f; timePassed < 1f; timePassed += Time.deltaTime)
        {
            if (canvasGroup == null) break;
            canvasGroup.alpha = 1f - timePassed;
            yield return null;
        }
        Destroy(popup);
    }
}
