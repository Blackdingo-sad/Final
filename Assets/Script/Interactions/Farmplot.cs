using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmPlot : MonoBehaviour
{
    public bool hasPlant = false;

    public void Plant(GameObject plantPrefab)
    {
        if (hasPlant) return;

        Instantiate(plantPrefab, transform.position, Quaternion.identity, transform);
        hasPlant = true;
    }

    public void Undo()
    {
        Destroy(gameObject);
    }
}
