using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour
{
    public string targetScene = "puzzle 1";

    public void Go()
    {
        SceneManager.LoadScene(targetScene);
    }
}