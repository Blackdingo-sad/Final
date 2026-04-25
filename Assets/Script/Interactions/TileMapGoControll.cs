using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapGoControll : MonoBehaviour
{
    [SerializeField] private List<GameObject> triggerPoints;

    private TilemapCollider2D _tilemapCollider;
    private TilemapRenderer _tilemapRenderer;

    private const string LayerDecor = "Decor";
    private const string LayerWalkunder = "walkunder";

    void Start()
    {
        _tilemapCollider = GetComponent<TilemapCollider2D>();
        _tilemapRenderer = GetComponent<TilemapRenderer>();

        if (_tilemapCollider != null)
            _tilemapCollider.enabled = false;

        foreach (GameObject point in triggerPoints)
        {
            if (point == null) continue;

            TileMapTriggerZone trigger = point.GetComponent<TileMapTriggerZone>();
            if (trigger == null)
                trigger = point.AddComponent<TileMapTriggerZone>();

            trigger.Init(this);
        }
    }

    public void OnPlayerEnterZone()
    {
        if (_tilemapCollider != null)
            _tilemapCollider.enabled = !_tilemapCollider.enabled;

        if (_tilemapRenderer != null)
        {
            bool isWalkunder = _tilemapRenderer.sortingLayerName == LayerWalkunder;
            _tilemapRenderer.sortingLayerName = isWalkunder ? LayerDecor : LayerWalkunder;
            Debug.Log($"[TileMapGoControll] Sorting layer ? {_tilemapRenderer.sortingLayerName}");
        }
    }
}

[RequireComponent(typeof(Collider2D))]
public class TileMapTriggerZone : MonoBehaviour
{
    private TileMapGoControll _controller;

    public void Init(TileMapGoControll controller)
    {
        _controller = controller;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[TileMapTriggerZone] Player entered trigger: {gameObject.name}");
            _controller.OnPlayerEnterZone();
        }
    }
}
