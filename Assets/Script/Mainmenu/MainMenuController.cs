using System.IO;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene chuyen den khi nhan Start hoac Load")]
    [Tooltip("Dien ten scene game chinh, phai khop voi ten trong Build Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Load Button")]
    [SerializeField] private Button loadButton;

    private string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    private void Start()
    {
        // Reset toan bo trang thai khi ve Main Menu
        ResetGameState();

        if (loadButton != null)
            loadButton.interactable = File.Exists(SavePath);
    }

    public void OnStartPressed()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[MainMenuController] Save cu da xoa.");
        }
        ResetGameState();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnLoadPressed()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[MainMenuController] No save file!");
            return;
        }
        ResetGameState();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Gan vao nut "Back to Main Menu" trong game scene
    public void OnBackToMainMenuPressed()
    {
        SaveController.Instance?.SaveGame();
        ResetGameState();
        SceneManager.LoadScene("Mainmenu");
    }

    // Reset tat ca static/global state truoc khi chuyen scene
    private void ResetGameState()
    {
        PauseController.SetPause(false, freezeTime: true);
    }
}

