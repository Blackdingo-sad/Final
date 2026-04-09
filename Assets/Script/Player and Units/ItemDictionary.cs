using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;

    private void Awake()
    {
        BuildDictionaryIfNeeded();
    }

    private void BuildDictionaryIfNeeded()
    {
        if (itemDictionary != null && itemDictionary.Count > 0)
        {
            return;
        }

        itemDictionary = new Dictionary<int, GameObject>();

        if (itemPrefabs == null)
        {
            return;
        }

        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i + 1;
            }
        }

        foreach (Item item in itemPrefabs)
        {
            if (item == null) continue;

            if (!itemDictionary.ContainsKey(item.ID))
            {
                itemDictionary[item.ID] = item.gameObject;
            }
        }
    }

    public GameObject GetItemPrefab(int itemID)
    {
        BuildDictionaryIfNeeded();

        if (itemDictionary == null)
        {
            Debug.LogWarning("Item dictionary is not initialized.");
            return null;
        }

        itemDictionary.TryGetValue(itemID, out GameObject prefab);
        if (prefab == null)
        {
            Debug.LogWarning($"Item with ID {itemID} not found in the dictionary.");
        }
        return prefab;
    }
}
