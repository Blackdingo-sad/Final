using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldInteractController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase fieldTile;

    [Header("Interact")]
    [SerializeField] private float interactRange = 1.2f;

    [SerializeField] private GameObject fieldPlotPrefab;
    [SerializeField] private Grid grid;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPlow();
        }
    }

    void TryPlow()
    {
        Debug.Log("Pressed F");

        if (ToolManager.I == null)
        {
            Debug.Log("ToolManager NULL");
            return;
        }

        if (ToolManager.I.CurrentTool == null)
        {
            Debug.Log("CurrentTool NULL");
            return;
        }

        Debug.Log("Tool: " + ToolManager.I.CurrentTool.toolType);

        Vector3Int cellPos = grid.WorldToCell(transform.position);
        Vector3 cellCenter = grid.GetCellCenterWorld(cellPos);

        Instantiate(fieldPlotPrefab, cellCenter, Quaternion.identity);
    }
}