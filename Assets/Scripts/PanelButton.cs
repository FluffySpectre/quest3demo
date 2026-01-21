using UnityEngine;
using Oculus.Interaction;

public class PanelButton : MonoBehaviour
{
    public int pageIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        eventWrapper.WhenSelect.AddListener(() =>
        {
            var displayPanel = GetComponentInParent<DisplayPanel>();
            if (displayPanel != null)
            {
                displayPanel.ShowPage(pageIndex);
            }
        });
    }
}
