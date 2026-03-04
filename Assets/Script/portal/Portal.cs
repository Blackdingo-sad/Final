using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Target")]
    public string targetSceneName;
    public string targetSpawnID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneTransitionManager.targetSpawnID = targetSpawnID;
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
