using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene"; // Tên scene game chính
    [SerializeField] private SaveController saveController;

    private void Start()
    {
        if (saveController == null)
            saveController = FindFirstObjectByType<SaveController>();
    }

    /// <summary>
    /// G?n vào nút Load. N?u ?ang ? Menu Scene thì chuy?n sang Game Scene r?i load.
    /// N?u ?ang ? trong Game Scene r?i thì load th?ng.
    /// </summary>
    public void OnLoadButtonPressed()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != gameSceneName)
        {
            // ?ang ? Menu Scene ? chuy?n sang Game Scene, SaveController.Start() s? t? LoadGame()
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // ?ang trong Game Scene ? reload tr?c ti?p t? file save
            if (saveController == null)
                saveController = FindFirstObjectByType<SaveController>();

            if (saveController != null)
                saveController.LoadGame();
            else
                Debug.LogError("[LoadController] Không tìm th?y SaveController trong scene!");
        }
    }
}
