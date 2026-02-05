using UnityEngine;

public class BearFencePuzzleInteract : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private string promptMessage = "Giúp Bác Gấu dỡ hàng rào";

    [Header("Puzzle UI")]
    [SerializeField] private GameObject puzzlePanel;

    [SerializeField] private OrderPuzzleController puzzleController;


    private bool puzzleStarted = false;

    // ===== Interface implementation =====
    public string PromptMessage => promptMessage;

    public void Interact(GameObject interactor)
    {
        if (puzzleStarted) return;

        puzzleStarted = true;
        OpenPuzzle(interactor);
    }

    // ===== Logic =====
    private void OpenPuzzle(GameObject interactor)
    {
        if (puzzlePanel == null)
        {
            Debug.LogError("Puzzle Panel chưa được gán cho Bear!");
            return;
        }

        puzzlePanel.SetActive(true);

        // Khoá player
        if (interactor.TryGetComponent<PlayerController2D>(out var player))
        {
            player.enabled = false;
        }

        if (interactor.TryGetComponent<PlayerInteractor2D>(out var interactor2D))
        {
            promptMessage = null;
        }

        // puzzle giải xong
        puzzleController.OnPuzzleSolved += () =>
        {
            OnPuzzleCompleted(interactor);
        };
    }

    // Gọi từ UI khi puzzle hoàn thành
    public void OnPuzzleCompleted(GameObject interactor)
    {
        puzzlePanel.SetActive(false);

        if (interactor.TryGetComponent<PlayerController2D>(out var player))
        {
            player.enabled = true;
        }

        Debug.Log("Bear puzzle completed");
    }
}
