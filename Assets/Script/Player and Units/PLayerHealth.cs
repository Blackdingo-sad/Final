using TMPro;
using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class PLayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    public TMP_Text healthText;
    public Animator healthTextAnim;

    [Header("Respawn")]
    [Tooltip("Diem hoi sinh - keo Transform vao day")]
    public Transform respawnPoint;
    [Tooltip("Map boundary sau khi hoi sinh - keo PolygonCollider2D vao")]
    public PolygonCollider2D respawnMapBoundary;

    [Header("Death Monologue")]
    [Tooltip("NPCDialogue ScriptableObject chua loi thoai khi chet")]
    public NPCDialogue deathDialogue;

    [Header("Daily Heal (00:00)")]
    public bool healOnNewDay = true;

    private bool isAnimPlaying = false;
    private bool _isDead = false;
    private int _lastHealDay = -1;
    private WorldTime _worldTime;

    // Monologue state
    private bool _isDialogueActive = false;
    private bool _isTyping = false;
    private int _dialogueIndex = 0;
    private DialogueController _dialogueUI;

    private void Start()
    {
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        healthTextAnim.enabled = false;

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

    // ?? Daily Heal ?????????????????????????????????????????????

    private void OnWorldTimeChanged(object sender, TimeSpan time)
    {
        if (!healOnNewDay) return;
        int day         = (int)(time.TotalMinutes / WorldTimeConstants.MinutesInDay);
        int minuteOfDay = (int)(time.TotalMinutes % WorldTimeConstants.MinutesInDay);
        if (minuteOfDay == 0 && day != _lastHealDay)
        {
            _lastHealDay = day;
            int amount = maxHealth - currentHealth;
            if (amount > 0) ChangeHealth(amount);
            Debug.Log("[PLayerHealth] New day - healed to full!");
        }
    }

    // ?? Damage / Death ?????????????????????????????????????????

    public void ChangeHealth(int amount)
    {
        if (_isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;

        if (!isAnimPlaying)
            StartCoroutine(PlayAnimOnce());

        if (currentHealth <= 0)
        {
            _isDead = true;
            RespawnSequence();
        }
    }

    // ?? Respawn (giong MapTransation) ??????????????????????????

    private async void RespawnSequence()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Dam bao timeScale = 1 truoc khi fade
        Time.timeScale = 1f;
        PauseController.SetPause(false);

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeOut();

        // Teleport ve respawn point
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            if (rb != null) rb.position = respawnPoint.position;
        }
        else
        {
            Debug.LogWarning("[PLayerHealth] respawnPoint chua duoc gan!");
        }

        // Cap nhat camera boundary
        if (respawnMapBoundary != null)
        {
            CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.m_BoundingShape2D = respawnMapBoundary;
                confiner.InvalidateCache();
            }
        }

        // Hoi phuc mau day
        currentHealth = maxHealth;
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();

        _isDead = false;
        Debug.Log("[PLayerHealth] Respawn complete!");

        // Chay monologue
        if (deathDialogue != null
            && deathDialogue.dialogueLines != null
            && deathDialogue.dialogueLines.Length > 0)
        {
            StartMonologue();
        }
    }

    // ?? Monologue ??????????????????????????????????????????????

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

    // ?? HP Anim ????????????????????????????????????????????????

    private System.Collections.IEnumerator PlayAnimOnce()
    {
        isAnimPlaying = true;
        healthTextAnim.enabled = true;
        healthTextAnim.Play("HP_Update", 0, 0f);

        yield return new WaitForEndOfFrame();
        float animLength = healthTextAnim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        healthTextAnim.enabled = false;
        isAnimPlaying = false;
    }
}
