using UnityEngine;

/// <summary>
/// ???c thêm t? ??ng vào enemy khi spawn b?i RaidEventController.
/// Di chuy?n ??n FieldPlot g?n nh?t có cây và phá h?y sau th?i gian ??m ng??c.
/// </summary>
public class RaidEnemy : MonoBehaviour
{
    private Sprite _destroyedCropSprite;
    private float _destroyTime = 10f;
    private float _moveSpeed = 2f;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private FieldPlot _targetPlot;
    private bool _isDestroying = false;
    private float _destroyTimer;

    // Kho?ng cách coi là "?ã ??n" plot
    private const float REACH_DISTANCE = 0.4f;

    /// <summary>G?i ngay sau Instantiate ?? truy?n config t? RaidEventController.</summary>
    public void Initialize(Sprite destroyedCropSprite, float destroyTime, float moveSpeed)
    {
        _destroyedCropSprite = destroyedCropSprite;
        _destroyTime = destroyTime;
        _moveSpeed = moveSpeed;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_rb != null)
        {
            _rb.freezeRotation = true;
            _rb.gravityScale = 0f; // T?t gravity ?? enemy không b? r?i xu?ng
        }
    }

    private void Start()
    {
        FindNextTarget();
    }

    private void Update()
    {
        if (PauseController.IsGamePaused)
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            return;
        }

        // ?ang phá h?y: ??ng yên và ??m ng??c
        if (_isDestroying)
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            _destroyTimer -= Time.deltaTime;
            if (_destroyTimer <= 0f)
                FinishDestruction();
            return;
        }

        // Tìm l?i n?u target không còn h?p l?
        if (_targetPlot == null || !_targetPlot.HasCrops || _targetPlot.IsBeingRaided)
            FindNextTarget();

        if (_targetPlot == null)
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, _targetPlot.transform.position);
        if (dist <= REACH_DISTANCE)
            StartDestruction();
        else
            MoveTowardTarget();
    }

    private void FindNextTarget()
    {
        FieldPlot[] plots = Object.FindObjectsByType<FieldPlot>(FindObjectsSortMode.None);
        float closestDist = float.MaxValue;
        FieldPlot closest = null;

        foreach (FieldPlot plot in plots)
        {
            if (!plot.HasCrops || plot.IsBeingRaided) continue;
            float dist = Vector2.Distance(transform.position, plot.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = plot;
            }
        }

        _targetPlot = closest;

        if (_targetPlot != null)
            Debug.Log("<color=orange>[RaidEnemy] " + gameObject.name + " targeting plot: " + _targetPlot.name + "</color>");
        else
            Debug.Log("[RaidEnemy] " + gameObject.name + " — no crop targets left.");
    }

    private void MoveTowardTarget()
    {
        if (_rb == null || _targetPlot == null) return;

        Vector2 dir = ((Vector2)_targetPlot.transform.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = dir * _moveSpeed;

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = dir.x > 0;
    }

    private void StartDestruction()
    {
        if (_isDestroying || _targetPlot == null) return;

        _isDestroying = true;
        _destroyTimer = _destroyTime;

        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        _targetPlot.StartRaidDestruction(_destroyedCropSprite);
        Debug.Log("<color=red>[RaidEnemy] Started destroying " + _targetPlot.name + " — " + _destroyTime + "s remaining...</color>");
    }

    private void FinishDestruction()
    {
        if (_targetPlot != null)
        {
            _targetPlot.FinishRaidDestruction();
            Debug.Log("<color=red>[RaidEnemy] Finished destroying " + _targetPlot.name + "!</color>");
            _targetPlot = null;
        }

        _isDestroying = false;

        // Tìm m?c tiêu ti?p theo sau khi phá xong
        FindNextTarget();
    }

    private void OnDestroy()
    {
        // Khi enemy b? gi?t b?i player, h?y raid ?ang di?n ra
        if (_isDestroying && _targetPlot != null)
            _targetPlot.CancelRaidDestruction();
    }

    /// <summary>
    /// G?i khi enemy b? player t?n công l?n ??u.
    /// T?t RaidEnemy, b?t l?i Enemy_Movement ?? enemy ?u?i và t?n công player.
    /// </summary>
    public void SwitchToChaseMode()
    {
        // H?y raid ?ang di?n ra n?u có
        if (_isDestroying && _targetPlot != null)
        {
            _targetPlot.CancelRaidDestruction();
            _targetPlot = null;
        }

        // B?t l?i Enemy_Movement ?? ?u?i player
        Enemy_Movement em = GetComponent<Enemy_Movement>();
        if (em != null) em.enabled = true;

        // D?ng movement c?a RaidEnemy
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        // T?t RaidEnemy, nh??ng quy?n ?i?u khi?n cho Enemy_Movement
        enabled = false;
        Debug.Log("<color=yellow>[RaidEnemy] Switching to chase mode — now targeting player!</color>");
    }
}
