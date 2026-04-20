using UnityEngine;

[CreateAssetMenu(menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Thông tin cây tr?ng")]
    public string cropName;
    public int seedItemID;           // ID c?a h?t gi?ng trong inventory
    public GameObject harvestItemPrefab; // Prefab item thu ho?ch ???c

    [Header("T?ng tr??ng")]
    public float growTime = 10f;     // Th?i gian l?n (giây)
    public int harvestQuantity = 1;

    [Header("Hi?n th?")]
    public Sprite seedSprite;        // Sprite khi m?i tr?ng
    public Sprite growingSprite;     // Sprite ?ang l?n
    public Sprite readySprite;       // Sprite khi thu ho?ch ???c
}
