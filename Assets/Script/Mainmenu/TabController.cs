using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateTab(0);
    }

    // Update is called once per frame
    public void ActivateTab(int tabNO)
    {
        for (int i = 0; i < tabImages.Length; i++)
        {
            pages [i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        pages [tabNO].SetActive(true);
        tabImages[tabNO].color = Color.white;
    }
}
