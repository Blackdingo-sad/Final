using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Crop Info")]
    public string cropName;

    [Header("Growth")]
    public float growTime = 10f;

    [Header("Sprites (overlay on plot)")]
    public Sprite growingSprite;
    public Sprite readySprite;

    [Header("Harvest Items")]
    public List<HarvestEntry> harvestItems;
}

[System.Serializable]
public class HarvestEntry
{
    public GameObject itemPrefab;
    public int quantity = 1;
}

