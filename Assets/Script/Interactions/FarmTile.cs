using UnityEngine;

/// <summary>
/// ??t script này lên prefab FarmTile (ô ??t ?ã cu?c).
/// Tr?ng thái: Tilled ? Seeded ? Growing ? Ready
/// </summary>
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

    /// <summary>
    /// Tr?ng h?t gi?ng lên ô ??t này.
    /// </summary>
    public bool TryPlant(CropData data)
    {
        if (state != FarmState.Tilled) return false;

        cropData = data;
        growTimer = cropData.growTime;
        SetState(FarmState.Seeded);

        // B?t ??u t?ng tr??ng ngay sau 1 frame
        Invoke(nameof(StartGrowing), 0.1f);
        return true;
    }

    private void StartGrowing()
    {
        SetState(FarmState.Growing);
    }

    private void Harvest()
    {
        if (cropData == null || cropData.harvestItemPrefab == null) return;

        for (int i = 0; i < cropData.harvestQuantity; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            Instantiate(cropData.harvestItemPrefab,
                        (Vector2)transform.position + offset,
                        Quaternion.identity);
        }

        Debug.Log($"<color=green>Thu ho?ch {cropData.harvestQuantity}x {cropData.cropName}!</color>");
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
                cropRenderer.enabled = true;
                cropRenderer.sprite = cropData?.seedSprite;
                break;
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

    // ?? IInteractable ??????????????????????????????????????????????

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

        if (state == FarmState.Tilled)
        {
            // Tìm seed phù h?p trong hotbar/inventory c?a player
            FarmingHandler handler = FindObjectOfType<FarmingHandler>();
            if (handler != null)
            {
                handler.TryPlantSeed(this);
            }
        }
    }
}
