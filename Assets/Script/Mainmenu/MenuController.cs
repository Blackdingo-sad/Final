using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public string gameSceneName = "Game";
    [Header("Options")]
    public GameObject optionsPanel;   // Panel Options (ẩn lúc đầu)
    public Slider volumeSlider;       // Slider 0..1

    void Start()
    {
        if (optionsPanel) optionsPanel.SetActive(false);

        float v = PlayerPrefs.GetFloat("masterVol", 1f);
        AudioListener.volume = v;
        if (volumeSlider)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = v;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }
    public void NewGame() => SceneManager.LoadScene(gameSceneName);
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void ShowOptions(bool show)   // gán cho nút Options / Back
    {
        if (optionsPanel) optionsPanel.SetActive(show);
    }
    public void SetVolume(float v)       // gán cho Slider OnValueChanged
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("masterVol", v);
    }
}

