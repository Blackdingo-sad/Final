using TMPro;
using UnityEngine;

public class PLayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    public TMP_Text healthText;
    public Animator healthTextAnim;

    private bool isAnimPlaying = false;

    private void Start()
    {
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        healthTextAnim.enabled = false; 
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;

        
        if (!isAnimPlaying)
        {
            StartCoroutine(PlayAnimOnce());
        }

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

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
