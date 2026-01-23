using UnityEngine;

public class DisplayPanel : MonoBehaviour
{
    public GameObject[] pages;

    void Start()
    {
        ShowPage(-1); // Hide all pages initially
    }

    public void ShowPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
    }
}
