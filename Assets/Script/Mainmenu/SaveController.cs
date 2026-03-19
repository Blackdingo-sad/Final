using System.IO;
using UnityEngine;
using Cinemachine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner == null)
        {
            Debug.LogError("CinemachineConfiner not found!");
            return;
        }

        if (confiner.m_BoundingShape2D == null)
        {
            Debug.LogError("BoundingShape2D is NULL!");
            return;
        }
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindFirstObjectByType<CinemachineConfiner2D>().m_BoundingShape2D.gameObject.name

        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = saveData.playerPosition;
            CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            confiner.m_BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();
        }
        else
        {
            Debug.LogWarning("No save file found!");
            SaveGame();
        }
    }
}
