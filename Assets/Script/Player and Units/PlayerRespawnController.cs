using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerRespawnController : MonoBehaviour
{
    [Header("Respawn")]
    [Tooltip("Diem hoi sinh - keo Transform vao day")]
    public Transform respawnPoint;

    [Tooltip("Map boundary sau khi hoi sinh - keo PolygonCollider2D vao de camera doi boundary dung")]
    public PolygonCollider2D respawnMapBoundary;

    [Tooltip("Mau sau khi hoi sinh (0 = hoi day)")]
    public int respawnHealth = 0;

    [Header("Death Monologue")]
    [Tooltip("NPCDialogue ScriptableObject chua loi thoai khi chet")]
    public NPCDialogue deathDialogue;

    [Header("Daily Heal (00:00)")]
    [Tooltip("Tick de tu hoi day mau khi sang ngay moi")]
    public bool healOnNewDay = true;

    private PLayerHealth _health;
    private Rigidbody2D _rb;
    private WorldTime _worldTime;
    private int _lastHealDay = -1;
    private bool _isDead = false;

    private bool _isDialogueActive = false;
    private bool _isTyping = false;
    private int _dialogueIndex = 0;
    private DialogueController _dialogueUI;

    private void Start()
    {
        _health     = GetComponent<PLayerHealth>();
        _rb         = GetComponent<Rigidbody2D>();
        _dialogueUI = DialogueController.Instance;
        _worldTime  = FindFirstObjectByType<WorldTime>();
        if (_worldTime != null)
            _worldTime.WorldTimeChanged += OnWorldTimeChanged;
    }

    private void OnDestroy()
    {
        if (_worldTime != null)
            _worldTime.WorldTimeChanged -= OnWorldTimeChanged;
    }

    private void Update()
    {
        if (_isDialogueActive && Input.GetMouseButtonDown(0))
            NextMonologueLine();
    }

    // Daily Heal

    private void OnWorldTimeChanged(object sender, TimeSpan time)
    {
        if (!healOnNewDay) return;
        int day         = (int)(time.TotalMinutes / WorldTimeConstants.MinutesInDay);
        int minuteOfDay = (int)(time.TotalMinutes % WorldTimeConstants.MinutesInDay);
        if (minuteOfDay == 0 && day != _lastHealDay)
        {
            _lastHealDay = day;
            if (_health != null)
            {
                int amount = _health.maxHealth - _health.currentHealth;
                if (amount > 0) _health.ChangeHealth(amount);
            }
            Debug.Log("[PlayerRespawn] New day - healed to full!");
        }
    }

    // Death & Respawn

    public void OnPlayerDied()
    {
        if (_isDead) return;
        _isDead = true;
        RespawnSequence();
    }

    private async void RespawnSequence()
    {
        try
        {
            Debug.Log("[PlayerRespawn] Step 1: RespawnSequence started.");

            if (_rb != null) _rb.linearVelocity = Vector2.zero;

            // Dam bao timeScale = 1 truoc khi FadeOut
            Time.timeScale = 1f;
            PauseController.SetPause(false);

            Debug.Log("[PlayerRespawn] Step 2: Fading out...");
            if (ScreenFader.Instance != null)
                await ScreenFader.Instance.FadeOut();
            else
                Debug.LogWarning("[PlayerRespawn] ScreenFader.Instance is null - skipping fade.");

            // Teleport ve respawn point
            Debug.Log("[PlayerRespawn] Step 3: Teleporting to respawn point...");
            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
                if (_rb != null) _rb.position = respawnPoint.position;
                Debug.Log($"[PlayerRespawn] Teleported to {respawnPoint.position}");
            }
            else
            {
                Debug.LogError("[PlayerRespawn] respawnPoint chua duoc gan! Dung lai.");
                _isDead = false;
                return;
            }

            // Cap nhat camera boundary neu co
            if (respawnMapBoundary != null)
            {
                CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    confiner.m_BoundingShape2D = respawnMapBoundary;
                    confiner.InvalidateCache();
                    Debug.Log("[PlayerRespawn] Step 4: Camera boundary updated.");
                }
            }

            // Phuc hoi mau
            if (_health != null)
            {
                int targetHp = respawnHealth > 0 ? respawnHealth : _health.maxHealth;
                int heal     = targetHp - _health.currentHealth;
                if (heal != 0) _health.ChangeHealth(heal);
                Debug.Log($"[PlayerRespawn] Step 5: Health restored to {_health.currentHealth}/{_health.maxHealth}");
            }

            Debug.Log("[PlayerRespawn] Step 6: Fading in...");
            if (ScreenFader.Instance != null)
                await ScreenFader.Instance.FadeIn();

            _isDead = false;
            Debug.Log("[PlayerRespawn] Step 7: Respawn complete!");

            if (deathDialogue != null
                && deathDialogue.dialogueLines != null
                && deathDialogue.dialogueLines.Length > 0)
            {
                StartMonologue();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerRespawn] EXCEPTION in RespawnSequence: {e.Message}\n{e.StackTrace}");
            // Dam bao game khong bi ket du co loi
            _isDead = false;
            Time.timeScale = 1f;
            PauseController.SetPause(false);
        }
    }

    // Monologue - dung dung typewriter cua NPCDialogue

    private void StartMonologue()
    {
        if (_dialogueUI == null) _dialogueUI = DialogueController.Instance;
        if (_dialogueUI == null || deathDialogue == null) return;

        _dialogueIndex    = 0;
        _isDialogueActive = true;

        _dialogueUI.SetNPCInfo(deathDialogue.npcName, deathDialogue.npcPortrait);
        _dialogueUI.ShowDialogueUI(true);
        _dialogueUI.ClearChoices();
        PauseController.SetPause(true, freezeTime: true);

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        _isTyping = true;
        _dialogueUI.SetDialogueText("");

        foreach (char letter in deathDialogue.dialogueLines[_dialogueIndex])
        {
            _dialogueUI.SetDialogueText(_dialogueUI.dialogueText.text + letter);
            yield return new WaitForSecondsRealtime(deathDialogue.typingSpeed);
        }

        _isTyping = false;

        bool autoProgress = deathDialogue.autoProgressLines != null
            && deathDialogue.autoProgressLines.Length > _dialogueIndex
            && deathDialogue.autoProgressLines[_dialogueIndex];

        if (autoProgress)
        {
            yield return new WaitForSecondsRealtime(deathDialogue.autoProgressDelay);
            NextMonologueLine();
        }
    }

    private void NextMonologueLine()
    {
        if (_dialogueUI == null) return;

        if (_isTyping)
        {
            StopAllCoroutines();
            _dialogueUI.SetDialogueText(deathDialogue.dialogueLines[_dialogueIndex]);
            _isTyping = false;
            return;
        }

        _dialogueIndex++;

        if (_dialogueIndex < deathDialogue.dialogueLines.Length)
            DisplayCurrentLine();
        else
            EndMonologue();
    }

    private void EndMonologue()
    {
        StopAllCoroutines();
        _isDialogueActive = false;
        _isTyping         = false;

        if (_dialogueUI != null)
        {
            _dialogueUI.SetDialogueText(string.Empty);
            _dialogueUI.ShowDialogueUI(false);
        }

        PauseController.SetPause(false, freezeTime: true);
    }
}
