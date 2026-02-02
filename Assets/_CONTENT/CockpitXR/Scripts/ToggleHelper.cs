using UnityEngine;

public class ToggleHelper : MonoBehaviour
{
    public GameObject targetObject;

    public void ToggleActiveState()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
