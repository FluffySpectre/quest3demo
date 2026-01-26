using UnityEngine;
using UnityEngine.SceneManagement;

public class PalmMenuHandler : MonoBehaviour
{
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
