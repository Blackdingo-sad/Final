using UnityEngine;

public class FarmTile : MonoBehaviour, IInteractable
{
    public enum FarmState { Tilled, Seeded, Growing, Ready }

    [Header("Tr?ng thái")]
    public FarmState state = FarmState.Tilled;

    [Header("Hi?n th?")]
    [SerializeField] private SpriteRenderer cropRenderer;
    [SerializeField] private Sprite tilledSprite;

    private CropData cropData;
    private float growTimer;
    private SpriteRenderer tileRenderer;

    private void Awake()
    {
        tileRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (state == FarmState.Growing)
        {
            growTimer -= Time.deltaTime;
            if (growTimer <= 0f)
            {
                SetState(FarmState.Ready);
            }
        }
    }


    public bool TryPlant(CropData data)
    {
        if (state != FarmState.Tilled) return false;

        cropData = data;
        growTimer = cropData.growTime;
        SetState(FarmState.Seeded);
        Invoke(nameof(StartGrowing), 0.1f);
        return true;
    }

    private void StartGrowing()
    {
        SetState(FarmState.Growing);
    }

    private void Harvest()
    {
        if (cropData == null || cropData.harvestItems == null) return;

        foreach (HarvestEntry entry in cropData.harvestItems)
        {
            if (entry.itemPrefab == null) continue;
            for (int i = 0; i < entry.quantity; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.5f;
                Instantiate(entry.itemPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        Debug.Log($"<color=green>Thu ho?ch {cropData.cropName}!</color>");
        Destroy(gameObject);
    }

    private void SetState(FarmState newState)
    {
        state = newState;
        UpdateVisual();
        Debug.Log($"<color=cyan>FarmTile ? {newState}</color>");
    }

    private void UpdateVisual()
    {
        if (cropRenderer == null) return;

        switch (state)
        {
            case FarmState.Tilled:
                cropRenderer.sprite = null;
                cropRenderer.enabled = false;
                break;
            case FarmState.Seeded:
            case FarmState.Growing:
                cropRenderer.enabled = true;
                cropRenderer.sprite = cropData?.growingSprite;
                break;
            case FarmState.Ready:
                cropRenderer.enabled = true;
                cropRenderer.sprite = cropData?.readySprite;
                break;
        }
    }

  

    public bool CanInteract()
    {
        return state == FarmState.Tilled || state == FarmState.Ready;
    }

    public void Interact()
    {
        if (state == FarmState.Ready)
        {
            Harvest();
            return;
        }

        //if (state == FarmState.Tilled)
        //{
   
        //    FarmingHandler handler = FindObjectOfType<FarmingHandler>();
        //    if (handler != null)
        //    {
        //        handler.TryPlantSeed(this);
        //    }
        //}
    }
}
