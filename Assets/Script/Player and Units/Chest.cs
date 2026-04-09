using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool IsOpen { get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab; // Prefab của item sẽ được tạo ra khi mở rương
    public Sprite openSprite;

    public bool CanInteract()
    {
        return !IsOpen;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenChest();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    private void OpenChest()
    {
        SetOpened(true);
        // drop item
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
         //   droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    public void SetOpened(bool opened)
    {
        IsOpen = opened;
        if (IsOpen)
        {
            GetComponent<SpriteRenderer>().sprite = openSprite;
        }
    }
}
