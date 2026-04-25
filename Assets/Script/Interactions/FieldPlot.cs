using UnityEngine;

public class FieldPlot : MonoBehaviour, IInteractable
{
    public enum PlotState { Empty, Growing, ReadyToHarvest }

    [Header("Crop Visual (child GameObject)")]
    [SerializeField] private SpriteRenderer cropRenderer;

    private PlotState _state = PlotState.Empty;
    private SpriteRenderer _plotRenderer;
    private float _growTimer;
    private CropData _cropData;

    void Awake()
    {
        _plotRenderer = GetComponent<SpriteRenderer>();

        // Auto-create child crop renderer if not assigned
        if (cropRenderer == null)
        {
            GameObject cropObj = new GameObject("CropVisual");
            cropObj.transform.SetParent(transform, false);
            cropObj.transform.localPosition = Vector3.zero;
            cropRenderer = cropObj.AddComponent<SpriteRenderer>();
        }

        // Always match plot's sorting layer and set order above it
        if (_plotRenderer != null)
        {
            cropRenderer.sortingLayerID = _plotRenderer.sortingLayerID;
            cropRenderer.sortingOrder   = _plotRenderer.sortingOrder + 1;
        }
        else
        {
            cropRenderer.sortingOrder = 1;
        }

        cropRenderer.sprite = null;
        cropRenderer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_state != PlotState.Growing) return;

        _growTimer -= Time.deltaTime;
        if (_growTimer <= 0f)
            SetState(PlotState.ReadyToHarvest);
    }

    // Called when player clicks on the plot
    void OnMouseDown()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        player?.MoveToAndInteract(transform, this);
    }

    public bool CanInteract() => true;

    public void Interact()
    {
        switch (_state)
        {
            case PlotState.Empty:
                Debug.Log($"[FieldPlot] Interact called. SeedSelectionManager found: {SeedSelectionManager.Instance != null}");

                // If hotbar has a seed selected, plant directly without opening inventory
                Item hotbarSeed = HotbarController.Instance?.GetSelectedItem();
                if (hotbarSeed?.itemType == ItemType.Seed)
                {
                    PlantSeed(hotbarSeed);
                    ConsumeItem(hotbarSeed);
                }
                else if (SeedSelectionManager.Instance != null)
                    SeedSelectionManager.Instance.StartSelection(this);
                else
                    Debug.LogWarning("[FieldPlot] SeedSelectionManager.Instance is NULL! Make sure it exists in the scene.");
                break;

            case PlotState.Growing:
                Debug.Log($"[FieldPlot] Crop growing, {_growTimer:F1}s remaining...");
                break;

            case PlotState.ReadyToHarvest:
                Harvest();
                break;
        }
    }

    // Called by SeedSelectionManager when player selects a seed
    public void PlantSeed(Item seedItem)
    {
        _cropData = seedItem.cropData;
        float growTime = _cropData != null ? _cropData.growTime : 30f;
        _growTimer = growTime;

        Debug.Log($"[FieldPlot] PlantSeed called: {seedItem.Name}");
        Debug.Log($"[FieldPlot] cropData = {(_cropData != null ? _cropData.cropName : "NULL")}");
        Debug.Log($"[FieldPlot] growingSprite = {(_cropData?.growingSprite != null ? _cropData.growingSprite.name : "NULL")}");
        Debug.Log($"[FieldPlot] cropRenderer = {(cropRenderer != null ? cropRenderer.gameObject.name : "NULL")}");

        TriggerPlayerAnimation("Plant");
        SetState(PlotState.Growing);
        Debug.Log($"[FieldPlot] Planted {seedItem.Name}, ready in {growTime}s");
    }

    // Consume 1 seed from the item stack, destroy if empty
    void ConsumeItem(Item item)
    {
        item.RemoveFromStack(1);
        if (item.quantity <= 0)
        {
            Slot slot = item.transform.parent?.GetComponent<Slot>();
            if (slot != null) slot.currentItem = null;
            Object.Destroy(item.gameObject);
            InventoryController.Instance?.RebuildItemCounts();
        }
    }

    void Harvest()
    {
        TriggerPlayerAnimation("Harvest");

        if (_cropData != null && _cropData.harvestItems != null)
        {
            foreach (HarvestEntry entry in _cropData.harvestItems)
            {
                if (entry.itemPrefab == null) continue;
                for (int i = 0; i < entry.quantity; i++)
                    Instantiate(entry.itemPrefab, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);
            }
        }

        Debug.Log($"[FieldPlot] Harvested {_cropData?.cropName}!");
        _cropData = null;
        SetState(PlotState.Empty);
    }

    void SetState(PlotState newState)
    {
        _state = newState;
        UpdateCropVisual();
    }

    void UpdateCropVisual()
    {
        if (cropRenderer == null)
        {
            Debug.LogWarning("[FieldPlot] cropRenderer is NULL in UpdateCropVisual!");
            return;
        }

        switch (_state)
        {
            case PlotState.Empty:
                cropRenderer.sprite = null;
                cropRenderer.gameObject.SetActive(false);
                break;

            case PlotState.Growing:
                cropRenderer.sprite = _cropData?.growingSprite;
                cropRenderer.gameObject.SetActive(_cropData?.growingSprite != null);
                Debug.Log($"[FieldPlot] UpdateCropVisual Growing: sprite={(cropRenderer.sprite != null ? cropRenderer.sprite.name : "NULL")}, active={cropRenderer.gameObject.activeSelf}");
                break;

            case PlotState.ReadyToHarvest:
                cropRenderer.sprite = _cropData?.readySprite;
                cropRenderer.gameObject.SetActive(_cropData?.readySprite != null);
                Debug.Log($"[FieldPlot] UpdateCropVisual Ready: sprite={(cropRenderer.sprite != null ? cropRenderer.sprite.name : "NULL")}, active={cropRenderer.gameObject.activeSelf}");
                break;
        }
    }

    void TriggerPlayerAnimation(string animName)
    {
        PlayerFarming farming = Object.FindFirstObjectByType<PlayerFarming>();
        farming?.PlayFarmAnimation(animName);
    }
}

