using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPoint : MonoBehaviour
{
    public string spawnID;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneTransitionManager.targetSpawnID == spawnID)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                player.transform.position = transform.position;
            }
        }
    }
}