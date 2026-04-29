using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyController : MonoBehaviour
{
    public static CurrencyController Instance;
    [SerializeField] private int startingGold = 100;
    private int playerGold = 100;
    public event Action<int> OnGoldChanged;

    [Header("Gold Display Texts (Inventory, Shop, ...)")]
    [SerializeField] private List<TMP_Text> goldDisplayTexts = new List<TMP_Text>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            playerGold = startingGold;
        }
    }

    public int GetGold() => playerGold;

    public bool SpendGold(int amount)
    {
        if (playerGold >= amount)
        {
            playerGold -= amount;
            OnGoldChanged?.Invoke(playerGold);
            RefreshDisplays();
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGoldChanged?.Invoke(playerGold);
        RefreshDisplays();
    }

    public void SetGold(int amount)
    {
        playerGold = amount;
        OnGoldChanged?.Invoke(playerGold);
        RefreshDisplays();
    }

    private void RefreshDisplays()
    {
        foreach (TMP_Text t in goldDisplayTexts)
            if (t != null) t.text = playerGold.ToString();
    }
}
