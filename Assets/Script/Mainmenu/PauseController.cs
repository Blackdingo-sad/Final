using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [SerializeField] InventoryController inventoryController;
    [SerializeField] private GameObject inventoryPanel;

    //void Update()
    //{
    //    if (Keyboard.current.tabKey.wasPressedThisFrame)
    //    {
    //        IsGamePaused = !IsGamePaused;

    //        inventoryPanel.SetActive(IsGamePaused); 

    //        if (IsGamePaused)
    //        {
    //            // mở inventory nhỏ lại
    //            inventoryController.SetInventoryScale(1f);
    //        }
    //        else
    //        {
    //            // đóng inventory phóng to
    //            inventoryController.SetInventoryScale(10f);
    //        }
    //    }
    //}
    public static bool IsGamePaused { get; private set; } = false;

    /// <summary>
    /// pause = true/false.
    /// freezeTime = true: set Time.timeScale = 0 (dung cho inventory).
    /// freezeTime = false: giu timeScale binh thuong (dung cho map transition / fade).
    /// </summary>
    public static void SetPause(bool pause, bool freezeTime = false)
    {
        IsGamePaused = pause;
        if (freezeTime)
            Time.timeScale = pause ? 0f : 1f;
    }
}
