using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour
{
    public GameObject Melee;
    public Animator HandAnimator;

    bool isAttacking = false;

    private void Update()
    {
        if (PauseController.IsGamePaused) return;

        if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
        {
            OnAttack();
        }
    }

    void OnAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            if (HandAnimator != null)
            {
                HandAnimator.gameObject.SetActive(true);
                StartCoroutine(PlayAttackAnimation());
            }
        }
    }

    IEnumerator PlayAttackAnimation()
    {
        yield return null;
        HandAnimator.Play("Hand", 0, 0);

        // ??i 1 frame ?? Animator c?p nh?t clip info
        yield return null;
        float clipLength = HandAnimator.GetCurrentAnimatorStateInfo(0).length;

        Debug.Log($"<color=yellow>Attack! Hitbox ON for {clipLength}s</color>");
        Melee.SetActive(true);

        yield return new WaitForSeconds(clipLength);

        Debug.Log("<color=yellow>Attack ended. Hitbox OFF</color>");
        Melee.SetActive(false);
        isAttacking = false;
        HandAnimator.gameObject.SetActive(false);
    }
}
