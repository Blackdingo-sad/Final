using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Qu?n lý s? ki?n raid c?a enemy vào ban ?êm.
/// G?n vào b?t k? GameObject nào trong Hierarchy (ví d?: GameController).
/// </summary>
public class RaidEventController : MonoBehaviour
{
    public static RaidEventController Instance { get; private set; }

    [Header("Raid Chance")]
    [Range(0f, 100f)]
    [Tooltip("% xác su?t raid xu?t hi?n m?i ?êm lúc 00:01 khi có cây ?ang tr?ng")]
    public float raidChance = 35f;

    [Header("Enemy")]
    [Tooltip("Prefab enemy s? ???c spawn trong s? ki?n raid")]
    public GameObject enemyPrefab;

    [Header("Crop Destruction")]
    [Tooltip("Sprite hi?n th? trên ô cây khi ?ang b? enemy phá h?y")]
    public Sprite destroyedCropSprite;
    [Min(1f)]
    [Tooltip("Th?i gian (giây) enemy c?n ?? phá h?y 1 ô cây")]
    public float destroyTime = 10f;

    [Header("Map Bounds")]
    [Tooltip("BoxCollider2D xác ??nh vùng map — enemy ch? spawn bên trong vùng này")]
    public BoxCollider2D mapBoundCollider;

    [Header("Spawn Settings")]
    [Min(1)]
    [Tooltip("S? enemy t?i thi?u spawn m?i l?n raid")]
    public int minEnemiesPerRaid = 1;
    [Min(1)]
    [Tooltip("S? enemy t?i ?a spawn m?i l?n raid")]
    public int maxEnemiesPerRaid = 3;
    [Tooltip("Bán kính spawn xung quanh các plot có cây")]
    public float spawnRadiusAroundPlot = 3f;

    private WorldTime _worldTime;
    private bool _raidTriggeredThisDay = false;
    private int _lastCheckedDay = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _worldTime = FindFirstObjectByType<WorldTime>();
        if (_worldTime != null)
            _worldTime.WorldTimeChanged += OnWorldTimeChanged;
        else
            Debug.LogWarning("[RaidEventController] WorldTime not found in scene!");
    }

    private void OnDestroy()
    {
        if (_worldTime != null)
            _worldTime.WorldTimeChanged -= OnWorldTimeChanged;
    }

    private void OnWorldTimeChanged(object sender, TimeSpan time)
    {
        int currentDay = (int)(time.TotalMinutes / WorldTimeConstants.MinutesInDay);
        int minuteOfDay = (int)(time.TotalMinutes % WorldTimeConstants.MinutesInDay);

        // Reset flag khi sang ngày m?i
        if (currentDay != _lastCheckedDay)
        {
            _raidTriggeredThisDay = false;
            _lastCheckedDay = currentDay;
        }

        // Kích ho?t ?úng lúc 00:01, ch? 1 l?n m?i ngày
        if (minuteOfDay == 1 && !_raidTriggeredThisDay)
        {
            _raidTriggeredThisDay = true;
            TryTriggerRaidEvent();
        }
    }

    private void TryTriggerRaidEvent()
    {
        // Ki?m tra có plot nào ?ang có cây không
        FieldPlot[] allPlots = UnityEngine.Object.FindObjectsByType<FieldPlot>(FindObjectsSortMode.None);
        List<FieldPlot> activePlots = new List<FieldPlot>();
        foreach (FieldPlot plot in allPlots)
        {
            if (plot.HasCrops) activePlots.Add(plot);
        }

        if (activePlots.Count == 0)
        {
            Debug.Log("[RaidEventController] 00:01 check — No crops planted, raid skipped.");
            return;
        }

        float roll = UnityEngine.Random.Range(0f, 100f);
        Debug.Log("[RaidEventController] Raid roll: " + roll.ToString("F1") + "% (threshold: " + raidChance + "%) — active plots: " + activePlots.Count);

        if (roll <= raidChance)
        {
            Debug.Log("<color=red>[RaidEventController] RAID EVENT triggered!</color>");
            SpawnRaidEnemies(activePlots);
        }
        else
        {
            Debug.Log("<color=green>[RaidEventController] Raid did not trigger this night. Crops are safe.</color>");
        }
    }

    private void SpawnRaidEnemies(List<FieldPlot> activePlots)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[RaidEventController] Enemy prefab not assigned!");
            return;
        }

        Bounds bounds = GetMapBounds();
        int count = UnityEngine.Random.Range(minEnemiesPerRaid, maxEnemiesPerRaid + 1);

        for (int i = 0; i < count; i++)
        {
            // Spawn g?n m?t plot ng?u nhiên trong danh sách có cây
            FieldPlot nearPlot = activePlots[UnityEngine.Random.Range(0, activePlots.Count)];
            Vector2 spawnPos = GetSpawnPositionNearPlot(nearPlot.transform.position, bounds);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // T?t Enemy_Movement bình th??ng ?? RaidEnemy ki?m soát di chuy?n
            Enemy_Movement normalMovement = enemy.GetComponent<Enemy_Movement>();
            if (normalMovement != null) normalMovement.enabled = false;

            // G?n và kh?i t?o RaidEnemy behavior
            RaidEnemy raidBehavior = enemy.AddComponent<RaidEnemy>();

            // L?y moveSpeed t? Enemy_Movement n?u có
            float speed = normalMovement != null ? normalMovement.moveSpeed : 2f;
            raidBehavior.Initialize(destroyedCropSprite, destroyTime, speed);

            Debug.Log("[RaidEventController] Spawned raid enemy at " + spawnPos + " near " + nearPlot.name);
        }
    }

    private Vector2 GetSpawnPositionNearPlot(Vector3 plotPos, Bounds bounds)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * spawnRadiusAroundPlot;
            Vector2 candidate = (Vector2)plotPos + offset;
            if (bounds.Contains(new Vector3(candidate.x, candidate.y, 0f)))
                return candidate;
        }

        // Fallback: clamp vào bounds
        Vector2 fallback = (Vector2)plotPos + UnityEngine.Random.insideUnitCircle.normalized * spawnRadiusAroundPlot;
        fallback.x = Mathf.Clamp(fallback.x, bounds.min.x, bounds.max.x);
        fallback.y = Mathf.Clamp(fallback.y, bounds.min.y, bounds.max.y);
        return fallback;
    }

    private Bounds GetMapBounds()
    {
        if (mapBoundCollider != null)
            return mapBoundCollider.bounds;

        Debug.LogWarning("[RaidEventController] mapBoundCollider not assigned! Using default bounds.");
        return new Bounds(Vector3.zero, new Vector3(50f, 50f, 0f));
    }
}
