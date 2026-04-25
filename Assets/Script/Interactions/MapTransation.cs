using UnityEngine;
using Cinemachine;

public class MapTransation : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundry;
    private CinemachineConfiner2D confiner;
    [SerializeField] private Direction direction;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private float additivePos = 2f;

    private static bool isTransitioning;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Teleport
    }

    private void Awake()
    {
        isTransitioning = false;
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTransitioning) return;
        if (!collision.CompareTag("Player")) return;

        FadeTransition(collision.gameObject);
    }

    private async void FadeTransition(GameObject player)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        try
        {
            PauseController.SetPause(true);

            if (ScreenFader.Instance != null)
            {
                await ScreenFader.Instance.FadeOut();
            }

            UpdatePlayerPosition(player);
            Physics2D.SyncTransforms();

            if (confiner != null && mapBoundry != null)
            {
                confiner.m_BoundingShape2D = mapBoundry;
                confiner.InvalidateCache();
            }

            if (ScreenFader.Instance != null)
            {
                await ScreenFader.Instance.FadeIn();
            }
        }
        finally
        {
            PauseController.SetPause(false);
            isTransitioning = false;
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (player == null) return;

        if (direction == Direction.Teleport)
        {
            if (teleportTargetPosition != null)
            {
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.position = teleportTargetPosition.position;
                }
                player.transform.position = teleportTargetPosition.position;
            }
            return;
        }

        Vector3 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivePos;
                break;
            case Direction.Down:
                newPos.y -= additivePos;
                break;
            case Direction.Left:
                newPos.x -= additivePos;
                break;
            case Direction.Right:
                newPos.x += additivePos;
                break;
        }

        player.transform.position = newPos;
    }
}
