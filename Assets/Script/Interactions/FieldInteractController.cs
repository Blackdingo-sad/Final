using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldInteractController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap fieldTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase fieldTile;

    [Header("Interact")]
    [SerializeField] private float interactRange = 1.2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {

            TryPlow();
        }
    }

    void TryPlow()
    {
        Vector3Int cellPos = groundTilemap.WorldToCell(transform.position);
        Vector3 cellCenter = groundTilemap.GetCellCenterWorld(cellPos);

        if (Vector3.Distance(transform.position, cellCenter) > interactRange)
            return;

        // Không cho cày nếu có fence hoặc tree
       // if (HasBlockingTile(cellPos))
        //    return;

        TileBase currentTile = groundTilemap.GetTile(cellPos);

        if (currentTile == grassTile)
        {
            groundTilemap.SetTile(cellPos, null);
            fieldTilemap.SetTile(cellPos, fieldTile);

            Debug.Log($"Plowed at {cellPos}");
        }
    }
}