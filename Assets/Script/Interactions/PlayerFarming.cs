using System.Collections;
using UnityEngine;

public class PlayerFarming : MonoBehaviour
{
    [Header("Animators")]
    public Animator HoeAnimator;
    public Animator PlantAnimator;
    public Animator WaterAnimator;
    public Animator HarvestAnimator;

    public bool IsBusy { get; private set; }

    public void PlayFarmAnimation(string action)
    {
        if (IsBusy) return;
        StartCoroutine(PlayAnim(action));
    }

    IEnumerator PlayAnim(string action)
    {
        IsBusy = true;

        Animator anim = action switch
        {
            "Hoe"     => HoeAnimator,
            "Plant"   => PlantAnimator,
            "Water"   => WaterAnimator,
            "Harvest" => HarvestAnimator,
            _         => null
        };

        if (anim != null)
        {
            anim.gameObject.SetActive(true);
            yield return null;
            anim.Play(action, 0, 0);
            yield return null;
            float length = anim.GetCurrentAnimatorStateInfo(0).length;
            Debug.Log($"[PlayerFarming] Playing '{action}' animation ({length:F2}s)");
            yield return new WaitForSeconds(length);
            anim.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"[PlayerFarming] No animator assigned for action: {action}");
        }

        IsBusy = false;
    }
}
